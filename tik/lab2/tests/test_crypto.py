from core.crypto import otp_apply, otp_encrypt


def test_otp_encrypt_decrypt() -> None:
    plaintext = b"hello world"
    ciphertext, key = otp_encrypt(plaintext)
    decrypted = otp_apply(ciphertext, key)
    assert decrypted == plaintext

