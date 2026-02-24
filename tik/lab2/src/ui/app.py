from __future__ import annotations

import secrets
import tkinter as tk
from tkinter import filedialog, messagebox, ttk

from core.analysis import analyze_lfsr
from core.crypto import otp_apply, otp_encrypt
from core.lfsr import parse_polynomial, scrambler_xor, seed_bytes_from_int, seed_int_from_bytes
from core.variants import VARIANT_SCRAMBLERS
from io_utils import write_bytes
from ui.widgets import ByteField


VARIANT_ID = 2
SCRAMBLERS = VARIANT_SCRAMBLERS[VARIANT_ID]


class CipherTab(ttk.Frame):
    def __init__(self, master: tk.Misc, mode: str) -> None:
        super().__init__(master, padding=10)
        self.mode = mode  # encrypt | decrypt
        self.method_var = tk.StringVar(value="otp")
        self.scrambler_idx_var = tk.StringVar(value="1")
        self.polynomial_var = tk.StringVar()
        self.status_var = tk.StringVar(value="Готово.")
        self._build()
        self._set_polynomial_by_scrambler()
        self._toggle_scrambler_controls()

    def _build(self) -> None:
        opts = ttk.LabelFrame(self, text="Параметры", padding=8)
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

        ttk.Label(opts, text="Вариант:").grid(row=1, column=0, sticky="w", pady=(6, 0))
        ttk.Label(opts, text=str(VARIANT_ID)).grid(row=1, column=1, sticky="w", pady=(6, 0))

        ttk.Label(opts, text="Скремблер:").grid(row=1, column=2, sticky="w", pady=(6, 0))
        self.scrambler_combo = ttk.Combobox(
            opts,
            width=4,
            state="readonly",
            values=["1", "2"],
            textvariable=self.scrambler_idx_var,
        )
        self.scrambler_combo.grid(row=1, column=3, sticky="w", pady=(6, 0), padx=(0, 8))
        self.scrambler_combo.bind("<<ComboboxSelected>>", lambda _: self._set_polynomial_by_scrambler())

        ttk.Label(opts, text="Полином:").grid(row=1, column=4, sticky="w", pady=(6, 0))
        ttk.Label(opts, textvariable=self.polynomial_var).grid(row=1, column=5, columnspan=2, sticky="w", pady=(6, 0))

        in_title = "Открытый текст" if self.mode == "encrypt" else "Шифртекст"
        out_title = "Шифртекст" if self.mode == "encrypt" else "Расшифрованный текст"
        key_title = "Ключ / начальное состояние"

        self.input_field = ByteField(self, in_title)
        self.input_field.grid(row=1, column=0, sticky="nsew", pady=(10, 6))
        self.key_field = ByteField(self, key_title)
        self.key_field.grid(row=2, column=0, sticky="nsew", pady=6)
        self.output_field = ByteField(self, out_title)
        self.output_field.grid(row=3, column=0, sticky="nsew", pady=(6, 8))

        actions = ttk.Frame(self)
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
        self.grid_rowconfigure(3, weight=1)

    def _toggle_scrambler_controls(self) -> None:
        enabled = self.method_var.get() == "scrambler"
        state = "readonly" if enabled else "disabled"
        self.scrambler_combo.configure(state=state)

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
        self.seed_var = tk.StringVar(value="1")
        self.bits_var = tk.StringVar(value="2048")
        self._build()
        self._set_polynomial_by_scrambler()

    def _build(self) -> None:
        top = ttk.LabelFrame(self, text="Параметры исследования", padding=8)
        top.grid(row=0, column=0, sticky="ew")
        top.grid_columnconfigure(6, weight=1)

        ttk.Label(top, text="Вариант:").grid(row=0, column=0, sticky="w")
        ttk.Label(top, text=str(VARIANT_ID)).grid(row=0, column=1, sticky="w", padx=(0, 8))

        ttk.Label(top, text="Скремблер:").grid(row=0, column=2, sticky="w")
        scr_combo = ttk.Combobox(top, width=4, state="readonly", values=["1", "2"], textvariable=self.scrambler_idx_var)
        scr_combo.grid(row=0, column=3, sticky="w", padx=(0, 8))
        scr_combo.bind("<<ComboboxSelected>>", lambda _: self._set_polynomial_by_scrambler())

        ttk.Label(top, text="Полином:").grid(row=0, column=4, sticky="w")
        ttk.Label(top, textvariable=self.polynomial_var).grid(row=0, column=5, sticky="w", padx=(4, 8))

        ttk.Label(top, text="Seed (int/bin/0x):").grid(row=1, column=0, sticky="w", pady=(8, 0))
        ttk.Entry(top, textvariable=self.seed_var, width=20).grid(row=1, column=1, sticky="w", pady=(8, 0))
        ttk.Label(top, text="Бит для анализа:").grid(row=1, column=2, sticky="w", pady=(8, 0))
        ttk.Entry(top, textvariable=self.bits_var, width=10).grid(row=1, column=3, sticky="w", pady=(8, 0))
        ttk.Button(top, text="Анализ выбранного", command=self._analyze_selected).grid(
            row=1, column=5, sticky="w", pady=(8, 0)
        )
        ttk.Button(top, text="Анализ обоих", command=self._analyze_both).grid(row=1, column=6, sticky="w", pady=(8, 0))

        self.output = tk.Text(self, width=120, height=25, wrap="word")
        self.output.grid(row=1, column=0, sticky="nsew", pady=(8, 0))

        self.grid_columnconfigure(0, weight=1)
        self.grid_rowconfigure(1, weight=1)

    def _set_polynomial_by_scrambler(self) -> None:
        idx = int(self.scrambler_idx_var.get()) - 1
        self.polynomial_var.set(SCRAMBLERS[idx])

    def _parse_seed(self) -> int:
        raw = self.seed_var.get().strip().lower()
        if not raw:
            raise ValueError("Укажите seed.")
        if raw.startswith("0x"):
            seed = int(raw, 16)
        elif set(raw) <= {"0", "1"} and len(raw) > 1:
            seed = int(raw, 2)
        else:
            seed = int(raw, 10)
        if seed <= 0:
            raise ValueError("Seed должен быть > 0.")
        return seed

    def _format_stats(self, title: str, polynomial: str, seed: int, bits: int) -> str:
        stats = analyze_lfsr(polynomial, seed, sample_bits=bits)
        return (
            f"{title}\n"
            f"Полином: {polynomial}\n"
            f"Seed: {seed}\n"
            f"Период: {stats.period}\n"
            f"Нули/единицы: {stats.zeros}/{stats.ones}\n"
            f"Критерий chi^2: {stats.chi_square:.4f}\n"
            f"|ones-zeros|: {stats.balanced_delta}\n"
            f"Циклические сдвиги уникальны: {'да' if stats.cyclic_shifts_unique else 'нет'}\n"
            f"Макс. |автокорреляция|: {stats.max_abs_autocorrelation:.4f}\n"
            "----------------------------------------\n"
        )

    def _analyze_selected(self) -> None:
        try:
            polynomial = self.polynomial_var.get().strip()
            seed = self._parse_seed()
            bits = int(self.bits_var.get())
            text = self._format_stats("Исследование выбранного скремблера", polynomial, seed, bits)
            self.output.delete("1.0", tk.END)
            self.output.insert("1.0", text)
        except Exception as exc:
            messagebox.showerror("Ошибка анализа", str(exc))

    def _analyze_both(self) -> None:
        try:
            seed = self._parse_seed()
            bits = int(self.bits_var.get())
            text = ""
            for idx, polynomial in enumerate(SCRAMBLERS, start=1):
                title = f"Скремблер {idx}"
                text += self._format_stats(title, polynomial, seed, bits)
            self.output.delete("1.0", tk.END)
            self.output.insert("1.0", text)
        except Exception as exc:
            messagebox.showerror("Ошибка анализа", str(exc))


class Lab2App(tk.Tk):
    def __init__(self) -> None:
        super().__init__()
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
