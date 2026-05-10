# Защищённое приложение с ролевым управлением доступом
# Главное окно приложения

import tkinter as tk
from tkinter import ttk, messagebox
import sqlite3
from database_manager import DatabaseManager
from security_manager import SecurityManager
from role_manager import Role
import logging

logger = logging.getLogger(__name__)


class SecureApplication:
    # Главное приложение с защитой
    
    def __init__(self, root):
        self.root = root
        self.root.title("Защищённое приложение - Управление ролями и доступом")
        self.root.geometry("900x650")
        self.root.minsize(700, 500)
        
        self.db_manager = DatabaseManager()
        self.current_user = None
        self.current_role = None
        self.current_username = None
        
        self.login_frame = None
        self.main_frame = None
        
        self.show_login_screen()
    
    def show_login_screen(self):
        # Экран аутентификации
        self.clear_window()
        
        self.login_frame = ttk.Frame(self.root, padding="20")
        self.login_frame.place(relx=0.5, rely=0.5, anchor=tk.CENTER)
        
        ttk.Label(
            self.login_frame,
            text="Вход в приложение",
            font=("Arial", 16, "bold")
        ).grid(row=0, column=0, columnspan=2, pady=20)
        
        ttk.Label(self.login_frame, text="Имя пользователя:").grid(row=1, column=0, sticky=tk.W, pady=5)
        self.username_entry = ttk.Entry(self.login_frame, width=30)
        self.username_entry.grid(row=1, column=1, pady=5)
        
        ttk.Label(self.login_frame, text="Пароль:").grid(row=2, column=0, sticky=tk.W, pady=5)
        self.password_entry = ttk.Entry(self.login_frame, width=30, show="*")
        self.password_entry.grid(row=2, column=1, pady=5)
        
        # Информация для тестирования
        info_text = """
Тестовые учётные данные:
Admin: admin / admin123
Editor: editor / editor123
Viewer: viewer / viewer123
        """
        ttk.Label(self.login_frame, text=info_text, justify=tk.LEFT, foreground="gray").grid(
            row=3, column=0, columnspan=2, pady=15, sticky=tk.W
        )
        
        button_frame = ttk.Frame(self.login_frame)
        button_frame.grid(row=4, column=0, columnspan=2, pady=20)
        
        ttk.Button(button_frame, text="Вход", command=self.login).pack(side=tk.LEFT, padx=5)
        ttk.Button(button_frame, text="Выход", command=self.root.quit).pack(side=tk.LEFT, padx=5)
        
        self.password_entry.bind('<Return>', lambda e: self.login())
    
    def login(self):
        # Процесс входа
        username = self.username_entry.get()
        password = self.password_entry.get()
        
        if not username or not password:
            messagebox.showerror("Ошибка", "Заполните все поля")
            return
        
        user_info, error = self.db_manager.authenticate_user(username, password)
        
        if error:
            messagebox.showerror("Ошибка входа", error)
            self.password_entry.delete(0, tk.END)
        else:
            self.current_user = user_info
            user_id, username, role = user_info
            self.current_username = username
            self.current_role = role
            self.show_main_screen()
    
    def show_main_screen(self):
        # Главный экран приложения
        self.clear_window()
        
        self.main_frame = ttk.Frame(self.root)
        self.main_frame.pack(fill=tk.BOTH, expand=True, padx=10, pady=10)
        
        # Заголовок с информацией о пользователе
        header_frame = ttk.Frame(self.main_frame)
        header_frame.pack(fill=tk.X, pady=10, padx=10)
        
        user_id, username, role = self.current_user
        header_text = f"Пользователь: {username} ({Role.get_role_description(role)})"
        ttk.Label(header_frame, text=header_text, font=("Arial", 12, "bold")).pack(side=tk.LEFT)
        ttk.Button(header_frame, text="Выход", command=self.logout).pack(side=tk.RIGHT)
        
        # Основной контент с вкладками
        notebook = ttk.Notebook(self.main_frame)
        notebook.pack(fill=tk.BOTH, expand=True, pady=10)
        
        # Вкладка просмотра данных (всем доступна)
        if Role.has_permission(self.current_role, 'view_data'):
            view_frame = ttk.Frame(notebook)
            notebook.add(view_frame, text="Просмотр данных")
            self._create_view_tab(view_frame)
        
        # Вкладка создания/редактирования данных
        if Role.has_permission(self.current_role, 'create_data'):
            edit_frame = ttk.Frame(notebook)
            notebook.add(edit_frame, text="Создать новое")
            self._create_edit_tab(edit_frame)
        
        # Вкладка удаления данных (только для админа)
        if Role.has_permission(self.current_role, 'delete_data'):
            delete_frame = ttk.Frame(notebook)
            notebook.add(delete_frame, text="Удалить запись")
            self._create_delete_tab(delete_frame)
        
        # Вкладка логов (только для администраторов)
        if Role.has_permission(self.current_role, 'view_logs'):
            logs_frame = ttk.Frame(notebook)
            notebook.add(logs_frame, text="Логи безопасности")
            self._create_logs_tab(logs_frame)
    
    def _create_view_tab(self, parent):
        # Создание вкладки просмотра данных
        ttk.Label(parent, text="Записи в системе:", font=("Arial", 11, "bold")).pack(pady=(10, 5))
        
        # Список данных — занимает всё доступное место
        list_frame = ttk.Frame(parent)
        list_frame.pack(fill=tk.BOTH, expand=True, padx=10, pady=(0, 5))
        
        scrollbar_y = ttk.Scrollbar(list_frame)
        scrollbar_y.pack(side=tk.RIGHT, fill=tk.Y)
        
        scrollbar_x = ttk.Scrollbar(list_frame, orient=tk.HORIZONTAL)
        scrollbar_x.pack(side=tk.BOTTOM, fill=tk.X)
        
        self.data_listbox = tk.Listbox(
            list_frame,
            yscrollcommand=scrollbar_y.set,
            xscrollcommand=scrollbar_x.set,
            font=("Courier", 10),
            activestyle='dotbox'
        )
        self.data_listbox.pack(side=tk.LEFT, fill=tk.BOTH, expand=True)
        scrollbar_y.config(command=self.data_listbox.yview)
        scrollbar_x.config(command=self.data_listbox.xview)
        
        # Подсказка
        hint = "Двойной клик — просмотр записи"
        if Role.has_permission(self.current_role, 'edit_data'):
            hint += " | Выберите запись и нажмите «Редактировать»"
        ttk.Label(parent, text=hint, foreground="gray").pack()
        
        # Кнопки управления
        button_frame = ttk.Frame(parent)
        button_frame.pack(pady=8)
        
        ttk.Button(button_frame, text="🔄 Обновить", command=self.refresh_data_view).pack(side=tk.LEFT, padx=5)
        ttk.Button(button_frame, text="🔍 Просмотр", command=self.open_view_dialog).pack(side=tk.LEFT, padx=5)
        if Role.has_permission(self.current_role, 'edit_data'):
            ttk.Button(button_frame, text="✏️ Редактировать", command=self.open_edit_dialog).pack(side=tk.LEFT, padx=5)
        
        # Двойной клик — просмотр
        self.data_listbox.bind('<Double-Button-1>', lambda e: self.open_view_dialog())
        
        self.refresh_data_view()
    
    def _on_data_select(self, event):
        # Обработка выбора записи в списке (устаревшее, оставлено для совместимости)
        pass
    
    def open_view_dialog(self):
        # Открыть окно просмотра информации о записи
        selection = self.data_listbox.curselection()
        if not selection:
            messagebox.showwarning("Предупреждение", "Выберите запись для просмотра")
            return
        
        idx = selection[0]
        data = self.db_manager.get_all_user_data()
        if idx >= len(data):
            return
        
        record_id, user_id, username, content, created_at = data[idx]
        self.selected_record = (record_id, user_id, username, content, created_at)
        
        # Создание окна просмотра
        view_window = tk.Toplevel(self.root)
        view_window.title("Просмотр записи")
        view_window.geometry("550x420")
        view_window.grab_set()
        
        f = ttk.Frame(view_window, padding="15")
        f.pack(fill=tk.BOTH, expand=True)
        
        # Метаданные
        meta_frame = ttk.LabelFrame(f, text="Информация", padding="8")
        meta_frame.pack(fill=tk.X, pady=(0, 10))
        meta_frame.columnconfigure(1, weight=1)
        
        ttk.Label(meta_frame, text="Автор:", font=("Arial", 10, "bold")).grid(row=0, column=0, sticky=tk.W, padx=(0, 10))
        ttk.Label(meta_frame, text=username).grid(row=0, column=1, sticky=tk.W)
        
        ttk.Label(meta_frame, text="Дата:", font=("Arial", 10, "bold")).grid(row=1, column=0, sticky=tk.W, padx=(0, 10), pady=(4, 0))
        ttk.Label(meta_frame, text=created_at).grid(row=1, column=1, sticky=tk.W, pady=(4, 0))
        
        # Содержание
        ttk.Label(f, text="Содержание:", font=("Arial", 10, "bold")).pack(anchor=tk.W)
        
        text_frame = ttk.Frame(f)
        text_frame.pack(fill=tk.BOTH, expand=True, pady=(5, 10))
        
        sb = ttk.Scrollbar(text_frame)
        sb.pack(side=tk.RIGHT, fill=tk.Y)
        
        text_widget = tk.Text(text_frame, wrap=tk.WORD, yscrollcommand=sb.set)
        text_widget.pack(side=tk.LEFT, fill=tk.BOTH, expand=True)
        sb.config(command=text_widget.yview)
        
        text_widget.insert("1.0", content)
        text_widget.config(state=tk.DISABLED)
        
        # Кнопки
        btn_frame = ttk.Frame(f)
        btn_frame.pack(fill=tk.X)
        
        if Role.can_edit_record(self.current_user[0], self.current_role, user_id):
            def edit_from_view():
                view_window.destroy()
                self.open_edit_dialog()
            ttk.Button(btn_frame, text="✏️ Редактировать", command=edit_from_view).pack(side=tk.LEFT, padx=5)
        
        ttk.Button(btn_frame, text="Закрыть", command=view_window.destroy).pack(side=tk.RIGHT, padx=5)
    
    def open_edit_dialog(self):
        # Открыть диалог редактирования
        # Если запись не выбрана через окно просмотра — берём из listbox
        if not hasattr(self, 'selected_record'):
            selection = self.data_listbox.curselection()
            if not selection:
                messagebox.showwarning("Предупреждение", "Выберите запись для редактирования")
                return
            idx = selection[0]
            data = self.db_manager.get_all_user_data()
            if idx >= len(data):
                return
            record_id, user_id, username, content, created_at = data[idx]
            self.selected_record = (record_id, user_id, username, content, created_at)
        
        record_id, user_id, username, content, created_at = self.selected_record
        user_id_current = self.current_user[0]
        
        # Проверка прав на редактирование
        if not Role.can_edit_record(user_id_current, self.current_role, user_id):
            messagebox.showerror("Ошибка", "Недостаточно прав для редактирования этой записи")
            return
        
        # Создание диалога редактирования
        edit_window = tk.Toplevel(self.root)
        edit_window.title("Редактирование записи")
        edit_window.geometry("550x420")
        edit_window.grab_set()
        
        f = ttk.Frame(edit_window, padding="15")
        f.pack(fill=tk.BOTH, expand=True)
        
        ttk.Label(f, text=f"Автор: {username}", font=("Arial", 10, "bold")).pack(anchor=tk.W)
        ttk.Label(f, text=f"Дата: {created_at}", foreground="gray").pack(anchor=tk.W, pady=(2, 8))
        ttk.Label(f, text="Редактируйте содержание:").pack(anchor=tk.W)
        
        text_frame = ttk.Frame(f)
        text_frame.pack(fill=tk.BOTH, expand=True, pady=(5, 10))
        
        sb = ttk.Scrollbar(text_frame)
        sb.pack(side=tk.RIGHT, fill=tk.Y)
        
        text_widget = tk.Text(text_frame, wrap=tk.WORD, yscrollcommand=sb.set)
        text_widget.pack(side=tk.LEFT, fill=tk.BOTH, expand=True)
        sb.config(command=text_widget.yview)
        text_widget.insert("1.0", content)
        
        def save_edit():
            new_content = text_widget.get("1.0", tk.END).strip()
            success, message = self.db_manager.update_user_data(
                record_id, new_content, user_id_current, self.current_role
            )
            if success:
                messagebox.showinfo("Успех", message)
                edit_window.destroy()
                del self.selected_record
                self.refresh_data_view()
            else:
                messagebox.showerror("Ошибка", message)
        
        button_frame = ttk.Frame(f)
        button_frame.pack(fill=tk.X)
        ttk.Button(button_frame, text="💾 Сохранить", command=save_edit).pack(side=tk.LEFT, padx=5)
        ttk.Button(button_frame, text="Отмена", command=edit_window.destroy).pack(side=tk.LEFT, padx=5)
    
    def _create_edit_tab(self, parent):
        # Создание вкладки создания новых данных
        ttk.Label(parent, text="Создание новой записи", font=("Arial", 11, "bold")).pack(pady=10)
        
        ttk.Label(parent, text="Введите содержание:").pack(pady=5)
        ttk.Label(parent, text="(Максимум 500 символов)", foreground="gray").pack()
        
        self.data_text = tk.Text(parent, height=15, width=80)
        self.data_text.pack(padx=10, pady=10, fill=tk.BOTH, expand=True)
        
        button_frame = ttk.Frame(parent)
        button_frame.pack(pady=10)
        
        ttk.Button(button_frame, text="Сохранить", command=self.save_data).pack(side=tk.LEFT, padx=5)
        ttk.Button(button_frame, text="Очистить", command=lambda: self.data_text.delete("1.0", tk.END)).pack(
            side=tk.LEFT, padx=5
        )
    
    def _create_delete_tab(self, parent):
        # Создание вкладки удаления
        ttk.Label(parent, text="Удаление записей", font=("Arial", 11, "bold")).pack(pady=10)
        ttk.Label(parent, text="Выберите запись для удаления:").pack(pady=5)
        
        frame = ttk.Frame(parent)
        frame.pack(fill=tk.BOTH, expand=True, padx=10, pady=10)
        
        scrollbar = ttk.Scrollbar(frame)
        scrollbar.pack(side=tk.RIGHT, fill=tk.Y)
        
        self.delete_listbox = tk.Listbox(frame, yscrollcommand=scrollbar.set)
        self.delete_listbox.pack(side=tk.LEFT, fill=tk.BOTH, expand=True)
        scrollbar.config(command=self.delete_listbox.yview)
        
        # Загрузка данных
        self.refresh_delete_view()
        
        button_frame = ttk.Frame(parent)
        button_frame.pack(pady=10)
        
        ttk.Button(button_frame, text="Удалить выбранное", command=self.delete_data).pack(side=tk.LEFT, padx=5)
        ttk.Button(button_frame, text="Обновить", command=self.refresh_delete_view).pack(side=tk.LEFT, padx=5)
    
    def _create_logs_tab(self, parent):
        # Создание вкладки логов безопасности
        ttk.Label(parent, text="Логи событий безопасности", font=("Arial", 11, "bold")).pack(pady=10)
        
        frame = ttk.Frame(parent)
        frame.pack(fill=tk.BOTH, expand=True, padx=10, pady=10)
        
        scrollbar = ttk.Scrollbar(frame)
        scrollbar.pack(side=tk.RIGHT, fill=tk.Y)
        
        self.logs_text = tk.Text(frame, yscrollcommand=scrollbar.set, height=20)
        self.logs_text.pack(side=tk.LEFT, fill=tk.BOTH, expand=True)
        scrollbar.config(command=self.logs_text.yview)
        
        self.load_logs()
    
    def refresh_data_view(self):
        # Обновление просмотра данных
        if not hasattr(self, 'data_listbox'):
            return
        
        self.data_listbox.delete(0, tk.END)
        
        data = self.db_manager.get_all_user_data()
        if not data:
            self.data_listbox.insert(tk.END, "  (Записей нет)")
            return
        
        for record_id, user_id, username, content, timestamp in data:
            # Первая строка содержимого без лишних пробелов
            first_line = content.split('\n')[0].strip()[:60]
            if len(first_line) < len(content.strip()):
                first_line += "..."
            self.data_listbox.insert(tk.END, f"  [{username}]  {timestamp[:16]}  —  {first_line}")
    
    def refresh_delete_view(self):
        # Обновление списка для удаления
        self.delete_listbox.delete(0, tk.END)
        
        data = self.db_manager.get_all_user_data()
        self.data_to_delete = data
        for record_id, user_id, username, content, timestamp in data:
            self.delete_listbox.insert(tk.END, f"{username} ({timestamp}): {content[:40]}...")
    
    def save_data(self):
        # Сохранение данных
        data = self.data_text.get("1.0", tk.END).strip()
        
        if not data:
            messagebox.showwarning("Предупреждение", "Введите содержание записи")
            return
        
        user_id = self.current_user[0]
        success, message = self.db_manager.save_user_data(user_id, self.current_username, data)
        
        if success:
            messagebox.showinfo("Успех", message)
            self.data_text.delete("1.0", tk.END)
            self.refresh_data_view()
        else:
            messagebox.showerror("Ошибка", message)
    
    def delete_data(self):
        # Удаление данных
        selection = self.delete_listbox.curselection()
        if not selection:
            messagebox.showwarning("Предупреждение", "Выберите запись для удаления")
            return
        
        if not messagebox.askyesno("Подтверждение", "Вы уверены, что хотите удалить эту запись?"):
            return
        
        idx = selection[0]
        if idx < len(self.data_to_delete):
            record_id = self.data_to_delete[idx][0]
            user_id = self.current_user[0]
            success, message = self.db_manager.delete_user_data(record_id, user_id, self.current_role)
            
            if success:
                messagebox.showinfo("Успех", message)
                self.refresh_delete_view()
                self.refresh_data_view()
            else:
                messagebox.showerror("Ошибка", message)
    
    def load_logs(self):
        # Загрузка логов безопасности
        try:
            conn = sqlite3.connect(self.db_manager.db_path)
            cursor = conn.cursor()
            
            cursor.execute('''
                SELECT sl.timestamp, sl.action, sl.details, u.username
                FROM security_logs sl
                LEFT JOIN users u ON sl.user_id = u.id
                ORDER BY sl.timestamp DESC
                LIMIT 50
            ''')
            
            logs = cursor.fetchall()
            conn.close()
            
            self.logs_text.config(state=tk.NORMAL)
            self.logs_text.delete("1.0", tk.END)
            
            for timestamp, action, details, username in logs:
                log_entry = f"[{timestamp}] {username or 'СИСТЕМА'}: {action} - {details}\n"
                self.logs_text.insert(tk.END, log_entry)
            
            self.logs_text.config(state=tk.DISABLED)
        
        except Exception as e:
            self.logs_text.config(state=tk.NORMAL)
            self.logs_text.insert(tk.END, f"Ошибка при загрузке логов: {e}")
            self.logs_text.config(state=tk.DISABLED)
    
    def logout(self):
        # Выход из приложения
        if messagebox.askyesno("Выход", "Вы уверены?"):
            self.current_user = None
            self.current_role = None
            self.show_login_screen()
    
    def clear_window(self):
        # Очистка окна
        for widget in self.root.winfo_children():
            widget.destroy()


import os
import json
import hashlib
import uuid
from pathlib import Path


def verify_license():
    # Проверка лицензии при запуске приложения.
    # Лицензия создаётся инсталлятором в папке license/license.json
    # рядом с папкой app/, т.е. на уровень выше текущего файла.
    # Ищем license.json рядом с приложением (../license/license.json)
    app_dir = Path(__file__).parent.resolve()
    license_path = app_dir.parent / 'license' / 'license.json'

    if not license_path.exists():
        # Лицензии нет — разрешаем запуск только если приложение
        # запускается напрямую из папки разработки (без установки)
        dev_mode_marker = app_dir / 'installer.py'
        if dev_mode_marker.exists():
            return True  # режим разработки
        return False, f"Файл лицензии не найден:\n{license_path}\n\nУстановите приложение через инсталлятор."

    try:
        with open(license_path, 'r') as f:
            license_data = json.load(f)

        # Проверка контрольной суммы
        stored_checksum = license_data.pop('checksum')
        data_str = json.dumps(license_data, sort_keys=True)
        calculated = hashlib.sha256(data_str.encode()).hexdigest()

        if stored_checksum != calculated:
            return False, "Файл лицензии повреждён или изменён.\nПереустановите приложение."

        # Проверка машинного ID
        current_id = hashlib.sha256(str(uuid.getnode()).encode()).hexdigest()[:16]
        if current_id != license_data.get('machine_id'):
            return False, "Лицензия привязана к другому устройству.\nПереустановите приложение на этом компьютере."

        return True, None

    except Exception as e:
        return False, f"Ошибка проверки лицензии: {e}"


def main():
    # Главная функция
    # Проверка лицензии до создания окна
    result = verify_license()
    if result is True:
        pass  # режим разработки
    else:
        ok, error_msg = result
        if not ok:
            root = tk.Tk()
            root.withdraw()
            tk.messagebox.showerror("Ошибка лицензии", error_msg)
            root.destroy()
            return

    root = tk.Tk()
    app = SecureApplication(root)
    root.mainloop()


if __name__ == "__main__":
    main()