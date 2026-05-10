# Менеджер базы данных с защитой от SQL-инъекций и логированием событий

import sqlite3
import logging
from datetime import datetime, timedelta
from security_manager import SecurityManager

logger = logging.getLogger(__name__)


class DatabaseManager:
    # Менеджер базы данных с защитой от SQL-инъекций
    
    def __init__(self, db_path='app_data.db'):
        self.db_path = db_path
        self.failed_logins = {}  # Отслеживание неудачных попыток входа
        self.initialize_database()
    
    def initialize_database(self):
        # Инициализация базы данных с защитой
        try:
            conn = sqlite3.connect(self.db_path)
            cursor = conn.cursor()
            
            # Таблица пользователей
            cursor.execute('''
                CREATE TABLE IF NOT EXISTS users (
                    id INTEGER PRIMARY KEY AUTOINCREMENT,
                    username TEXT UNIQUE NOT NULL,
                    password_hash TEXT NOT NULL,
                    role TEXT NOT NULL,
                    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
                    locked_until TIMESTAMP,
                    failed_attempts INTEGER DEFAULT 0
                )
            ''')
            
            # Таблица данных (ОБЩАЯ для всех пользователей)
            cursor.execute('''
                CREATE TABLE IF NOT EXISTS user_data (
                    id INTEGER PRIMARY KEY AUTOINCREMENT,
                    user_id INTEGER NOT NULL,
                    username TEXT NOT NULL,
                    data_content TEXT NOT NULL,
                    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
                    FOREIGN KEY (user_id) REFERENCES users(id)
                )
            ''')
            
            # Таблица логов безопасности
            cursor.execute('''
                CREATE TABLE IF NOT EXISTS security_logs (
                    id INTEGER PRIMARY KEY AUTOINCREMENT,
                    user_id INTEGER,
                    action TEXT NOT NULL,
                    details TEXT,
                    timestamp TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
                    ip_address TEXT
                )
            ''')
            
            conn.commit()
            
            # Миграция: добавить колонку username если её нет (для старых БД)
            self._migrate_database(cursor, conn)
            
            # Создание тестовых пользователей
            self._create_test_users(cursor, conn)
            
            conn.close()
            logger.info("База данных инициализирована успешно")
        except Exception as e:
            logger.error(f"Ошибка при инициализации БД: {e}")
            raise
    
    def _migrate_database(self, cursor, conn):
        # Миграция старых БД: добавляет колонку username если её нет
        try:
            cursor.execute("PRAGMA table_info(user_data)")
            columns = [row[1] for row in cursor.fetchall()]
            if 'username' not in columns:
                cursor.execute("ALTER TABLE user_data ADD COLUMN username TEXT NOT NULL DEFAULT 'unknown'")
                # Заполнить имена из таблицы users
                cursor.execute("""
                    UPDATE user_data SET username = (
                        SELECT username FROM users WHERE users.id = user_data.user_id
                    )
                """)
                conn.commit()
                logger.info("Миграция БД: добавлена колонка username")
        except Exception as e:
            logger.warning(f"Миграция БД: {e}")

    def _create_test_users(self, cursor, conn):
        # Создание тестовых пользователей
        test_users = [
            ('admin', 'admin123', 'admin'),
            ('editor', 'editor123', 'editor'),
            ('viewer', 'viewer123', 'viewer')
        ]
        
        for username, password, role in test_users:
            try:
                cursor.execute('SELECT id FROM users WHERE username = ?', (username,))
                if not cursor.fetchone():
                    password_hash = SecurityManager.hash_password(password)
                    cursor.execute(
                        'INSERT INTO users (username, password_hash, role) VALUES (?, ?, ?)',
                        (username, password_hash, role)
                    )
                    logger.info(f"Создан тестовый пользователь: {username}")
            except sqlite3.IntegrityError:
                pass
        
        conn.commit()
    
    def authenticate_user(self, username, password):
        # Аутентификация пользователя с защитой от brute-force
        try:
            username = SecurityManager.sanitize_input(username, is_username=True)
        except ValueError as e:
            logger.warning(f"Ошибка валидации имени пользователя: {e}")
            return None, "Неверное имя пользователя"
        
        # Проверка блокировки
        if username in self.failed_logins:
            locked_until, attempts = self.failed_logins[username]
            if datetime.now() < locked_until:
                remaining = (locked_until - datetime.now()).seconds // 60
                logger.warning(f"Попытка входа заблокированного пользователя: {username}")
                # Записываем в БД: получаем user_id по имени
                user_id = self._get_user_id(username)
                self._log_security_event(
                    user_id, 'login_blocked',
                    f'Вход заблокирован для {username}, осталось {remaining} мин.'
                )
                return None, f"Аккаунт заблокирован на {remaining} минут"
            else:
                del self.failed_logins[username]
        
        try:
            conn = sqlite3.connect(self.db_path)
            cursor = conn.cursor()
            
            # Защита от SQL-инъекций через параметризованный запрос
            cursor.execute(
                'SELECT id, password_hash, role FROM users WHERE username = ?',
                (username,)
            )
            result = cursor.fetchone()
            conn.close()
            
            if result:
                user_id, stored_hash, role = result
                if SecurityManager.verify_password(password, stored_hash):
                    # Успешная аутентификация
                    if username in self.failed_logins:
                        del self.failed_logins[username]
                    
                    self._log_security_event(user_id, 'login_success', f'Успешный вход')
                    logger.info(f"Успешная аутентификация пользователя: {username}")
                    return (user_id, username, role), None
                else:
                    # Неверный пароль — записываем в БД и в лог
                    self._record_failed_login(username)
                    attempts = self.failed_logins.get(username, (None, 0))[1]
                    logger.warning(f"Неверный пароль для пользователя: {username}")
                    self._log_security_event(
                        user_id, 'login_failed',
                        f'Неверный пароль для {username} (попытка {attempts}/{SecurityManager.MAX_LOGIN_ATTEMPTS})'
                    )
                    return None, "Неверное имя пользователя или пароль"
            else:
                # Пользователь не найден
                logger.warning(f"Попытка входа с несуществующим пользователем: {username}")
                self._log_security_event(
                    None, 'login_unknown_user',
                    f'Попытка входа с несуществующим логином: {username}'
                )
                return None, "Неверное имя пользователя или пароль"
        
        except Exception as e:
            logger.error(f"Ошибка при аутентификации: {e}")
            return None, "Ошибка при проверке учётных данных"
    
    def _record_failed_login(self, username):
        # Запись неудачной попытки входа
        if username not in self.failed_logins:
            self.failed_logins[username] = (
                datetime.now() + timedelta(minutes=SecurityManager.LOCKOUT_DURATION),
                1
            )
        else:
            locked_until, attempts = self.failed_logins[username]
            self.failed_logins[username] = (locked_until, attempts + 1)
        
        if self.failed_logins[username][1] >= SecurityManager.MAX_LOGIN_ATTEMPTS:
            logger.warning(f"Пользователь {username} заблокирован после множества неудачных попыток")
    
    def save_user_data(self, user_id, username, data):
        # Сохранение данных пользователя с защитой от атак
        try:
            # Санитизация входных данных
            sanitized_data = SecurityManager.sanitize_input(
                data,
                max_length=SecurityManager.MAX_DATA_LEN
            )
            
            conn = sqlite3.connect(self.db_path)
            cursor = conn.cursor()
            
            # Параметризованный запрос (защита от SQL-инъекций)
            cursor.execute(
                'INSERT INTO user_data (user_id, username, data_content) VALUES (?, ?, ?)',
                (user_id, username, sanitized_data)
            )
            
            conn.commit()
            conn.close()
            
            self._log_security_event(user_id, 'data_save', 'Данные сохранены')
            logger.info(f"Данные сохранены пользователем {username}")
            return True, "Данные успешно сохранены"
        
        except Exception as e:
            logger.error(f"Ошибка при сохранении данных: {e}")
            return False, f"Ошибка при сохранении: {str(e)}"
    
    def get_all_user_data(self):
        # Получение всех данных (для просмотра в зависимости от роли)
        try:
            conn = sqlite3.connect(self.db_path)
            cursor = conn.cursor()
            
            cursor.execute(
                'SELECT id, user_id, username, data_content, created_at FROM user_data ORDER BY created_at DESC'
            )
            results = cursor.fetchall()
            conn.close()
            
            return results if results else []
        
        except Exception as e:
            logger.error(f"Ошибка при получении данных: {e}")
            return []
    
    def get_user_data(self, user_id):
        # Получение данных конкретного пользователя
        try:
            conn = sqlite3.connect(self.db_path)
            cursor = conn.cursor()
            
            cursor.execute(
                'SELECT id, data_content, created_at, user_id FROM user_data WHERE user_id = ? ORDER BY created_at DESC',
                (user_id,)
            )
            results = cursor.fetchall()
            conn.close()
            
            return results if results else []
        
        except Exception as e:
            logger.error(f"Ошибка при получении данных: {e}")
            return []
    
    def update_user_data(self, data_id, new_content, user_id, user_role):
        # Обновление данных с проверкой прав
        try:
            # Санитизация входных данных
            sanitized_data = SecurityManager.sanitize_input(
                new_content,
                max_length=SecurityManager.MAX_DATA_LEN
            )
            
            conn = sqlite3.connect(self.db_path)
            cursor = conn.cursor()
            
            # Проверка прав на редактирование
            cursor.execute(
                'SELECT user_id FROM user_data WHERE id = ?',
                (data_id,)
            )
            result = cursor.fetchone()
            
            if not result:
                conn.close()
                return False, "Запись не найдена"
            
            record_user_id = result[0]
            
            # Проверка прав доступа
            from role_manager import Role
            if not Role.can_edit_record(user_id, user_role, record_user_id):
                conn.close()
                logger.warning(f"Попытка несанкционированного редактирования пользователем {user_id}")
                return False, "Недостаточно прав для редактирования этой записи"
            
            # Обновление записи
            cursor.execute(
                'UPDATE user_data SET data_content = ? WHERE id = ?',
                (sanitized_data, data_id)
            )
            
            conn.commit()
            conn.close()
            
            self._log_security_event(user_id, 'data_update', f'Запись {data_id} обновлена')
            logger.info(f"Запись {data_id} обновлена пользователем {user_id}")
            return True, "Запись успешно обновлена"
        
        except Exception as e:
            logger.error(f"Ошибка при обновлении данных: {e}")
            return False, f"Ошибка при обновлении: {str(e)}"
    
    def delete_user_data(self, data_id, user_id, user_role):
        # Удаление данных с проверкой прав
        try:
            conn = sqlite3.connect(self.db_path)
            cursor = conn.cursor()
            
            # Получение информации о записи
            cursor.execute(
                'SELECT user_id FROM user_data WHERE id = ?',
                (data_id,)
            )
            result = cursor.fetchone()
            
            if not result:
                conn.close()
                return False, "Запись не найдена"
            
            record_user_id = result[0]
            
            # Проверка прав доступа (только Admin может удалять)
            from role_manager import Role
            if not Role.can_delete_record(user_id, user_role, record_user_id):
                conn.close()
                logger.warning(f"Попытка несанкционированного удаления пользователем {user_id}")
                return False, "Недостаточно прав для удаления этой записи"
            
            cursor.execute('DELETE FROM user_data WHERE id = ?', (data_id,))
            conn.commit()
            conn.close()
            
            self._log_security_event(user_id, 'data_delete', f'Данные удалены (id={data_id})')
            logger.info(f"Данные удалены пользователем {user_id}")
            return True, "Запись успешно удалена"
        
        except Exception as e:
            logger.error(f"Ошибка при удалении данных: {e}")
            return False, f"Ошибка при удалении: {str(e)}"
    
    def _get_user_id(self, username):
        # Получить user_id по имени пользователя (для логирования до аутентификации)
        try:
            conn = sqlite3.connect(self.db_path)
            cursor = conn.cursor()
            cursor.execute('SELECT id FROM users WHERE username = ?', (username,))
            result = cursor.fetchone()
            conn.close()
            return result[0] if result else None
        except Exception:
            return None

    def _log_security_event(self, user_id, action, details):
        # Логирование событий безопасности
        try:
            conn = sqlite3.connect(self.db_path)
            cursor = conn.cursor()
            
            cursor.execute(
                'INSERT INTO security_logs (user_id, action, details) VALUES (?, ?, ?)',
                (user_id, action, details)
            )
            
            conn.commit()
            conn.close()
        except Exception as e:
            logger.error(f"Ошибка при логировании события: {e}")