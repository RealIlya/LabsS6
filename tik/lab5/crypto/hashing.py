#Хэширование: MD5, SHA-1, SHA-256, SHA-512, SHA3-256, SHA3-512


import hashlib


ALGORITHMS = ["MD5", "SHA-1", "SHA-256", "SHA-512", "SHA3-256", "SHA3-512"]

_MAPPING = {
    "MD5":      hashlib.md5,
    "SHA-1":    hashlib.sha1,
    "SHA-256":  hashlib.sha256,
    "SHA-512":  hashlib.sha512,
    "SHA3-256": hashlib.sha3_256,
    "SHA3-512": hashlib.sha3_512,
}


def compute(algo: str, data: bytes) -> str:
    #Вычисляет хэш данных заданным алгоритмом. Возвращает hex-строку.
    fn = _MAPPING.get(algo)
    if fn is None:
        raise ValueError(f"Неизвестный алгоритм: {algo}")
    return fn(data).hexdigest()


def compute_all(algorithms: list[str], data: bytes) -> dict[str, str]:
    #Вычисляет хэши сразу несколькими алгоритмами. Возвращает {algo: hex}.
    return {algo: compute(algo, data) for algo in algorithms}


def format_results(file_path: str, results: dict[str, str]) -> str:
    #Форматирует результаты хэширования в читаемую строку.
    lines = [f"Файл: {file_path}\n"]
    for algo, h in results.items():
        lines.append(f"{algo:<12}: {h}")
    return "\n".join(lines)
