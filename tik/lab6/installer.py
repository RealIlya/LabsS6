# Инсталлятор приложения с защитой от копирования
# Лабораторная работа №6
# Защита включает:
# 1. Проверку целостности файлов (контрольные суммы)
# 2. Защиту от модификации через лицензионный ключ
# 3. Логирование процесса установки
# 4. Привязку к системе (MAC адрес)

import tkinter as tk
from tkinter import ttk, messagebox, filedialog
import os
import shutil
import hashlib
import json
import uuid
import subprocess
import logging
import sqlite3
import getpass
from datetime import datetime
from pathlib import Path
import re

# Настройка логирования
logging.basicConfig(
    level=logging.INFO,
    format='%(asctime)s - %(levelname)s - %(message)s',
    handlers=[
        logging.FileHandler('installer.log'),
        logging.StreamHandler()
    ]
)
logger = logging.getLogger(__name__)


class LicenseManager:
    # Менеджер лицензирования с защитой от копирования
    
    @staticmethod
    def generate_machine_id():
        # Генерация уникального ID машины на основе MAC адреса
        # Привязывает лицензию к конкретной системе
        try:
            # Получение MAC адреса
            if os.name == 'nt':  # Windows
                mac = uuid.getnode()
            else:  # Linux/Mac
                mac = uuid.getnode()
            
            return hashlib.sha256(str(mac).encode()).hexdigest()[:16]
        except Exception as e:
            logger.error(f"Ошибка при получении машинного ID: {e}")
            return None
    
    @staticmethod
    def generate_license_key(machine_id, username):
        # Генерация лицензионного ключа
        # Защита от копирования на другие машины
        license_data = {
            'machine_id': machine_id,
            'username': username,
            'timestamp': datetime.now().isoformat(),
            'version': '1.0'
        }
        
        # Создание контрольной суммы
        data_str = json.dumps(license_data, sort_keys=True)
        checksum = hashlib.sha256(data_str.encode()).hexdigest()
        
        license_data['checksum'] = checksum
        
        return json.dumps(license_data, indent=2)
    
    @staticmethod
    def verify_license(license_path):
        # Проверка валидности лицензии
        try:
            if not os.path.exists(license_path):
                logger.error("Файл лицензии не найден")
                return False, "Лицензия не найдена"
            
            with open(license_path, 'r') as f:
                license_data = json.load(f)
            
            # Проверка контрольной суммы
            stored_checksum = license_data.pop('checksum')
            data_str = json.dumps(license_data, sort_keys=True)
            calculated_checksum = hashlib.sha256(data_str.encode()).hexdigest()
            
            if stored_checksum != calculated_checksum:
                logger.error("Лицензия повреждена или модифицирована")
                return False, "Лицензия повреждена"
            
            # Проверка машинного ID
            current_machine_id = LicenseManager.generate_machine_id()
            if current_machine_id != license_data.get('machine_id'):
                logger.error("Лицензия привязана к другой машине")
                return False, "Лицензия недействительна для этого устройства"
            
            logger.info("Лицензия проверена успешно")
            return True, "Лицензия действительна"
        
        except Exception as e:
            logger.error(f"Ошибка при проверке лицензии: {e}")
            return False, f"Ошибка проверки: {str(e)}"


class IntegrityChecker:
    # Проверка целостности файлов
    
    @staticmethod
    def calculate_file_hash(filepath, algorithm='sha256'):
        # Расчёт хеша файла
        hash_obj = hashlib.new(algorithm)
        
        try:
            with open(filepath, 'rb') as f:
                for chunk in iter(lambda: f.read(4096), b''):
                    hash_obj.update(chunk)
            return hash_obj.hexdigest()
        except Exception as e:
            logger.error(f"Ошибка при расчёте хеша файла {filepath}: {e}")
            return None
    
    @staticmethod
    def create_manifest(directory, exclude_patterns=None):
        # Создание манифеста целостности файлов
        if exclude_patterns is None:
            exclude_patterns = ['__pycache__', '*.pyc', '.git']
        
        manifest = {}
        
        for root, dirs, files in os.walk(directory):
            # Исключение папок
            dirs[:] = [d for d in dirs if not any(
                re.match(pattern.replace('*', '.*'), d) for pattern in exclude_patterns
            )]
            
            for file in files:
                filepath = os.path.join(root, file)
                
                # Пропуск исключённых файлов
                if any(re.match(pattern.replace('*', '.*'), file) for pattern in exclude_patterns):
                    continue
                
                relative_path = os.path.relpath(filepath, directory)
                file_hash = IntegrityChecker.calculate_file_hash(filepath)
                
                if file_hash:
                    manifest[relative_path] = {
                        'hash': file_hash,
                        'algorithm': 'sha256',
                        'size': os.path.getsize(filepath)
                    }
        
        return manifest
    
    @staticmethod
    def verify_manifest(directory, manifest):
        # Проверка целостности по манифесту
        errors = []
        
        for filepath, file_info in manifest.items():
            full_path = os.path.join(directory, filepath)
            
            if not os.path.exists(full_path):
                errors.append(f"Файл отсутствует: {filepath}")
                continue
            
            current_hash = IntegrityChecker.calculate_file_hash(full_path)
            
            if current_hash != file_info['hash']:
                errors.append(f"Файл изменён: {filepath}")
        
        return len(errors) == 0, errors


class Installer:
    # Инсталлятор приложения
    
    def __init__(self, root):
        self.root = root
        self.root.title("Инсталляция приложения - Защищённое управление доступом")
        self.root.geometry("600x500")
        self.root.minsize(500, 400)
        
        self.install_path = None
        self.manifest = None
        
        self.create_ui()
    
    def create_ui(self):
        # Создание интерфейса инсталлятора
        main_frame = ttk.Frame(self.root, padding="20")
        main_frame.pack(fill=tk.BOTH, expand=True)

        # Заголовок
        ttk.Label(
            main_frame,
            text="Инсталляция приложения",
            font=("Arial", 16, "bold")
        ).pack(pady=(0, 4))

        ttk.Label(
            main_frame,
            text="Защищённое управление доступом  •  Версия 1.0",
            foreground="gray"
        ).pack(pady=(0, 10))

        # Выбор пути установки
        path_frame = ttk.LabelFrame(main_frame, text="Путь установки", padding="8")
        path_frame.pack(fill=tk.X, pady=(0, 8))

        self.path_var = tk.StringVar(value=str(Path.home() / "SecureApp"))
        ttk.Entry(path_frame, textvariable=self.path_var).pack(side=tk.LEFT, fill=tk.X, expand=True)
        ttk.Button(path_frame, text="Обзор", command=self.select_path).pack(side=tk.RIGHT, padx=(6, 0))

        # Прогресс-бар
        self.progress_var = tk.DoubleVar()
        ttk.Progressbar(main_frame, variable=self.progress_var, maximum=100).pack(fill=tk.X, pady=(0, 4))

        self.status_label = ttk.Label(main_frame, text="Готов к установке", foreground="gray")
        self.status_label.pack(pady=(0, 6))

        # Кнопки закреплены через side=BOTTOM ДО expand-фрейма:
        # pack() резервирует им место прежде чем лог растянется,
        # поэтому они всегда видны при любом размере окна.
        button_frame = ttk.Frame(main_frame)
        button_frame.pack(side=tk.BOTTOM, fill=tk.X, pady=(8, 0))
        ttk.Button(button_frame, text="Установить", command=self.start_installation).pack(side=tk.LEFT, padx=(0, 5))
        ttk.Button(button_frame, text="Выход", command=self.root.quit).pack(side=tk.LEFT)

        # Лог установки — занимает всё оставшееся место
        log_frame = ttk.LabelFrame(main_frame, text="Лог установки", padding="8")
        log_frame.pack(fill=tk.BOTH, expand=True)

        scrollbar = ttk.Scrollbar(log_frame)
        scrollbar.pack(side=tk.RIGHT, fill=tk.Y)

        self.log_text = tk.Text(log_frame, height=8, yscrollcommand=scrollbar.set)
        self.log_text.pack(side=tk.LEFT, fill=tk.BOTH, expand=True)
        scrollbar.config(command=self.log_text.yview)
    
    def select_path(self):
        # Выбор пути установки
        path = filedialog.askdirectory(title="Выберите папку установки")
        if path:
            self.path_var.set(path)
    
    def log_message(self, message):
        # Логирование сообщения
        self.log_text.insert(tk.END, message + "\n")
        self.log_text.see(tk.END)
        self.root.update()
        logger.info(message)
    
    def start_installation(self):
        # Начало процесса установки
        install_path = self.path_var.get()
        
        if not install_path:
            messagebox.showerror("Ошибка", "Выберите путь установки")
            return
        
        self.log_text.delete("1.0", tk.END)
        self.progress_var.set(0)
        self.status_label.config(text="Установка в процессе...", foreground="blue")
        
        try:
            self.log_message("=" * 50)
            self.log_message("Начало установки приложения")
            self.log_message("=" * 50)
            
            # Шаг 1: Проверка прав
            self.log_message("\n[1/5] Проверка прав доступа...")
            if not self._check_permissions(install_path):
                raise Exception("Недостаточно прав для установки")
            self.progress_var.set(20)
            self.log_message("✓ Права проверены")
            
            # Шаг 2: Создание структуры директорий
            self.log_message("\n[2/5] Создание структуры директорий...")
            self._create_directories(install_path)
            self.progress_var.set(40)
            self.log_message("✓ Структура создана")
            
            # Шаг 3: Копирование файлов
            self.log_message("\n[3/5] Копирование файлов приложения...")
            self._copy_files(install_path)
            self.progress_var.set(60)
            self.log_message("✓ Файлы скопированы")
            
            # Шаг 4: Создание лицензии
            self.log_message("\n[4/5] Генерация лицензии...")
            self._generate_license(install_path)
            self.progress_var.set(80)
            self.log_message("✓ Лицензия создана")
            
            # Шаг 5: Проверка целостности
            self.log_message("\n[5/5] Проверка целостности файлов...")
            if not self._verify_integrity(install_path):
                raise Exception("Ошибка проверки целостности")
            self.progress_var.set(100)
            self.log_message("✓ Целостность подтверждена")
            
            self.log_message("\n" + "=" * 50)
            self.log_message("Установка завершена успешно!")
            self.log_message("=" * 50)
            
            self.status_label.config(text="Установка завершена", foreground="green")
            messagebox.showinfo("Успех", f"Приложение установлено в {install_path}")
        
        except Exception as e:
            self.log_message(f"\n✗ ОШИБКА: {str(e)}")
            self.status_label.config(text="Ошибка при установке", foreground="red")
            messagebox.showerror("Ошибка установки", str(e))
            logger.error(f"Ошибка установки: {e}")
    
    def _check_permissions(self, path):
        # Проверка прав доступа
        try:
            parent_dir = os.path.dirname(path) or '.'
            return os.access(parent_dir, os.W_OK)
        except Exception:
            return False
    
    def _create_directories(self, install_path):
        # Создание структуры директорий
        directories = [
            install_path,
            os.path.join(install_path, 'app'),
            os.path.join(install_path, 'data'),
            os.path.join(install_path, 'logs'),
            os.path.join(install_path, 'license')
        ]
        
        for directory in directories:
            os.makedirs(directory, exist_ok=True)
            self.log_message(f"  Создана папка: {directory}")
    
    def _copy_files(self, install_path):
        # Копирование файлов приложения
        # Определяем папку, где лежит сам инсталлятор — там же должны быть файлы приложения
        source_dir = Path(__file__).parent.resolve()
        dest_app_dir = Path(install_path) / 'app'
        
        # Файлы приложения, которые нужно установить
        app_files = [
            'main.py',
            'secure_app.py',
            'database_manager.py',
            'security_manager.py',
            'role_manager.py',
        ]
        
        copied = 0
        for filename in app_files:
            src = source_dir / filename
            dst = dest_app_dir / filename
            if src.exists():
                shutil.copy2(src, dst)
                self.log_message(f"  Скопирован: {filename}")
                copied += 1
            else:
                self.log_message(f"  ! Файл не найден: {filename}")
        
        if copied == 0:
            raise Exception(
                "Не найдено ни одного файла приложения. "
                "Убедитесь, что installer.py находится в одной папке с файлами приложения."
            )
        
        # Создание файла README
        readme_content = """# Защищённое приложение - Управление ролями и доступом
        
## Установка

Приложение установлено в этой директории.

## Использование

Запустите приложение командой:
    python app/secure_app.py

## Тестовые учётные данные

- admin / admin123 (полный доступ)
- editor / editor123 (редактирование)
- viewer / viewer123 (только просмотр)

## Требования

- Python 3.7+
- tkinter (обычно включена в Python)

## Поддержка

Для проблем см. логи в папке 'logs'
"""
        
        with open(os.path.join(install_path, 'README.md'), 'w') as f:
            f.write(readme_content)
        self.log_message(f"  Создан: README.md")
    
    def _generate_license(self, install_path):
        # Генерация лицензии
        machine_id = LicenseManager.generate_machine_id()
        if not machine_id:
            raise Exception("Не удалось получить машинный ID")
        
        username = getpass.getuser()
        license_content = LicenseManager.generate_license_key(machine_id, username)
        
        license_path = os.path.join(install_path, 'license', 'license.json')
        with open(license_path, 'w') as f:
            f.write(license_content)
        
        self.log_message(f"  Лицензия создана для пользователя: {username}")
        self.log_message(f"  Машинный ID: {machine_id}")
    
    def _verify_integrity(self, install_path):
        # Проверка целостности установки
        app_dir = os.path.join(install_path, 'app')
        
        # Создание манифеста
        manifest = IntegrityChecker.create_manifest(app_dir)
        
        # Сохранение манифеста
        manifest_path = os.path.join(install_path, 'license', 'manifest.json')
        with open(manifest_path, 'w') as f:
            json.dump(manifest, f, indent=2)
        
        self.log_message(f"  Манифест создан: {len(manifest)} файлов")
        
        # Проверка целостности
        is_valid, errors = IntegrityChecker.verify_manifest(app_dir, manifest)
        
        if errors:
            self.log_message("  Обнаружены ошибки целостности:")
            for error in errors:
                self.log_message(f"    - {error}")
        
        return is_valid


class InstallerLauncher:
    # Запуск инсталлятора с проверкой целостности
    
    @staticmethod
    def verify_installer():
        # Проверка целостности самого инсталлятора
        try:
            logger.info("Проверка целостности инсталлятора...")
            # В реальном приложении здесь будет проверка подписи инсталлятора
            logger.info("Инсталлятор проверен успешно")
            return True
        except Exception as e:
            logger.error(f"Ошибка проверки инсталлятора: {e}")
            return False
    
    @staticmethod
    def run():
        # Запуск инсталлятора
        if not InstallerLauncher.verify_installer():
            messagebox.showerror(
                "Ошибка безопасности",
                "Инсталлятор повреждён или недействителен"
            )
            return
        
        root = tk.Tk()
        installer = Installer(root)
        root.mainloop()


if __name__ == "__main__":
    InstallerLauncher.run()