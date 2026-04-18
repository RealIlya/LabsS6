"""
Тесты для криптографического приложения (crypto_app.py)
Запуск: python -m pytest test_crypto_app.py -v
        python test_crypto_app.py          (без pytest)
"""

import os
import sys
import json
import base64
import hashlib
import tempfile
import unittest
import shutil

from cryptography.hazmat.primitives.ciphers import Cipher, algorithms, modes
from cryptography.hazmat.primitives import hashes, serialization
from cryptography.hazmat.primitives.asymmetric import rsa, padding as rsa_padding
from cryptography.hazmat.primitives.padding import PKCS7
from cryptography.hazmat.backends import default_backend
from cryptography.exceptions import InvalidSignature


# ══════════════════════════════════════════════════════════════════════════════
#  Вспомогательные функции (дублируют логику вкладок для изолированного теста)
# ══════════════════════════════════════════════════════════════════════════════

def sym_generate_params(algo: str) -> dict:
    sizes = {
        "AES-CBC":  (32, 16),
        "AES-CFB":  (32, 16),
        "DES-CBC":  (8,  8),
        "3DES-CBC": (24, 8),
    }
    key_len, iv_len = sizes[algo]
    return {
        "algorithm": algo,
        "key": base64.b64encode(os.urandom(key_len)).decode(),
        "iv":  base64.b64encode(os.urandom(iv_len)).decode(),
    }


def sym_encrypt(params: dict, data: bytes) -> bytes:
    algo = params["algorithm"]
    key  = base64.b64decode(params["key"])
    iv   = base64.b64decode(params["iv"])

    if algo in ("AES-CBC", "DES-CBC", "3DES-CBC"):
        block = 128 if algo.startswith("AES") else 64
        padder = PKCS7(block).padder()
        data = padder.update(data) + padder.finalize()

    cipher = _make_cipher(algo, key, iv)
    enc = cipher.encryptor()
    return enc.update(data) + enc.finalize()


def sym_decrypt(params: dict, ct: bytes) -> bytes:
    algo = params["algorithm"]
    key  = base64.b64decode(params["key"])
    iv   = base64.b64decode(params["iv"])

    cipher = _make_cipher(algo, key, iv)
    dec = cipher.decryptor()
    pt = dec.update(ct) + dec.finalize()

    if algo in ("AES-CBC", "DES-CBC", "3DES-CBC"):
        block = 128 if algo.startswith("AES") else 64
        unpadder = PKCS7(block).unpadder()
        pt = unpadder.update(pt) + unpadder.finalize()
    return pt


def _make_cipher(algo, key, iv):
    if algo == "AES-CBC":
        return Cipher(algorithms.AES(key), modes.CBC(iv), backend=default_backend())
    if algo == "AES-CFB":
        return Cipher(algorithms.AES(key), modes.CFB(iv), backend=default_backend())
    if algo == "DES-CBC":
        return Cipher(algorithms.TripleDES(key * 3), modes.CBC(iv), backend=default_backend())
    if algo == "3DES-CBC":
        return Cipher(algorithms.TripleDES(key), modes.CBC(iv), backend=default_backend())
    raise ValueError(algo)


def rsa_generate_keys(size=2048):
    priv = rsa.generate_private_key(65537, size, backend=default_backend())
    pub  = priv.public_key()
    return priv, pub


def rsa_encrypt(pub_key, data: bytes) -> dict:
    aes_key = os.urandom(32)
    aes_iv  = os.urandom(16)
    padder  = PKCS7(128).padder()
    padded  = padder.update(data) + padder.finalize()
    cipher  = Cipher(algorithms.AES(aes_key), modes.CBC(aes_iv), backend=default_backend())
    ct      = cipher.encryptor().update(padded) + cipher.encryptor().finalize()
    enc_key = pub_key.encrypt(
        aes_key,
        rsa_padding.OAEP(mgf=rsa_padding.MGF1(hashes.SHA256()),
                         algorithm=hashes.SHA256(), label=None)
    )
    return {"enc_key": base64.b64encode(enc_key).decode(),
            "iv":      base64.b64encode(aes_iv).decode(),
            "ct":      base64.b64encode(ct).decode()}


def rsa_decrypt(priv_key, payload: dict) -> bytes:
    aes_key = priv_key.decrypt(
        base64.b64decode(payload["enc_key"]),
        rsa_padding.OAEP(mgf=rsa_padding.MGF1(hashes.SHA256()),
                         algorithm=hashes.SHA256(), label=None)
    )
    aes_iv = base64.b64decode(payload["iv"])
    ct     = base64.b64decode(payload["ct"])
    cipher = Cipher(algorithms.AES(aes_key), modes.CBC(aes_iv), backend=default_backend())
    pt_pad = cipher.decryptor().update(ct) + cipher.decryptor().finalize()
    unpadder = PKCS7(128).unpadder()
    return unpadder.update(pt_pad) + unpadder.finalize()


def rsa_sign(priv_key, data: bytes) -> bytes:
    return priv_key.sign(
        data,
        rsa_padding.PSS(mgf=rsa_padding.MGF1(hashes.SHA256()),
                        salt_length=rsa_padding.PSS.MAX_LENGTH),
        hashes.SHA256()
    )


def rsa_verify(pub_key, data: bytes, signature: bytes) -> bool:
    try:
        pub_key.verify(
            signature, data,
            rsa_padding.PSS(mgf=rsa_padding.MGF1(hashes.SHA256()),
                            salt_length=rsa_padding.PSS.MAX_LENGTH),
            hashes.SHA256()
        )
        return True
    except InvalidSignature:
        return False


def compute_hash(algo: str, data: bytes) -> str:
    fn = {"MD5": hashlib.md5, "SHA-1": hashlib.sha1,
          "SHA-256": hashlib.sha256, "SHA-512": hashlib.sha512,
          "SHA3-256": hashlib.sha3_256, "SHA3-512": hashlib.sha3_512}[algo]
    return fn(data).hexdigest()


# ══════════════════════════════════════════════════════════════════════════════
#  Тест 1: Симметричное шифрование
# ══════════════════════════════════════════════════════════════════════════════

class TestSymmetricEncryption(unittest.TestCase):
    PLAINTEXT = b"Test plaintext for symmetric encryption. 12345 !@#$%"

    def _roundtrip(self, algo):
        params = sym_generate_params(algo)
        ct = sym_encrypt(params, self.PLAINTEXT)
        pt = sym_decrypt(params, ct)
        self.assertEqual(pt, self.PLAINTEXT, f"{algo}: расшифрованный текст не совпадает")

    # ── корректность алгоритмов ────────────────────────────────────────────

    def test_aes_cbc_roundtrip(self):
        self._roundtrip("AES-CBC")

    def test_aes_cfb_roundtrip(self):
        self._roundtrip("AES-CFB")

    def test_des_cbc_roundtrip(self):
        self._roundtrip("DES-CBC")

    def test_3des_cbc_roundtrip(self):
        self._roundtrip("3DES-CBC")

    # ── шифртекст ≠ открытый текст ────────────────────────────────────────

    def test_ciphertext_differs_from_plaintext(self):
        for algo in ("AES-CBC", "AES-CFB", "DES-CBC", "3DES-CBC"):
            params = sym_generate_params(algo)
            ct = sym_encrypt(params, self.PLAINTEXT)
            self.assertNotEqual(ct, self.PLAINTEXT,
                                f"{algo}: шифртекст совпадает с открытым текстом!")

    # ── разные ключи дают разные шифртексты ──────────────────────────────

    def test_different_keys_different_ciphertext(self):
        p1 = sym_generate_params("AES-CBC")
        p2 = sym_generate_params("AES-CBC")
        ct1 = sym_encrypt(p1, self.PLAINTEXT)
        ct2 = sym_encrypt(p2, self.PLAINTEXT)
        self.assertNotEqual(ct1, ct2)

    # ── неправильный ключ → ошибка при расшифровке ───────────────────────

    def test_wrong_key_fails(self):
        params = sym_generate_params("AES-CBC")
        ct = sym_encrypt(params, self.PLAINTEXT)
        bad_params = sym_generate_params("AES-CBC")   # другой ключ + IV
        with self.assertRaises(Exception):
            sym_decrypt(bad_params, ct)

    # ── параметры записываются в файл / читаются из файла ─────────────────

    def test_params_file_io(self):
        with tempfile.NamedTemporaryFile("w", suffix=".json", delete=False) as f:
            params = sym_generate_params("AES-CBC")
            json.dump(params, f)
            fname = f.name
        try:
            with open(fname) as f:
                loaded = json.load(f)
            self.assertEqual(params["key"], loaded["key"])
            self.assertEqual(params["iv"],  loaded["iv"])
        finally:
            os.unlink(fname)

    # ── зашифрованные данные сохраняются в файл ───────────────────────────

    def test_ciphertext_saved_to_file(self):
        params = sym_generate_params("AES-CBC")
        ct = sym_encrypt(params, self.PLAINTEXT)
        with tempfile.NamedTemporaryFile(delete=False, suffix=".enc") as f:
            f.write(ct)
            fname = f.name
        try:
            self.assertTrue(os.path.getsize(fname) > 0)
            ct_read = open(fname, "rb").read()
            self.assertEqual(ct, ct_read)
        finally:
            os.unlink(fname)

    # ── пустой файл ──────────────────────────────────────────────────────

    def test_empty_data(self):
        for algo in ("AES-CBC", "AES-CFB"):
            params = sym_generate_params(algo)
            ct = sym_encrypt(params, b"")
            pt = sym_decrypt(params, ct)
            self.assertEqual(pt, b"")

    # ── большой файл (1 МБ) ──────────────────────────────────────────────

    def test_large_file(self):
        data = os.urandom(1024 * 1024)
        params = sym_generate_params("AES-CBC")
        ct = sym_encrypt(params, data)
        pt = sym_decrypt(params, ct)
        self.assertEqual(pt, data)

    # ── бинарные данные ──────────────────────────────────────────────────

    def test_binary_data(self):
        data = bytes(range(256)) * 10
        params = sym_generate_params("AES-CFB")
        ct = sym_encrypt(params, data)
        pt = sym_decrypt(params, ct)
        self.assertEqual(pt, data)


# ══════════════════════════════════════════════════════════════════════════════
#  Тест 2: Асимметричное шифрование (RSA)
# ══════════════════════════════════════════════════════════════════════════════

class TestAsymmetricEncryption(unittest.TestCase):
    PLAINTEXT = b"Secret RSA message for testing purposes."

    @classmethod
    def setUpClass(cls):
        cls.priv2048, cls.pub2048 = rsa_generate_keys(2048)

    # ── генерация ключей ─────────────────────────────────────────────────

    def test_key_generation_2048(self):
        priv, pub = rsa_generate_keys(2048)
        self.assertEqual(priv.key_size, 2048)

    def test_key_generation_4096(self):
        priv, pub = rsa_generate_keys(4096)
        self.assertEqual(priv.key_size, 4096)

    # ── ключи сохраняются в PEM-файлы ────────────────────────────────────

    def test_keys_saved_to_pem_files(self):
        priv, pub = rsa_generate_keys(2048)
        with tempfile.TemporaryDirectory() as d:
            priv_path = os.path.join(d, "private.pem")
            pub_path  = os.path.join(d, "public.pem")
            with open(priv_path, "wb") as f:
                f.write(priv.private_bytes(serialization.Encoding.PEM,
                        serialization.PrivateFormat.TraditionalOpenSSL,
                        serialization.NoEncryption()))
            with open(pub_path, "wb") as f:
                f.write(pub.public_bytes(serialization.Encoding.PEM,
                        serialization.PublicFormat.SubjectPublicKeyInfo))
            self.assertTrue(os.path.exists(priv_path))
            self.assertTrue(os.path.exists(pub_path))

    # ── ключи загружаются из файла ────────────────────────────────────────

    def test_keys_loaded_from_file(self):
        priv, pub = rsa_generate_keys(2048)
        priv_pem = priv.private_bytes(serialization.Encoding.PEM,
                   serialization.PrivateFormat.TraditionalOpenSSL,
                   serialization.NoEncryption())
        pub_pem = pub.public_bytes(serialization.Encoding.PEM,
                  serialization.PublicFormat.SubjectPublicKeyInfo)
        loaded_priv = serialization.load_pem_private_key(priv_pem, password=None,
                                                         backend=default_backend())
        loaded_pub  = serialization.load_pem_public_key(pub_pem, backend=default_backend())
        self.assertEqual(loaded_priv.key_size, 2048)
        self.assertEqual(loaded_pub.key_size,  2048)

    # ── корректная схема шифрования/расшифровки ───────────────────────────

    def test_encrypt_decrypt_roundtrip(self):
        payload = rsa_encrypt(self.pub2048, self.PLAINTEXT)
        pt = rsa_decrypt(self.priv2048, payload)
        self.assertEqual(pt, self.PLAINTEXT)

    # ── результат шифрования сохраняется в файл ───────────────────────────

    def test_ciphertext_saved_to_file(self):
        payload = rsa_encrypt(self.pub2048, self.PLAINTEXT)
        with tempfile.NamedTemporaryFile("w", suffix=".enc", delete=False) as f:
            json.dump(payload, f)
            fname = f.name
        try:
            loaded = json.load(open(fname))
            self.assertIn("enc_key", loaded)
            self.assertIn("iv", loaded)
            self.assertIn("ct", loaded)
        finally:
            os.unlink(fname)

    # ── чужой закрытый ключ не расшифрует данные ─────────────────────────

    def test_wrong_private_key_fails(self):
        payload = rsa_encrypt(self.pub2048, self.PLAINTEXT)
        other_priv, _ = rsa_generate_keys(2048)
        with self.assertRaises(Exception):
            rsa_decrypt(other_priv, payload)

    # ── повторное шифрование одних данных даёт разный шифртекст (OAEP) ───

    def test_probabilistic_encryption(self):
        p1 = rsa_encrypt(self.pub2048, self.PLAINTEXT)
        p2 = rsa_encrypt(self.pub2048, self.PLAINTEXT)
        self.assertNotEqual(p1["ct"], p2["ct"])

    # ── большой файл (гибридное шифрование) ──────────────────────────────

    def test_large_file_hybrid(self):
        data = os.urandom(100 * 1024)   # 100 KB
        payload = rsa_encrypt(self.pub2048, data)
        pt = rsa_decrypt(self.priv2048, payload)
        self.assertEqual(pt, data)


# ══════════════════════════════════════════════════════════════════════════════
#  Тест 3: Цифровая подпись (RSA-PSS + SHA-256)
# ══════════════════════════════════════════════════════════════════════════════

class TestDigitalSignature(unittest.TestCase):
    DATA     = b"Document to be signed for integrity check."
    DATA_ALT = b"Tampered document content!"

    @classmethod
    def setUpClass(cls):
        cls.priv, cls.pub = rsa_generate_keys(2048)
        cls.signature = rsa_sign(cls.priv, cls.DATA)

    # ── корректная подпись проходит верификацию ───────────────────────────

    def test_valid_signature_verifies(self):
        self.assertTrue(rsa_verify(self.pub, self.DATA, self.signature))

    # ── изменённые данные не проходят верификацию ─────────────────────────

    def test_tampered_data_fails(self):
        self.assertFalse(rsa_verify(self.pub, self.DATA_ALT, self.signature))

    # ── чужой открытый ключ не верифицирует подпись ───────────────────────

    def test_wrong_public_key_fails(self):
        _, other_pub = rsa_generate_keys(2048)
        self.assertFalse(rsa_verify(other_pub, self.DATA, self.signature))

    # ── подпись сохраняется в файл (JSON) ────────────────────────────────

    def test_signature_saved_to_file(self):
        with tempfile.NamedTemporaryFile("w", suffix=".sig", delete=False) as f:
            payload = {"algorithm": "RSA-PSS-SHA256",
                       "signature": base64.b64encode(self.signature).decode()}
            json.dump(payload, f, indent=2)
            fname = f.name
        try:
            loaded = json.load(open(fname))
            sig_loaded = base64.b64decode(loaded["signature"])
            self.assertEqual(sig_loaded, self.signature)
            self.assertEqual(loaded["algorithm"], "RSA-PSS-SHA256")
        finally:
            os.unlink(fname)

    # ── подпись загружается из файла и верифицируется ─────────────────────

    def test_signature_loaded_from_file_and_verified(self):
        with tempfile.NamedTemporaryFile("w", suffix=".sig", delete=False) as f:
            json.dump({"algorithm": "RSA-PSS-SHA256",
                       "signature": base64.b64encode(self.signature).decode()}, f)
            fname = f.name
        try:
            loaded = json.load(open(fname))
            sig = base64.b64decode(loaded["signature"])
            self.assertTrue(rsa_verify(self.pub, self.DATA, sig))
        finally:
            os.unlink(fname)

    # ── подпись нечитаемой / испорченной подписи → False ─────────────────

    def test_corrupted_signature_fails(self):
        corrupted = bytearray(self.signature)
        corrupted[0] ^= 0xFF
        self.assertFalse(rsa_verify(self.pub, self.DATA, bytes(corrupted)))

    # ── бинарные данные подписываются / верифицируются ────────────────────

    def test_binary_data_signature(self):
        data = os.urandom(4096)
        sig  = rsa_sign(self.priv, data)
        self.assertTrue(rsa_verify(self.pub, data, sig))

    # ── пустые данные можно подписать ────────────────────────────────────

    def test_empty_data_signature(self):
        sig = rsa_sign(self.priv, b"")
        self.assertTrue(rsa_verify(self.pub, b"", sig))

    # ── параметры верификации читаются из файла ───────────────────────────

    def test_verification_params_from_file(self):
        priv, pub = rsa_generate_keys(2048)
        with tempfile.TemporaryDirectory() as d:
            pub_path = os.path.join(d, "public.pem")
            with open(pub_path, "wb") as f:
                f.write(pub.public_bytes(serialization.Encoding.PEM,
                        serialization.PublicFormat.SubjectPublicKeyInfo))
            data = b"Params loaded from file"
            sig  = rsa_sign(priv, data)
            # load back and verify
            loaded_pub = serialization.load_pem_public_key(
                open(pub_path, "rb").read(), backend=default_backend())
            self.assertTrue(rsa_verify(loaded_pub, data, sig))


# ══════════════════════════════════════════════════════════════════════════════
#  Тест 4: Хэширование
# ══════════════════════════════════════════════════════════════════════════════

class TestHashing(unittest.TestCase):
    DATA      = b"The quick brown fox jumps over the lazy dog"
    DATA_ZERO = b""

    # Известные хэш-значения для проверки корректности вычислений
    KNOWN = {
        "MD5":      "9e107d9d372bb6826bd81d3542a419d6",
        "SHA-1":    "2fd4e1c67a2d28fced849ee1bb76e7391b93eb12",
        "SHA-256":  "d7a8fbb307d7809469ca9abcb0082e4f8d5651e46d3cdb762d02d0bf37c9e592",
        "SHA-512":  ("07e547d9586f6a73f73fbac0435ed76951218fb7d0c8d788a309d785436bbb64"
                     "2e93a252a954f23912547d1e8a3b5ed6e1bfd7097821233fa0538f3db854fee6"),
        "SHA3-256": "69070dda01975c8c120c3aada1b282394e7f032fa9cf32f4cb2259a0897dfc04",
        "SHA3-512": ("01dedd5de4ef14642445ba5f5b97c15e47b9ad931326e4b0727cd94cefc44ff"
                     "f23f07bf543139939b49128caf436dc1bdee54fcb24023a08d9403f9b4bf0d450"),
    }

    # ── известные значения ────────────────────────────────────────────────

    def test_md5_known_value(self):
        self.assertEqual(compute_hash("MD5", self.DATA), self.KNOWN["MD5"])

    def test_sha1_known_value(self):
        self.assertEqual(compute_hash("SHA-1", self.DATA), self.KNOWN["SHA-1"])

    def test_sha256_known_value(self):
        self.assertEqual(compute_hash("SHA-256", self.DATA), self.KNOWN["SHA-256"])

    def test_sha512_known_value(self):
        self.assertEqual(compute_hash("SHA-512", self.DATA), self.KNOWN["SHA-512"])

    def test_sha3_256_known_value(self):
        self.assertEqual(compute_hash("SHA3-256", self.DATA), self.KNOWN["SHA3-256"])

    def test_sha3_512_known_value(self):
        self.assertEqual(compute_hash("SHA3-512", self.DATA), self.KNOWN["SHA3-512"])

    # ── детерминизм ───────────────────────────────────────────────────────

    def test_same_data_same_hash(self):
        for algo in self.KNOWN:
            h1 = compute_hash(algo, self.DATA)
            h2 = compute_hash(algo, self.DATA)
            self.assertEqual(h1, h2, f"{algo}: хэш не детерминирован!")

    # ── лавинный эффект ───────────────────────────────────────────────────

    def test_different_data_different_hash(self):
        alt_data = b"The quick brown fox jumps over the lazy cat"
        for algo in self.KNOWN:
            self.assertNotEqual(compute_hash(algo, self.DATA),
                                compute_hash(algo, alt_data),
                                f"{algo}: хэши совпали для разных данных!")

    # ── длина хэшей ───────────────────────────────────────────────────────

    def test_hash_lengths(self):
        expected_hex_len = {
            "MD5": 32, "SHA-1": 40, "SHA-256": 64, "SHA-512": 128,
            "SHA3-256": 64, "SHA3-512": 128,
        }
        for algo, length in expected_hex_len.items():
            h = compute_hash(algo, self.DATA)
            self.assertEqual(len(h), length,
                             f"{algo}: ожидалась длина {length}, получено {len(h)}")

    # ── пустые данные ─────────────────────────────────────────────────────

    def test_empty_data_hashes(self):
        known_empty = {
            "MD5":     "d41d8cd98f00b204e9800998ecf8427e",
            "SHA-256": "e3b0c44298fc1c149afbf4c8996fb92427ae41e4649b934ca495991b7852b855",
        }
        for algo, expected in known_empty.items():
            self.assertEqual(compute_hash(algo, b""), expected)

    # ── хэш сохраняется в файл ───────────────────────────────────────────

    def test_hash_saved_to_file(self):
        h = compute_hash("SHA-256", self.DATA)
        with tempfile.NamedTemporaryFile("w", suffix=".txt", delete=False) as f:
            f.write(f"SHA-256 : {h}\n")
            fname = f.name
        try:
            content = open(fname).read()
            self.assertIn(h, content)
            self.assertIn("SHA-256", content)
        finally:
            os.unlink(fname)

    # ── хэш вычисляется из файла ─────────────────────────────────────────

    def test_hash_from_file(self):
        with tempfile.NamedTemporaryFile(delete=False) as f:
            f.write(self.DATA)
            fname = f.name
        try:
            file_data = open(fname, "rb").read()
            self.assertEqual(compute_hash("SHA-256", file_data),
                             compute_hash("SHA-256", self.DATA))
        finally:
            os.unlink(fname)

    # ── несколько алгоритмов сразу ────────────────────────────────────────

    def test_multi_algorithm_output(self):
        results = {algo: compute_hash(algo, self.DATA) for algo in self.KNOWN}
        self.assertEqual(len(results), len(self.KNOWN))
        for algo, h in results.items():
            self.assertIsInstance(h, str)
            self.assertTrue(len(h) > 0)

    # ── большой файл (2 МБ) ──────────────────────────────────────────────

    def test_large_file_hash(self):
        data = os.urandom(2 * 1024 * 1024)
        h1 = compute_hash("SHA-256", data)
        h2 = compute_hash("SHA-256", data)
        self.assertEqual(h1, h2)
        self.assertEqual(len(h1), 64)


# ══════════════════════════════════════════════════════════════════════════════
#  Тест 5: Интеграционные тесты (полный рабочий цикл через файловую систему)
# ══════════════════════════════════════════════════════════════════════════════

class TestIntegration(unittest.TestCase):
    """Полный цикл: записать файл → шифровать → сохранить → загрузить → расшифровать"""

    ORIGINAL = b"Integration test: full encrypt-decrypt cycle via filesystem."

    def setUp(self):
        self.tmpdir = tempfile.mkdtemp()

    def tearDown(self):
        shutil.rmtree(self.tmpdir)

    def _path(self, name):
        return os.path.join(self.tmpdir, name)

    # ── Симметричное: файл → зашифровать → файл → расшифровать → файл ────

    def test_symmetric_full_cycle_aes_cbc(self):
        # 1. Сохранить входной файл
        src = self._path("input.txt")
        open(src, "wb").write(self.ORIGINAL)

        # 2. Сгенерировать параметры и сохранить в JSON
        params = sym_generate_params("AES-CBC")
        params_file = self._path("params.json")
        json.dump(params, open(params_file, "w"))

        # 3. Зашифровать и сохранить
        ct = sym_encrypt(params, open(src, "rb").read())
        enc_file = self._path("input.enc")
        open(enc_file, "wb").write(ct)

        # 4. Загрузить параметры, загрузить шифртекст, расшифровать
        loaded_params = json.load(open(params_file))
        ct_loaded = open(enc_file, "rb").read()
        pt = sym_decrypt(loaded_params, ct_loaded)

        # 5. Сохранить расшифрованный файл
        dec_file = self._path("input.dec")
        open(dec_file, "wb").write(pt)

        self.assertEqual(open(dec_file, "rb").read(), self.ORIGINAL)

    # ── Асимметричное: ключи → зашифровать → файл → расшифровать ─────────

    def test_asymmetric_full_cycle(self):
        priv, pub = rsa_generate_keys(2048)

        priv_file = self._path("private.pem")
        pub_file  = self._path("public.pem")
        open(priv_file, "wb").write(priv.private_bytes(
            serialization.Encoding.PEM, serialization.PrivateFormat.TraditionalOpenSSL,
            serialization.NoEncryption()))
        open(pub_file, "wb").write(pub.public_bytes(
            serialization.Encoding.PEM, serialization.PublicFormat.SubjectPublicKeyInfo))

        payload = rsa_encrypt(pub, self.ORIGINAL)
        enc_file = self._path("file.rsa.enc")
        json.dump(payload, open(enc_file, "w"))

        loaded_priv = serialization.load_pem_private_key(
            open(priv_file, "rb").read(), password=None, backend=default_backend())
        loaded_payload = json.load(open(enc_file))
        pt = rsa_decrypt(loaded_priv, loaded_payload)
        self.assertEqual(pt, self.ORIGINAL)

    # ── Подпись: подписать → файл → загрузить → верифицировать ───────────

    def test_signature_full_cycle(self):
        priv, pub = rsa_generate_keys(2048)
        data_file = self._path("doc.txt")
        open(data_file, "wb").write(self.ORIGINAL)

        sig = rsa_sign(priv, open(data_file, "rb").read())
        sig_file = self._path("doc.sig")
        json.dump({"algorithm": "RSA-PSS-SHA256",
                   "signature": base64.b64encode(sig).decode()},
                  open(sig_file, "w"), indent=2)

        loaded_sig = base64.b64decode(json.load(open(sig_file))["signature"])
        result = rsa_verify(pub, open(data_file, "rb").read(), loaded_sig)
        self.assertTrue(result)

    # ── Хэш: файл → хэш → сохранить → проверить ─────────────────────────

    def test_hash_full_cycle(self):
        data_file = self._path("data.bin")
        open(data_file, "wb").write(self.ORIGINAL)

        data = open(data_file, "rb").read()
        results = {algo: compute_hash(algo, data)
                   for algo in ("SHA-256", "SHA-512", "MD5")}

        hash_file = self._path("data.hashes.txt")
        with open(hash_file, "w") as f:
            f.write(f"Файл: {data_file}\n\n")
            for algo, h in results.items():
                f.write(f"{algo:<12}: {h}\n")

        content = open(hash_file).read()
        for algo, h in results.items():
            self.assertIn(h, content)

    # ── Подделка подписи обнаруживается при верификации ───────────────────

    def test_tampered_file_detected(self):
        priv, pub = rsa_generate_keys(2048)
        data = b"Original content"
        sig  = rsa_sign(priv, data)
        tampered = b"Modified content"
        self.assertFalse(rsa_verify(pub, tampered, sig))


# ══════════════════════════════════════════════════════════════════════════════
#  Точка входа
# ══════════════════════════════════════════════════════════════════════════════

if __name__ == "__main__":
    loader = unittest.TestLoader()
    suite  = unittest.TestSuite()
    for cls in (TestSymmetricEncryption, TestAsymmetricEncryption,
                TestDigitalSignature, TestHashing, TestIntegration):
        suite.addTests(loader.loadTestsFromTestCase(cls))
    runner = unittest.TextTestRunner(verbosity=2)
    result = runner.run(suite)
    sys.exit(0 if result.wasSuccessful() else 1)