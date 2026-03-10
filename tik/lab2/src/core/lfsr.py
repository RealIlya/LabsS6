from __future__ import annotations

import math
import re


def parse_polynomial(polynomial: str) -> tuple[int, list[int]]:
    # Старшая степень задаёт разрядность, остальные степени становятся taps
    raw = polynomial.replace(" ", "")
    if not raw:
        raise ValueError("Полином не задан.")

    terms = raw.split("+")
    exponents: set[int] = set()
    for term in terms:
        if term == "1":
            exponents.add(0)
            continue
        if term == "x":
            exponents.add(1)
            continue
        if re.fullmatch(r"x\^\d+", term):
            exponents.add(int(term[2:]))
            continue
        if re.fullmatch(r"x\d+", term):
            exponents.add(int(term[1:]))
            continue
        raise ValueError(f"Некорректный член полинома: {term}")

    degree = max(exponents)
    taps = sorted(exp for exp in exponents if exp != degree)
    if degree < 2:
        raise ValueError("Степень полинома должна быть >= 2.")
    if not taps:
        raise ValueError("У полинома нет коэффициентов обратной связи.")
    if any(tap < 0 or tap >= degree for tap in taps):
        raise ValueError("Некорректные степени полинома.")
    return degree, taps


def seed_bytes_len_for_polynomial(polynomial: str) -> int:
    degree, _ = parse_polynomial(polynomial)
    return math.ceil(degree / 8)


def seed_has_truncated_high_bits(seed_bytes: bytes, polynomial: str) -> tuple[bool, int]:
    # Старшие биты вне разрядности регистра будут отброшены
    degree, _ = parse_polynomial(polynomial)
    if not seed_bytes:
        return False, degree
    seed_value = int.from_bytes(seed_bytes, byteorder="big")
    return seed_value.bit_length() > degree, degree


def seed_int_from_bytes(seed_bytes: bytes, polynomial: str) -> int:
    # В seed остаются только младшие биты, которые помещаются в регистр
    degree, _ = parse_polynomial(polynomial)
    if not seed_bytes:
        raise ValueError("Пустое начальное состояние скремблера.")
    mask = (1 << degree) - 1
    seed = int.from_bytes(seed_bytes, byteorder="big") & mask
    if seed == 0:
        raise ValueError("Начальное состояние не должно быть нулевым.")
    return seed


def seed_bytes_from_int(seed: int, polynomial: str) -> bytes:
    if seed <= 0:
        raise ValueError("Начальное состояние должно быть положительным.")
    degree, _ = parse_polynomial(polynomial)
    mask = (1 << degree) - 1
    seed &= mask
    if seed == 0:
        raise ValueError("Начальное состояние не должно быть нулевым.")
    width = math.ceil(degree / 8)
    return seed.to_bytes(width, byteorder="big")


class LFSR:
    """Fibonacci LFSR.

    Тапы берутся из всех членов полинома, кроме старшей степени.
    Для x^n + x^a + ... + 1 feedback = bit[a] xor ... xor bit[0].
    """

    def __init__(self, polynomial: str, seed: int):
        degree, taps = parse_polynomial(polynomial)
        if seed <= 0:
            raise ValueError("Начальное состояние не должно быть нулевым.")
        self.polynomial = polynomial
        self.degree = degree
        self.taps = taps
        self.mask = (1 << degree) - 1
        self.initial_state = seed & self.mask
        if self.initial_state == 0:
            raise ValueError("Начальное состояние не должно быть нулевым.")
        self.state = self.initial_state

    def clone(self) -> "LFSR":
        return LFSR(self.polynomial, self.state)

    def reset(self) -> None:
        self.state = self.initial_state

    def step(self) -> int:
        # Сначала выдаём младший бит, затем сдвигаем регистр и подаём feedback
        out_bit = self.state & 1
        feedback = 0
        for tap in self.taps:
            feedback ^= (self.state >> tap) & 1
        self.state = (self.state >> 1) | (feedback << (self.degree - 1))
        return out_bit

    def generate_bits(self, count: int) -> list[int]:
        return [self.step() for _ in range(count)]

    def generate_bytes(self, count: int) -> bytes:
        data = bytearray()
        for _ in range(count):
            b = 0
            for _ in range(8):
                b = (b << 1) | self.step()
            data.append(b)
        return bytes(data)

    def period(self, max_steps: int | None = None) -> int:
        # Период равен длине цикла до повторения состояния регистра
        probe = self.clone()
        seen: dict[int, int] = {}
        steps = 0
        while True:
            if probe.state in seen:
                return steps - seen[probe.state]
            if max_steps is not None and steps >= max_steps:
                return steps
            seen[probe.state] = steps
            probe.step()
            steps += 1


def scrambler_xor(data: bytes, polynomial: str, seed: int) -> bytes:
    lfsr = LFSR(polynomial, seed)
    gamma = lfsr.generate_bytes(len(data))
    return bytes(a ^ b for a, b in zip(data, gamma))
