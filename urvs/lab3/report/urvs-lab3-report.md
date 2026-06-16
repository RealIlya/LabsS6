# Лабораторная работа № 3

по дисциплине «Управление ресурсами в ОС UNIX»

**Вариант 2**

## 1. Цель работы

Практическое освоение механизма взаимодействия процессов посредством сигналов и программных каналов.

Задание варианта 2: исходный процесс создает программный канал `K1` и порождает два процесса `P1` и `P2`, каждый из которых готовит данные для обработки их основным процессом. Подготовленные данные последовательно помещаются процессами-сыновьями в программный канал и передаются основному процессу. Посредством выдачи сообщений необходимо информировать обо всех этапах работы программы.

## 2. Анализ задачи

1. Создать программный канал `K1` для передачи данных от дочерних процессов родителю.
2. Породить процессы `P1` и `P2`.
3. Организовать ожидание сигнала процессом `P1`, чтобы соблюсти порядок передачи данных, заданный схемой варианта.
4. Передать в канал две структуры фиксированного размера: номер процесса (`4` байта) и сообщение процесса (`40` байт).
5. Обеспечить чтение структур из `K1` родительским процессом `P0` и печать полученной информации.
6. Выдавать сообщения о создании процессов, записи данных в канал, передаче сигнала и завершении работы.
7. Дождаться завершения дочерних процессов через `waitpid()`.

## 3. Используемые функции

<p align="right">Таблица 1</p>

| Функция                         | Назначение                                                  |
| :------------------------------ | :---------------------------------------------------------- |
| `pipe()`                        | Создание программного канала `K1`.                          |
| `fork()`                        | Порождение процессов `P1` и `P2`.                           |
| `sigaction()`                   | Установка обработчика сигнала в `P1`.                       |
| `sigprocmask()`, `sigsuspend()` | Управление маской сигнала и ожидание события синхронизации. |
| `kill()`                        | Посылка сигнала процессом `P2` процессу `P1`.               |
| `read()`, `write()`             | Передача структур данных через канал `K1`.                  |
| `waitpid()`                     | Ожидание завершения дочерних процессов.                     |
| `strtol()`                      | Разбор необязательного номера сигнала из командной строки.  |

## 4. Формат вывода результата

```text
P0(pid=...): created K1
P0(pid=...): created P1(pid=...)
P1(pid=...): waiting for signal from P2
P0(pid=...): created P2(pid=...)
P2(pid=...): writing data to K1
P2(pid=...): sending signal to P1(pid=...)
P0: process=2 message=Message from process P2
P1(pid=...): writing data to K1
P0: process=1 message=Message from process P1
P0(pid=...): all children finished
```

## 5. Программа на языке Си

**Спецификация**

- Рабочая директория: `C:\Users\Admin\Desktop\LabsS6\urvs\lab3`.
- Исходный файл: `c/main.c`.
- Платформа выполнения: `Linux/WSL`.
- Сборка: `gcc ./c/main.c -Wall -Wextra -Werror -std=c11 -o ./c/lab3.out`.
- Запуск: `./c/lab3.out [signal_number]`.

**Исходный текст**

```c
#define _POSIX_C_SOURCE 200809L

#include <errno.h>
#include <signal.h>
#include <stdint.h>
#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include <sys/types.h>
#include <sys/wait.h>
#include <unistd.h>

#define MESSAGE_SIZE 40

struct payload {
    int32_t process_no;
    char message[MESSAGE_SIZE];
};

_Static_assert(sizeof(struct payload) == 44, "payload must occupy exactly 44 bytes");

static volatile sig_atomic_t p1_ready = 0;
static int sync_signal = SIGUSR1;

static void on_sync_signal(int signo) {
    (void)signo;
    p1_ready = 1;
}

static int write_all(int fd, const void *buffer, size_t size) {
    const char *ptr = (const char *)buffer;
    size_t written = 0;

    while (written < size) {
        ssize_t rc = write(fd, ptr + written, size - written);
        if (rc < 0) {
            if (errno == EINTR) {
                continue;
            }
            return -1;
        }
        written += (size_t)rc;
    }

    return 0;
}

static int read_all(int fd, void *buffer, size_t size) {
    char *ptr = (char *)buffer;
    size_t received = 0;

    while (received < size) {
        ssize_t rc = read(fd, ptr + received, size - received);
        if (rc == 0) {
            if (received == 0) {
                return 0;
            }
            errno = EPIPE;
            return -1;
        }
        if (rc < 0) {
            if (errno == EINTR) {
                continue;
            }
            return -1;
        }
        received += (size_t)rc;
    }

    return 1;
}

static void make_payload(struct payload *data, int32_t process_no, const char *message) {
    int written;

    memset(data, 0, sizeof(*data));
    data->process_no = process_no;
    written = snprintf(data->message, sizeof(data->message), "%s", message);
    if (written < 0 || (size_t)written >= sizeof(data->message)) {
        errno = EOVERFLOW;
        fprintf(stderr, "error: payload message is too long\n");
        _exit(1);
    }
}

static void child_p1(int write_fd) {
    struct payload data;
    struct sigaction sa;
    sigset_t blocked_set;
    sigset_t empty_set;

    memset(&sa, 0, sizeof(sa));
    sa.sa_handler = on_sync_signal;
    sigemptyset(&sa.sa_mask);

    if (sigaction(sync_signal, &sa, NULL) != 0) {
        fprintf(stderr, "error: P1 cannot install signal handler: %s\n", strerror(errno));
        _exit(1);
    }

    sigemptyset(&blocked_set);
    sigemptyset(&empty_set);
    sigaddset(&blocked_set, sync_signal);

    if (sigprocmask(SIG_BLOCK, &blocked_set, NULL) != 0) {
        fprintf(stderr, "error: P1 cannot block sync signal: %s\n", strerror(errno));
        close(write_fd);
        _exit(1);
    }

    /* Сигнал остается заблокированным до начала sigsuspend(), поэтому событие не теряется. */
    printf("P1(pid=%ld): waiting for signal from P2\n", (long)getpid());
    fflush(stdout);

    while (!p1_ready) {
        sigsuspend(&empty_set);
    }

    if (sigprocmask(SIG_UNBLOCK, &blocked_set, NULL) != 0) {
        fprintf(stderr, "error: P1 cannot unblock sync signal: %s\n", strerror(errno));
        close(write_fd);
        _exit(1);
    }

    make_payload(&data, 1, "Message from process P1");

    printf("P1(pid=%ld): writing data to K1\n", (long)getpid());
    fflush(stdout);

    if (write_all(write_fd, &data, sizeof(data)) != 0) {
        fprintf(stderr, "error: P1 cannot write to K1: %s\n", strerror(errno));
        close(write_fd);
        _exit(1);
    }

    printf("P1(pid=%ld): finished sending data\n", (long)getpid());
    fflush(stdout);

    close(write_fd);
    _exit(0);
}

static void child_p2(int write_fd, pid_t p1_pid) {
    struct payload data;

    make_payload(&data, 2, "Message from process P2");

    printf("P2(pid=%ld): writing data to K1\n", (long)getpid());
    fflush(stdout);

    if (write_all(write_fd, &data, sizeof(data)) != 0) {
        fprintf(stderr, "error: P2 cannot write to K1: %s\n", strerror(errno));
        close(write_fd);
        _exit(1);
    }

    printf("P2(pid=%ld): sending signal to P1(pid=%ld)\n", (long)getpid(), (long)p1_pid);
    fflush(stdout);

    if (kill(p1_pid, sync_signal) != 0) {
        fprintf(stderr, "error: P2 cannot send signal to P1: %s\n", strerror(errno));
        close(write_fd);
        _exit(1);
    }

    printf("P2(pid=%ld): finished sending data\n", (long)getpid());
    fflush(stdout);

    close(write_fd);
    _exit(0);
}

int main(int argc, char **argv) {
    int k1[2];
    pid_t p1;
    pid_t p2;
    int child_status;
    sigset_t blocked_set;
    sigset_t validation_set;
    char *end;
    long signal_value;

    if (argc > 2) {
        fprintf(stderr, "usage: %s [signal_number]\n", argv[0]);
        return 1;
    }

    if (argc == 2) {
        errno = 0;
        signal_value = strtol(argv[1], &end, 10);
        sigemptyset(&validation_set);
        if (*end != '\0' || errno != 0 || signal_value <= 0 ||
            signal_value == SIGKILL || signal_value == SIGSTOP ||
            sigaddset(&validation_set, (int)signal_value) != 0) {
            fprintf(stderr, "error: signal_number must be a valid catchable signal\n");
            return 1;
        }
        sync_signal = (int)signal_value;
    }

    sigemptyset(&blocked_set);
    sigaddset(&blocked_set, sync_signal);

    if (sigprocmask(SIG_BLOCK, &blocked_set, NULL) != 0) {
        fprintf(stderr, "error: cannot block sync signal in P0: %s\n", strerror(errno));
        return 1;
    }

    if (pipe(k1) != 0) {
        fprintf(stderr, "error: pipe failed: %s\n", strerror(errno));
        return 1;
    }

    printf("P0(pid=%ld): created K1\n", (long)getpid());
    fflush(stdout);

    p1 = fork();
    if (p1 < 0) {
        fprintf(stderr, "error: fork for P1 failed: %s\n", strerror(errno));
        close(k1[0]);
        close(k1[1]);
        return 1;
    }

    if (p1 == 0) {
        close(k1[0]);
        child_p1(k1[1]);
    }

    printf("P0(pid=%ld): created P1(pid=%ld)\n", (long)getpid(), (long)p1);
    fflush(stdout);

    p2 = fork();
    if (p2 < 0) {
        fprintf(stderr, "error: fork for P2 failed: %s\n", strerror(errno));
        kill(p1, SIGTERM);
        waitpid(p1, NULL, 0);
        close(k1[0]);
        close(k1[1]);
        return 1;
    }

    if (p2 == 0) {
        close(k1[0]);
        child_p2(k1[1], p1);
    }

    printf("P0(pid=%ld): created P2(pid=%ld)\n", (long)getpid(), (long)p2);
    printf("P0(pid=%ld): reading data from K1\n", (long)getpid());
    fflush(stdout);

    if (sigprocmask(SIG_UNBLOCK, &blocked_set, NULL) != 0) {
        fprintf(stderr, "error: cannot unblock sync signal in P0: %s\n", strerror(errno));
        kill(p1, SIGTERM);
        kill(p2, SIGTERM);
        waitpid(p1, NULL, 0);
        waitpid(p2, NULL, 0);
        close(k1[0]);
        close(k1[1]);
        return 1;
    }

    close(k1[1]);

    for (;;) {
        struct payload data;
        int rc = read_all(k1[0], &data, sizeof(data));

        if (rc == 0) {
            break;
        }
        if (rc < 0) {
            fprintf(stderr, "error: P0 cannot read from K1: %s\n", strerror(errno));
            close(k1[0]);
            kill(p1, SIGTERM);
            kill(p2, SIGTERM);
            waitpid(p1, NULL, 0);
            waitpid(p2, NULL, 0);
            return 1;
        }

        printf("P0: process=%d message=%s\n", (int)data.process_no, data.message);
        fflush(stdout);
    }

    close(k1[0]);

    if (waitpid(p1, &child_status, 0) < 0) {
        fprintf(stderr, "error: waitpid for P1 failed: %s\n", strerror(errno));
        return 1;
    }
    if (!WIFEXITED(child_status) || WEXITSTATUS(child_status) != 0) {
        fprintf(stderr, "error: P1 finished abnormally\n");
        return 1;
    }

    if (waitpid(p2, &child_status, 0) < 0) {
        fprintf(stderr, "error: waitpid for P2 failed: %s\n", strerror(errno));
        return 1;
    }
    if (!WIFEXITED(child_status) || WEXITSTATUS(child_status) != 0) {
        fprintf(stderr, "error: P2 finished abnormally\n");
        return 1;
    }

    printf("P0(pid=%ld): all children finished\n", (long)getpid());
    return 0;
}
```

## 6. Тесты

<p align="right">Таблица 2</p>

| Тест                                             | Команда            | Ожидаемый результат                                                                                                                 |
| :----------------------------------------------- | :----------------- | :---------------------------------------------------------------------------------------------------------------------------------- |
| Позитивный запуск по умолчанию                   | `./c/lab3.out`     | Сообщение от `P2` попадает в канал раньше сообщения от `P1`, после чего `P0` печатает обе структуры и завершает работу с кодом `0`. |
| Позитивный запуск с явным номером сигнала        | `./c/lab3.out 10`  | Используется переданный перехватываемый сигнал, обмен через `K1` остается корректным, код `0`.                                      |
| Негативный запуск с недопустимым номером сигнала | `./c/lab3.out 0`   | Сообщение об ошибке `signal_number must be a valid catchable signal`, код `1`.                                                      |
| Негативный запуск с лишними аргументами          | `./c/lab3.out 2 7` | Сообщение `usage`, код `1`.                                                                                                         |
