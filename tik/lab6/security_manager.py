# Менеджер безопасности приложения
# Защита от основных типов кибератак

import hashlib
import secrets
import re
import html
import logging

# Настройка логирования
logging.basicConfig(
    level=logging.INFO,
    format='%(asctime)s - %(name)s - %(levelname)s - %(message)s',
    handlers=[
        logging.FileHandler('app_security.log'),
        logging.StreamHandler()
    ]
)
logger = logging.getLogger(__name__)


class SecurityManager:
    # Менеджер безопасности приложения
    
    # Ограничения размеров для защиты от переполнения буфера
    MAX_USERNAME_LEN = 50
    MAX_PASSWORD_LEN = 128
    MAX_DATA_LEN = 500
    MAX_LOGIN_ATTEMPTS = 5
    LOCKOUT_DURATION = 15  # минуты
    
    # Паттерны для валидации
    USERNAME_PATTERN = r'^[a-zA-Z0-9_]{3,50}$'
    
    @staticmethod
    def sanitize_input(data, max_length=500, is_username=False):
        # Защита от переполнения буфера и ошибок канонизации
        # Валидация и санитизация входных данных
        if not isinstance(data, str):
            raise ValueError("Входные данные должны быть строкой")
        
        # Защита от переполнения буфера
        if len(data) > max_length:
            raise ValueError(f"Данные превышают максимальную длину ({max_length} символов)")
        
        # Валидация имени пользователя
        if is_username:
            if not re.match(SecurityManager.USERNAME_PATTERN, data):
                raise ValueError("Неверный формат имени пользователя")
            return data.strip()
        
        # Удаление опасных символов и нормализация
        # Защита от XSS через экранирование HTML
        data = html.escape(data.strip())
        
        # Удаление управляющих символов
        data = ''.join(char for char in data if ord(char) >= 32 or char in '\n\t')
        
        return data
    
    @staticmethod
    def hash_password(password, salt=None):
        # Безопасное хеширование пароля с солью
        if salt is None:
            salt = secrets.token_hex(32)
        
        # Использование PBKDF2 для защиты от brute-force атак
        hashed = hashlib.pbkdf2_hmac(
            'sha256',
            password.encode('utf-8'),
            salt.encode('utf-8') if isinstance(salt, str) else salt,
            100000  # Количество итераций
        )
        return salt + hashlib.sha256(hashed).hexdigest()
    
    @staticmethod
    def verify_password(password, stored_hash):
        # Проверка пароля
        if len(stored_hash) < 64:
            return False
        
        salt = stored_hash[:64]
        stored = stored_hash[64:]
        
        hashed = hashlib.pbkdf2_hmac(
            'sha256',
            password.encode('utf-8'),
            salt.encode('utf-8'),
            100000
        )
        return hashlib.sha256(hashed).hexdigest() == stored
    
    @staticmethod
    def escape_sql(value):
        # Защита от SQL-инъекций через параметризованные запросы
        # (используется в DatabaseManager)
        if isinstance(value, str):
            # Двойное экранирование одиночных кавычек
            return value.replace("'", "''")
        return value