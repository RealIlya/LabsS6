# ══════════════════════════════════════════════════════════════════════════════
#  CRYPTO BACKEND
# ══════════════════════════════════════════════════════════════════════════════

import os, base64, hashlib, json
from datetime import datetime

# ── Хэширование ───────────────────────────────────────────────────────────────

HASH_ALGORITHMS = ["MD5", "SHA-1", "SHA-256", "SHA-512", "SHA3-256", "SHA3-512"]
_HASH_MAP = {
    "MD5":      hashlib.md5,   "SHA-1":    hashlib.sha1,
    "SHA-256":  hashlib.sha256,"SHA-512":  hashlib.sha512,
    "SHA3-256": hashlib.sha3_256,"SHA3-512": hashlib.sha3_512,
}

def hash_compute(algo: str, data: bytes) -> str:
    fn = _HASH_MAP.get(algo)
    if fn is None: raise ValueError(f"Неизвестный алгоритм: {algo}")
    return fn(data).hexdigest()

def hash_compute_all(algos: list, data: bytes) -> dict:
    return {a: hash_compute(a, data) for a in algos}

def hash_format(source: str, results: dict) -> str:
    ts = datetime.now().strftime("%Y-%m-%d %H:%M:%S")
    lines = ["=== Хэширование ===", f"Дата/время : {ts}", f"Источник   : {source}", ""]
    for algo, h in results.items():
        lines.append(f"{algo:<12}: {h}")
    return "\n".join(lines)


# ── Симметричное шифрование ───────────────────────────────────────────────────

from cryptography.hazmat.primitives.ciphers import Cipher, algorithms, modes
from cryptography.hazmat.primitives.padding import PKCS7
from cryptography.hazmat.backends import default_backend

SYM_ALGORITHMS = ["AES-CBC", "AES-CFB", "DES-CBC", "3DES-CBC"]

def sym_key_iv_size(algo: str) -> tuple:
    match algo:
        case "AES-CBC" | "AES-CFB": return 32, 16
        case "DES-CBC":              return 8, 8
        case "3DES-CBC":             return 24, 8
        case _: raise ValueError(f"Неизвестный алгоритм: {algo}")

def sym_block_size(algo: str) -> int:
    return 128 if algo.startswith("AES") else 64

def sym_generate_params(algo: str) -> dict:
    kl, il = sym_key_iv_size(algo)
    return {"algorithm": algo,
            "key": base64.b64encode(os.urandom(kl)).decode(),
            "iv":  base64.b64encode(os.urandom(il)).decode()}

def sym_make_cipher(algo: str, key: bytes, iv: bytes):
    match algo:
        case "AES-CBC":  return Cipher(algorithms.AES(key), modes.CBC(iv), backend=default_backend())
        case "AES-CFB":  return Cipher(algorithms.AES(key), modes.CFB(iv), backend=default_backend())
        case "DES-CBC":
            k = key * 3 if len(key) == 8 else key
            return Cipher(algorithms.TripleDES(k), modes.CBC(iv), backend=default_backend())
        case "3DES-CBC": return Cipher(algorithms.TripleDES(key), modes.CBC(iv), backend=default_backend())
        case _: raise ValueError(f"Неизвестный алгоритм: {algo}")

def sym_encrypt(algo: str, key: bytes, iv: bytes, data: bytes) -> bytes:
    if algo in ("AES-CBC", "DES-CBC", "3DES-CBC"):
        p = PKCS7(sym_block_size(algo)).padder()
        data = p.update(data) + p.finalize()
    c = sym_make_cipher(algo, key, iv)
    e = c.encryptor()
    return e.update(data) + e.finalize()

def sym_decrypt(algo: str, key: bytes, iv: bytes, ct: bytes) -> bytes:
    c = sym_make_cipher(algo, key, iv)
    d = c.decryptor()
    pt = d.update(ct) + d.finalize()
    if algo in ("AES-CBC", "DES-CBC", "3DES-CBC"):
        u = PKCS7(sym_block_size(algo)).unpadder()
        pt = u.update(pt) + u.finalize()
    return pt


# ── Асимметричное шифрование (RSA + AES гибрид) ───────────────────────────────

from cryptography.hazmat.primitives import hashes, serialization
from cryptography.hazmat.primitives.asymmetric import rsa, padding as rsa_padding

def asym_generate_key_pair(key_size: int = 2048):
    priv = rsa.generate_private_key(public_exponent=65537, key_size=key_size,
                                    backend=default_backend())
    return priv, priv.public_key()

def asym_private_to_pem(k) -> bytes:
    return k.private_bytes(serialization.Encoding.PEM,
                           serialization.PrivateFormat.TraditionalOpenSSL,
                           serialization.NoEncryption())

def asym_public_to_pem(k) -> bytes:
    return k.public_bytes(serialization.Encoding.PEM,
                          serialization.PublicFormat.SubjectPublicKeyInfo)

def asym_load_private(pem: bytes):
    return serialization.load_pem_private_key(pem, password=None, backend=default_backend())

def asym_load_public(pem: bytes):
    return serialization.load_pem_public_key(pem, backend=default_backend())

def _oaep():
    return rsa_padding.OAEP(mgf=rsa_padding.MGF1(algorithm=hashes.SHA256()),
                            algorithm=hashes.SHA256(), label=None)

def asym_encrypt(pub_key, data: bytes) -> dict:
    aes_key, aes_iv = os.urandom(32), os.urandom(16)
    padder = PKCS7(128).padder()
    padded = padder.update(data) + padder.finalize()
    cipher = Cipher(algorithms.AES(aes_key), modes.CBC(aes_iv), backend=default_backend())
    ct = cipher.encryptor().update(padded) + cipher.encryptor().finalize()
    enc_key = pub_key.encrypt(aes_key, _oaep())
    return {"enc_key": base64.b64encode(enc_key).decode(),
            "iv":      base64.b64encode(aes_iv).decode(),
            "ct":      base64.b64encode(ct).decode()}

def asym_decrypt(priv_key, payload: dict) -> bytes:
    aes_key = priv_key.decrypt(base64.b64decode(payload["enc_key"]), _oaep())
    aes_iv  = base64.b64decode(payload["iv"])
    ct      = base64.b64decode(payload["ct"])
    cipher  = Cipher(algorithms.AES(aes_key), modes.CBC(aes_iv), backend=default_backend())
    pt_pad  = cipher.decryptor().update(ct) + cipher.decryptor().finalize()
    u = PKCS7(128).unpadder()
    return u.update(pt_pad) + u.finalize()


# ── Цифровая подпись (RSA-PSS + SHA-256) ─────────────────────────────────────

from cryptography.exceptions import InvalidSignature

def _pss():
    return rsa_padding.PSS(mgf=rsa_padding.MGF1(hashes.SHA256()),
                           salt_length=rsa_padding.PSS.MAX_LENGTH)

def sig_sign(priv_key, data: bytes) -> bytes:
    return priv_key.sign(data, _pss(), hashes.SHA256())

def sig_verify(pub_key, data: bytes, signature: bytes) -> bool:
    try:
        pub_key.verify(signature, data, _pss(), hashes.SHA256())
        return True
    except InvalidSignature:
        return False

def sig_to_payload(sig: bytes) -> dict:
    return {"algorithm": "RSA-PSS-SHA256",
            "signature": base64.b64encode(sig).decode()}

def sig_from_payload(payload: dict) -> bytes:
    return base64.b64decode(payload["signature"])


# ══════════════════════════════════════════════════════════════════════════════
#  UI — вспомогательные функции
# ══════════════════════════════════════════════════════════════════════════════

import tkinter as tk
import tkinter.ttk as ttk
from tkinter import messagebox, scrolledtext, filedialog


def open_dialog(title="Открыть файл", filetypes=None) -> str:
    return filedialog.askopenfilename(title=title,
               filetypes=filetypes or [("Все файлы", "*.*")])

def save_dialog(title="Сохранить файл", ext="", filetypes=None) -> str:
    return filedialog.asksaveasfilename(title=title, defaultextension=ext,
               filetypes=filetypes or [("Все файлы", "*.*")])

def ui_log(widget: tk.Text, msg: str):
    widget.config(state=tk.NORMAL)
    widget.insert(tk.END, msg + "\n")
    widget.see(tk.END)
    widget.config(state=tk.DISABLED)

def ui_clear_log(widget: tk.Text):
    widget.config(state=tk.NORMAL)
    widget.delete("1.0", tk.END)
    widget.config(state=tk.DISABLED)

def save_log_to_file(content: str):
    path = save_dialog("Сохранить вывод", ".txt", [("Текстовые файлы", "*.txt")])
    if path:
        with open(path, "w", encoding="utf-8") as f:
            f.write(content)
        messagebox.showinfo("Сохранено", f"Файл сохранён:\n{path}")

def make_file_row(parent, label: str, attr: str, obj, with_save=False) -> tk.StringVar:
    """[Label] [Entry] [Обзор… / Сохранить…]"""
    row = ttk.Frame(parent)
    row.pack(fill=tk.X, padx=8, pady=2)
    ttk.Label(row, text=label, width=28).pack(side=tk.LEFT)
    var = tk.StringVar()
    setattr(obj, attr, var)
    ttk.Entry(row, textvariable=var, width=38).pack(side=tk.LEFT, padx=4)
    if with_save:
        ttk.Button(row, text="Сохранить…",
                   command=lambda v=var: v.set(save_dialog())).pack(side=tk.LEFT, padx=2)
    else:
        ttk.Button(row, text="Обзор…",
                   command=lambda v=var: v.set(open_dialog())).pack(side=tk.LEFT, padx=2)
    return var

def make_text_row(parent, label: str, attr: str, obj, default="", width=48) -> tk.StringVar:
    """[Label] [Entry] — строка ввода текста / Base64"""
    row = ttk.Frame(parent)
    row.pack(fill=tk.X, padx=8, pady=2)
    ttk.Label(row, text=label, width=28).pack(side=tk.LEFT)
    var = tk.StringVar(value=default)
    setattr(obj, attr, var)
    ttk.Entry(row, textvariable=var, width=width).pack(side=tk.LEFT, padx=4)
    return var

def make_output_section(parent, title="Результат / Журнал", height=9):
    """ScrolledText с заголовком и кнопкой сохранения."""
    pad = dict(padx=8, pady=3)
    hdr = ttk.Frame(parent)
    hdr.pack(fill=tk.X, **pad)
    ttk.Label(hdr, text=title, font=("Segoe UI", 9, "bold")).pack(side=tk.LEFT)
    box = scrolledtext.ScrolledText(parent, height=height, state=tk.DISABLED,
                                    font=("Courier New", 9))
    box.pack(fill=tk.BOTH, expand=True, **pad)

    def _save():
        content = box.get("1.0", tk.END).strip()
        if not content:
            messagebox.showwarning("Пусто", "Нет данных для сохранения."); return
        save_log_to_file(content)

    ttk.Button(hdr, text="Сохранить вывод в .txt", command=_save).pack(side=tk.RIGHT)
    return box

def section(parent, text):
    """Жирный заголовок раздела."""
    ttk.Label(parent, text=text, font=("Segoe UI", 9, "bold")).pack(
        anchor=tk.W, padx=8, pady=(6, 1))

def hint(parent, text):
    """Серая подсказка-разделитель."""
    ttk.Label(parent, text=text, foreground="gray").pack(anchor=tk.W, padx=36, pady=0)


# ══════════════════════════════════════════════════════════════════════════════
#  ВКЛАДКА: Симметричное шифрование
# ══════════════════════════════════════════════════════════════════════════════

class SymmetricTab(ttk.Frame):
    def __init__(self, parent):
        super().__init__(parent)
        self._build()

    def _build(self):
        pad = dict(padx=8, pady=3)

        # Алгоритм
        r = ttk.Frame(self); r.pack(fill=tk.X, **pad)
        ttk.Label(r, text="Алгоритм:", width=28).pack(side=tk.LEFT)
        self.algo_var = tk.StringVar(value=SYM_ALGORITHMS[0])
        ttk.Combobox(r, textvariable=self.algo_var, values=SYM_ALGORITHMS,
                     state="readonly", width=14).pack(side=tk.LEFT)

        ttk.Separator(self, orient=tk.HORIZONTAL).pack(fill=tk.X, pady=4)

        # ── Параметры ─────────────────────────────────────────────────────────
        section(self, "▶ Параметры — ключ и IV")
        make_file_row(self, "JSON-файл параметров:", "params_path", self)
        hint(self, "— или Base64 вручную —")
        make_text_row(self, "Ключ (Key, Base64):", "key_var", self)
        make_text_row(self, "Вектор (IV, Base64):", "iv_var", self)

        ttk.Separator(self, orient=tk.HORIZONTAL).pack(fill=tk.X, pady=4)

        # ── Шифрование ────────────────────────────────────────────────────────
        section(self, "▶ Шифрование — открытые данные")
        make_text_row(self, "Текст (открытый):", "enc_text_var", self)
        hint(self, "— или файл —")
        make_file_row(self, "Входной файл:", "enc_input_path", self)
        make_file_row(self, "Выходной файл (.enc):", "enc_output_path", self, with_save=True)

        ttk.Separator(self, orient=tk.HORIZONTAL).pack(fill=tk.X, pady=4)

        # ── Дешифрование ─────────────────────────────────────────────────────
        section(self, "▶ Дешифрование — данные шифртекста")
        make_text_row(self, "Шифртекст (Base64):", "dec_ct_var", self)
        hint(self, "— или файл —")
        make_file_row(self, "Файл шифртекста (.enc/.bin):", "dec_input_path", self)
        make_file_row(self, "Выходной файл (.dec):", "dec_output_path", self, with_save=True)

        # Поле результата дешифрования (редактируемое)
        dec_res_hdr = ttk.Frame(self); dec_res_hdr.pack(fill=tk.X, padx=8, pady=(4, 0))
        ttk.Label(dec_res_hdr, text="Расшифрованный текст:",
                  font=("Segoe UI", 9, "bold")).pack(side=tk.LEFT)
        ttk.Button(dec_res_hdr, text="Копировать",
                   command=self._copy_dec_result).pack(side=tk.RIGHT)
        ttk.Button(dec_res_hdr, text="Сохранить в .txt",
                   command=self._save_dec_result).pack(side=tk.RIGHT, padx=4)
        self.dec_result_box = tk.Text(self, height=3, font=("Courier New", 9),
                                      wrap=tk.WORD, relief=tk.SUNKEN, bd=1)
        self.dec_result_box.pack(fill=tk.X, padx=8, pady=(0, 4))

        # Кнопки
        bf = ttk.Frame(self); bf.pack(fill=tk.X, **pad)
        ttk.Button(bf, text="Генерировать параметры", command=self._gen).pack(side=tk.LEFT, padx=4)
        ttk.Button(bf, text="Зашифровать",            command=self._encrypt).pack(side=tk.LEFT, padx=4)
        ttk.Button(bf, text="Дешифровать",            command=self._decrypt).pack(side=tk.LEFT, padx=4)
        ttk.Button(bf, text="Очистить",               command=self._clear).pack(side=tk.RIGHT, padx=4)

        self.log = make_output_section(self, "Журнал операций", height=4)

    # ── helpers ───────────────────────────────────────────────────────────────

    def _load_params(self):
        k = self.key_var.get().strip()
        iv = self.iv_var.get().strip()
        if k and iv:
            try:
                return self.algo_var.get(), base64.b64decode(k), base64.b64decode(iv)
            except Exception:
                raise ValueError("Некорректный Base64 в полях Ключ/IV!")
        path = self.params_path.get()
        if not path:
            raise ValueError("Укажите параметры: JSON-файл или поля Ключ/IV!")
        with open(path) as f:
            p = json.load(f)
        return p.get("algorithm", self.algo_var.get()), \
               base64.b64decode(p["key"]), base64.b64decode(p["iv"])

    # ── handlers ──────────────────────────────────────────────────────────────

    def _gen(self):
        try:
            algo = self.algo_var.get()
            p = sym_generate_params(algo)
            self.key_var.set(p["key"])
            self.iv_var.set(p["iv"])
            ui_log(self.log,
                   f"[OK] Параметры для {algo}\n"
                   f"     Key : {p['key']}\n"
                   f"     IV  : {p['iv']}")
            path = save_dialog("Сохранить параметры", ".json", [("JSON", "*.json")])
            if path:
                with open(path, "w") as f:
                    json.dump(p, f, indent=2)
                self.params_path.set(path)
                ui_log(self.log, f"[OK] Параметры сохранены → {path}")
        except Exception as e:
            messagebox.showerror("Ошибка", str(e))

    def _encrypt(self):
        try:
            algo, key, iv = self._load_params()

            text = self.enc_text_var.get().strip()
            src_path = self.enc_input_path.get()
            if text:
                data, src_path = text.encode("utf-8"), None
            elif src_path:
                data = open(src_path, "rb").read()
            else:
                raise ValueError("Введите открытый текст или выберите входной файл!")

            ct = sym_encrypt(algo, key, iv, data)
            ct_b64 = base64.b64encode(ct).decode()

            # Автозаполнение поля дешифрования
            self.dec_ct_var.set(ct_b64)

            if src_path:
                out = self.enc_output_path.get() or src_path + ".enc"
                open(out, "wb").write(ct)
                ui_log(self.log,
                       f"[OK] Зашифровано ({algo})\n"
                       f"     Вход  : {src_path}\n"
                       f"     Выход : {out}  ({len(ct)} байт)\n"
                       f"     Шифртекст вставлен в поле дешифрования (Base64)")
            else:
                ui_log(self.log,
                       f"[OK] Зашифровано ({algo})\n"
                       f"     Шифртекст (Base64):\n     {ct_b64}\n"
                       f"     Шифртекст вставлен в поле дешифрования")
                out = self.enc_output_path.get()
                if out:
                    open(out, "wb").write(ct)
                    ui_log(self.log, f"     Сохранено → {out}")
                elif messagebox.askyesno("Сохранить?", "Сохранить шифртекст в файл?"):
                    p = save_dialog("Сохранить шифртекст", ".bin", [("Бинарный", "*.bin")])
                    if p:
                        open(p, "wb").write(ct)
                        ui_log(self.log, f"     Сохранено → {p}")
        except Exception as e:
            messagebox.showerror("Ошибка", str(e))
            ui_log(self.log, f"[ERR] {e}")

    def _decrypt(self):
        try:
            algo, key, iv = self._load_params()

            ct_b64  = self.dec_ct_var.get().strip()
            src_path = self.dec_input_path.get()

            if ct_b64:
                # Приоритет 1: Base64 из поля
                try:
                    ct = base64.b64decode(ct_b64)
                except Exception:
                    raise ValueError("Поле «Шифртекст (Base64)» содержит некорректный Base64!")
                src_path = None
            elif src_path:
                # Приоритет 2: файл — может быть бинарным (.enc/.bin) или JSON (с полем "ct")
                raw = open(src_path, "rb").read()
                try:
                    obj = json.loads(raw)
                    if "ct" in obj:
                        # JSON-формат: берём "ct", а если есть "key"/"iv" — заполняем поля
                        if "key" in obj and not self.key_var.get():
                            self.key_var.set(obj["key"])
                        if "iv" in obj and not self.iv_var.get():
                            self.iv_var.set(obj["iv"])
                            # Перечитываем параметры, если поля только что заполнились
                            algo, key, iv = self._load_params()
                        ct_b64_from_json = obj["ct"]
                        self.dec_ct_var.set(ct_b64_from_json)
                        ct = base64.b64decode(ct_b64_from_json)
                    else:
                        ct = raw  # JSON без "ct" — читаем как бинарный
                except (json.JSONDecodeError, UnicodeDecodeError):
                    ct = raw  # не JSON — бинарный файл
            else:
                raise ValueError(
                    "Укажите данные для дешифрования:\n"
                    "  • вставьте Base64-шифртекст в поле «Шифртекст (Base64)»\n"
                    "  • или выберите файл шифртекста (.enc / .bin / .json)")

            pt = sym_decrypt(algo, key, iv, ct)
            out = self.dec_output_path.get() or (src_path + ".dec" if src_path else None)

            # Пытаемся декодировать как UTF-8
            try:
                pt_str = pt.decode("utf-8")
            except UnicodeDecodeError:
                pt_str = None

            # Показываем в поле результата
            self.dec_result_box.delete("1.0", tk.END)
            if pt_str is not None:
                self.dec_result_box.insert(tk.END, pt_str)
                ui_log(self.log, f"[OK] Дешифровано ({algo}) — текст отображён в поле результата")
            else:
                self.dec_result_box.insert(tk.END, f"<бинарные данные, {len(pt)} байт>")
                ui_log(self.log, f"[OK] Дешифровано ({algo})  ({len(pt)} байт, бинарный)")

            # Сохранение в файл
            if out:
                open(out, "wb").write(pt)
                ui_log(self.log, f"     Сохранено → {out}")
        except Exception as e:
            messagebox.showerror("Ошибка", str(e))
            ui_log(self.log, f"[ERR] {e}")

    def _copy_dec_result(self):
        text = self.dec_result_box.get("1.0", tk.END).strip()
        if text:
            self.clipboard_clear(); self.clipboard_append(text)
            messagebox.showinfo("Скопировано", "Текст скопирован в буфер обмена.")
        else:
            messagebox.showwarning("Пусто", "Нет текста для копирования.")

    def _save_dec_result(self):
        text = self.dec_result_box.get("1.0", tk.END).strip()
        if not text or text.startswith("<бинарные"):
            messagebox.showwarning("Пусто", "Нет текстового результата для сохранения.")
            return
        p = save_dialog("Сохранить расшифрованный текст", ".txt", [("Текстовые файлы", "*.txt")])
        if p:
            open(p, "w", encoding="utf-8").write(text)
            messagebox.showinfo("Сохранено", f"Сохранено → {p}")

    def _clear(self):
        for v in (self.enc_text_var, self.dec_ct_var, self.key_var, self.iv_var,
                  self.enc_input_path, self.enc_output_path,
                  self.dec_input_path, self.dec_output_path, self.params_path):
            v.set("")
        self.dec_result_box.delete("1.0", tk.END)
        ui_clear_log(self.log)


# ══════════════════════════════════════════════════════════════════════════════
#  ВКЛАДКА: Асимметричное шифрование
# ══════════════════════════════════════════════════════════════════════════════

class AsymmetricTab(ttk.Frame):
    KEY_SIZES = ["2048", "4096"]

    def __init__(self, parent):
        super().__init__(parent)
        self._build()

    def _build(self):
        pad = dict(padx=8, pady=3)

        r = ttk.Frame(self); r.pack(fill=tk.X, **pad)
        ttk.Label(r, text="Размер ключа (бит):", width=28).pack(side=tk.LEFT)
        self.key_size_var = tk.StringVar(value="2048")
        ttk.Combobox(r, textvariable=self.key_size_var, values=self.KEY_SIZES,
                     state="readonly", width=8).pack(side=tk.LEFT)

        ttk.Separator(self, orient=tk.HORIZONTAL).pack(fill=tk.X, pady=4)

        # ── Ключи ─────────────────────────────────────────────────────────────
        section(self, "▶ Ключи")
        make_file_row(self, "Файл закрытого ключа:", "priv_path", self)
        make_text_row(self, "  или PEM-текст закр. ключа:", "priv_text", self)
        make_file_row(self, "Файл открытого ключа:", "pub_path", self)
        make_text_row(self, "  или PEM-текст откр. ключа:", "pub_text", self)

        ttk.Separator(self, orient=tk.HORIZONTAL).pack(fill=tk.X, pady=4)

        # ── Шифрование ────────────────────────────────────────────────────────
        section(self, "▶ Шифрование — открытые данные")
        make_text_row(self, "Текст (открытый):", "enc_text_var", self)
        hint(self, "— или файл —")
        make_file_row(self, "Входной файл:", "enc_input_path", self)
        make_file_row(self, "Выходной JSON-файл:", "enc_output_path", self, with_save=True)

        ttk.Separator(self, orient=tk.HORIZONTAL).pack(fill=tk.X, pady=4)

        # ── Дешифрование ─────────────────────────────────────────────────────
        section(self, "▶ Дешифрование — данные шифртекста")
        make_text_row(self, "enc_key (Base64):", "dec_enc_key_var", self)
        make_text_row(self, "IV (Base64):", "dec_iv_var", self)
        make_text_row(self, "ct (Base64):", "dec_ct_var", self)
        hint(self, "— или JSON-файл —")
        make_file_row(self, "JSON-файл шифртекста:", "dec_input_path", self)
        make_file_row(self, "Выходной файл (.dec):", "dec_output_path", self, with_save=True)

        # Поле результата дешифрования
        dec_res_hdr = ttk.Frame(self); dec_res_hdr.pack(fill=tk.X, padx=8, pady=(4, 0))
        ttk.Label(dec_res_hdr, text="Расшифрованный текст:",
                  font=("Segoe UI", 9, "bold")).pack(side=tk.LEFT)
        ttk.Button(dec_res_hdr, text="Копировать",
                   command=self._copy_dec_result).pack(side=tk.RIGHT)
        ttk.Button(dec_res_hdr, text="Сохранить в .txt",
                   command=self._save_dec_result).pack(side=tk.RIGHT, padx=4)
        self.dec_result_box = tk.Text(self, height=3, font=("Courier New", 9),
                                      wrap=tk.WORD, relief=tk.SUNKEN, bd=1)
        self.dec_result_box.pack(fill=tk.X, padx=8, pady=(0, 4))

        # Кнопки
        bf = ttk.Frame(self); bf.pack(fill=tk.X, **pad)
        ttk.Button(bf, text="Генерировать ключи",   command=self._gen).pack(side=tk.LEFT, padx=4)
        ttk.Button(bf, text="Зашифровать (публ.)",  command=self._encrypt).pack(side=tk.LEFT, padx=4)
        ttk.Button(bf, text="Дешифровать (закр.)",  command=self._decrypt).pack(side=tk.LEFT, padx=4)
        ttk.Button(bf, text="Очистить",             command=self._clear).pack(side=tk.RIGHT, padx=4)

        self.log = make_output_section(self, "Журнал операций", height=4)

    # ── helpers ───────────────────────────────────────────────────────────────

    def _get_pub_key(self):
        t = self.pub_text.get().strip()
        if t: return asym_load_public(t.encode())
        p = self.pub_path.get()
        if not p: raise ValueError("Укажите открытый ключ (файл или PEM-текст)!")
        return asym_load_public(open(p, "rb").read())

    def _get_priv_key(self):
        t = self.priv_text.get().strip()
        if t: return asym_load_private(t.encode())
        p = self.priv_path.get()
        if not p: raise ValueError("Укажите закрытый ключ (файл или PEM-текст)!")
        return asym_load_private(open(p, "rb").read())

    def _fill_dec_fields(self, payload: dict):
        """Заполнить поля дешифрования из payload-словаря."""
        self.dec_enc_key_var.set(payload.get("enc_key", ""))
        self.dec_iv_var.set(payload.get("iv", ""))
        self.dec_ct_var.set(payload.get("ct", ""))

    # ── handlers ──────────────────────────────────────────────────────────────

    def _gen(self):
        try:
            size = int(self.key_size_var.get())
            priv, pub = asym_generate_key_pair(size)
            priv_pem, pub_pem = asym_private_to_pem(priv), asym_public_to_pem(pub)
            self.priv_text.set(priv_pem.decode().strip())
            self.pub_text.set(pub_pem.decode().strip())
            ui_log(self.log, f"[OK] Ключевая пара RSA-{size} сгенерирована и вставлена в поля")
            if messagebox.askyesno("Сохранить ключи?", "Сохранить ключи в PEM-файлы?"):
                pp = save_dialog("Сохранить закрытый ключ", ".pem", [("PEM", "*.pem")])
                if pp: open(pp, "wb").write(priv_pem); self.priv_path.set(pp)
                up = save_dialog("Сохранить открытый ключ", ".pem", [("PEM", "*.pem")])
                if up: open(up, "wb").write(pub_pem); self.pub_path.set(up)
                ui_log(self.log, "[OK] Ключи сохранены")
        except Exception as e:
            messagebox.showerror("Ошибка", str(e))

    def _encrypt(self):
        try:
            pub = self._get_pub_key()
            text = self.enc_text_var.get().strip()
            src_path = self.enc_input_path.get()
            if text:
                data, src_path = text.encode("utf-8"), None
            elif src_path:
                data = open(src_path, "rb").read()
            else:
                raise ValueError("Введите открытый текст или выберите входной файл!")

            payload = asym_encrypt(pub, data)

            # Автозаполнение полей дешифрования
            self._fill_dec_fields(payload)

            out = self.enc_output_path.get() or (src_path + ".rsa.json" if src_path else None)
            if out:
                json.dump(payload, open(out, "w"), indent=2)
                ui_log(self.log,
                       f"[OK] Зашифровано (RSA+AES)\n"
                       f"     Выход: {out}\n"
                       f"     Поля дешифрования заполнены автоматически")
            else:
                ui_log(self.log,
                       f"[OK] Зашифровано (RSA+AES)\n"
                       f"     enc_key: {payload['enc_key'][:48]}…\n"
                       f"     iv     : {payload['iv']}\n"
                       f"     ct     : {payload['ct'][:48]}…\n"
                       f"     Поля дешифрования заполнены автоматически")
                if messagebox.askyesno("Сохранить?", "Сохранить шифртекст в JSON-файл?"):
                    p = save_dialog("Сохранить шифртекст", ".json", [("JSON", "*.json")])
                    if p:
                        json.dump(payload, open(p, "w"), indent=2)
                        self.dec_input_path.set(p)
                        ui_log(self.log, f"     Сохранено → {p}")
        except Exception as e:
            messagebox.showerror("Ошибка", str(e)); ui_log(self.log, f"[ERR] {e}")

    def _decrypt(self):
        try:
            priv = self._get_priv_key()

            ek = self.dec_enc_key_var.get().strip()
            iv = self.dec_iv_var.get().strip()
            ct = self.dec_ct_var.get().strip()
            json_path = self.dec_input_path.get()

            if ek and iv and ct:
                payload = {"enc_key": ek, "iv": iv, "ct": ct}
            elif json_path:
                payload = json.load(open(json_path))
                self._fill_dec_fields(payload)
            else:
                raise ValueError(
                    "Укажите данные для дешифрования:\n"
                    "  • заполните поля enc_key / IV / ct вручную\n"
                    "  • или выберите JSON-файл шифртекста")

            pt = asym_decrypt(priv, payload)
            out = self.dec_output_path.get() or (json_path + ".dec" if json_path else None)

            # Пытаемся декодировать как UTF-8
            try:
                pt_str = pt.decode("utf-8")
            except UnicodeDecodeError:
                pt_str = None

            # Показываем в поле результата
            self.dec_result_box.delete("1.0", tk.END)
            if pt_str is not None:
                self.dec_result_box.insert(tk.END, pt_str)
                ui_log(self.log, f"[OK] Дешифровано (RSA+AES) — текст отображён в поле результата")
            else:
                self.dec_result_box.insert(tk.END, f"<бинарные данные, {len(pt)} байт>")
                ui_log(self.log, f"[OK] Дешифровано (RSA+AES)  ({len(pt)} байт, бинарный)")

            # Сохранение
            if out:
                open(out, "wb").write(pt)
                ui_log(self.log, f"     Сохранено → {out}")
        except Exception as e:
            messagebox.showerror("Ошибка", str(e)); ui_log(self.log, f"[ERR] {e}")

    def _copy_dec_result(self):
        text = self.dec_result_box.get("1.0", tk.END).strip()
        if text:
            self.clipboard_clear(); self.clipboard_append(text)
            messagebox.showinfo("Скопировано", "Текст скопирован в буфер обмена.")
        else:
            messagebox.showwarning("Пусто", "Нет текста для копирования.")

    def _save_dec_result(self):
        text = self.dec_result_box.get("1.0", tk.END).strip()
        if not text or text.startswith("<бинарные"):
            messagebox.showwarning("Пусто", "Нет текстового результата для сохранения.")
            return
        p = save_dialog("Сохранить расшифрованный текст", ".txt", [("Текстовые файлы", "*.txt")])
        if p:
            open(p, "w", encoding="utf-8").write(text)
            messagebox.showinfo("Сохранено", f"Сохранено → {p}")

    def _clear(self):
        for v in (self.priv_text, self.pub_text, self.enc_text_var,
                  self.dec_enc_key_var, self.dec_iv_var, self.dec_ct_var,
                  self.priv_path, self.pub_path,
                  self.enc_input_path, self.enc_output_path,
                  self.dec_input_path, self.dec_output_path):
            v.set("")
        self.dec_result_box.delete("1.0", tk.END)
        ui_clear_log(self.log)


# ══════════════════════════════════════════════════════════════════════════════
#  ВКЛАДКА: Цифровая подпись
# ══════════════════════════════════════════════════════════════════════════════

class SignatureTab(ttk.Frame):
    def __init__(self, parent):
        super().__init__(parent)
        self._build()

    def _build(self):
        pad = dict(padx=8, pady=3)

        # ── Ключи ─────────────────────────────────────────────────────────────
        section(self, "▶ Ключи")
        make_file_row(self, "Файл закрытого ключа:", "priv_path", self)
        make_text_row(self, "  или PEM-текст закр. ключа:", "priv_text", self)
        make_file_row(self, "Файл открытого ключа:", "pub_path", self)
        make_text_row(self, "  или PEM-текст откр. ключа:", "pub_text", self)

        ttk.Separator(self, orient=tk.HORIZONTAL).pack(fill=tk.X, pady=4)

        # ── Данные ────────────────────────────────────────────────────────────
        section(self, "▶ Данные (подписываемые / проверяемые)")
        make_text_row(self, "Текст:", "data_text", self)
        hint(self, "— или файл —")
        make_file_row(self, "Файл данных:", "data_path", self)

        ttk.Separator(self, orient=tk.HORIZONTAL).pack(fill=tk.X, pady=4)

        # ── Подпись ───────────────────────────────────────────────────────────
        section(self, "▶ Подпись")
        make_text_row(self, "Значение подписи (Base64):", "sig_var", self)
        hint(self, "— или файл —")
        make_file_row(self, "Файл подписи (.sig.json):", "sig_path", self, with_save=True)

        # Кнопки
        bf = ttk.Frame(self); bf.pack(fill=tk.X, **pad)
        ttk.Button(bf, text="Генерировать ключи RSA-2048", command=self._gen).pack(side=tk.LEFT, padx=4)
        ttk.Button(bf, text="Подписать",                   command=self._sign).pack(side=tk.LEFT, padx=4)
        ttk.Button(bf, text="Проверить подпись",           command=self._verify).pack(side=tk.LEFT, padx=4)
        ttk.Button(bf, text="Очистить",                    command=self._clear).pack(side=tk.RIGHT, padx=4)

        self.log = make_output_section(self, "Результат / Журнал", height=9)

    def _get_data(self):
        text = self.data_text.get().strip()
        if text: return text.encode("utf-8")
        path = self.data_path.get()
        if path: return open(path, "rb").read()
        raise ValueError("Введите текст или выберите файл данных!")

    def _get_priv_key(self):
        t = self.priv_text.get().strip()
        if t: return asym_load_private(t.encode())
        p = self.priv_path.get()
        if not p: raise ValueError("Укажите закрытый ключ (файл или PEM-текст)!")
        return asym_load_private(open(p, "rb").read())

    def _get_pub_key(self):
        t = self.pub_text.get().strip()
        if t: return asym_load_public(t.encode())
        p = self.pub_path.get()
        if not p: raise ValueError("Укажите открытый ключ (файл или PEM-текст)!")
        return asym_load_public(open(p, "rb").read())

    def _gen(self):
        try:
            priv, pub = asym_generate_key_pair(2048)
            priv_pem = asym_private_to_pem(priv)
            pub_pem  = asym_public_to_pem(pub)

            # Ключи сразу доступны в полях — сохранение необязательно
            self.priv_text.set(priv_pem.decode().strip())
            self.pub_text.set(pub_pem.decode().strip())
            ui_log(self.log, "[OK] Ключевая пара RSA-2048 сгенерирована и вставлена в поля")

            if messagebox.askyesno("Сохранить ключи?", "Сохранить ключи в PEM-файлы?"):
                pp = save_dialog("Сохранить закрытый ключ", ".pem", [("PEM", "*.pem")])
                if pp:
                    open(pp, "wb").write(priv_pem)
                    self.priv_path.set(pp)
                up = save_dialog("Сохранить открытый ключ", ".pem", [("PEM", "*.pem")])
                if up:
                    open(up, "wb").write(pub_pem)
                    self.pub_path.set(up)
                ui_log(self.log, "[OK] Ключи сохранены в файлы")
        except Exception as e:
            messagebox.showerror("Ошибка", str(e))

    def _sign(self):
        try:
            priv = self._get_priv_key()
            data = self._get_data()
            sig = sig_sign(priv, data)
            payload = sig_to_payload(sig)

            # Вставить подпись в поле сразу
            self.sig_var.set(payload["signature"])

            sig_path = self.sig_path.get()
            if not sig_path:
                sig_path = save_dialog("Сохранить подпись", ".json", [("JSON", "*.json")])
            if sig_path:
                json.dump(payload, open(sig_path, "w"), indent=2)
                self.sig_path.set(sig_path)

            ui_log(self.log,
                   f"[OK] Подпись создана (RSA-PSS-SHA256)\n"
                   f"     Base64: {payload['signature'][:60]}…"
                   + (f"\n     Файл  : {sig_path}" if sig_path else ""))
        except Exception as e:
            messagebox.showerror("Ошибка", str(e)); ui_log(self.log, f"[ERR] {e}")

    def _verify(self):
        try:
            pub = self._get_pub_key()
            data = self._get_data()

            sig_text = self.sig_var.get().strip()
            sig_path = self.sig_path.get()
            if sig_text:
                sig_bytes = sig_from_payload({"signature": sig_text})
            elif sig_path:
                sig_bytes = sig_from_payload(json.load(open(sig_path)))
            else:
                raise ValueError("Введите подпись в поле или выберите файл подписи!")

            if sig_verify(pub, data, sig_bytes):
                ui_log(self.log, "[OK]  Подпись ДЕЙСТВИТЕЛЬНА")
                messagebox.showinfo("Проверка", " Подпись действительна!")
            else:
                ui_log(self.log, "[FAIL]  Подпись НЕДЕЙСТВИТЕЛЬНА")
                messagebox.showerror("Проверка", " Подпись недействительна!")
        except Exception as e:
            messagebox.showerror("Ошибка", str(e)); ui_log(self.log, f"[ERR] {e}")

    def _clear(self):
        for v in (self.data_text, self.sig_var, self.priv_text, self.pub_text,
                  self.data_path, self.priv_path, self.pub_path, self.sig_path):
            v.set("")
        ui_clear_log(self.log)


# ══════════════════════════════════════════════════════════════════════════════
#  ВКЛАДКА: Хэширование
# ══════════════════════════════════════════════════════════════════════════════

class HashTab(ttk.Frame):
    def __init__(self, parent):
        super().__init__(parent)
        self._last: dict = {}
        self._build()

    def _build(self):
        pad = dict(padx=8, pady=3)

        section(self, "▶ Входные данные")
        make_text_row(self, "Строка для хэширования:", "input_text", self)
        hint(self, "— или файл —")

        r = ttk.Frame(self); r.pack(fill=tk.X, **pad)
        ttk.Label(r, text="Файл:", width=28).pack(side=tk.LEFT)
        self.file_path = tk.StringVar()
        ttk.Entry(r, textvariable=self.file_path, width=38).pack(side=tk.LEFT, padx=4)
        ttk.Button(r, text="Обзор…",
                   command=lambda: self.file_path.set(open_dialog())).pack(side=tk.LEFT)

        ttk.Separator(self, orient=tk.HORIZONTAL).pack(fill=tk.X, pady=4)
        section(self, "▶ Алгоритмы")

        chk = ttk.Frame(self); chk.pack(fill=tk.X, **pad)
        self.algo_vars = {}
        for algo in HASH_ALGORITHMS:
            var = tk.BooleanVar(value=(algo == "SHA-256"))
            self.algo_vars[algo] = var
            ttk.Checkbutton(chk, text=algo, variable=var).pack(side=tk.LEFT, padx=6)

        bf = ttk.Frame(self); bf.pack(fill=tk.X, **pad)
        ttk.Button(bf, text="Вычислить хэши", command=self._compute).pack(side=tk.LEFT, padx=4)
        ttk.Button(bf, text="Очистить",       command=self._clear).pack(side=tk.RIGHT, padx=4)

        self.result_box = make_output_section(self, "Результаты хэширования", height=14)

    def _compute(self):
        selected = [a for a, v in self.algo_vars.items() if v.get()]
        if not selected:
            messagebox.showwarning("Предупреждение", "Выберите хотя бы один алгоритм!"); return
        try:
            path = self.file_path.get()
            text = self.input_text.get().strip()
            if path:
                data, source = open(path, "rb").read(), path
            elif text:
                data, source = text.encode("utf-8"), f'"{text}"'
            else:
                messagebox.showwarning("Предупреждение", "Укажите файл или введите текст!"); return

            self._last = hash_compute_all(selected, data)
            result = hash_format(source, self._last)

            self.result_box.config(state=tk.NORMAL)
            self.result_box.delete("1.0", tk.END)
            self.result_box.insert(tk.END, result)
            self.result_box.config(state=tk.DISABLED)
        except Exception as e:
            messagebox.showerror("Ошибка", str(e))

    def _clear(self):
        self.file_path.set(""); self.input_text.set("")
        ui_clear_log(self.result_box)
        self._last = {}


# ══════════════════════════════════════════════════════════════════════════════
#  ГЛАВНОЕ ОКНО
# ══════════════════════════════════════════════════════════════════════════════

class CryptoApp:
    def __init__(self, root: tk.Tk):
        root.title("Криптографическое приложение  |  cryptography library")
        root.geometry("920x720")
        root.resizable(True, True)
        ttk.Style().theme_use("clam")

        nb = ttk.Notebook(root)
        nb.pack(fill=tk.BOTH, expand=True, padx=6, pady=6)

        nb.add(SymmetricTab(nb),  text="  Симм. шифрование  ")
        nb.add(AsymmetricTab(nb), text="  Асимм. шифрование  ")
        nb.add(SignatureTab(nb),  text="  Цифровая подпись  ")
        nb.add(HashTab(nb),       text="  Хэширование       ")

        ttk.Label(root,
                  text="cryptography library  |  pip install cryptography  |  python main.py",
                  relief=tk.SUNKEN, anchor=tk.W,
                  foreground="gray").pack(side=tk.BOTTOM, fill=tk.X)


if __name__ == "__main__":
    root = tk.Tk()
    CryptoApp(root)
    root.mainloop()