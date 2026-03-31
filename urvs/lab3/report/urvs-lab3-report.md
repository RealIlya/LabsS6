# Лабораторная работа № 3

по дисциплине «Управление ресурсами в ОС UNIX»

**Вариант 2**

## 1. Цель работы

Практическое освоение механизма синхронизации процессов и их взаимодействия посредством программных каналов.

Задание варианта 2: исходный процесс создает программный канал `K1` и порождает процессы `P1` и `P2`; каждый процесс готовит данные и последовательно передает их в `K1`; основной процесс читает и печатает полученные данные, информируя о стадиях работы.

## 2. Анализ задачи

1. Принять параметр `records_per_child` или выбрать значение по умолчанию.
2. Создать канал данных `K1` и вспомогательный канал синхронизации.
3. Породить процесс `P1`, который начинает передачу первым.
4. Породить процесс `P2`, который ждет разрешающего сигнала от `P1`.
5. Процессы `P1` и `P2` формируют текстовые записи и записывают их в `K1`.
6. Родительский процесс `P0` читает строки из `K1` и выводит их на экран.
7. После завершения дочерних процессов `P0` выполняет `waitpid` и завершает работу.

## 3. Используемые функции

<p align="right">Таблица 1</p>

| Функция | Назначение |
|:--|:--|
| `pipe()` | Создание каналов обмена данными и синхронизации. |
| `fork()` | Создание процессов `P1` и `P2`. |
| `fdopen()` | Связывание файлового потока с дескриптором канала. |
| `read()`, `write()` | Передача байтов и сигнала синхронизации через канал. |
| `fgets()` | Чтение строк родительским процессом из `K1`. |
| `waitpid()` | Ожидание завершения дочерних процессов. |
| `kill()` | Аварийное завершение потомков при ошибке в родителе. |
| `nanosleep()` | Короткая задержка между отправками записей. |

## 4. Формат вывода результата

```text
P0(pid=...): create K1 and sync channel
P1(pid=...): start sending
P0: P1 pid=... index=... value=...
...
P2(pid=...): finish sending
P0(pid=...): all children finished
```

## 5. Программа на языке Си

**Спецификация**

- Рабочая директория: `C:\Users\Admin\Desktop\LabsS6\urvs\lab3`.
- Исходный файл: `c/main.c`.
- Платформа выполнения: `Linux/WSL`.
- Сборка: `gcc ./c/main.c -Wall -Wextra -Werror -std=c11 -o ./c/lab3.out`.
- Запуск: `./c/lab3.out [records_per_child]`.

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

#define DEFAULT_RECORDS 5
#define LINE_SIZE 256
#define WRITE_DELAY_MS 70
#define VALUE_LIMIT 1000
#define SYNC_TOKEN '1'

static void sleep_ms(long ms) {
    struct timespec ts;
    ts.tv_sec = ms / 1000;
    ts.tv_nsec = (ms % 1000) * 1000000L;
    nanosleep(&ts, NULL);
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

static void child_work(int child_no, int write_fd, int records_per_child, int sync_read_fd, int sync_write_fd) {
    int i;
    FILE *pipe_out = fdopen(write_fd, "w");
    unsigned int seed;

    if (!pipe_out) {
        fprintf(stderr, "error: child %d cannot open pipe stream: %s\n", child_no, strerror(errno));
        _exit(1);
    }

    seed = (unsigned int)time(NULL) + (unsigned int)child_no;
    srand(seed);

    if (sync_read_fd >= 0) {
        char token;
        if (read(sync_read_fd, &token, 1) != 1) {
            fprintf(stderr, "error: child %d cannot read sync token\n", child_no);
            fclose(pipe_out);
            _exit(1);
        }
        close(sync_read_fd);
    }

    printf("P%d(pid=%ld): start sending\n", child_no, (long)getpid());
    fflush(stdout);

    for (i = 0; i < records_per_child; i++) {
        int value = rand() % VALUE_LIMIT;
        fprintf(pipe_out, "P%d pid=%ld index=%d value=%d\n", child_no, (long)getpid(), i + 1, value);
        fflush(pipe_out);
        sleep_ms(WRITE_DELAY_MS);
    }

    if (sync_write_fd >= 0) {
        char token = SYNC_TOKEN;
        if (write(sync_write_fd, &token, 1) != 1) {
            fprintf(stderr, "error: child %d cannot write sync token\n", child_no);
            fclose(pipe_out);
            _exit(1);
        }
        close(sync_write_fd);
    }

    printf("P%d(pid=%ld): finish sending\n", child_no, (long)getpid());
    fflush(stdout);
    fclose(pipe_out);
    _exit(0);
}

int main(int argc, char **argv) {
    int records_per_child = DEFAULT_RECORDS;
    int k1[2];
    int ksync[2];
    pid_t p1;
    pid_t p2;

    if (argc > 2) {
        fprintf(stderr, "usage: %s [records_per_child]\n", argv[0]);
        return 1;
    }

    if (argc == 2 && parse_positive_int(argv[1], &records_per_child) != 0) {
        fprintf(stderr, "error: records_per_child must be a positive integer\n");
        return 1;
    }

    if (pipe(k1) != 0 || pipe(ksync) != 0) {
        fprintf(stderr, "error: pipe failed: %s\n", strerror(errno));
        return 1;
    }

    printf("P0(pid=%ld): create K1 and sync channel\n", (long)getpid());
    fflush(stdout);

    p1 = fork();
    if (p1 < 0) {
        fprintf(stderr, "error: fork for P1 failed: %s\n", strerror(errno));
        return 1;
    }

    if (p1 == 0) {
        close(k1[0]);
        close(ksync[0]);
        child_work(1, k1[1], records_per_child, -1, ksync[1]);
    }

    p2 = fork();
    if (p2 < 0) {
        fprintf(stderr, "error: fork for P2 failed: %s\n", strerror(errno));
        kill(p1, SIGTERM);
        waitpid(p1, NULL, 0);
        return 1;
    }

    if (p2 == 0) {
        close(k1[0]);
        close(ksync[1]);
        child_work(2, k1[1], records_per_child, ksync[0], -1);
    }

    close(k1[1]);
    close(ksync[0]);
    close(ksync[1]);

    {
        FILE *pipe_in = fdopen(k1[0], "r");
        char line[LINE_SIZE];

        if (!pipe_in) {
            fprintf(stderr, "error: P0 cannot open pipe stream: %s\n", strerror(errno));
            kill(p1, SIGTERM);
            kill(p2, SIGTERM);
            waitpid(p1, NULL, 0);
            waitpid(p2, NULL, 0);
            close(k1[0]);
            return 1;
        }

        printf("P0(pid=%ld): reading data from K1\n", (long)getpid());
        fflush(stdout);

        while (fgets(line, sizeof(line), pipe_in) != NULL) {
            printf("P0: %s", line);
            fflush(stdout);
        }

        fclose(pipe_in);
    }

    if (waitpid(p1, NULL, 0) < 0 || waitpid(p2, NULL, 0) < 0) {
        fprintf(stderr, "error: waitpid failed: %s\n", strerror(errno));
        return 1;
    }

    printf("P0(pid=%ld): all children finished\n", (long)getpid());
    return 0;
}
```

## 6. Тесты

<p align="right">Таблица 2</p>

| № | Назначение | Ожидаемый результат | Фактический результат |
|:--:|:--|:--|:--|
| 1 | Позитивный запуск: `./c/lab3.out 3` | Создание `P1/P2`, последовательная передача строк в `K1`, завершение `P0` | Соответствует ожидаемому |
| 2 | Позитивный запуск с параметром по умолчанию: `./c/lab3.out` | Корректный обмен данными и завершение | Соответствует ожидаемому |
| 3 | Негативный запуск: `./c/lab3.out 0` | Сообщение об ошибке и код выхода 1 | `error: records_per_child must be a positive integer`, код 1 |
| 4 | Негативный запуск: `./c/lab3.out 2 7` | Сообщение `usage` и код выхода 1 | `usage: ./c/lab3.out [records_per_child]`, код 1 |
