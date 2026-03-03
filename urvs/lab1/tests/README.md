# Тестирование ЛР1 (вариант 2)

## 1) Подготовка тестового дерева (PowerShell)
```powershell
$root = "$PWD\\.tmp_lab1_tree"
Remove-Item -Recurse -Force $root -ErrorAction SilentlyContinue
New-Item -ItemType Directory -Path "$root\\a\\x" -Force | Out-Null
New-Item -ItemType Directory -Path "$root\\a\\y" -Force | Out-Null
New-Item -ItemType Directory -Path "$root\\b" -Force | Out-Null
New-Item -ItemType File -Path "$root\\file.txt" -Force | Out-Null
```

## 2) Позитивный тест (C, Windows)
```powershell
clang .\c\main.c -Wall -Wextra -Werror -std=c11 -o .\c\lab1.exe
.\c\lab1.exe "$PWD\\.tmp_lab1_tree\\a\\x"
```
Ожидаемо:
- первая строка содержит `начальный каталог`;
- выводятся только подкаталоги текущего уровня;
- затем идут родительские каталоги до корня диска.

## 3) Негативный тест (C, Windows)
```powershell
.\c\lab1.exe "$PWD\\no_such_dir"
```
Ожидаемо:
- ненулевой код завершения;
- сообщение об ошибке в `stderr`.

## 4) Позитивный тест (shell, Linux/WSL)
```bash
bash ./shell/lab1_variant2.sh /tmp/lab1_tree/a/x
```
Ожидаемо: тот же формат и логика прохода до `/`.
