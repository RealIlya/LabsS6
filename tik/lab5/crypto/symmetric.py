"""
Симметричное шифрование: AES-CBC, AES-CFB, DES-CBC, 3DES-CBC
"""

import os
import base64

from cryptography.hazmat.primitives.ciphers import Cipher, algorithms, modes
from cryptography.hazmat.primitives.padding import PKCS7
from cryptography.hazmat.backends import default_backend


ALGORITHMS = ["AES-CBC", "AES-CFB", "DES-CBC", "3DES-CBC"]


def key_iv_size(algo: str) -> tuple[int, int]:
    """Возвращает (размер_ключа, размер_IV) в байтах."""
    match algo:
        case "AES-CBC" | "AES-CFB":
            return 32, 16       # AES-256
        case "DES-CBC":
            return 8, 8
        case "3DES-CBC":
            return 24, 8
        case _:
            raise ValueError(f"Неизвестный алгоритм: {algo}")


def block_size(algo: str) -> int:
    """Возвращает размер блока в битах (для PKCS7)."""
    return 128 if algo.startswith("AES") else 64


def generate_params(algo: str) -> dict:
    """Генерирует случайные ключ и IV для заданного алгоритма."""
    key_len, iv_len = key_iv_size(algo)
    return {
        "algorithm": algo,
        "key": base64.b64encode(os.urandom(key_len)).decode(),
        "iv":  base64.b64encode(os.urandom(iv_len)).decode(),
    }


def make_cipher(algo: str, key: bytes, iv: bytes):
    """Создаёт объект Cipher для заданного алгоритма."""
    match algo:
        case "AES-CBC":
            return Cipher(algorithms.AES(key), modes.CBC(iv), backend=default_backend())
        case "AES-CFB":
            return Cipher(algorithms.AES(key), modes.CFB(iv), backend=default_backend())
        case "DES-CBC":
            k = key * 3 if len(key) == 8 else key
            return Cipher(algorithms.TripleDES(k), modes.CBC(iv), backend=default_backend())
        case "3DES-CBC":
            return Cipher(algorithms.TripleDES(key), modes.CBC(iv), backend=default_backend())
        case _:
            raise ValueError(f"Неизвестный алгоритм: {algo}")


def encrypt(algo: str, key: bytes, iv: bytes, data: bytes) -> bytes:
    """Шифрует данные. Для CBC-режимов применяет PKCS7 padding."""
    if algo in ("AES-CBC", "DES-CBC", "3DES-CBC"):
        padder = PKCS7(block_size(algo)).padder()
        data = padder.update(data) + padder.finalize()

    cipher = make_cipher(algo, key, iv)
    enc = cipher.encryptor()
    return enc.update(data) + enc.finalize()


def decrypt(algo: str, key: bytes, iv: bytes, ct: bytes) -> bytes:
    """Дешифрует данные. Для CBC-режимов снимает PKCS7 padding."""
    cipher = make_cipher(algo, key, iv)
    dec = cipher.decryptor()
    pt = dec.update(ct) + dec.finalize()

    if algo in ("AES-CBC", "DES-CBC", "3DES-CBC"):
        unpadder = PKCS7(block_size(algo)).unpadder()
        pt = unpadder.update(pt) + unpadder.finalize()
    return pt
