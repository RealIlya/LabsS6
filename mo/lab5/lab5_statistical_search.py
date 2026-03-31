import numpy as np
import matplotlib.pyplot as plt
from scipy.optimize import minimize
import os
import math

SCRIPT_DIR = os.path.dirname(os.path.abspath(__file__))
SAVE_DIR = os.path.join(SCRIPT_DIR, 'plots_lab5')
os.makedirs(SAVE_DIR, exist_ok=True)

C = np.array([2, 4, 2, 6, 2, 3])
a = np.array([-3, -6, 2, 6, -3, 8])
b = np.array([6, -8, -8, 8, -4, -1])

bounds = [(-13, 13), (-13, 13)]
V = (bounds[0][1] - bounds[0][0]) * (bounds[1][1] - bounds[1][0])  # 20 * 20 = 400

def f(X):
    """Целевая функция"""
    x, y = X
    val = sum(C[i] / (1 + (x - a[i])**2 + (y - b[i])**2) for i in range(6))
    return val

def minus_f(X):
    return -f(X)

# --- 1. Простой случайный поиск ---
def calculate_N(P, eps):
    """Вычисляет необходимое число испытаний"""
    V_eps = eps * eps
    P_eps = V_eps / V
    if P_eps >= 1:
        return 1
    N = math.ceil(math.log(1 - P) / math.log(1 - P_eps))
    return N

def simple_random_search(P, eps, seed=None):
    """Простой случайный поиск глобального экстремума"""
    if seed is not None:
        np.random.seed(seed)
    
    N = calculate_N(P, eps)
    best_point = None
    best_val = -np.inf
    
    coords = np.random.uniform(-13, 13, (N, 2))
    for pt in coords:
        val = f(pt)
        if val > best_val:
            best_val = val
            best_point = pt
            
    return N, best_point, best_val

# --- 2. Алгоритм глобального поиска 1 ---
def global_search_1(m, seed=None):
    """
    Алгоритм 1: Многократный запуск из случайных точек.
    Поиск прекращается после m попыток.
    """
    if seed is not None:
        np.random.seed(seed)
    
    best_global_point = None
    best_global_val = -np.inf
    evals = 0
    no_improvement_count = 0
    
    for i in range(m):
        x0 = np.random.uniform(-13, 13, 2)
        
        res = minimize(minus_f, x0, method='Nelder-Mead', 
                      bounds=bounds, options={'maxiter': 1000})
        evals += res.nfev
        
        val = -res.fun
        if val > best_global_val:
            best_global_val = val
            best_global_point = res.x
            no_improvement_count = 0
        else:
            no_improvement_count += 1
            
    return evals, best_global_point, best_global_val

# --- 3. Алгоритм глобального поиска 2 ---
def global_search_2(m, seed=None):
    """
    Алгоритм 2: Случайный поиск улучшающей точки, затем локальный спуск.
    """
    if seed is not None:
        np.random.seed(seed)
    
    # Начинаем с первой случайной точки
    x0 = np.random.uniform(-13, 13, 2)
    res = minimize(minus_f, x0, method='Nelder-Mead', 
                  bounds=bounds, options={'maxiter': 1000})
    
    best_global_point = res.x
    best_global_val = -res.fun
    evals = res.nfev
    
    consecutive_fails = 0
    
    while consecutive_fails < m:
        # Ненаправленный случайный поиск точки x2: f(x2) < f(x1*)
        found_better = False
        for _ in range(1000):  # Лимит попыток на одну итерацию
            x_new = np.random.uniform(-13, 13, 2)
            val_new = f(x_new)
            evals += 1
            
            if val_new > best_global_val:
                # Найдена лучшая точка, запускаем локальный спуск
                res = minimize(minus_f, x_new, method='Nelder-Mead',
                             bounds=bounds, options={'maxiter': 1000})
                evals += res.nfev
                
                val = -res.fun
                if val > best_global_val:
                    best_global_val = val
                    best_global_point = res.x
                    consecutive_fails = 0
                    found_better = True
                    break
        
        if not found_better:
            consecutive_fails += 1
            
    return evals, best_global_point, best_global_val

# --- 4. Алгоритм глобального поиска 3 ---
def global_search_3(m, seed=None):
    """
    Алгоритм 3: Выход из области притяжения текущего локального минимума.
    """
    if seed is not None:
        np.random.seed(seed)
    
    x0 = np.random.uniform(-13, 13, 2)
    res = minimize(minus_f, x0, method='Nelder-Mead',
                  bounds=bounds, options={'maxiter': 1000})
    
    best_global_point = res.x
    best_global_val = -res.fun
    evals = res.nfev
    
    consecutive_fails = 0
    
    while consecutive_fails < m:
        # Движемся из текущего локального оптимума в случайном направлении
        direction = np.random.randn(2)
        direction = direction / np.linalg.norm(direction)
        
        # Пробуем выйти из области притяжения
        step_size = 2.0
        x_new = best_global_point + step_size * direction
        x_new = np.clip(x_new, -13, 13)
        
        # Локальный спуск из новой точки
        res = minimize(minus_f, x_new, method='Nelder-Mead',
                      bounds=bounds, options={'maxiter': 1000})
        evals += res.nfev
        
        val = -res.fun
        if val > best_global_val:
            best_global_val = val
            best_global_point = res.x
            consecutive_fails = 0
        else:
            consecutive_fails += 1
            
    return evals, best_global_point, best_global_val

# --- Визуализация ---
def plot_function():
    """Построение графика целевой функции"""
    x_vals = np.linspace(-13, 13, 200)
    y_vals = np.linspace(-13, 13, 200)
    X_grid, Y_grid = np.meshgrid(x_vals, y_vals)
    
    Z = np.zeros_like(X_grid)
    for i in range(X_grid.shape[0]):
        for j in range(X_grid.shape[1]):
            Z[i, j] = f([X_grid[i, j], Y_grid[i, j]])
    
    plt.figure(figsize=(12, 10))
    
    # Контурный график
    levels = np.linspace(Z.min(), Z.max(), 30)
    contour = plt.contourf(X_grid, Y_grid, Z, levels=levels, cmap='viridis')
    plt.colorbar(contour, label='f(x, y)')
    
    # Отметим локальные максимумы (центры притяжения)
    for i in range(len(a)):
        plt.plot(a[i], b[i], '*', markersize=15, markeredgecolor='white',
                label=f'Центр {i+1}' if i == 0 else '')
    
    plt.xlabel('x', fontsize=12)
    plt.ylabel('y', fontsize=12)
    plt.title('Целевая функция (поиск максимума)', fontsize=14, fontweight='bold')
    plt.grid(True, alpha=0.3)
    plt.legend()
    
    save_path = os.path.join(SAVE_DIR, 'function_landscape.png')
    plt.savefig(save_path, dpi=150, bbox_inches='tight')
    plt.close()
    print(f"Сохранен график: function_landscape.png")

def test_with_multiple_seeds(algorithm_func, m, num_seeds=5):
    """Тестирование алгоритма с разными начальными значениями ГСЧ"""
    results = []
    for seed in range(num_seeds):
        evals, point, val = algorithm_func(m, seed=seed)
        results.append({
            'seed': seed,
            'evals': evals,
            'point': point,
            'value': val
        })
    return results


def run_lab5():
    plot_function()
    
    print()
    print("1. ПРОСТОЙ СЛУЧАЙНЫЙ ПОИСК")
    print(f"{'eps':<10}| {'P':<6}| {'N':<12}| {'(x*, y*)':<35}| {'f(x*, y*)'}")
    print("-" * 90)
    
    test_params = [
        (0.5, 0.90), (0.5, 0.95), (0.5, 0.99),
        (0.1, 0.90), (0.1, 0.95), (0.1, 0.99)
    ]
    
    np.random.seed(42)
    for eps, P in test_params:
        N, pt, val = simple_random_search(P, eps)
        pt_str = f"({pt[0]:8.4f}, {pt[1]:8.4f})"
        print(f"{eps:<10}| {P:<6}| {N:<12}| {pt_str:<35}| {val:10.6f}")
    
    print()
    print("2. АЛГОРИТМ ГЛОБАЛЬНОГО ПОИСКА 1 (Многократный запуск)")
    print(f"{'m':<10}| {'Вычисл. f':<12}| {'(x*, y*)':<35}| {'f(x*, y*)'}")
    print("-" * 90)
    
    for m in [5, 10, 20, 50]:
        evals, pt, val = global_search_1(m, seed=42)
        pt_str = f"({pt[0]:8.4f}, {pt[1]:8.4f})"
        print(f"{m:<10}| {evals:<12}| {pt_str:<35}| {val:10.6f}")
    
    print()
    print("3. АЛГОРИТМ ГЛОБАЛЬНОГО ПОИСКА 2 (Случайный поиск → Спуск)")
    print(f"{'m':<10}| {'Вычисл. f':<12}| {'(x*, y*)':<35}| {'f(x*, y*)'}")
    print("-" * 90)
    
    for m in [5, 10, 20]:
        evals, pt, val = global_search_2(m, seed=42)
        pt_str = f"({pt[0]:8.4f}, {pt[1]:8.4f})"
        print(f"{m:<10}| {evals:<12}| {pt_str:<35}| {val:10.6f}")
    
    print()
    print("4. АЛГОРИТМ ГЛОБАЛЬНОГО ПОИСКА 3 (Выход из области притяжения)")
    print(f"{'m':<10}| {'Вычисл. f':<12}| {'(x*, y*)':<35}| {'f(x*, y*)'}")
    print("-" * 90)
    
    for m in [5, 10, 20, 50]:
        evals, pt, val = global_search_3(m, seed=42)
        pt_str = f"({pt[0]:8.4f}, {pt[1]:8.4f})"
        print(f"{m:<10}| {evals:<12}| {pt_str:<35}| {val:10.6f}")
    
    print()
    print("5. ИССЛЕДОВАНИЕ УСТОЙЧИВОСТИ (5 разных начальных значений ГСЧ)")
    
    m_test = 20
    
    print(f"\nАлгоритм 1 (m={m_test}):")
    print(f"{'Seed':<10}| {'Вычисл. f':<12}| {'(x*, y*)':<35}| {'f(x*, y*)'}")
    print("-" * 90)
    results_1 = test_with_multiple_seeds(global_search_1, m_test, 5)
    for r in results_1:
        pt_str = f"({r['point'][0]:8.4f}, {r['point'][1]:8.4f})"
        print(f"{r['seed']:<10}| {r['evals']:<12}| {pt_str:<35}| {r['value']:10.6f}")
    
    print(f"\nАлгоритм 2 (m={m_test}):")
    print(f"{'Seed':<10}| {'Вычисл. f':<12}| {'(x*, y*)':<35}| {'f(x*, y*)'}")
    print("-" * 90)
    results_2 = test_with_multiple_seeds(global_search_2, m_test, 5)
    for r in results_2:
        pt_str = f"({r['point'][0]:8.4f}, {r['point'][1]:8.4f})"
        print(f"{r['seed']:<10}| {r['evals']:<12}| {pt_str:<35}| {r['value']:10.6f}")
    
    print(f"\nАлгоритм 3 (m={m_test}):")
    print(f"{'Seed':<10}| {'Вычисл. f':<12}| {'(x*, y*)':<35}| {'f(x*, y*)'}")
    print("-" * 90)
    results_3 = test_with_multiple_seeds(global_search_3, m_test, 5)
    for r in results_3:
        pt_str = f"({r['point'][0]:8.4f}, {r['point'][1]:8.4f})"
        print(f"{r['seed']:<10}| {r['evals']:<12}| {pt_str:<35}| {r['value']:10.6f}")

    print()
    print("СТАТИСТИКА ПО УСТОЙЧИВОСТИ")
    print()
    
    for name, results in [("Алгоритм 1", results_1), 
                          ("Алгоритм 2", results_2), 
                          ("Алгоритм 3", results_3)]:
        values = [r['value'] for r in results]
        evals_list = [r['evals'] for r in results]
        print(f"\n{name}:")
        print(f"  Среднее значение f: {np.mean(values):.6f}")
        print(f"  Макс. значение f:   {np.max(values):.6f}")
        print(f"  Мин. значение f:    {np.min(values):.6f}")
        print(f"  Ст. откл. f:        {np.std(values):.6f}")
        print(f"  Среднее вычисл.:    {np.mean(evals_list):.1f}")

if __name__ == "__main__":
    run_lab5()
