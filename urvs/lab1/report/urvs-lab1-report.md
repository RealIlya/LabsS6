# Лабораторная работа № 1

по дисциплине «Управление ресурсами в ОС UNIX»

**Вариант 2**

## 1. Цель работы

Ознакомиться с устройством файловой системы ОС UNIX, механизмами ее функционирования, программными средствами для работы с ней (командный язык Shell, язык Си).

Задание варианта 2: программа просматривает каталог, имя которого указано параметром, выводит имена встретившихся каталогов; затем переходит в родительский каталог и повторяет действия до достижения корневого каталога.

## 2. Анализ задачи

1. Проверить корректность входного параметра (наличие ровно одного аргумента и существование каталога).
2. Перейти в начальный каталог.
3. Вывести заголовок уровня: «начальный каталог» для первого шага и «родительский каталог» для следующих.
4. Просмотреть текущий каталог и вывести имена вложенных каталогов, исключая `.` и `..`.
5. Перейти в родительский каталог.
6. Повторять шаги 3-5, пока текущий каталог не станет корневым.

## 3. Используемые функции

<p align="right">Таблица 1</p>

| Функция | Назначение |
|:--|:--|
| `chdir()` / `_chdir()` | Переход в указанный каталог. |
| `getcwd()` / `GetCurrentDirectoryA()` | Получение пути текущего каталога. |
| `opendir()`, `readdir()`, `closedir()` | Обход каталога в POSIX-ветке. |
| `FindFirstFileA()`, `FindNextFileA()` | Обход каталога в Windows-ветке. |
| `fprintf()` | Вывод диагностических сообщений в `stderr`. |

## 4. Формат вывода результата

```text
каталог <имя каталога> начальный каталог
  каталог <имя>
  каталог <имя>
каталог <имя каталога> родительский каталог
  каталог <имя>
  каталог <имя>
...
```

## 5. Программа на языке Shell

**Спецификация**

Программа разработана и протестирована в WSL (`sh`).

Рабочая директория: `C:\Users\Admin\Desktop\LabsS6\urvs\lab1`.

Название файла с программой: `shell/lab1_variant2.sh`.

Инструкция по запуску:

- `sh ./shell/lab1_variant2.sh /path/to/start/dir`

Параметры:

- Первый параметр (обязательный): путь к начальному каталогу.

**Исходный текст**

```sh
#!/bin/sh
set -eu

usage() {
    echo "Usage: $0 <directory>" >&2
}

if [ "$#" -ne 1 ]; then
    usage
    exit 1
fi

if [ ! -d "$1" ]; then
    echo "error: '$1' is not a directory" >&2
    exit 2
fi

current=$(cd "$1" && pwd -P)
first=1

while :; do
    if [ "$first" -eq 1 ]; then
        echo "каталог $current начальный каталог"
    else
        echo "каталог $current родительский каталог"
    fi

    find "$current" -mindepth 1 -maxdepth 1 -type d -printf '%f\n' | LC_ALL=C sort \
    | while IFS= read -r name; do
        [ -n "$name" ] || continue
        echo "  каталог $name"
    done

    parent=$(dirname "$current")
    if [ "$parent" = "$current" ]; then
        break
    fi

    current=$parent
    first=0
done
```

## 6. Программа на языке Си

**Спецификация**

Один исходный файл с препроцессорным ветвлением по ОС: `#ifdef _WIN32 ... #else ... #endif`.

Рабочая директория: `C:\Users\Admin\Desktop\LabsS6\urvs\lab1`.

Название файла с программой: `c/main.c`.

Сборка и запуск:

- Windows (clang): `clang .\c\main.c -Wall -Wextra -Werror -std=c11 -o .\c\lab1.exe` и `./c/lab1.exe <dir>`.
- WSL/Linux (gcc): `gcc ./c/main.c -Wall -Wextra -Werror -std=c11 -o ./c/lab1.out` и `./c/lab1.out <dir>`.

Параметры:

- Первый параметр (обязательный): путь к начальному каталогу.

**Исходный текст**

```c
#ifdef _WIN32
#define _CRT_SECURE_NO_WARNINGS
#endif

#include <errno.h>
#include <locale.h>
#include <stdio.h>
#include <stdlib.h>
#include <string.h>

#ifdef _WIN32
#include <direct.h>
#include <windows.h>
#define CHDIR _chdir
#else
#include <dirent.h>
#include <sys/stat.h>
#include <unistd.h>
#define CHDIR chdir
#endif

#ifdef _WIN32
static char *acp_to_utf8_alloc(const char *src) {
    int wlen;
    int u8len;
    wchar_t *wbuf;
    char *u8buf;

    if (!src) {
        return NULL;
    }

    wlen = MultiByteToWideChar(CP_ACP, 0, src, -1, NULL, 0);
    if (wlen <= 0) {
        return NULL;
    }

    wbuf = (wchar_t *)malloc((size_t)wlen * sizeof(wchar_t));
    if (!wbuf) {
        return NULL;
    }

    if (MultiByteToWideChar(CP_ACP, 0, src, -1, wbuf, wlen) <= 0) {
        free(wbuf);
        return NULL;
    }

    u8len = WideCharToMultiByte(CP_UTF8, 0, wbuf, -1, NULL, 0, NULL, NULL);
    if (u8len <= 0) {
        free(wbuf);
        return NULL;
    }

    u8buf = (char *)malloc((size_t)u8len);
    if (!u8buf) {
        free(wbuf);
        return NULL;
    }

    if (WideCharToMultiByte(CP_UTF8, 0, wbuf, -1, u8buf, u8len, NULL, NULL) <= 0) {
        free(wbuf);
        free(u8buf);
        return NULL;
    }

    free(wbuf);
    return u8buf;
}
#endif

static char *get_cwd_alloc(void) {
#ifdef _WIN32
    DWORD size = GetCurrentDirectoryA(0, NULL);
    char *buf;
    if (size == 0) {
        return NULL;
    }
    buf = (char *)malloc((size_t)size);
    if (!buf) {
        return NULL;
    }
    if (GetCurrentDirectoryA(size, buf) == 0) {
        free(buf);
        return NULL;
    }
    return buf;
#else
    return getcwd(NULL, 0);
#endif
}

static int is_root_path(const char *path) {
#ifdef _WIN32
    size_t len = strlen(path);
    if (len == 3 && path[1] == ':' && (path[2] == '\\' || path[2] == '/')) {
        return 1;
    }
    return 0;
#else
    return strcmp(path, "/") == 0;
#endif
}

static int print_subdirs(void) {
#ifdef _WIN32
    WIN32_FIND_DATAA data;
    HANDLE h = FindFirstFileA("*", &data);
    if (h == INVALID_HANDLE_VALUE) {
        return -1;
    }

    do {
        if (!(data.dwFileAttributes & FILE_ATTRIBUTE_DIRECTORY)) {
            continue;
        }
        if (strcmp(data.cFileName, ".") == 0 || strcmp(data.cFileName, "..") == 0) {
            continue;
        }
        {
            char *name_u8 = acp_to_utf8_alloc(data.cFileName);
            if (name_u8) {
                printf("  каталог %s\\n", name_u8);
                free(name_u8);
            } else {
                printf("  каталог %s\\n", data.cFileName);
            }
        }
    } while (FindNextFileA(h, &data) != 0);

    FindClose(h);
    return 0;
#else
    DIR *d = opendir(".");
    struct dirent *ent;
    if (!d) {
        return -1;
    }

    while ((ent = readdir(d)) != NULL) {
        int is_dir = 0;
#ifdef DT_DIR
        if (ent->d_type == DT_DIR) {
            is_dir = 1;
        } else if (ent->d_type == DT_UNKNOWN)
#endif
        {
            struct stat st;
            if (stat(ent->d_name, &st) == 0 && S_ISDIR(st.st_mode)) {
                is_dir = 1;
            }
        }

        if (!is_dir) {
            continue;
        }
        if (strcmp(ent->d_name, ".") == 0 || strcmp(ent->d_name, "..") == 0) {
            continue;
        }
        printf("  каталог %s\\n", ent->d_name);
    }

    closedir(d);
    return 0;
#endif
}

int main(int argc, char **argv) {
    int first = 1;

#ifdef _WIN32
    SetConsoleOutputCP(CP_UTF8);
    SetConsoleCP(CP_UTF8);
    setlocale(LC_ALL, ".UTF-8");
#endif

    if (argc != 2) {
        fprintf(stderr, "usage: %s <directory>\\n", argv[0]);
        return 1;
    }

    if (CHDIR(argv[1]) != 0) {
        fprintf(stderr, "error: cannot enter '%s': %s\\n", argv[1], strerror(errno));
        return 1;
    }

    while (1) {
        char *cwd = get_cwd_alloc();
        char *cwd_u8 = NULL;
        if (!cwd) {
            fprintf(stderr, "error: cannot get current directory\\n");
            return 1;
        }

#ifdef _WIN32
        cwd_u8 = acp_to_utf8_alloc(cwd);
        printf(
            "каталог %s %s\\n",
            cwd_u8 ? cwd_u8 : cwd,
            first ? "начальный каталог" : "родительский каталог"
        );
#else
        printf("каталог %s %s\\n", cwd, first ? "начальный каталог" : "родительский каталог");
#endif

        if (print_subdirs() != 0) {
            fprintf(stderr, "error: cannot list subdirectories in '%s'\\n", cwd);
            free(cwd_u8);
            free(cwd);
            return 1;
        }

        if (is_root_path(cwd)) {
            free(cwd_u8);
            free(cwd);
            break;
        }

        free(cwd_u8);
        free(cwd);

        if (CHDIR("..") != 0) {
            fprintf(stderr, "error: cannot go to parent directory: %s\\n", strerror(errno));
            return 1;
        }
        first = 0;
    }

    return 0;
}
```

## 7. Тесты

<p align="right">Таблица 2</p>

| № | Назначение | Ожидаемый результат | Результат (Shell) | Результат (C) |
|:--:|:--|:--|:--|:--|
| 1 | Каталог не указан | Ошибка и ненулевой код выхода | `Usage: ...` / ненулевой код | `usage: ...` / код 1 |
| 2 | Каталог не существует | Ошибка и ненулевой код выхода | `error: '...' is not a directory` | `error: cannot enter '...': ...` / код 1 |
| 3 | Позитивный запуск в текущем каталоге | Вывод начального каталога, затем подкаталогов и подъём к родителю | Соответствует ожидаемому | Соответствует ожидаемому |
| 4 | Позитивный запуск в WSL | Аналогичный корректный обход до `/` | Соответствует ожидаемому | Соответствует ожидаемому |
