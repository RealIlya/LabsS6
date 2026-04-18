"""
Асимметричное шифрование: RSA-2048 / RSA-4096 (гибридная схема RSA+AES)
"""

import os
import base64

from cryptography.hazmat.primitives.ciphers import Cipher, algorithms, modes
from cryptography.hazmat.primitives.padding import PKCS7
from cryptography.hazmat.primitives import hashes, serialization
from cryptography.hazmat.primitives.asymmetric import rsa, padding as rsa_padding
from cryptography.hazmat.backends import default_backend


def generate_key_pair(key_size: int = 2048):
    """Генерирует пару RSA-ключей. Возвращает (private_key, public_key)."""
    private_key = rsa.generate_private_key(
        public_exponent=65537,
        key_size=key_size,
        backend=default_backend()
    )
    return private_key, private_key.public_key()


def private_key_to_pem(private_key) -> bytes:
    """Сериализует закрытый ключ в PEM (без пароля)."""
    return private_key.private_bytes(
        serialization.Encoding.PEM,
        serialization.PrivateFormat.TraditionalOpenSSL,
        serialization.NoEncryption()
    )


def public_key_to_pem(public_key) -> bytes:
    """Сериализует открытый ключ в PEM."""
    return public_key.public_bytes(
        serialization.Encoding.PEM,
        serialization.PublicFormat.SubjectPublicKeyInfo
    )


def load_private_key(pem_data: bytes):
    """Загружает закрытый ключ из PEM-байт."""
    return serialization.load_pem_private_key(
        pem_data, password=None, backend=default_backend()
    )


def load_public_key(pem_data: bytes):
    """Загружает открытый ключ из PEM-байт."""
    return serialization.load_pem_public_key(pem_data, backend=default_backend())


def encrypt(public_key, data: bytes) -> dict:
    """
    Гибридное шифрование: генерирует AES-256 ключ, шифрует данные AES-CBC,
    затем шифрует AES-ключ с помощью RSA-OAEP.
    Возвращает словарь {"enc_key", "iv", "ct"} с base64-значениями.
    """
    aes_key = os.urandom(32)
    aes_iv  = os.urandom(16)

    padder = PKCS7(128).padder()
    padded = padder.update(data) + padder.finalize()

    cipher = Cipher(algorithms.AES(aes_key), modes.CBC(aes_iv), backend=default_backend())
    ct = cipher.encryptor().update(padded) + cipher.encryptor().finalize()

    enc_key = public_key.encrypt(
        aes_key,
        rsa_padding.OAEP(
            mgf=rsa_padding.MGF1(algorithm=hashes.SHA256()),
            algorithm=hashes.SHA256(),
            label=None
        )
    )

    return {
        "enc_key": base64.b64encode(enc_key).decode(),
        "iv":      base64.b64encode(aes_iv).decode(),
        "ct":      base64.b64encode(ct).decode(),
    }


def decrypt(private_key, payload: dict) -> bytes:
    """
    Гибридное дешифрование: расшифровывает AES-ключ с помощью RSA,
    затем дешифрует данные с помощью AES-CBC.
    """
    aes_key = private_key.decrypt(
        base64.b64decode(payload["enc_key"]),
        rsa_padding.OAEP(
            mgf=rsa_padding.MGF1(algorithm=hashes.SHA256()),
            algorithm=hashes.SHA256(),
            label=None
        )
    )
    aes_iv = base64.b64decode(payload["iv"])
    ct     = base64.b64decode(payload["ct"])

    cipher = Cipher(algorithms.AES(aes_key), modes.CBC(aes_iv), backend=default_backend())
    pt_padded = cipher.decryptor().update(ct) + cipher.decryptor().finalize()

    unpadder = PKCS7(128).unpadder()
    return unpadder.update(pt_padded) + unpadder.finalize()
