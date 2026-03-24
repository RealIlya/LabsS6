# Тесты ЛР2 (вариант 2)

## Сборка

```bash
cd /mnt/c/Users/Admin/Desktop/LabsS6/urvs/lab2
gcc ./c/main.c -Wall -Wextra -Werror -std=c11 -o ./c/lab2.out
```

## Тест 1: позитивный запуск с фиксированными параметрами

```bash
./c/lab2.out 5 123
```

Ожидается:

- строки вида `reader: step=... x=... sin(x)=...`;
- итоговая строка `done: n=5 seed=123`;
- код возврата `0`.

## Тест 2: позитивный запуск без параметров

```bash
./c/lab2.out
```

Ожидается:

- случайное число шагов `n` в разумных пределах;
- корректное завершение после записи `STOP`;
- код возврата `0`.

## Тест 3: негативный запуск (`n=0`)

```bash
./c/lab2.out 0
echo $?
```

Ожидается:

- сообщение `error: n must be a positive integer` в `stderr`;
- код возврата `1`.

## Тест 4: негативный запуск (невалидный `seed`)

```bash
./c/lab2.out 5 bad
echo $?
```

Ожидается:

- сообщение `error: seed must be a positive integer` в `stderr`;
- код возврата `1`.
