# Лабораторная работа 1 (вариант 2)

Реализация задания:

- просмотреть каталог, указанный параметром;
- вывести имена вложенных каталогов;
- перейти к родительскому каталогу и повторять до достижения корня.

## Структура

- `shell/lab1_variant2.sh` — shell-реализация.
- `c/main.c` — C-реализация (один исходник, ветвление по ОС через `#ifdef _WIN32`).
- `tests/README.md` — сценарии проверки.

## Запуск shell-версии

```bash
bash ./shell/lab1_variant2.sh /path/to/start/dir
```

## Сборка и запуск C-версии

Linux:

```bash
cc ./c/main.c -Wall -Wextra -Werror -std=c11 -o ./c/lab1
./c/lab1 /path/to/start/dir
```

Windows (clang):

```powershell
clang .\c\main.c -Wall -Wextra -Werror -std=c11 -o .\c\lab1.exe
.\c\lab1.exe C:\path\to\start\dir
```

## Формат вывода

```text
каталог <имя каталога> начальный каталог
  каталог <имя>
  каталог <имя>
каталог <имя каталога> родительский каталог
  каталог <имя>
...
```
