"""
Графический интерфейс для генератора ПСП ANSI X9.17 + статистические тесты NIST.
"""

import tkinter as tk
from tkinter import ttk, filedialog, messagebox
import os
import time

from generator_ansi import (
    encrypt_3des,
    run_tests,
    read_file_content,
    save_file_content,
)

# ─── Цветовая палитра (тёмная тема) ───
BG       = "#1E1E1E"
CARD     = "#252526"
PRIMARY  = "#3A3A3C"
PRIMARY_HOVER = "#4A4A4C"
TEXT     = "#D4D4D4"
SUBTLE   = "#808080"
BORDER   = "#3C3C3C"
OUTPUT_BG = "#1E1E1E"
OUTPUT_FG = "#D4D4D4"
CURSOR   = "#D4D4D4"


class App(ttk.Frame):
    def __init__(self, root: tk.Tk):
        super().__init__(root)
        self.root = root
        self.root.title("Лабораторная работа №4: Псевдослучайная генерация (ANSI X9.17)")
        self.root.geometry("920x680")
        self.root.minsize(800, 600)
        self.root.configure(bg=BG)

        self._style()

        self.k1_var = tk.StringVar(value="A1B2C3D4E5F60789")
        self.k2_var = tk.StringVar(value="9876543210FEDCBA")
        self.s0_var = tk.StringVar(value="1122334455667788")
        self.m_var  = tk.StringVar(value="157")

        self._build()
        self._try_load_config()

        self.gen_active = False
        self.gen_k1 = self.gen_k2 = self.gen_s_prev = self.gen_temp = 0
        self.gen_m = self.gen_i = 0
        self.gen_result_parts = []

    # ─── Стили ───
    def _style(self):
        s = ttk.Style()
        s.theme_use("clam")

        s.configure(".", background=BG, foreground=TEXT, font=("TkDefaultFont", 10))
        s.configure("TFrame", background=BG)
        s.configure("TLabel", background=BG, foreground=TEXT)
        s.configure("TLabelFrame", background=CARD, foreground=TEXT, bordercolor=BORDER)
        s.configure("TLabelFrame.Label", background=CARD, foreground=TEXT)
        s.configure("TButton", background=PRIMARY, foreground=TEXT, bordercolor=BORDER)
        s.map("TButton",
              background=[("active", PRIMARY_HOVER), ("pressed", "#0A4D7A")],
              foreground=[("active", "#FFFFFF")])
        s.configure("TEntry", fieldbackground=BG, foreground=TEXT, bordercolor=BORDER)
        s.configure("TNotebook", background=BG)
        s.configure("TNotebook.Tab", background=CARD, foreground=TEXT, padding=[12, 4])
        s.map("TNotebook.Tab",
              background=[("selected", BG)],
              foreground=[("selected", TEXT)])
        s.configure("TProgressbar", background=PRIMARY, troughcolor=BORDER, thickness=6)

    # ─── UI ───
    def _build(self):
        nb = ttk.Notebook(self)
        nb.pack(fill="both", expand=True, padx=8, pady=8)

        tab1 = ttk.Frame(nb)
        nb.add(tab1, text="Генерация ПСП (ANSI X9.17)")
        self._build_gen_tab(tab1)

        tab2 = ttk.Frame(nb)
        nb.add(tab2, text="Статистические тесты NIST")
        self._build_test_tab(tab2)

    def _build_gen_tab(self, parent):
        parent.columnconfigure(0, weight=1)
        parent.columnconfigure(1, weight=1)

        btn_load = ttk.Button(parent, text="Загрузить параметры из файла...", command=self._load_params)
        btn_load.grid(row=0, column=0, columnspan=2, pady=(10, 6), sticky="w", padx=10)

        labels = [
            "Ключ K1 (16 hex-символов):",
            "Ключ K2 (16 hex-символов):",
            "Начальное значение s0 (16 hex-символов):",
            "Количество 64-битных блоков m:",
        ]
        vars_ = [self.k1_var, self.k2_var, self.s0_var, self.m_var]
        for i, (lbl, var) in enumerate(zip(labels, vars_)):
            ttk.Label(parent, text=lbl).grid(row=i + 1, column=0, sticky="e", padx=(10, 4), pady=4)
            ttk.Entry(parent, textvariable=var, width=30).grid(row=i + 1, column=1, sticky="w", padx=(0, 10), pady=4)

        self.gen_progress = ttk.Progressbar(parent, mode="determinate")
        self.gen_progress.grid(row=5, column=0, columnspan=2, sticky="ew", padx=10, pady=(14, 2))

        self.gen_status = ttk.Label(parent, text="", foreground=SUBTLE)
        self.gen_status.grid(row=6, column=0, columnspan=2, sticky="w", padx=10, pady=(0, 6))

        self.btn_gen = ttk.Button(parent, text="Сгенерировать последовательность", command=self._generate)
        self.btn_gen.grid(row=7, column=0, columnspan=2, pady=4)

        gen_frame = ttk.LabelFrame(parent, text="Результат")
        gen_frame.grid(row=8, column=0, columnspan=2, sticky="nsew", padx=10, pady=(0, 6))
        gen_frame.columnconfigure(0, weight=1)
        gen_frame.rowconfigure(0, weight=1)
        parent.rowconfigure(8, weight=1)

        self.gen_text = tk.Text(gen_frame, wrap="none", state="normal",
                                font=("Menlo", 10), bg=OUTPUT_BG, fg=OUTPUT_FG,
                                insertbackground=CURSOR, selectbackground=PRIMARY,
                                selectforeground="#FFFFFF", relief="flat",
                                highlightthickness=1, highlightbackground=BORDER,
                                highlightcolor=PRIMARY)
        sb = ttk.Scrollbar(gen_frame, orient="vertical", command=self.gen_text.yview)
        self.gen_text.configure(yscrollcommand=sb.set)
        self.gen_text.grid(row=0, column=0, sticky="nsew")
        sb.grid(row=0, column=1, sticky="ns")

        btn_save = ttk.Button(parent, text="Сохранить последовательность...", command=self._save_gen)
        btn_save.grid(row=9, column=0, columnspan=2, pady=6)

    def _build_test_tab(self, parent):
        parent.columnconfigure(0, weight=1)
        parent.rowconfigure(1, weight=1)

        info = ttk.Label(parent,
                         text="Тестируемая последовательность берётся из результатов вкладки «Генерация».",
                         foreground=TEXT)
        info.grid(row=0, column=0, pady=(10, 4))

        self.test_progress = ttk.Progressbar(parent, mode="determinate")
        self.test_progress.grid(row=0, column=0, sticky="ew", padx=10, pady=(50, 2))

        self.test_status = ttk.Label(parent, text="", foreground=SUBTLE)
        self.test_status.grid(row=0, column=0, sticky="w", padx=10, pady=(60, 0))

        btn_run = ttk.Button(parent, text="Запустить тесты", command=self._run_tests)
        btn_run.grid(row=0, column=0, pady=(80, 4))

        test_frame = ttk.LabelFrame(parent, text="Лог тестов")
        test_frame.grid(row=1, column=0, sticky="nsew", padx=10, pady=6)
        test_frame.columnconfigure(0, weight=1)
        test_frame.rowconfigure(0, weight=1)

        self.test_text = tk.Text(test_frame, wrap="none", state="normal", font=("Menlo", 10),
                                 bg=OUTPUT_BG, fg=OUTPUT_FG, insertbackground=CURSOR,
                                 selectbackground=PRIMARY, selectforeground="#FFFFFF",
                                 relief="flat", highlightthickness=1, highlightbackground=BORDER,
                                 highlightcolor=PRIMARY)
        sb = ttk.Scrollbar(test_frame, orient="vertical", command=self.test_text.yview)
        self.test_text.configure(yscrollcommand=sb.set)
        self.test_text.grid(row=0, column=0, sticky="nsew")
        sb.grid(row=0, column=1, sticky="ns")

        btn_save = ttk.Button(parent, text="Сохранить лог тестов...", command=self._save_test)
        btn_save.grid(row=2, column=0, pady=6)

    # ─── Действия ───
    def _try_load_config(self):
        for path in [
            os.path.join(os.path.dirname(os.path.abspath(__file__)), "config.txt"),
            os.path.join(os.path.dirname(os.path.abspath(__file__)), "..", "config.txt"),
        ]:
            content = read_file_content(path)
            if content:
                params = content.strip().split()
                if len(params) >= 4:
                    self.k1_var.set(params[0])
                    self.k2_var.set(params[1])
                    self.s0_var.set(params[2])
                    self.m_var.set(params[3])
                break

    def _load_params(self):
        path = filedialog.askopenfilename(
            title="Выберите файл с параметрами (K1, K2, s0, m)",
            filetypes=[("Text files", "*.txt"), ("All files", "*.*")],
        )
        if not path:
            return
        content = read_file_content(path)
        params = content.strip().split()
        if len(params) >= 4:
            self.k1_var.set(params[0])
            self.k2_var.set(params[1])
            self.s0_var.set(params[2])
            self.m_var.set(params[3])
            self._set_gen_text("Параметры загружены. Нажмите «Сгенерировать».")
        else:
            self._set_gen_text("Ошибка: файл должен содержать 4 параметра: K1 K2 s0 m")

    def _generate(self):
        if self.gen_active:
            return

        k1_hex = self.k1_var.get().strip()
        k2_hex = self.k2_var.get().strip()
        s0_hex = self.s0_var.get().strip()
        try:
            m = int(self.m_var.get().strip())
        except ValueError:
            messagebox.showerror("Ошибка", "m должно быть целым числом.")
            return
        if len(k1_hex) != 16 or len(k2_hex) != 16 or len(s0_hex) != 16:
            messagebox.showerror("Ошибка", "K1, K2, s0 должны содержать ровно 16 hex-символов.")
            return

        self.gen_k1 = int(k1_hex, 16)
        self.gen_k2 = int(k2_hex, 16)
        self.gen_s_prev = int(s0_hex, 16)
        self.gen_m = m
        self.gen_i = 0
        self.gen_result_parts = []
        self.gen_active = True

        d = int(time.time() * 1000)
        self.gen_temp = encrypt_3des(d, self.gen_k1, self.gen_k2)

        self.btn_gen.configure(state="disabled")
        self.gen_progress.configure(value=0)
        self.gen_status.configure(text=f"Генерация... (0/{m})")
        self._set_gen_text("Генерация...")

        self._gen_step()

    def _gen_step(self):
        if not self.gen_active or self.gen_i >= self.gen_m:
            seq = ''.join(self.gen_result_parts)
            self.gen_active = False
            self.btn_gen.configure(state="normal")
            self.gen_progress.configure(value=100)
            self.gen_status.configure(text=f"Готово: {len(seq)} бит")
            self._set_gen_text(seq)
            return

        x_i = encrypt_3des(self.gen_temp ^ self.gen_s_prev, self.gen_k1, self.gen_k2)
        self.gen_s_prev = encrypt_3des(x_i ^ self.gen_temp, self.gen_k1, self.gen_k2)

        bits = ''.join('1' if (x_i >> b) & 1 else '0' for b in range(63, -1, -1))
        self.gen_result_parts.append(bits)

        self.gen_i += 1

        pct = int(self.gen_i / self.gen_m * 100)
        self.gen_progress.configure(value=pct)
        self.gen_status.configure(text=f"Генерация блока {self.gen_i} из {self.gen_m} ({pct}%)")

        self.root.after(0, self._gen_step)

    def _run_tests(self):
        content = self.gen_text.get("1.0", "end").strip()
        seq = ''.join(c for c in content if c in '01')
        if not seq:
            messagebox.showwarning("Внимание", "Сначала сгенерируйте последовательность.")
            return

        self.test_progress.configure(value=0)
        self.test_status.configure(text="Запуск тестов...")
        self._set_test_text("Вычисление...")
        self.root.update_idletasks()

        try:
            log = run_tests(seq)
            self.test_progress.configure(value=100)
            self.test_status.configure(text="Тесты завершены")
            self._set_test_text(log)
        except Exception as exc:
            self.test_progress.configure(value=0)
            self.test_status.configure(text="Ошибка")
            self._set_test_text(f"Ошибка: {exc}")

    def _save_gen(self):
        path = filedialog.asksaveasfilename(
            title="Сохранить последовательность",
            defaultextension=".txt",
            filetypes=[("Text files", "*.txt"), ("All files", "*.*")],
        )
        if not path:
            return
        save_file_content(path, self.gen_text.get("1.0", "end"))

    def _save_test(self):
        path = filedialog.asksaveasfilename(
            title="Сохранить лог тестов",
            defaultextension=".txt",
            filetypes=[("Text files", "*.txt"), ("All files", "*.*")],
        )
        if not path:
            return
        save_file_content(path, self.test_text.get("1.0", "end"))

    def _set_gen_text(self, text: str):
        self.gen_text.configure(state="normal")
        self.gen_text.delete("1.0", "end")
        self.gen_text.insert("end", text)

    def _set_test_text(self, text: str):
        self.test_text.configure(state="normal")
        self.test_text.delete("1.0", "end")
        self.test_text.insert("end", text)


def main():
    root = tk.Tk()
    App(root).pack(fill="both", expand=True)
    root.mainloop()


if __name__ == "__main__":
    main()
