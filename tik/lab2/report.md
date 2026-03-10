# Лабораторная работа № 2
## Гаммирование. Моделирование работы скремблера

## Цель работы
Освоить практическое применение режима однократного гаммирования, исследовать побитное непрерывное шифрование данных и изучить шифрование с использованием скремблера.

## Задание
Вариант: 2.

Скремблеры варианта:
- $x^9 + x^3 + 1$;
- $x^9 + x^4 + 1$.

Требовалось:
1. Реализовать приложение для шифрования:
   - однократное гаммирование: входной текст хранится в файле, ключ генерируется случайно, шифротекст и ключ сохраняются в отдельные файлы;
   - шифрование каждым скремблером варианта: входной текст и начальное состояние задаются отдельно, результат сохраняется в файл;
   - просмотр и изменение данных в двоичном, шестнадцатеричном и символьном представлениях.
2. Реализовать приложение для дешифрования:
   - однократное гаммирование: шифротекст и ключ считываются из файлов, результат сохраняется в файл;
   - дешифрование каждым скремблером варианта: шифротекст и начальное состояние задаются отдельно, результат сохраняется в файл;
   - просмотр и изменение данных в двоичном, шестнадцатеричном и символьном представлениях.
3. Реализовать исследование ПСП скремблеров:
   - получение периода;
   - проверку равномерности по критерию $\chi^2$;
   - исследование свойств сбалансированности, цикличности и корреляции.

## Описание метода решения заданий
Для режима однократного гаммирования использованы соотношения:

$$
C_i = P_i \oplus K_i,\qquad P_i = C_i \oplus K_i,\qquad K_i = C_i \oplus P_i.
$$

Шифрование и дешифрование выполняются одной и той же XOR-операцией. Абсолютная стойкость достигается только при случайном ключе длины сообщения и его однократном использовании.

Для скремблера использован линейный сдвиговый регистр с обратной связью (LFSR). Полином задаёт схему обратной связи, начальное состояние задаёт старт регистра, а генерируемая гамма XOR-ится с данными. Для варианта 2 исследуются два полинома: $x^9 + x^3 + 1$ и $x^9 + x^4 + 1$.

## Описание разработанного программного средства
Реализовано desktop-приложение на Python с графическим интерфейсом Tkinter.

Архитектура разделена на слои:
- `src/core` — алгоритмы OTP, LFSR, анализ ПСП и преобразования представлений;
- `src/ui` — окна, вкладки и переиспользуемые виджеты интерфейса;
- `src/io_utils.py` — файловый ввод и вывод;
- `tests` — проверки core-логики.

Приложение содержит три вкладки:
1. `Шифрование`.
2. `Дешифрование`.
3. `Исследование ПСП`.

## Описание ключевых алгоритмов
1. **Преобразование представлений**  
   В модуле `src/core/encoding.py` реализованы взаимные преобразования между `bytes`, текстом, шестнадцатеричным и двоичным представлениями.

2. **Однократное гаммирование**  
   В модуле `src/core/crypto.py` реализованы генерация случайного ключа и XOR-преобразование данных.

3. **LFSR и скремблер**  
   В модуле `src/core/lfsr.py` реализованы разбор полинома, генерация битовой последовательности, преобразование начального состояния и наложение гаммы на данные.

4. **Исследование ПСП**  
   В модуле `src/core/analysis.py` вычисляются:
   - период;
   - количество нулей и единиц;
   - значение критерия $\chi^2$;
   - модуль разности между количеством нулей и единиц;
   - уникальность циклических сдвигов;
   - максимальная по модулю автокорреляция.

## Описание проведённых исследований
Цель исследования: сравнить свойства последовательностей, генерируемых двумя скремблерами варианта 2.

Для анализа использовались разные ненулевые начальные состояния.

Полученные результаты показывают:
- для полинома $x^9 + x^3 + 1$ период существенно меньше максимального и качество последовательности заметно зависит от начального состояния;
- для полинома $x^9 + x^4 + 1$ период достигает значения $2^9 - 1 = 511$, а статистические показатели существенно лучше и стабильнее.

Следовательно, во втором случае генератор формирует более качественную ПСП для учебного потокового шифрования.

По результатам исследования можно сформулировать следующие выводы:
1. Исследование ПСП необходимо проводить отдельно для каждого скремблера варианта.
2. Для анализа качества ПСП достаточно использовать показатели периода, $\chi^2$, сбалансированности, цикличности и корреляции.
3. Полином $x^9 + x^4 + 1$ даёт последовательность существенно лучшего качества, так как обеспечивает максимальный период и более равномерные статистические свойства.
4. Полином $x^9 + x^3 + 1$ формирует ПСП худшего качества: период мал, а свойства последовательности заметно слабее.
5. Начальное состояние влияет на конкретную фазу последовательности и на вид первых бит, но агрегированные статистические характеристики могут совпадать у последовательностей, отличающихся циклическим сдвигом.

## Тестирование программы
1. **Проверка OTP на реальных файлах**  
   Использованы:
   - `data/in/open.txt`;
   - сформированные файлы `data/out/otp_key.bin` и `data/out/otp_cipher.bin`.

   Получено:
   - открытый текст: `48454C4C4F204C414232204F545020414E4420534352414D424C45520D0A`;
   - ключ: `522FCCB273F8D8F448D3957ED3EEF05F7492511BDBDDE28DDD1BA4B780D6`;
   - шифротекст: `1A6A80FE3CD894B50AE1B53187BED01E3AD67148988FA3C09F57E1E58DDC`.

   Файл `data/out/otp_plain_restored.txt` совпадает с исходным `data/in/open.txt`, что подтверждает корректность шифрования и дешифрования.

2. **Проверка скремблера на реальных файлах**  
   Использованы:
   - `data/in/open.txt`;
   - `data/in/seed.bin = 0155`;
   - полином $x^9 + x^3 + 1$.

   Получено:
   - фактический seed: `341`;
   - шифротекст: `E2BBC91BBB0AF3E017CF2AE0BC055F03E5BE350C93F8BFC815B86FEDAC5F`;
   - восстановленный файл `data/out/scrambler1_plain_restored.txt` совпадает с `data/in/open.txt`.

3. **Проверка ПСП на реальных входных данных**  
   Для `seed = 341` получены:
   - для $x^9 + x^3 + 1$: период `21`, $\chi^2 = 0.4286`, нули/единицы `9/12`, максимальная по модулю автокорреляция `0.6190`;
   - для $x^9 + x^4 + 1$: период `511`, $\chi^2 = 0.0020`, нули/единицы `255/256`, максимальная по модулю автокорреляция `0.0020`.

   Эти результаты подтверждают, что второй скремблер формирует существенно более качественную ПСП.

4. **Проверка аналитического свойства повторного использования ключа**  
   Для двух сообщений, зашифрованных одним и тем же ключом:

$$
C_1 \oplus C_2 = P_1 \oplus P_2,\qquad
P_2 = P_1 \oplus C_1 \oplus C_2.
$$

На тестовом примере по известным $P_1$, $C_1$, $C_2$ корректно восстанавливается второе сообщение.

5. **Проверка интерфейсных сценариев**  
   Проверены:
   - загрузка данных из файлов;
   - сохранение результата в файл;
   - редактирование представлений `SYM/HEX/BIN`;
   - исследование ПСП для выбранного скремблера и для обоих скремблеров варианта.

6. **Проверка второго скремблера на реальных файлах**  
   Использованы:
   - `data/in/open.txt`;
   - `data/in/seed.bin = 0155`;
   - файл `data/out/scrambler2_cipher.bin`.

   Получено:
   - восстановленный файл `data/out/scrambler2_plain_restored.txt` совпадает с `data/in/open.txt`;
   - это подтверждает корректность обратимого преобразования и для полинома $x^9 + x^4 + 1$.

7. **Проверка разных seed-файлов для одного скремблера**  
   Использованы файлы:
   - `data/in/seed_44.bin = 44`;
   - `data/in/seed_4422.bin = 44 22`;
   - полином $x^9 + x^3 + 1$.

   Получено:
   - для `seed_44.bin`: фактический seed `68`, первые биты ПСП `001000100001100101101`, файл `data/out/scrambler1_seed44_cipher.bin`;
   - для `seed_4422.bin`: фактический seed `34`, первые биты ПСП `010001000011001011010`, файл `data/out/scrambler1_seed4422_cipher.bin`.

   При этом агрегированные характеристики ПСП совпадают, но шифротексты различаются. Это подтверждает, что разные начальные состояния могут задавать разные фазы одной и той же циклической последовательности.

## Вывод
В работе реализовано программное средство для шифрования и дешифрования в режиме однократного гаммирования и при помощи двух скремблеров варианта 2.

Приложение обеспечивает файловый ввод и вывод, просмотр и изменение данных в представлениях `SYM/HEX/BIN`, а также исследование ПСП по требуемым критериям: период, $\chi^2$, сбалансированность, цикличность и корреляция.

По результатам экспериментов скремблер с полиномом $x^9 + x^4 + 1$ показывает существенно лучшие статистические свойства по сравнению со скремблером $x^9 + x^3 + 1$, что делает его предпочтительным для данной лабораторной работы.

## Код программы

### `src/core/analysis.py`
```python
from __future__ import annotations

from dataclasses import dataclass

from core.lfsr import LFSR


@dataclass
class SequenceStats:
    degree: int
    effective_seed: int
    analyzed_bits: int
    period: int
    zeros: int
    ones: int
    chi_square: float
    balanced_delta: int
    cyclic_shifts_unique: bool
    max_abs_autocorrelation: float
    preview_bits: str


def _chi_square_for_bits(bits: list[int]) -> float:
    # Здесь сравниваем число нулей и единиц с ожидаемым распределением 50/50
    n = len(bits)
    if n == 0:
        return 0.0
    ones = sum(bits)
    zeros = n - ones
    expected = n / 2
    return ((zeros - expected) ** 2) / expected + ((ones - expected) ** 2) / expected


def _max_abs_autocorrelation(bits: list[int], max_lag: int | None = None) -> float:
    # Нулевой сдвиг не учитываем, ищем наибольшее сходство со сдвинутыми копиями
    n = len(bits)
    if n < 2:
        return 0.0
    bipolar = [1 if b else -1 for b in bits]
    if max_lag is None:
        max_lag = min(n - 1, 128)
    max_corr = 0.0
    for lag in range(1, max_lag + 1):
        total = 0
        for i in range(n):
            total += bipolar[i] * bipolar[(i + lag) % n]
        corr = total / n
        max_corr = max(max_corr, abs(corr))
    return max_corr


def _cyclic_shifts_unique(bits: list[int]) -> bool:
    n = len(bits)
    if n == 0:
        return False
    rotations = set()
    for shift in range(n):
        key = tuple(bits[shift:] + bits[:shift])
        rotations.add(key)
    return len(rotations) == n


def analyze_lfsr(polynomial: str, seed: int) -> SequenceStats:
    # Метрики считаются на одном полном периоде, чтобы честно сравнивать разные фазы
    lfsr = LFSR(polynomial, seed)
    period = lfsr.period(max_steps=(1 << lfsr.degree))

    seq_lfsr = LFSR(polynomial, seed)
    analyzed_bits = max(period, 1)
    bits = seq_lfsr.generate_bits(analyzed_bits)
    preview_bits = "".join(str(bit) for bit in bits[: min(32, len(bits))])

    ones = sum(bits)
    zeros = len(bits) - ones
    chi_square = _chi_square_for_bits(bits)
    delta = abs(ones - zeros)

    period_bits = bits[:period] if period > 0 else bits
    cyclic_unique = _cyclic_shifts_unique(period_bits)
    max_corr = _max_abs_autocorrelation(period_bits)

    return SequenceStats(
        degree=lfsr.degree,
        effective_seed=lfsr.initial_state,
        analyzed_bits=len(bits),
        period=period,
        zeros=zeros,
        ones=ones,
        chi_square=chi_square,
        balanced_delta=delta,
        cyclic_shifts_unique=cyclic_unique,
        max_abs_autocorrelation=max_corr,
        preview_bits=preview_bits,
    )


def build_verdict(stats: SequenceStats) -> str:
    # Пороговые значения эвристические и нужны только для читаемого вывода
    max_period = (1 << stats.degree) - 1
    period_ratio = stats.period / max_period if max_period > 0 else 0.0
    balance_ratio = stats.balanced_delta / stats.analyzed_bits if stats.analyzed_bits > 0 else 1.0
    chi_ok = stats.chi_square <= 3.84
    corr = stats.max_abs_autocorrelation

    findings: list[tuple[str, str]] = []
    score = 0

    if period_ratio >= 0.95:
        findings.append(("Период", f"хороший: {stats.period} из {max_period}"))
        score += 2
    elif period_ratio >= 0.5:
        findings.append(("Период", f"средний: {stats.period} из {max_period}"))
        score += 1
    else:
        findings.append(("Период", f"слабый: {stats.period} из {max_period}"))

    if balance_ratio <= 0.05:
        findings.append(("Баланс 0/1", f"хороший: разница {stats.balanced_delta} бит"))
        score += 2
    elif balance_ratio <= 0.15:
        findings.append(("Баланс 0/1", f"приемлемый: разница {stats.balanced_delta} бит"))
        score += 1
    else:
        findings.append(("Баланс 0/1", f"слабый: разница {stats.balanced_delta} бит"))

    if chi_ok:
        findings.append(("Равномерность", f"chi^2 = {stats.chi_square:.4f}, явного перекоса не видно"))
        score += 2
    else:
        findings.append(("Равномерность", f"chi^2 = {stats.chi_square:.4f}, распределение заметно перекошено"))

    if stats.cyclic_shifts_unique:
        findings.append(("Цикличность", "сдвиги уникальны, цикл ведет себя корректно"))
        score += 2
    else:
        findings.append(("Цикличность", "есть повторы сдвигов, это плохой признак"))

    if corr <= 0.2:
        findings.append(("Автокорреляция", f"низкая: {corr:.4f}"))
        score += 2
    elif corr <= 0.35:
        findings.append(("Автокорреляция", f"умеренная: {corr:.4f}"))
        score += 1
    else:
        findings.append(("Автокорреляция", f"высокая: {corr:.4f}"))

    if score >= 9:
        overall = "Итог: последовательность хорошего качества для учебного скремблера."
    elif score >= 6:
        overall = "Итог: последовательность приемлемая, но показатели не идеальны."
    else:
        overall = "Итог: последовательность слабая, полином или seed стоит заменить."

    lines = ["Вердикт:"]
    for name, text in findings:
        lines.append(f"- {name}: {text}.")
    lines.append(overall)
    return "\n".join(lines)
```

### `src/core/crypto.py`
```python
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

```

### `src/core/encoding.py`
```python
from __future__ import annotations


def bytes_to_text(data: bytes) -> str:
    """Lossless one-byte to one-char mapping for reversible symbolic view."""
    return data.decode("latin-1")


def text_to_bytes(value: str) -> bytes:
    return value.encode("latin-1")


def bytes_to_hex(data: bytes) -> str:
    return " ".join(f"{b:02X}" for b in data)


def hex_to_bytes(value: str) -> bytes:
    cleaned = value.replace(" ", "").replace("\n", "").replace("\t", "").replace("\r", "")
    if not cleaned:
        return b""
    if len(cleaned) % 2 != 0:
        raise ValueError("HEX-строка должна содержать чётное число символов.")
    try:
        return bytes.fromhex(cleaned)
    except ValueError as exc:
        raise ValueError("Некорректный HEX-формат.") from exc


def bytes_to_bin(data: bytes) -> str:
    return " ".join(f"{b:08b}" for b in data)


def bin_to_bytes(value: str) -> bytes:
    cleaned = value.replace(" ", "").replace("\n", "").replace("\t", "").replace("\r", "")
    if not cleaned:
        return b""
    if any(ch not in "01" for ch in cleaned):
        raise ValueError("BIN-строка может содержать только 0 и 1.")
    if len(cleaned) % 8 != 0:
        raise ValueError("BIN-строка должна содержать число бит, кратное 8.")
    out = bytearray()
    for idx in range(0, len(cleaned), 8):
        out.append(int(cleaned[idx : idx + 8], 2))
    return bytes(out)

```

### `src/core/lfsr.py`
```python
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
```

### `src/io_utils.py`
```python
from __future__ import annotations

from pathlib import Path


def read_bytes(path: str | Path) -> bytes:
    # Файловый ввод-вывод вынесен отдельно, чтобы не смешивать его с core и UI
    return Path(path).read_bytes()


def write_bytes(path: str | Path, data: bytes) -> None:
    Path(path).write_bytes(data)

```

