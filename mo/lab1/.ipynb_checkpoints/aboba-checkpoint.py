#!/usr/bin/env python
import math
import matplotlib.pyplot as plt

# ==========================================
# 1. ОПРЕДЕЛЕНИЕ ФУНКЦИИ И ПАРАМЕТРОВ (ВАРИАНТ 2)
# ==========================================

def target_function(x):
    """Целевая функция f(x) = (x - 2)^2"""
    return (x - 2)**2

# Параметры варианта
START_A = -2.0
START_B = 20.0
VARIANT_EPS = 1e-7

# ==========================================
# 2. РЕАЛИЗАЦИЯ МЕТОДОВ
# ==========================================

def method_dichotomy(func, a, b, eps, verbose=False):
    """
    Метод дихотомии.
    Возвращает (точка минимума, значение функции, кол-во вычислений, история итераций)
    """
    delta = eps / 2.1  # delta должна быть меньше eps (по условию delta < eps)
    calls = 0
    history = []
    iteration = 0
    prev_len = b - a
    
    while (b - a) > eps:
        iteration += 1
        x1 = (a + b - delta) / 2
        x2 = (a + b + delta) / 2
        
        fx1 = func(x1)
        fx2 = func(x2)
        calls += 2
        
        current_len = b - a
        ratio = prev_len / current_len if iteration > 1 else 0
        
        # Сохраняем данные для отчета (только если нужно)
        if verbose:
            history.append({
                'i': iteration,
                'x1': x1, 'x2': x2,
                'fx1': fx1, 'fx2': fx2,
                'a': a, 'b': b,
                'len': current_len,
                'ratio': ratio
            })
            
        prev_len = current_len

        if fx1 < fx2:
            b = x2
        else:
            a = x1
            
    min_x = (a + b) / 2
    return min_x, func(min_x), calls, history


def method_golden_section(func, a, b, eps, verbose=False):
    """
    Метод золотого сечения.
    """
    phi = (1 + math.sqrt(5)) / 2 # Золотая пропорция
    calls = 0
    history = []
    iteration = 0
    
    # Инициализация точек
    x1 = b - (b - a) / phi
    x2 = a + (b - a) / phi
    fx1 = func(x1)
    fx2 = func(x2)
    calls += 2
    
    prev_len = b - a
    
    while (b - a) > eps:
        iteration += 1
        current_len = b - a
        ratio = prev_len / current_len if iteration > 1 else 0
        
        if verbose:
            history.append({
                'i': iteration,
                'x1': x1, 'x2': x2,
                'fx1': fx1, 'fx2': fx2,
                'a': a, 'b': b,
                'len': current_len,
                'ratio': ratio
            })
            
        prev_len = current_len

        if fx1 < fx2:
            b = x2
            x2 = x1
            fx2 = fx1
            x1 = b - (b - a) / phi
            fx1 = func(x1)
            calls += 1
        else:
            a = x1
            x1 = x2
            fx1 = fx2
            x2 = a + (b - a) / phi
            fx2 = func(x2)
            calls += 1
            
    min_x = (a + b) / 2
    return min_x, func(min_x), calls, history

def search_interval(func, x0, delta=0.1):
    """
    Алгоритм поиска интервала, содержащего минимум (алгоритм Свенна).
    """
    history = []
    
    # Шаг 1
    f0 = func(x0)
    
    if func(x0 + delta) < f0:
        # Убывает вправо
        x1 = x0 + delta
        h = delta
    elif func(x0 - delta) < f0:
        # Убывает влево
        x1 = x0 - delta
        h = -delta
    else:
        # x0 уже лежит в окрестности минимума или delta слишком велика
        return [x0 - delta, x0 + delta], history

    k = 1
    xk = x1
    xk_prev = x0
    history.append({'k': 0, 'x': x0, 'f': f0})
    history.append({'k': 1, 'x': x1, 'f': func(x1)})

    # Шаг 2 и 3
    while True:
        h *= 2 # Удваиваем шаг
        xk_next = xk + h
        fk_next = func(xk_next)
        fk = func(xk)
        
        history.append({'k': k + 1, 'x': xk_next, 'f': fk_next})
        
        if not (fk < fk_next):
            # Если функция начала расти
            # Интервал найден между предыдущей точкой и следующей
            # Упорядочиваем границы
            interval = sorted([xk_prev, xk_next])
            return interval, history
        
        xk_prev = xk
        xk = xk_next
        k += 1

# ==========================================
# 3. ВСПОМОГАТЕЛЬНЫЕ ФУНКЦИИ ВЫВОДА
# ==========================================

def print_table(history, method_name):
    print(f"\nТаблица результатов: {method_name}")
    print("-" * 105)
    print(f"{'i':<4} | {'x1':^12} | {'x2':^12} | {'f(x1)':^12} | {'f(x2)':^12} | {'a':^12} | {'b':^12} | {'b-a':^12} | {'ratio':^8}")
    print("-" * 105)
    for row in history:
        # ratio может быть 0 на первой итерации
        r_str = f"{row['ratio']:.4f}" if row['ratio'] != 0 else "-"
        print(f"{row['i']:<4} | {row['x1']: .9f} | {row['x2']: .9f} | {row['fx1']: .9f} | {row['fx2']: .9f} | {row['a']: .9f} | {row['b']: .9f} | {row['len']: .9e} | {r_str:^8}")
    print("-" * 105)

def print_interval_table(history):
    print(f"\nТаблица поиска интервала:")
    print("-" * 40)
    print(f"{'step':<5} | {'x':^15} | {'f(x)':^15}")
    print("-" * 40)
    for row in history:
        print(f"{row['k']:<5} | {row['x']: .9f} | {row['f']: .9f}")
    print("-" * 40)

# ==========================================
# 4. ОСНОВНОЙ БЛОК ВЫПОЛНЕНИЯ
# ==========================================

if __name__ == "__main__":
    print("ЛАБОРАТОРНАЯ РАБОТА №1. ВАРИАНТ 2.")
    print(f"Функция: f(x) = (x - 2)^2 на интервале [{START_A}, {START_B}]\n")

    # --- ЗАДАНИЕ 1: Таблицы для eps = 10^-7 ---
    print(">>> ИССЛЕДОВАНИЕ МЕТОДОВ ПРИ EPS = 1e-7")
    
    # Дихотомия
    min_d, val_d, calls_d, hist_d = method_dichotomy(target_function, START_A, START_B, VARIANT_EPS, verbose=True)
    print_table(hist_d, "Метод Дихотомии")
    print(f"Результат: x_min = {min_d}, f(x_min) = {val_d}, Вызовов функции: {calls_d}")

    # Золотое сечение
    min_g, val_g, calls_g, hist_g = method_golden_section(target_function, START_A, START_B, VARIANT_EPS, verbose=True)
    print_table(hist_g, "Метод Золотого Сечения")
    print(f"Результат: x_min = {min_g}, f(x_min) = {val_g}, Вызовов функции: {calls_g}")

    # --- ЗАДАНИЕ 2: График зависимости вычислений от точности ---
    print("\n>>> ПОСТРОЕНИЕ ГРАФИКА ЗАВИСИМОСТИ")
    
    epsilons = [10**(-i) for i in range(1, 8)] # от 10^-1 до 10^-7
    log_eps = [-math.log10(e) for e in epsilons] # для оси X (1, 2, ... 7)
    
    dich_calls_list = []
    gold_calls_list = []
    
    print(f"{'Eps':<10} | {'Dichotomy':<10} | {'Golden':<10}")
    for eps in epsilons:
        _, _, c_d, _ = method_dichotomy(target_function, START_A, START_B, eps)
        _, _, c_g, _ = method_golden_section(target_function, START_A, START_B, eps)
        dich_calls_list.append(c_d)
        gold_calls_list.append(c_g)
        print(f"{eps:<10} | {c_d:<10} | {c_g:<10}")

    plt.figure(figsize=(10, 6))
    plt.plot(log_eps, dich_calls_list, marker='o', label='Дихотомия')
    plt.plot(log_eps, gold_calls_list, marker='s', label='Золотое сечение')
    plt.title('Зависимость кол-ва вычислений функции от точности')
    plt.xlabel('-lg(ε)')
    plt.ylabel('Количество вычислений f(x)')
    plt.grid(True)
    plt.legend()
    plt.show() # Раскомментируйте, чтобы показать график в окне
    print("График построен (вызовите plt.show() если запускаете локально).")

    # --- ЗАДАНИЕ 3: Поиск интервала ---
    print("\n>>> ПОИСК ИНТЕРВАЛА, СОДЕРЖАЩЕГО МИНИМУМ")
    # Берем произвольную точку, например, начало отрезка или любую другую
    # Так как минимум в точке 2, возьмем x0 = -1 (левее), чтобы алгоритм "пошел" вправо
    x_start = -1.0 
    found_interval, int_history = search_interval(target_function, x_start, delta=0.5)
    
    print(f"Начальная точка: {x_start}")
    print_interval_table(int_history)
    print(f"Найденный интервал: {found_interval}")
    print(f"Проверка: минимум x=2 входит в интервал? {'ДА' if found_interval[0] <= 2 <= found_interval[1] else 'НЕТ'}")