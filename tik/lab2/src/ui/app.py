from __future__ import annotations

import secrets
import tkinter as tk
from tkinter import filedialog, messagebox, ttk

from core.analysis import analyze_lfsr, build_verdict
from core.crypto import otp_apply, otp_encrypt
from core.lfsr import (
    parse_polynomial,
    scrambler_xor,
    seed_bytes_from_int,
    seed_has_truncated_high_bits,
    seed_int_from_bytes,
)
from io_utils import write_bytes
from ui.widgets import ByteField


SCRAMBLERS = ("x9 + x3 + 1", "x9 + x4 + 1")


def _warn_if_seed_truncated(seed_bytes: bytes, polynomial: str) -> None:
    # Предупреждаем, если введённый seed длиннее разрядности текущего регистра
    truncated, degree = seed_has_truncated_high_bits(seed_bytes, polynomial)
    if truncated:
        messagebox.showwarning(
            "Предупреждение о seed",
            f"Для текущего полинома будут использованы только младшие {degree} бит seed.",
        )


class CipherTab(ttk.Frame):
    def __init__(self, master: tk.Misc, mode: str) -> None:
        super().__init__(master, padding=10)
        self.mode = mode  # encrypt | decrypt
        self.method_var = tk.StringVar(value="otp")
        self.scrambler_idx_var = tk.StringVar(value="1")
        self.polynomial_var = tk.StringVar()
        self.status_var = tk.StringVar(value="Готово.")
        self._content: ttk.Frame | None = None
        self._build()
        self._set_polynomial_by_scrambler()
        self._toggle_scrambler_controls()

    def _build(self) -> None:
        # Вкладки шифрования и дешифрования собраны на одном классе, различается только режим
        canvas = tk.Canvas(self, highlightthickness=0)
        scrollbar = ttk.Scrollbar(self, orient="vertical", command=canvas.yview)
        content = ttk.Frame(canvas)
        self._content = content

        content.bind(
            "<Configure>",
            lambda _event: canvas.configure(scrollregion=canvas.bbox("all")),
        )
        canvas.bind(
            "<Configure>",
            lambda event: canvas.itemconfigure(content_window, width=event.width),
        )

        content_window = canvas.create_window((0, 0), window=content, anchor="nw")
        canvas.configure(yscrollcommand=scrollbar.set)

        canvas.grid(row=0, column=0, sticky="nsew")
        scrollbar.grid(row=0, column=1, sticky="ns")

        for widget in (canvas, content):
            widget.bind("<MouseWheel>", self._on_mousewheel)
            widget.bind("<Button-4>", self._on_mousewheel)
            widget.bind("<Button-5>", self._on_mousewheel)

        opts = ttk.LabelFrame(content, text="Параметры", padding=8)
        opts.grid(row=0, column=0, sticky="ew")
        opts.grid_columnconfigure(6, weight=1)

        ttk.Label(opts, text="Режим:").grid(row=0, column=0, padx=(0, 6))
        ttk.Radiobutton(
            opts,
            text="Однократное гаммирование",
            value="otp",
            variable=self.method_var,
            command=self._toggle_scrambler_controls,
        ).grid(row=0, column=1, sticky="w", padx=(0, 8))
        ttk.Radiobutton(
            opts,
            text="Скремблер",
            value="scrambler",
            variable=self.method_var,
            command=self._toggle_scrambler_controls,
        ).grid(row=0, column=2, sticky="w", padx=(0, 10))

        ttk.Label(opts, text="Скремблер:").grid(row=1, column=0, sticky="w", pady=(6, 0))
        self.scrambler_combo = ttk.Combobox(
            opts,
            width=4,
            state="readonly",
            values=["1", "2"],
            textvariable=self.scrambler_idx_var,
        )
        self.scrambler_combo.grid(row=1, column=1, sticky="w", pady=(6, 0), padx=(0, 8))
        self.scrambler_combo.bind("<<ComboboxSelected>>", lambda _: self._set_polynomial_by_scrambler())

        ttk.Label(opts, text="Полином:").grid(row=1, column=2, sticky="w", pady=(6, 0))
        self.poly_entry = ttk.Entry(opts, textvariable=self.polynomial_var, width=35)
        self.poly_entry.grid(row=1, column=3, columnspan=4, sticky="ew", pady=(6, 0))

        in_title = "Открытый текст" if self.mode == "encrypt" else "Шифртекст"
        out_title = "Шифртекст" if self.mode == "encrypt" else "Расшифрованный текст"
        key_title = "Ключ / начальное состояние"

        self.input_field = ByteField(content, in_title)
        self.input_field.grid(row=1, column=0, sticky="nsew", pady=(10, 6))
        self.key_field = ByteField(content, key_title)
        self.key_field.grid(row=2, column=0, sticky="nsew", pady=6)
        self.output_field = ByteField(content, out_title)
        self.output_field.grid(row=3, column=0, sticky="nsew", pady=(6, 8))

        actions = ttk.Frame(content)
        actions.grid(row=4, column=0, sticky="ew")
        ttk.Button(actions, text="Сгенерировать ключ/seed", command=self._generate_material).grid(
            row=0, column=0, sticky="w"
        )
        ttk.Button(
            actions,
            text="Зашифровать" if self.mode == "encrypt" else "Расшифровать",
            command=self._run_crypto,
        ).grid(row=0, column=1, sticky="w", padx=8)
        ttk.Button(
            actions,
            text="Зашифровать и сохранить..." if self.mode == "encrypt" else "Расшифровать и сохранить...",
            command=self._run_and_save,
        ).grid(row=0, column=2, sticky="w", padx=8)
        ttk.Label(actions, textvariable=self.status_var).grid(row=0, column=3, sticky="w", padx=12)
        actions.grid_columnconfigure(3, weight=1)

        self.grid_columnconfigure(0, weight=1)
        self.grid_rowconfigure(0, weight=1)
        content.grid_columnconfigure(0, weight=1)
        content.grid_rowconfigure(3, weight=1)

    def _on_mousewheel(self, event: tk.Event) -> str:
        canvas = self.winfo_children()[0]
        if not isinstance(canvas, tk.Canvas):
            return "break"
        if getattr(event, "delta", 0):
            canvas.yview_scroll(int(-event.delta / 120), "units")
        elif getattr(event, "num", None) == 4:
            canvas.yview_scroll(-1, "units")
        elif getattr(event, "num", None) == 5:
            canvas.yview_scroll(1, "units")
        return "break"

    def _toggle_scrambler_controls(self) -> None:
        # Полином и выбор скремблера активны только в режиме LFSR
        enabled = self.method_var.get() == "scrambler"
        state = "readonly" if enabled else "disabled"
        entry_state = "normal" if enabled else "disabled"
        self.scrambler_combo.configure(state=state)
        self.poly_entry.configure(state=entry_state)

    def _set_polynomial_by_scrambler(self) -> None:
        idx = int(self.scrambler_idx_var.get()) - 1
        self.polynomial_var.set(SCRAMBLERS[idx])

    def _generate_material(self) -> None:
        try:
            data = self.input_field.get_bytes()
            if self.method_var.get() == "otp":
                _, key = otp_encrypt(data)
                self.key_field.set_bytes(key)
                self.status_var.set(f"Сгенерирован ключ ({len(key)} байт).")
            else:
                polynomial = self.polynomial_var.get().strip()
                degree, _ = parse_polynomial(polynomial)
                seed = secrets.randbelow((1 << degree) - 1) + 1
                self.key_field.set_bytes(seed_bytes_from_int(seed, polynomial))
                self.status_var.set(f"Сгенерирован seed: {seed}.")
        except Exception as exc:
            messagebox.showerror("Ошибка генерации", str(exc))

    def _execute_crypto(self) -> tuple[bytes, bytes | None]:
        # Вся крипто логика остаётся в core, UI только собирает вход и показывает результат
        data = self.input_field.get_bytes()
        if self.method_var.get() == "otp":
            key = self.key_field.get_bytes()
            if self.mode == "encrypt" and not key:
                out, key = otp_encrypt(data)
                self.key_field.set_bytes(key)
            else:
                if not key:
                    raise ValueError("Укажите ключ для дешифрования.")
                out = otp_apply(data, key)
            key_to_save = key if self.mode == "encrypt" else None
        else:
            polynomial = self.polynomial_var.get().strip()
            if not polynomial:
                raise ValueError("Полином скремблера не задан.")
            key_bytes = self.key_field.get_bytes()
            if not key_bytes:
                raise ValueError("Укажите начальное состояние скремблера (seed).")
            _warn_if_seed_truncated(key_bytes, polynomial)
            seed = seed_int_from_bytes(key_bytes, polynomial)
            out = scrambler_xor(data, polynomial, seed)
            key_to_save = None

        self.output_field.set_bytes(out)
        action = "шифрование" if self.mode == "encrypt" else "дешифрование"
        self.status_var.set(f"Выполнено: {action}, {len(out)} байт.")
        return out, key_to_save

    def _run_crypto(self) -> None:
        try:
            self._execute_crypto()
        except Exception as exc:
            messagebox.showerror("Ошибка обработки", str(exc))

    def _save_bytes_dialog(self, data: bytes, title: str, initial_name: str) -> bool:
        path = filedialog.asksaveasfilename(title=title, initialfile=initial_name)
        if not path:
            return False
        write_bytes(path, data)
        return True

    def _run_and_save(self) -> None:
        try:
            out, key_for_save = self._execute_crypto()
            action = "шифрования" if self.mode == "encrypt" else "дешифрования"
            if not self._save_bytes_dialog(out, f"Сохранить результат {action}", "output.bin"):
                return
            if key_for_save is not None:
                self._save_bytes_dialog(key_for_save, "Сохранить ключ OTP", "key.bin")
            self.status_var.set("Выполнено и сохранено в файл(ы).")
        except Exception as exc:
            messagebox.showerror("Ошибка обработки", str(exc))


class ResearchTab(ttk.Frame):
    def __init__(self, master: tk.Misc) -> None:
        super().__init__(master, padding=10)
        self.scrambler_idx_var = tk.StringVar(value="1")
        self.polynomial_var = tk.StringVar()
        self._build()
        self._set_polynomial_by_scrambler()
        self.seed_field.set_bytes(seed_bytes_from_int(1, self.polynomial_var.get()))

    def _build(self) -> None:
        top = ttk.LabelFrame(self, text="Параметры исследования", padding=8)
        top.grid(row=0, column=0, sticky="ew")
        top.grid_columnconfigure(4, weight=1)

        ttk.Label(top, text="Скремблер:").grid(row=0, column=0, sticky="w")
        scr_combo = ttk.Combobox(top, width=4, state="readonly", values=["1", "2"], textvariable=self.scrambler_idx_var)
        scr_combo.grid(row=0, column=1, sticky="w", padx=(0, 8))
        scr_combo.bind("<<ComboboxSelected>>", lambda _: self._set_polynomial_by_scrambler())

        ttk.Label(top, text="Полином:").grid(row=0, column=2, sticky="w")
        ttk.Entry(top, textvariable=self.polynomial_var, width=33).grid(row=0, column=3, sticky="ew", padx=(4, 8))

        ttk.Button(top, text="Анализ выбранного", command=self._analyze_selected).grid(
            row=1, column=0, sticky="w", pady=(8, 0)
        )
        ttk.Button(top, text="Анализ обоих", command=self._analyze_both).grid(row=1, column=1, sticky="w", pady=(8, 0))

        self.seed_field = ByteField(self, "Начальное состояние", height=3)
        self.seed_field.grid(row=1, column=0, sticky="nsew", pady=(8, 0))

        self.output = tk.Text(self, width=120, height=20, wrap="word")
        self.output.grid(row=2, column=0, sticky="nsew", pady=(8, 0))

        self.grid_columnconfigure(0, weight=1)
        self.grid_rowconfigure(2, weight=1)

    def _set_polynomial_by_scrambler(self) -> None:
        idx = int(self.scrambler_idx_var.get()) - 1
        self.polynomial_var.set(SCRAMBLERS[idx])

    def _parse_seed(self) -> int:
        # Исследование ПСП использует тот же байтовый seed, что и режим скремблера
        polynomial = self.polynomial_var.get().strip()
        if not polynomial:
            raise ValueError("Полином скремблера не задан.")
        seed_bytes = self.seed_field.get_bytes()
        _warn_if_seed_truncated(seed_bytes, polynomial)
        return seed_int_from_bytes(seed_bytes, polynomial)

    def _format_stats(self, title: str, polynomial: str, seed: int) -> str:
        # Показываем и агрегированные метрики, и первые биты ПСП, чтобы видеть сдвиг фазы
        stats = analyze_lfsr(polynomial, seed)
        return (
            f"{title}\n"
            f"Полином: {polynomial}\n"
            f"Seed до маскирования: {seed}\n"
            f"Фактический seed: {stats.effective_seed}\n"
            f"Разрядность LFSR: {stats.degree}\n"
            f"Проанализировано бит: {stats.analyzed_bits} (один период)\n"
            f"Первые биты ПСП: {stats.preview_bits}\n"
            f"Период: {stats.period}\n"
            f"Нули/единицы: {stats.zeros}/{stats.ones}\n"
            f"Критерий chi^2: {stats.chi_square:.4f}\n"
            f"|ones-zeros|: {stats.balanced_delta}\n"
            f"Циклические сдвиги уникальны: {'да' if stats.cyclic_shifts_unique else 'нет'}\n"
            f"Макс. |автокорреляция|: {stats.max_abs_autocorrelation:.4f}\n"
            f"{build_verdict(stats)}\n"
            "----------------------------------------\n"
        )

    def _analyze_selected(self) -> None:
        try:
            polynomial = self.polynomial_var.get().strip()
            seed = self._parse_seed()
            text = self._format_stats("Исследование выбранного скремблера", polynomial, seed)
            self.output.delete("1.0", tk.END)
            self.output.insert("1.0", text)
        except Exception as exc:
            messagebox.showerror("Ошибка анализа", str(exc))

    def _analyze_both(self) -> None:
        try:
            seed = self._parse_seed()
            text = ""
            for idx, polynomial in enumerate(SCRAMBLERS, start=1):
                title = f"Скремблер {idx}"
                text += self._format_stats(title, polynomial, seed)
            self.output.delete("1.0", tk.END)
            self.output.insert("1.0", text)
        except Exception as exc:
            messagebox.showerror("Ошибка анализа", str(exc))


class Lab2App(tk.Tk):
    def __init__(self) -> None:
        super().__init__()
        style = ttk.Style(self)
        for theme_name in ("vista", "xpnative", "clam", "alt", "default"):
            if theme_name in style.theme_names():
                style.theme_use(theme_name)
                break

        self.title("ЛР2: гаммирование и скремблеры")
        self.geometry("1400x900")

        notebook = ttk.Notebook(self)
        notebook.pack(fill="both", expand=True)

        notebook.add(CipherTab(notebook, mode="encrypt"), text="Шифрование")
        notebook.add(CipherTab(notebook, mode="decrypt"), text="Дешифрование")
        notebook.add(ResearchTab(notebook), text="Исследование ПСП")


def run_app() -> None:
    app = Lab2App()
    app.mainloop()
