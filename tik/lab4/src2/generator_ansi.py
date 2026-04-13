"""
Генератор ПСП ANSI X9.17 на базе 3DES + статистические тесты NIST.
Порт с C++/Qt на чистый Python.
"""

import math
import time
from datetime import datetime

# ============================================================================
# ТАБЛИЦЫ DES
# ============================================================================

IP = [
    58, 50, 42, 34, 26, 18, 10, 2, 60, 52, 44, 36, 28,
    20, 12, 4, 62, 54, 46, 38, 30, 22, 14, 6, 64, 56,
    48, 40, 32, 24, 16, 8, 57, 49, 41, 33, 25, 17, 9,
    1, 59, 51, 43, 35, 27, 19, 11, 3, 61, 53, 45, 37,
    29, 21, 13, 5, 63, 55, 47, 39, 31, 23, 15, 7,
]

FP = [
    40, 8, 48, 16, 56, 24, 64, 32, 39, 7, 47, 15, 55,
    23, 63, 31, 38, 6, 46, 14, 54, 22, 62, 30, 37, 5,
    45, 13, 53, 21, 61, 29, 36, 4, 44, 12, 52, 20, 60,
    28, 35, 3, 43, 11, 51, 19, 59, 27, 34, 2, 42, 10,
    50, 18, 58, 26, 33, 1, 41, 9, 49, 17, 57, 25,
]

E = [
    32, 1, 2, 3, 4, 5, 4, 5, 6, 7, 8, 9,
    8, 9, 10, 11, 12, 13, 12, 13, 14, 15, 16, 17,
    16, 17, 18, 19, 20, 21, 20, 21, 22, 23, 24, 25,
    24, 25, 26, 27, 28, 29, 28, 29, 30, 31, 32, 1,
]

P = [
    16, 7, 20, 21, 29, 12, 28, 17, 1, 15, 23,
    26, 5, 18, 31, 10, 2, 8, 24, 14, 32, 27,
    3, 9, 19, 13, 30, 6, 22, 11, 4, 25,
]

PC1 = [
    57, 49, 41, 33, 25, 17, 9, 1, 58, 50, 42, 34, 26, 18, 10, 2,
    59, 51, 43, 35, 27, 19, 11, 3, 60, 52, 44, 36, 63, 55, 47, 39,
    31, 23, 15, 7, 62, 54, 46, 38, 30, 22, 14, 6, 61, 53, 45, 37,
    29, 21, 13, 5, 28, 20, 12, 4,
]

PC2 = [
    14, 17, 11, 24, 1, 5, 3, 28, 15, 6, 21, 10,
    23, 19, 12, 4, 26, 8, 16, 7, 27, 20, 13, 2,
    41, 52, 31, 37, 47, 55, 30, 40, 51, 45, 33, 48,
    44, 49, 39, 56, 34, 53, 46, 42, 50, 36, 29, 32,
]

SHIFTS = [1, 1, 2, 2, 2, 2, 2, 2, 1, 2, 2, 2, 2, 2, 2, 1]

S = [
    [
        14, 4, 13, 1, 2, 15, 11, 8, 3, 10, 6, 12, 5, 9, 0, 7,
        0, 15, 7, 4, 14, 2, 13, 1, 10, 6, 12, 11, 9, 5, 3, 8,
        4, 1, 14, 8, 13, 6, 2, 11, 15, 12, 9, 7, 3, 10, 5, 0,
        15, 12, 8, 2, 4, 9, 1, 7, 5, 11, 3, 14, 10, 0, 6, 13,
    ],
    [
        15, 1, 8, 14, 6, 11, 3, 4, 9, 7, 2, 13, 12, 0, 5, 10,
        3, 13, 4, 7, 15, 2, 8, 14, 12, 0, 1, 10, 6, 9, 11, 5,
        0, 14, 7, 11, 10, 4, 13, 1, 5, 8, 12, 6, 9, 3, 2, 15,
        13, 8, 10, 1, 3, 15, 4, 2, 11, 6, 7, 12, 0, 5, 14, 9,
    ],
    [
        10, 0, 9, 14, 6, 3, 15, 5, 1, 13, 12, 7, 11, 4, 2, 8,
        13, 7, 0, 9, 3, 4, 6, 10, 2, 8, 5, 14, 12, 11, 15, 1,
        13, 6, 4, 9, 8, 15, 3, 0, 11, 1, 2, 12, 5, 10, 14, 7,
        1, 10, 13, 0, 6, 9, 8, 7, 4, 15, 14, 3, 11, 5, 2, 12,
    ],
    [
        7, 13, 14, 3, 0, 6, 9, 10, 1, 2, 8, 5, 11, 12, 4, 15,
        13, 8, 11, 5, 6, 15, 0, 3, 4, 7, 2, 12, 1, 10, 14, 9,
        10, 6, 9, 0, 12, 11, 7, 13, 15, 1, 3, 14, 5, 2, 8, 4,
        3, 15, 0, 6, 10, 1, 13, 8, 9, 4, 5, 11, 12, 7, 2, 14,
    ],
    [
        2, 12, 4, 1, 7, 10, 11, 6, 8, 5, 3, 15, 13, 0, 14, 9,
        14, 11, 2, 12, 4, 7, 13, 1, 5, 0, 15, 10, 3, 9, 8, 6,
        4, 2, 1, 11, 10, 13, 7, 8, 15, 9, 12, 5, 6, 3, 0, 14,
        11, 8, 12, 7, 1, 14, 2, 13, 6, 15, 0, 9, 10, 4, 5, 3,
    ],
    [
        12, 1, 10, 15, 9, 2, 6, 8, 0, 13, 3, 4, 14, 7, 5, 11,
        10, 15, 4, 2, 7, 12, 9, 5, 6, 1, 13, 14, 0, 11, 3, 8,
        9, 14, 15, 5, 2, 8, 12, 3, 7, 0, 4, 10, 1, 13, 11, 6,
        4, 3, 2, 12, 9, 5, 15, 10, 11, 14, 1, 7, 6, 0, 8, 13,
    ],
    [
        4, 11, 2, 14, 15, 0, 8, 13, 3, 12, 9, 7, 5, 10, 6, 1,
        13, 0, 11, 7, 4, 9, 1, 10, 14, 3, 5, 12, 2, 15, 8, 6,
        1, 4, 11, 13, 12, 3, 7, 14, 10, 15, 6, 8, 0, 5, 9, 2,
        6, 11, 13, 8, 1, 4, 10, 7, 9, 5, 0, 15, 14, 2, 3, 12,
    ],
    [
        13, 2, 8, 4, 6, 15, 11, 1, 10, 9, 3, 14, 5, 0, 12, 7,
        1, 15, 13, 8, 10, 3, 7, 4, 12, 5, 6, 11, 0, 14, 9, 2,
        7, 11, 4, 1, 9, 12, 14, 2, 0, 6, 10, 13, 15, 3, 5, 8,
        2, 1, 14, 7, 4, 10, 8, 13, 15, 12, 9, 0, 3, 5, 6, 11,
    ],
]


# ============================================================================
# Вспомогательные функции DES
# ============================================================================

def _permute(input_val: int, table: list[int], size: int) -> int:
    """Перестановка битов по заданной таблице.
    
    size — размер ВЫХОДА (количество бит в результате).
    Таблицы DES используют 1-индексацию битов, где бит 1 = старший бит входа.
    Размер входа определяется максимальным индексом в таблице.
    """
    res = 0
    # Определяем размер входа по максимальному индексу в таблице
    input_size = max(table)
    for i in range(size):
        bit_index = input_size - table[i]  # table[i] = 1 => старший бит
        res = (res << 1) | ((input_val >> bit_index) & 1)
    return res


def des_encrypt(block: int, key: int) -> int:
    """DES шифрование одного 64-битного блока."""
    # Генерация подключей
    K = [0] * 16
    pc1 = _permute(key, PC1, 56)
    C = pc1 >> 28
    D = pc1 & 0xFFFFFFF

    for i in range(16):
        shift = SHIFTS[i]
        C = ((C << shift) | (C >> (28 - shift))) & 0xFFFFFFF
        D = ((D << shift) | (D >> (28 - shift))) & 0xFFFFFFF
        K[i] = _permute((C << 28) | D, PC2, 48)

    # Начальная перестановка
    block = _permute(block, IP, 64)
    L = block >> 32
    R = block & 0xFFFFFFFF

    # 16 раундов
    for i in range(16):
        # Expansion + XOR с ключом
        er = _permute(R, E, 48) ^ K[i]
        # S-блоки
        s_out = 0
        for j in range(8):
            row = (((er >> (42 - 6 * j + 5)) & 1) << 1) | ((er >> (42 - 6 * j)) & 1)
            col = (er >> (42 - 6 * j + 1)) & 0xF
            s_out = (s_out << 4) | S[j][row * 16 + col]
        # Перестановка P + XOR с L
        next_R = L ^ _permute(s_out, P, 32)
        L = R
        R = next_R

    # Финальная перестановка (R << 32 | L, не L << 32 | R)
    return _permute((R << 32) | L, FP, 64)


def des_decrypt(block: int, key: int) -> int:
    """DES дешифрование одного 64-битного блока."""
    # Генерация подключей (такая же, как в encrypt)
    K = [0] * 16
    pc1 = _permute(key, PC1, 56)
    C = pc1 >> 28
    D = pc1 & 0xFFFFFFF

    for i in range(16):
        shift = SHIFTS[i]
        C = ((C << shift) | (C >> (28 - shift))) & 0xFFFFFFF
        D = ((D << shift) | (D >> (28 - shift))) & 0xFFFFFFF
        K[i] = _permute((C << 28) | D, PC2, 48)

    # Начальная перестановка
    block = _permute(block, IP, 64)
    L = block >> 32
    R = block & 0xFFFFFFFF

    # 16 раундов в обратном порядке
    for i in range(15, -1, -1):
        er = _permute(R, E, 48) ^ K[i]
        s_out = 0
        for j in range(8):
            row = (((er >> (42 - 6 * j + 5)) & 1) << 1) | ((er >> (42 - 6 * j)) & 1)
            col = (er >> (42 - 6 * j + 1)) & 0xF
            s_out = (s_out << 4) | S[j][row * 16 + col]
        next_R = L ^ _permute(s_out, P, 32)
        L = R
        R = next_R

    return _permute((R << 32) | L, FP, 64)


def encrypt_3des(data: int, k1: int, k2: int) -> int:
    """3DES: E_K1( D_K2( E_K1(M) ) )"""
    return des_encrypt(des_decrypt(des_encrypt(data, k1), k2), k1)


# ============================================================================
# Генерация ANSI X9.17
# ============================================================================

def generate_ansi(k1_hex: str, k2_hex: str, s0_hex: str, m: int,
                  progress_cb=None) -> str:
    """
    Генерация псевдослучайной последовательности по алгоритму ANSI X9.17.
    Возвращает битовую строку длиной m*64.
    progress_cb — опциональный callback(current, total) для отображения прогресса.
    """
    k1 = int(k1_hex, 16)
    k2 = int(k2_hex, 16)
    s_prev = int(s0_hex, 16)

    # Текущее время в миллисекундах (аналог QDateTime::currentMSecsSinceEpoch)
    d = int(time.time() * 1000)
    temp = encrypt_3des(d, k1, k2)

    result = []
    for i in range(m):
        x_i = encrypt_3des(temp ^ s_prev, k1, k2)
        s_prev = encrypt_3des(x_i ^ temp, k1, k2)

        # 64 бита, старший первым
        for b in range(63, -1, -1):
            result.append('1' if (x_i >> b) & 1 else '0')

        if progress_cb:
            progress_cb(i + 1, m)

    return ''.join(result)


# ============================================================================
# Статистические тесты NIST
# ============================================================================

def frequency_test(seq: str) -> str:
    """[1] Частотный (монобитный) тест."""
    log = "[1] ЧАСТОТНЫЙ ТЕСТ\n"
    n = len(seq)
    Sn = sum(1 if c == '1' else -1 for c in seq)

    S = abs(Sn) / math.sqrt(n)

    log += f"Длина n = {n}\n"
    log += f"Сумма Sn = {Sn}\n"
    log += f"Статистика S = {S:.6f}\n"

    if S <= 1.82138636:
        log += "РЕЗУЛЬТАТ: ПРОЙДЕН (S <= 1.82138636)\n"
    else:
        log += "РЕЗУЛЬТАТ: ПРОВАЛЕН (S > 1.82138636)\n"

    return log


def runs_test(seq: str) -> str:
    """[2] Тест на последовательность одинаковых бит (runs test)."""
    log = "[2] ТЕСТ НА ПОСЛЕДОВАТЕЛЬНОСТЬ ОДИНАКОВЫХ БИТ\n"
    n = len(seq)
    ones = seq.count('1')

    pi = ones / n

    Vn = 1
    for i in range(n - 1):
        if seq[i] != seq[i + 1]:
            Vn += 1

    S = abs(Vn - 2.0 * n * pi * (1.0 - pi)) / \
        (2.0 * math.sqrt(2.0 * n) * pi * (1.0 - pi))

    log += f"Частота единиц pi = {pi:.6f}\n"
    log += f"Количество цепочек Vn = {Vn}\n"
    log += f"Статистика S = {S:.6f}\n"

    if S <= 1.82138636:
        log += "РЕЗУЛЬТАТ: ПРОЙДЕН (S <= 1.82138636)\n"
    else:
        log += "РЕЗУЛЬТАТ: ПРОВАЛЕН (S > 1.82138636)\n"

    return log


def extended_deviation_test(seq: str) -> str:
    """[3] Расширенный тест на произвольные отклонения."""
    log = "[3] РАСШИРЕННЫЙ ТЕСТ НА ПРОИЗВОЛЬНЫЕ ОТКЛОНЕНИЯ\n"

    s_prime = [0]
    current_sum = 0
    for c in seq:
        current_sum += 1 if c == '1' else -1
        s_prime.append(current_sum)
    s_prime.append(0)

    L = -1
    xi: dict[int, int] = {}
    for val in s_prime:
        if val == 0:
            L += 1
        if -9 <= val <= 9 and val != 0:
            xi[val] = xi.get(val, 0) + 1

    log += f"Количество нулей в S' (L) = {L}\n\n"
    log += "Состояние(j) | Встреч(xi) | Статистика(Y_j) | Результат\n"
    log += "--------------------------------------------------------\n"

    all_passed = True
    for j in range(-9, 10):
        if j == 0:
            continue

        x_j = xi.get(j, 0)
        Y_j = abs(x_j - L) / math.sqrt(2.0 * L * (4.0 * abs(j) - 2.0))

        res_str = "PASS" if Y_j <= 1.82138636 else "FAIL"
        if Y_j > 1.82138636:
            all_passed = False

        log += f"{j:>12} | {x_j:>10} | {Y_j:>15.6f} | {res_str}\n"

    log += "\nИТОГОВЫЙ РЕЗУЛЬТАТ: "
    log += "ПРОЙДЕН (Все Y_j <= 1.82138636)\n" if all_passed \
        else "ПРОВАЛЕН (Есть Y_j > 1.82138636)\n"

    return log


def run_tests(bit_sequence: str) -> str:
    """Запуск всех трёх тестов."""
    if not bit_sequence:
        return "Ошибка: Пустая последовательность."

    log = "========== РЕЗУЛЬТАТЫ ТЕСТОВ NIST ==========\n\n"
    log += frequency_test(bit_sequence) + "\n"
    log += runs_test(bit_sequence) + "\n"
    log += extended_deviation_test(bit_sequence)

    return log


# ============================================================================
# Файловые операции
# ============================================================================

def read_file_content(file_path: str) -> str:
    """Чтение текста из файла."""
    try:
        with open(file_path, 'r', encoding='utf-8') as f:
            return f.read()
    except (IOError, OSError):
        return ""


def save_file_content(file_path: str, content: str) -> bool:
    """Запись текста в файл."""
    try:
        with open(file_path, 'w', encoding='utf-8') as f:
            f.write(content)
        return True
    except (IOError, OSError):
        return False


# ============================================================================
# CLI-интерфейс
# ============================================================================

def main():
    import sys
    import os

    # Путь к config.txt относительно расположения скрипта
    script_dir = os.path.dirname(os.path.abspath(__file__))
    config_path = os.path.join(script_dir, "..", "config.txt")

    if len(sys.argv) > 1:
        config_path = sys.argv[1]

    if len(sys.argv) >= 5:
        # Параметры из командной строки
        k1_hex = sys.argv[1]
        k2_hex = sys.argv[2]
        s0_hex = sys.argv[3]
        m = int(sys.argv[4])
    else:
        # Попытка прочитать из config
        content = read_file_content(config_path)
        if content:
            params = content.strip().split()
            if len(params) >= 4:
                k1_hex = params[0]
                k2_hex = params[1]
                s0_hex = params[2]
                m = int(params[3])
            else:
                print("Ошибка: config.txt должен содержать 4 параметра: K1 K2 s0 m")
                sys.exit(1)
        else:
            print(f"Не найден файл {config_path}")
            sys.exit(1)

    print(f"K1 = {k1_hex}")
    print(f"K2 = {k2_hex}")
    print(f"s0 = {s0_hex}")
    print(f"m  = {m}")
    print()

    # Генерация
    print("Генерация последовательности...")
    seq = generate_ansi(k1_hex, k2_hex, s0_hex, m)
    print(f"Длина последовательности: {len(seq)} бит")
    print()

    # Тесты
    print(run_tests(seq))


if __name__ == "__main__":
    main()
