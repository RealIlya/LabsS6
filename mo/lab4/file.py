import numpy as np
import matplotlib.pyplot as plt
from scipy.optimize import minimize, minimize_scalar
import os

SCRIPT_DIR = os.path.dirname(os.path.abspath(__file__))
SAVE_DIR = os.path.join(SCRIPT_DIR, 'plots')
os.makedirs(SAVE_DIR, exist_ok=True)

# =============================================================================
# Задача условной оптимизации методом штрафных функций
# Минимизировать: f(x) = (x1 - 2)^2 + (x2 - 1)^2
# При ограничениях:
#   g1(x) = x1 + x2 - 3 <= 0  (неравенство)
#   g2(x) = x1^2 + x2^2 - 4 = 0  (равенство)
# =============================================================================

def f_objective(x):
    """Целевая функция (минимум в точке (2, 1) без ограничений)"""
    return (x[0] - 2)**2 + (x[1] - 1)**2

def grad_f(x):
    """Градиент целевой функции"""
    return np.array([2 * (x[0] - 2), 2 * (x[1] - 1)])

def g_ineq(x):
    """Ограничение-неравенство: g1(x) <= 0"""
    return x[0] + x[1] - 3

def g_eq(x):
    """Ограничение-равенство: g2(x) = 0"""
    return x[0]**2 + x[1]**2 - 4

def constraint_violation(x):
    """Величина нарушения ограничений"""
    viol_ineq = max(0, g_ineq(x))
    viol_eq = abs(g_eq(x))
    return viol_ineq + viol_eq

# =============================================================================
# Методы штрафных функций
# =============================================================================

def penalty_exterior(x, mu, rho):
    """
    Внешняя штрафная функция
    F(x, μ, ρ) = f(x) + μ * P(x)
    где P(x) = Σ max(0, g_i(x))^2 + Σ h_j(x)^2
    """
    f_val = f_objective(x)
    
    # Штраф за неравенства
    ineq_penalty = max(0, g_ineq(x))**2
    
    # Штраф за равенства
    eq_penalty = g_eq(x)**2
    
    return f_val + mu * (ineq_penalty + rho * eq_penalty)

def penalty_interior(x, mu):
    """
    Внутренняя штрафная функция (барьерный метод)
    Работает только для ограничений-неравенств
    F(x, μ) = f(x) - μ * Σ 1/g_i(x)
    """
    f_val = f_objective(x)
    
    # Логарифмический барьер для неравенств
    g1_val = g_ineq(x)
    if g1_val >= 0:
        return float('inf')
    
    barrier = -np.log(-g1_val)
    
    return f_val + mu * barrier

def penalty_mixed(x, mu, rho):
    """
    Смешанная штрафная функция
    Комбинация внешней для равенств и внутренней для неравенств
    """
    f_val = f_objective(x)
    
    # Внутренний штраф для неравенств (барьер)
    g1_val = g_ineq(x)
    if g1_val >= 0:
        return float('inf')
    barrier = -np.log(-g1_val)
    
    # Внешний штраф для равенств
    eq_penalty = g_eq(x)**2
    
    return f_val + mu * barrier + rho * eq_penalty

def quadratic_penalty(x, mu):
    """
    Квадратичная штрафная функция
    F(x, μ) = f(x) + μ/2 * (Σ max(0, g_i(x))^2 + Σ h_j(x)^2)
    """
    f_val = f_objective(x)
    
    ineq_penalty = max(0, g_ineq(x))**2
    eq_penalty = g_eq(x)**2
    
    return f_val + (mu / 2) * (ineq_penalty + eq_penalty)

def exact_penalty(x, mu):
    """
    Точная штрафная функция (L1 штраф)
    F(x, μ) = f(x) + μ * (Σ max(0, g_i(x)) + Σ |h_j(x)|)
    """
    f_val = f_objective(x)
    
    ineq_penalty = max(0, g_ineq(x))
    eq_penalty = abs(g_eq(x))
    
    return f_val + mu * (ineq_penalty + eq_penalty)

# =============================================================================
# Алгоритмы минимизации с штрафными функциями
# =============================================================================

def exterior_penalty_method(x0, mu0=1.0, mu_mult=10.0, max_outer_iter=20, tol=1e-6):
    """
    Метод внешней штрафной функции
    """
    x = np.array(x0, dtype=float)
    mu = mu0
    history = []
    
    for k in range(max_outer_iter):
        # Минимизация штрафной функции при фиксированном mu
        result = minimize(
            lambda x: penalty_exterior(x, mu, rho=mu),
            x,
            method='BFGS',
            options={'gtol': 1e-8}
        )
        x = result.x
        history.append(x.copy())
        
        # Проверка сходимости
        viol = constraint_violation(x)
        if viol < tol:
            break
        
        mu *= mu_mult
    
    return x, history, k+1

def interior_penalty_method(x0, mu0=1.0, mu_mult=0.1, max_outer_iter=20, tol=1e-6):
    """
    Метод внутренней штрафной функции (барьерный метод)
    """
    x = np.array(x0, dtype=float)
    mu = mu0
    history = []
    
    # Начальная точка должна быть допустимой
    if g_ineq(x) >= 0:
        # Сдвигаем точку внутрь допустимой области
        x = np.array([0.5, 0.5])
    
    for k in range(max_outer_iter):
        # Минимизация штрафной функции
        result = minimize(
            lambda x: penalty_interior(x, mu),
            x,
            method='BFGS',
            options={'gtol': 1e-8}
        )
        x = result.x
        history.append(x.copy())
        
        # Проверка сходимости
        if mu < tol:
            break
        
        mu *= mu_mult
    
    return x, history, k+1

def mixed_penalty_method(x0, mu0=1.0, rho0=1.0, mu_mult=0.1, rho_mult=10.0, max_outer_iter=20, tol=1e-6):
    """
    Смешанный метод штрафных функций
    """
    x = np.array(x0, dtype=float)
    mu = mu0
    rho = rho0
    history = []
    
    if g_ineq(x) >= 0:
        x = np.array([0.5, 0.5])
    
    for k in range(max_outer_iter):
        result = minimize(
            lambda x: penalty_mixed(x, mu, rho),
            x,
            method='BFGS',
            options={'gtol': 1e-8}
        )
        x = result.x
        history.append(x.copy())
        
        viol = constraint_violation(x)
        if viol < tol and mu < tol:
            break
        
        mu *= mu_mult
        rho *= rho_mult
    
    return x, history, k+1

def quadratic_penalty_method(x0, mu0=1.0, mu_mult=10.0, max_outer_iter=20, tol=1e-6):
    """
    Метод квадратичной штрафной функции
    """
    x = np.array(x0, dtype=float)
    mu = mu0
    history = []
    
    for k in range(max_outer_iter):
        result = minimize(
            lambda x: quadratic_penalty(x, mu),
            x,
            method='BFGS',
            options={'gtol': 1e-8}
        )
        x = result.x
        history.append(x.copy())
        
        viol = constraint_violation(x)
        if viol < tol:
            break
        
        mu *= mu_mult
    
    return x, history, k+1

def exact_penalty_method(x0, mu0=10.0, mu_mult=2.0, max_outer_iter=15, tol=1e-6):
    """
    Метод точной штрафной функции (L1)
    """
    x = np.array(x0, dtype=float)
    mu = mu0
    history = []
    
    for k in range(max_outer_iter):
        result = minimize(
            lambda x: exact_penalty(x, mu),
            x,
            method='BFGS',
            options={'gtol': 1e-8}
        )
        x = result.x
        history.append(x.copy())
        
        viol = constraint_violation(x)
        if viol < tol:
            break
        
        mu *= mu_mult
    
    return x, history, k+1

# =============================================================================
# Визуализация
# =============================================================================

def plot_constraints_and_trajectories(histories, titles, filename):
    """Построение ограничений и траекторий"""
    fig, ax = plt.subplots(figsize=(10, 8))
    
    # Сетка для контуров
    x1 = np.linspace(-2.5, 2.5, 400)
    x2 = np.linspace(-2.5, 2.5, 400)
    X1, X2 = np.meshgrid(x1, x2)
    
    # Целевая функция
    Z = (X1 - 2)**2 + (X2 - 1)**2
    contours = ax.contour(X1, X2, Z, levels=20, cmap='Blues', alpha=0.6)
    
    # Ограничение-неравенство: x1 + x2 - 3 <= 0
    x_line = np.linspace(-2.5, 2.5, 100)
    ax.plot(x_line, 3 - x_line, 'r-', linewidth=2, label='g1: x1+x2≤3')
    ax.fill_between(x_line, -2.5, 3 - x_line, color='red', alpha=0.1)
    
    # Ограничение-равенство: x1^2 + x2^2 = 4 (окружность)
    circle = plt.Circle((0, 0), 2, color='green', linewidth=2, fill=False, label='g2: x1²+x2²=4')
    ax.add_patch(circle)
    
    # Точка безусловного минимума
    ax.plot(2, 1, 'k*', markersize=15, label='Безусловный минимум (2,1)')
    
    # Траектории методов
    colors = ['blue', 'orange', 'green', 'purple', 'brown']
    for i, (hist, title) in enumerate(zip(histories, titles)):
        hist_arr = np.array(hist)
        ax.plot(hist_arr[:, 0], hist_arr[:, 1], 
                color=colors[i % len(colors)], linewidth=1.5, 
                marker='o', markersize=4, label=title, alpha=0.8)
        if len(hist) > 0:
            ax.plot(hist_arr[0, 0], hist_arr[0, 1], 
                   color=colors[i % len(colors)], marker='s', 
                   markersize=10, fillstyle='none', linewidth=2)
    
    ax.set_xlabel('x1', fontsize=12)
    ax.set_ylabel('x2', fontsize=12)
    ax.set_title('Методы штрафных функций: траектории минимизации', fontsize=14, fontweight='bold')
    ax.legend(loc='upper right', fontsize=9)
    ax.grid(True, linestyle='--', alpha=0.5)
    ax.set_xlim(-2.5, 2.5)
    ax.set_ylim(-2.5, 2.5)
    ax.set_aspect('equal')
    
    plt.tight_layout()
    save_path = os.path.join(SAVE_DIR, filename)
    plt.savefig(save_path, dpi=150, bbox_inches='tight')
    plt.close()
    print(f"График сохранён: {save_path}")

def plot_penalty_function_evolution(x0, filename):
    """Эволюция штрафной функции при увеличении μ"""
    fig, axes = plt.subplots(2, 3, figsize=(15, 10))
    axes = axes.flatten()
    
    mu_values = [0.1, 0.5, 1, 5, 10, 50]
    
    x1 = np.linspace(-2.5, 2.5, 200)
    x2 = np.linspace(-2.5, 2.5, 200)
    X1, X2 = np.meshgrid(x1, x2)
    
    for i, mu in enumerate(mu_values):
        Z = np.zeros_like(X1)
        for j in range(len(x1)):
            for k in range(len(x2)):
                Z[k, j] = penalty_exterior([X1[k, j], X2[k, j]], mu, mu)
        
        cs = axes[i].contourf(X1, X2, Z, levels=30, cmap='viridis')
        axes[i].plot(x1, 3 - x1, 'r-', linewidth=1.5)
        circle = plt.Circle((0, 0), 2, color='green', linewidth=1.5, fill=False)
        axes[i].add_patch(circle)
        axes[i].set_title(f'μ = {mu}', fontsize=11)
        axes[i].set_xlabel('x1')
        axes[i].set_ylabel('x2')
        axes[i].grid(True, alpha=0.3)
    
    plt.colorbar(cs, ax=axes, label='Значение штрафной функции', shrink=0.8)
    plt.suptitle('Эволюция внешней штрафной функции при увеличении μ', fontsize=14, fontweight='bold')
    plt.tight_layout()
    
    save_path = os.path.join(SAVE_DIR, filename)
    plt.savefig(save_path, dpi=150, bbox_inches='tight')
    plt.close()
    print(f"График сохранён: {save_path}")

def plot_convergence_comparison(results, filename):
    """Сравнение сходимости методов"""
    fig, axes = plt.subplots(1, 3, figsize=(15, 5))
    
    methods = list(results.keys())
    colors = ['blue', 'orange', 'green', 'purple', 'brown']
    
    # График 1: Нарушение ограничений по итерациям
    ax = axes[0]
    for i, method in enumerate(methods):
        hist = results[method]['history']
        violations = [constraint_violation(x) for x in hist]
        ax.semilogy(violations, color=colors[i % len(colors)], 
                   linewidth=2, marker='o', markersize=4, label=method)
    ax.set_xlabel('Итерация', fontsize=11)
    ax.set_ylabel('Нарушение ограничений (лог)', fontsize=11)
    ax.set_title('Сходимость по нарушению ограничений', fontsize=12, fontweight='bold')
    ax.legend(fontsize=9)
    ax.grid(True, linestyle='--', alpha=0.5)
    
    # График 2: Значение целевой функции
    ax = axes[1]
    for i, method in enumerate(methods):
        hist = results[method]['history']
        f_vals = [f_objective(x) for x in hist]
        ax.plot(f_vals, color=colors[i % len(colors)], 
               linewidth=2, marker='o', markersize=4, label=method)
    ax.set_xlabel('Итерация', fontsize=11)
    ax.set_ylabel('f(x)', fontsize=11)
    ax.set_title('Сходимость по целевой функции', fontsize=12, fontweight='bold')
    ax.legend(fontsize=9)
    ax.grid(True, linestyle='--', alpha=0.5)
    
    # График 3: Расстояние до решения
    ax = axes[2]
    # Аналитическое решение задачи
    x_opt = np.array([1.0, 1.732])  # Приближённое решение
    for i, method in enumerate(methods):
        hist = results[method]['history']
        distances = [np.linalg.norm(x - x_opt) for x in hist]
        ax.semilogy(distances, color=colors[i % len(colors)], 
                   linewidth=2, marker='o', markersize=4, label=method)
    ax.set_xlabel('Итерация', fontsize=11)
    ax.set_ylabel('||x - x*|| (лог)', fontsize=11)
    ax.set_title('Сходимость по расстоянию до оптимума', fontsize=12, fontweight='bold')
    ax.legend(fontsize=9)
    ax.grid(True, linestyle='--', alpha=0.5)
    
    plt.tight_layout()
    save_path = os.path.join(SAVE_DIR, filename)
    plt.savefig(save_path, dpi=150, bbox_inches='tight')
    plt.close()
    print(f"График сохранён: {save_path}")

# =============================================================================
# Основная программа
# =============================================================================

def run_all():
    print("=" * 70)
    print("Лабораторная работа №4: Метод штрафных функций")
    print("=" * 70)
    print("\nЗадача:")
    print("  Минимизировать: f(x) = (x1-2)² + (x2-1)²")
    print("  При ограничениях:")
    print("    g1(x) = x1 + x2 - 3 ≤ 0")
    print("    g2(x) = x1² + x2² - 4 = 0")
    print("=" * 70)
    
    x0 = [-1.0, 0.5]
    
    results = {}
    histories = []
    titles = []
    
    # Метод 1: Внешняя штрафная функция
    print("\n1. Метод внешней штрафной функции...")
    x_res, hist, iters = exterior_penalty_method(x0)
    results['Внешний штраф'] = {'x': x_res, 'history': hist, 'iters': iters}
    histories.append(hist)
    titles.append('Внешний штраф')
    print(f"   Решение: x = [{x_res[0]:.6f}, {x_res[1]:.6f}]")
    print(f"   f(x) = {f_objective(x_res):.6f}")
    print(f"   Нарушение: {constraint_violation(x_res):.2e}")
    print(f"   Итераций: {iters}")
    
    # Метод 2: Внутренняя штрафная функция
    print("\n2. Метод внутренней штрафной функции (барьерный)...")
    x_res, hist, iters = interior_penalty_method(x0)
    results['Внутренний штраф'] = {'x': x_res, 'history': hist, 'iters': iters}
    histories.append(hist)
    titles.append('Внутренний штраф')
    print(f"   Решение: x = [{x_res[0]:.6f}, {x_res[1]:.6f}]")
    print(f"   f(x) = {f_objective(x_res):.6f}")
    print(f"   Нарушение: {constraint_violation(x_res):.2e}")
    print(f"   Итераций: {iters}")
    
    # Метод 3: Смешанный метод
    print("\n3. Смешанный метод штрафных функций...")
    x_res, hist, iters = mixed_penalty_method(x0)
    results['Смешанный'] = {'x': x_res, 'history': hist, 'iters': iters}
    histories.append(hist)
    titles.append('Смешанный')
    print(f"   Решение: x = [{x_res[0]:.6f}, {x_res[1]:.6f}]")
    print(f"   f(x) = {f_objective(x_res):.6f}")
    print(f"   Нарушение: {constraint_violation(x_res):.2e}")
    print(f"   Итераций: {iters}")
    
    # Метод 4: Квадратичный штраф
    print("\n4. Метод квадратичной штрафной функции...")
    x_res, hist, iters = quadratic_penalty_method(x0)
    results['Квадратичный'] = {'x': x_res, 'history': hist, 'iters': iters}
    histories.append(hist)
    titles.append('Квадратичный')
    print(f"   Решение: x = [{x_res[0]:.6f}, {x_res[1]:.6f}]")
    print(f"   f(x) = {f_objective(x_res):.6f}")
    print(f"   Нарушение: {constraint_violation(x_res):.2e}")
    print(f"   Итераций: {iters}")
    
    # Метод 5: Точный штраф (L1)
    print("\n5. Метод точной штрафной функции (L1)...")
    x_res, hist, iters = exact_penalty_method(x0)
    results['Точный L1'] = {'x': x_res, 'history': hist, 'iters': iters}
    histories.append(hist)
    titles.append('Точный L1')
    print(f"   Решение: x = [{x_res[0]:.6f}, {x_res[1]:.6f}]")
    print(f"   f(x) = {f_objective(x_res):.6f}")
    print(f"   Нарушение: {constraint_violation(x_res):.2e}")
    print(f"   Итераций: {iters}")
    
    # Построение графиков
    print("\n" + "=" * 70)
    print("Построение графиков...")
    print("=" * 70)
    
    plot_constraints_and_trajectories(
        histories, titles, 
        'penalty_trajectories.png'
    )
    
    plot_penalty_function_evolution(x0, 'penalty_evolution.png')
    
    plot_convergence_comparison(results, 'convergence_comparison.png')
    
    print("\n" + "=" * 70)
    print("Все графики сохранены в папке:", SAVE_DIR)
    print("=" * 70)

if __name__ == "__main__":
    run_all()
