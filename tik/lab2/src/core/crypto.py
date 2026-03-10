from __future__ import annotations

import secrets


def xor_bytes(left: bytes, right: bytes) -> bytes:
    # XOR корректен только при полном совпадении длин
    if len(left) != len(right):
        raise ValueError("Длины данных и ключа должны совпадать.")
    return bytes(a ^ b for a, b in zip(left, right))


def otp_encrypt(plaintext: bytes) -> tuple[bytes, bytes]:
    # Для OTP ключ генерируется длиной сообщения
    key = secrets.token_bytes(len(plaintext))
    return xor_bytes(plaintext, key), key


def otp_apply(data: bytes, key: bytes) -> bytes:
    return xor_bytes(data, key)

