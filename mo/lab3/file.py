import numpy as np
import matplotlib.pyplot as plt
from scipy.optimize import minimize_scalar
import os

SCRIPT_DIR = os.path.dirname(os.path.abspath(__file__))
SAVE_DIR = os.path.join(SCRIPT_DIR, 'plots_task_1_2')
os.makedirs(SAVE_DIR, exist_ok=True)

def f_quad(x):
    """Целевая квадратичная функция (минимум в 3, 7)"""
    x1, x2 = x[0], x[1]
    return 10 * (x1 + x2 - 10)**2 + (x1 - x2 + 4)**2

def grad_quad(x):
    x1, x2 = x[0], x[1]
    df_dx1 = 20 * (x1 + x2 - 10) + 2 * (x1 - x2 + 4)
    df_dx2 = 20 * (x1 + x2 - 10) - 2 * (x1 - x2 + 4)
    return np.array([df_dx1, df_dx2])

def f_rosen(x):
    """Функция Розенброка (минимум в 1, 1)"""
    x1, x2 = x[0], x[1]
    return 100 * (x2 - x1**2)**2 + (1 - x1)**2

def grad_rosen(x):
    x1, x2 = x[0], x[1]
    df_dx1 = -400 * x1 * (x2 - x1**2) - 2 * (1 - x1)
    df_dx2 = 200 * (x2 - x1**2)
    return np.array([df_dx1, df_dx2])

class Objective:
    def __init__(self, f, grad):
        self.f = f
        self.grad = grad
        self.f_calls = 0
        self.g_calls = 0

    def evaluate(self, x):
        self.f_calls += 1
        return self.f(x)

    def evaluate_grad(self, x):
        self.g_calls += 1
        return self.grad(x)

    def reset_counters(self):
        self.f_calls = 0
        self.g_calls = 0

def bracket_unimodal(phi, start_step=1e-3, max_iter=1000):
    """Поиск интервала унимодальности для защиты от перескоков (особенно для Розенброка)"""
    f0 = phi(0.0)
    f1 = phi(start_step)
    f_m1 = phi(-start_step)
    
    if f1 < f0: h = start_step
    elif f_m1 < f0: h = -start_step
    else: return (-start_step, start_step)
        
    lam_prev, lam_curr = 0.0, h
    f_curr = phi(lam_curr)
    
    for _ in range(max_iter):
        h *= 2.0
        lam_next = lam_curr + h
        f_next = phi(lam_next)
        if f_next >= f_curr:
            return (lam_prev, lam_next) if h > 0 else (lam_next, lam_prev)
        lam_prev, f_curr, lam_curr = lam_curr, f_next, lam_next
        
    return (lam_prev, lam_curr) if h > 0 else (lam_curr, lam_prev)

def line_search(obj, x, d, tol=1e-5):
    def phi(lam): return obj.evaluate(x + lam * d)
    bracket = bracket_unimodal(phi)
    res = minimize_scalar(phi, method='bounded', bounds=bracket, options={'xatol': tol})
    return res.x


def steepest_descent(obj, x0, tol=1e-5, ls_tol=1e-5, max_iter=5000):
    """Алгоритм наискорейшего спуска"""
    x = np.array(x0, dtype=float)
    history = [x.copy()]
    
    for k in range(max_iter):
        g = obj.evaluate_grad(x)
        if np.linalg.norm(g) < tol: break
        
        S = -g
        lam = line_search(obj, x, S, tol=ls_tol)
        x = x + lam * S
        history.append(x.copy())
        
    return x, history, k+1

def conjugate_gradient(obj, x0, method='FR', tol=1e-5, ls_tol=1e-5, max_iter=5000):
    """Методы сопряженных градиентов"""
    x = np.array(x0, dtype=float)
    history = [x.copy()]
    n = len(x)
    
    g = obj.evaluate_grad(x)
    S = -g
    
    for k in range(max_iter):
        if np.linalg.norm(g) < tol: break
            
        if k > 0 and k % n == 0:
            S = -g
            
        lam = line_search(obj, x, S, tol=ls_tol)
        x_new = x + lam * S
        history.append(x_new.copy())
        
        g_new = obj.evaluate_grad(x_new)
        
        if method == 'FR':   # Флетчер-Ривс
            omega = np.dot(g_new, g_new) / (np.dot(g, g) + 1e-16)
        elif method == 'PR': # Полак-Рибьер
            omega = np.dot(g_new, (g_new - g)) / (np.dot(g, g) + 1e-16)
            omega = max(0, omega)
            
        S = -g_new + omega * S
        x = x_new
        g = g_new
        
    return x, history, k+1

def variable_metric(obj, x0, method='DFP', tol=1e-5, ls_tol=1e-5, max_iter=5000):
    """Методы переменной метрики"""
    x = np.array(x0, dtype=float)
    n = len(x)
    H = np.eye(n)
    history = [x.copy()]
    
    for k in range(max_iter):
        g = obj.evaluate_grad(x)
        if np.linalg.norm(g) < tol: break
            
        S = -np.dot(H, g)
        lam = line_search(obj, x, S, tol=ls_tol)
        
        x_new = x + lam * S
        history.append(x_new.copy())
        
        g_new = obj.evaluate_grad(x_new)
        dx = x_new - x
        dg = g_new - g
        
        if method == 'Broyden':
            v = dx - np.dot(H, dg)
            denom = np.dot(v, dg)
            if abs(denom) > 1e-12:
                H = H + np.outer(v, v) / denom
        elif method == 'DFP':
            term1 = np.outer(dx, dx) / (np.dot(dx, dg) + 1e-16)
            Hdg = np.dot(H, dg)
            term2 = np.outer(Hdg, Hdg) / (np.dot(dg, Hdg) + 1e-16)
            H = H + term1 - term2
            
        x = x_new
        
    return x, history, k+1

def plot_and_save(history, title, func, minimum, bounds, filename):
    plt.figure(figsize=(8, 6))
    x1 = np.linspace(bounds[0], bounds[1], 400)
    x2 = np.linspace(bounds[2], bounds[3], 400)
    X1, X2 = np.meshgrid(x1, x2)
    Z = func([X1, X2])

    levels = np.logspace(-1, 3, 20) if 'Розенброка' in title else 40
    plt.contour(X1, X2, Z, levels=levels, cmap='Blues_r', alpha=0.6)

    hist_arr = np.array(history)
    # Тонкая линия и маленькие маркеры, чтобы видеть зигзаги
    plt.plot(hist_arr[:, 0], hist_arr[:, 1], 'r.-', linewidth=1.0, markersize=4, label='Траектория')
    plt.plot(hist_arr[0, 0], hist_arr[0, 1], 'go', markersize=8, label='Старт')
    plt.plot(minimum[0], minimum[1], 'b*', markersize=12, label='Минимум')

    plt.title(title, fontsize=12, fontweight='bold')
    plt.xlabel('x1')
    plt.ylabel('x2')
    plt.legend()
    plt.grid(True, linestyle='--', alpha=0.5)

    save_path = os.path.join(SAVE_DIR, filename)
    plt.savefig(save_path, dpi=150, bbox_inches='tight')
    plt.close()

def run_experiment(func_name, func, grad, x0, minimum, bounds, prefix, ls_tol=1e-5):
    obj = Objective(func, grad)
    methods =[
        ("Наискорейший спуск", lambda: steepest_descent(obj, x0, ls_tol=ls_tol), f"{prefix}_steepest.png"),
        ("С. Градиенты (Флетчер-Ривс)", lambda: conjugate_gradient(obj, x0, method='FR', ls_tol=ls_tol), f"{prefix}_cg_fr.png"),
        ("С. Градиенты (Полак-Рибьер)", lambda: conjugate_gradient(obj, x0, method='PR', ls_tol=ls_tol), f"{prefix}_cg_pr.png"),
        ("Перем. Метрика (Бройден)", lambda: variable_metric(obj, x0, method='Broyden', ls_tol=ls_tol), f"{prefix}_vm_broyden.png"),
        ("Перем. Метрика (DFP)", lambda: variable_metric(obj, x0, method='DFP', ls_tol=ls_tol), f"{prefix}_vm_dfp.png")
    ]

    print(f"\n--- Функция: {func_name} (ls_tol = {ls_tol}) ---")
    print(f"{'Метод':<30} | {'Итер.':<6} | {'f(x) вызовы':<12} | {'grad вызовы':<12}")
    print("-" * 65)

    for name, method_func, filename in methods:
        obj.reset_counters()
        x_res, hist, iters = method_func()
        print(f"{name:<30} | {iters:<6} | {obj.f_calls:<12} | {obj.g_calls:<12}")
        plot_and_save(hist, f"{name} ({func_name})", func, minimum, bounds, filename)

def run_all():
    print("Сохраняем графики в папку:", SAVE_DIR)
    
    # Задание 1. Квадратичная функция (Идеальные условия)
    run_experiment("Квадратичная", f_quad, grad_quad,[0.0, 0.0], [3.0, 7.0], 
                   bounds=[-1, 5, -1, 8], prefix="quad")
    
    # Задание 1. Розенброк (Идеальные условия)
    run_experiment("Розенброк", f_rosen, grad_rosen,[-1.2, 1.0], [1.0, 1.0], 
                   bounds=[-1.5, 1.5, -0.5, 1.5], prefix="rosen")

    # Задание 2. Исследование сходимости (Плохой одномерный поиск)
    run_experiment("Квадратичная (грубый поиск)", f_quad, grad_quad, [0.0, 0.0],[3.0, 7.0], 
                   bounds=[-1, 7, -1, 10], prefix="quad_bad_ls", ls_tol=1e-1)
                   
if __name__ == "__main__":
    run_all()