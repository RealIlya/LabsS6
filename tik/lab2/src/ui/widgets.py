from __future__ import annotations

import tkinter as tk
from tkinter import filedialog, messagebox, ttk
from tkinter.scrolledtext import ScrolledText

from core.encoding import (
    bin_to_bytes,
    bytes_to_bin,
    bytes_to_hex,
    bytes_to_text,
    hex_to_bytes,
    text_to_bytes,
)
from io_utils import read_bytes, write_bytes


class ByteField(ttk.LabelFrame):
    def __init__(self, master: tk.Misc, title: str, height: int = 4) -> None:
        super().__init__(master, text=title, padding=8)
        self.source_var = tk.StringVar(value="text")
        self._build(height)

    def _build(self, height: int) -> None:
        src = ttk.Frame(self)
        src.grid(row=0, column=0, columnspan=3, sticky="w")
        ttk.Label(src, text="Источник ввода:").grid(row=0, column=0, padx=(0, 8))
        ttk.Radiobutton(src, text="SYM", value="text", variable=self.source_var).grid(row=0, column=1)
        ttk.Radiobutton(src, text="HEX", value="hex", variable=self.source_var).grid(row=0, column=2)
        ttk.Radiobutton(src, text="BIN", value="bin", variable=self.source_var).grid(row=0, column=3)

        self.boxes: dict[str, ScrolledText] = {}
        for idx, (rep, caption) in enumerate((("text", "SYM"), ("hex", "HEX"), ("bin", "BIN"))):
            ttk.Label(self, text=caption).grid(row=1 + idx, column=0, sticky="nw", padx=(0, 6))
            box = ScrolledText(self, height=height, width=90, wrap="word")
            box.grid(row=1 + idx, column=1, sticky="nsew", pady=2)
            self.boxes[rep] = box

        btns = ttk.Frame(self)
        btns.grid(row=1, column=2, rowspan=3, sticky="ns", padx=(8, 0))
        ttk.Button(btns, text="Синхр. из SYM", command=lambda: self.sync_from("text")).grid(
            row=0, column=0, sticky="ew", pady=2
        )
        ttk.Button(btns, text="Синхр. из HEX", command=lambda: self.sync_from("hex")).grid(
            row=1, column=0, sticky="ew", pady=2
        )
        ttk.Button(btns, text="Синхр. из BIN", command=lambda: self.sync_from("bin")).grid(
            row=2, column=0, sticky="ew", pady=2
        )
        ttk.Button(btns, text="Загрузить файл", command=self.load_dialog).grid(row=3, column=0, sticky="ew", pady=8)
        ttk.Button(btns, text="Сохранить файл", command=self.save_dialog).grid(row=4, column=0, sticky="ew", pady=2)

        self.grid_columnconfigure(1, weight=1)
        self.grid_rowconfigure(3, weight=1)

    def _set_box(self, rep: str, value: str) -> None:
        box = self.boxes[rep]
        box.delete("1.0", tk.END)
        box.insert("1.0", value)

    def _get_box(self, rep: str) -> str:
        return self.boxes[rep].get("1.0", "end-1c")

    def set_bytes(self, data: bytes) -> None:
        self._set_box("text", bytes_to_text(data))
        self._set_box("hex", bytes_to_hex(data))
        self._set_box("bin", bytes_to_bin(data))

    def _bytes_from_representation(self, rep: str) -> bytes:
        raw = self._get_box(rep)
        if rep == "text":
            return text_to_bytes(raw)
        if rep == "hex":
            return hex_to_bytes(raw)
        if rep == "bin":
            return bin_to_bytes(raw)
        raise ValueError("Неизвестный тип представления.")

    def get_bytes(self) -> bytes:
        return self._bytes_from_representation(self.source_var.get())

    def sync_from(self, rep: str) -> None:
        try:
            data = self._bytes_from_representation(rep)
            self.set_bytes(data)
            self.source_var.set(rep)
        except Exception as exc:
            messagebox.showerror("Ошибка формата", str(exc))

    def load_dialog(self) -> None:
        path = filedialog.askopenfilename(title="Выберите файл")
        if not path:
            return
        try:
            self.set_bytes(read_bytes(path))
        except Exception as exc:
            messagebox.showerror("Ошибка чтения", str(exc))

    def save_dialog(self) -> None:
        path = filedialog.asksaveasfilename(title="Сохранить файл как")
        if not path:
            return
        try:
            write_bytes(path, self.get_bytes())
        except Exception as exc:
            messagebox.showerror("Ошибка записи", str(exc))

