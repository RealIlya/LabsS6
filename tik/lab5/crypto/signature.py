"""
Цифровая подпись: RSA-PSS + SHA-256
"""

import base64

from cryptography.hazmat.primitives import hashes
from cryptography.hazmat.primitives.asymmetric import padding as rsa_padding
from cryptography.exceptions import InvalidSignature


ALGORITHM_LABEL = "RSA-PSS-SHA256"

_PSS = lambda: rsa_padding.PSS(
    mgf=rsa_padding.MGF1(hashes.SHA256()),
    salt_length=rsa_padding.PSS.MAX_LENGTH
)


def sign(private_key, data: bytes) -> bytes:
    """Подписывает данные закрытым ключом RSA-PSS + SHA-256."""
    return private_key.sign(data, _PSS(), hashes.SHA256())


def verify(public_key, data: bytes, signature: bytes) -> bool:
    """
    Проверяет подпись открытым ключом.
    Возвращает True если подпись верна, False если нет.
    """
    try:
        public_key.verify(signature, data, _PSS(), hashes.SHA256())
        return True
    except InvalidSignature:
        return False


def signature_to_payload(signature: bytes) -> dict:
    """Упаковывает подпись в словарь для сохранения в JSON."""
    return {
        "algorithm": ALGORITHM_LABEL,
        "signature": base64.b64encode(signature).decode(),
    }


def payload_to_signature(payload: dict) -> bytes:
    """Извлекает подпись из JSON-словаря."""
    return base64.b64decode(payload["signature"])
