## 1. Цель работы

Изучить программные средства создания процессов, получить навыки управления и синхронизации процессов, а также простейшие способы обмена данными между процессами.

Задание варианта 2: разработать программу, `n` раз вычисляющую `sin(x)` для случайного `x` из интервала `[-1; 1]` путем разложения в ряд и выводящую значения в файл; предварительно созданный процесс-потомок читает данные из файла и выводит их на экран до получения ключевого слова `STOP`.

## 2. Анализ задачи

1. Принять параметры запуска `n` и `seed` или выбрать значения по умолчанию.
2. Создать временный файл обмена между процессами.
3. Породить процесс-потомок `reader` для чтения данных из файла.
4. В процессе-родителе `n` раз:
   - сгенерировать случайный `x` из `[-1; 1]`;
   - вычислить `sin(x)` через разложение в ряд;
   - записать строку результата в файл.
5. По завершении вычислений записать в файл маркер `STOP`.
6. Дочерний процесс завершает работу при чтении `STOP`.
7. Родитель ожидает завершения потомка и удаляет временный файл.

## 3. Используемые функции

<p align="right">Таблица 1</p>

| Функция                         | Назначение                                                |
| :------------------------------ | :-------------------------------------------------------- |
| `fork()`                        | Порождение дочернего процесса.                            |
| `waitpid()`                     | Ожидание завершения дочернего процесса.                   |
| `fopen()`, `fseek()`, `fgets()` | Работа с временным файлом обмена.                         |
| `strtol()`                      | Преобразование аргументов командной строки в целые числа. |
| `fprintf()`, `fflush()`         | Запись результатов и сообщений об ошибках.                |
| `nanosleep()`                   | Короткая задержка между итерациями polling и записи.      |
| `unlink()`                      | Удаление временного файла после завершения.               |

## 4. Формат вывода результата

```text
reader: step=<номер> x=<значение> sin(x)=<значение>
...
done: n=<число_вычислений> seed=<seed>
```

## 5. Программа на языке Си

**Спецификация**

- Рабочая директория: `C:\Users\Admin\Desktop\LabsS6\urvs\lab2`.
- Исходный файл: `c/main.c`.
- Платформа выполнения: `Linux/WSL`.
- Сборка: `gcc ./c/main.c -Wall -Wextra -Werror -std=c11 -o ./c/lab2.out`.
- Запуск: `./c/lab2.out [n] [seed]`.

**Исходный текст**

```c
#define _POSIX_C_SOURCE 200809L

#include <errno.h>
#include <limits.h>
#include <signal.h>
#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include <sys/types.h>
#include <sys/wait.h>
#include <time.h>
#include <unistd.h>

#define MIN_N 5
#define MAX_N 15
#define SERIES_TERMS 16
#define LINE_SIZE 256
#define READER_DELAY_MS 100
#define WRITER_DELAY_MS 70

static void sleep_ms(long ms) {
    struct timespec ts;
    ts.tv_sec = ms / 1000;
    ts.tv_nsec = (ms % 1000) * 1000000L;
    nanosleep(&ts, NULL);
}

static double sin_series(double x, int terms) {
    double term = x;
    double sum = x;
    int k;

    for (k = 1; k < terms; k++) {
        term *= -x * x / ((2.0 * k) * (2.0 * k + 1.0));
        sum += term;
    }
    return sum;
}

static int parse_positive_int(const char *s, int *out) {
    char *end;
    long value;

    errno = 0;
    value = strtol(s, &end, 10);
    if (*end != '\0' || value <= 0 || value > INT_MAX || errno == ERANGE) {
        return 1;
    }
    *out = (int)value;
    return 0;
}

static void child_reader(const char *tmp_path) {
    long pos = 0;
    int done = 0;

    while (!done) {
        FILE *f = fopen(tmp_path, "r");
        if (!f) {
            fprintf(stderr, "error: cannot open '%s' for read: %s\n", tmp_path, strerror(errno));
            _exit(1);
        }

        if (fseek(f, pos, SEEK_SET) != 0) {
            fprintf(stderr, "error: cannot seek '%s'\n", tmp_path);
            fclose(f);
            _exit(1);
        }

        for (;;) {
            char line[LINE_SIZE];
            if (!fgets(line, sizeof(line), f)) {
                break;
            }
            pos = ftell(f);
            if (strcmp(line, "STOP\n") == 0 || strcmp(line, "STOP") == 0) {
                done = 1;
                break;
            }
            printf("reader: %s", line);
            fflush(stdout);
        }

        fclose(f);
        if (!done) {
            sleep_ms(READER_DELAY_MS);
        }
    }
    _exit(0);
}

int main(int argc, char **argv) {
    int n = 0;
    int i;
    unsigned int seed;
    pid_t child;
    char tmp_path[256];
    FILE *f;

    if (argc > 3) {
        fprintf(stderr, "usage: %s [n] [seed]\n", argv[0]);
        return 1;
    }

    if (argc == 3) {
        int parsed_seed = 0;
        if (parse_positive_int(argv[2], &parsed_seed) != 0) {
            fprintf(stderr, "error: seed must be a positive integer\n");
            return 1;
        }
        seed = (unsigned int)parsed_seed;
    } else {
        seed = (unsigned int)time(NULL);
    }

    srand(seed);

    if (argc >= 2) {
        if (parse_positive_int(argv[1], &n) != 0) {
            fprintf(stderr, "error: n must be a positive integer\n");
            return 1;
        }
    } else {
        n = MIN_N + rand() % (MAX_N - MIN_N + 1);
    }

    snprintf(tmp_path, sizeof(tmp_path), "/tmp/urvs_lab2_var2_%ld.tmp", (long)getpid());

    f = fopen(tmp_path, "w");
    if (!f) {
        fprintf(stderr, "error: cannot create '%s': %s\n", tmp_path, strerror(errno));
        return 1;
    }

    child = fork();
    if (child < 0) {
        fprintf(stderr, "error: fork failed: %s\n", strerror(errno));
        fclose(f);
        unlink(tmp_path);
        return 1;
    }
    if (child == 0) {
        fclose(f);
        child_reader(tmp_path);
    }

    for (i = 0; i < n; i++) {
        double x = ((double)rand() / (double)RAND_MAX) * 2.0 - 1.0;
        double y = sin_series(x, SERIES_TERMS);
        fprintf(f, "step=%d x=%.8f sin(x)=%.12f\n", i + 1, x, y);
        fflush(f);
        sleep_ms(WRITER_DELAY_MS);
    }

    fprintf(f, "STOP\n");
    if (fflush(f) != 0 || ferror(f)) {
        fprintf(stderr, "error: cannot write to '%s'\n", tmp_path);
        fclose(f);
        kill(child, SIGTERM);
        waitpid(child, NULL, 0);
        unlink(tmp_path);
        return 1;
    }
    fclose(f);

    if (waitpid(child, NULL, 0) < 0) {
        fprintf(stderr, "error: waitpid failed: %s\n", strerror(errno));
        unlink(tmp_path);
        return 1;
    }

    if (unlink(tmp_path) != 0) {
        fprintf(stderr, "error: cannot remove '%s': %s\n", tmp_path, strerror(errno));
        return 1;
    }

    printf("done: n=%d seed=%u\n", n, seed);
    return 0;
}
```

## 6. Тесты

<p align="right">Таблица 2</p>

|  №  | Назначение                                       | Ожидаемый результат                                   | Фактический результат                           |
| :-: | :----------------------------------------------- | :---------------------------------------------------- | :---------------------------------------------- |
|  1  | Позитивный запуск: `./c/lab2.out 5 123`          | Печать 5 строк `reader: ...` и завершение `done: ...` | Соответствует ожидаемому                        |
|  2  | Позитивный запуск без параметров: `./c/lab2.out` | Корректный расчет при случайных `n` и `seed`          | Соответствует ожидаемому                        |
|  3  | Негативный запуск: `./c/lab2.out 0`              | Сообщение об ошибке и код выхода 1                    | `error: n must be a positive integer`, код 1    |
|  4  | Негативный запуск: `./c/lab2.out 5 bad`          | Сообщение об ошибке и код выхода 1                    | `error: seed must be a positive integer`, код 1 |
