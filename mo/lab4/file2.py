import numpy as np
import matplotlib.pyplot as plt
from scipy.optimize import minimize_scalar
import os

SCRIPT_DIR = os.path.dirname(os.path.abspath(__file__))
SAVE_DIR = os.path.join(SCRIPT_DIR, 'plots_lab4')
os.makedirs(SAVE_DIR, exist_ok=True)

# --- 1. Целевая функция и ограничения ---

def f(x):
    """Целевая функция"""
    return 5 * (6*x[0] + 5*x[1] - 60)**2 + (x[0] - 2*x[1] - 4)**2

def grad_f(x):
    """Градиент целевой функции"""
    df_dx0 = 60 * (6*x[0] + 5*x[1] - 60) + 2 * (x[0] - 2*x[1] - 4)
    df_dx1 = 50 * (6*x[0] + 5*x[1] - 60) - 4 * (x[0] - 2*x[1] - 4)
    return np.array([df_dx0, df_dx1])

def g(x):
    """Ограничение g(x) <= 0"""
    return 5*x[0] + 3*x[1] - 30

def grad_g(x):
    return np.array([5.0, 3.0])

# --- 2. Функция штрафа ---

def Q(x, r):
    """Вспомогательная функция Q(x, r) = f(x) + r * P(x)"""
    penalty = 0.5 * (g(x) + abs(g(x))) # max(0, g(x))
    return f(x) + r * penalty**2

def grad_Q(x, r):
    """Градиент функции Q(x, r)"""
    penalty = 0.5 * (g(x) + abs(g(x)))
    if penalty > 0:
        return grad_f(x) + 2 * r * penalty * grad_g(x)
    return grad_f(x)

# --- 3. Одномерный поиск ---

def bracket_unimodal(phi, start_step=1e-3, max_iter=1000):
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

def line_search(x, d, r, tol=1e-5):
    def phi(lam): return Q(x + lam * d, r)
    bracket = bracket_unimodal(phi)
    res = minimize_scalar(phi, method='bounded', bounds=bracket, options={'xatol': tol})
    return res.x

# --- 4. Методы безусловной оптимизации ---

def gram_schmidt(vecs):
    """Ортогонализация Грама–Шмидта для метода вращающихся координат"""
    n = len(vecs)
    basis =[]
    for i, v in enumerate(vecs):
        w = v.copy()
        for b in basis:
            w = w - np.dot(w, b) * b
        nrm = np.linalg.norm(w)
        if nrm > 1e-10:
            basis.append(w / nrm)
        else:
            for idx in range(n):
                e = np.zeros(n)
                e[idx] = 1.0
                w2 = e.copy()
                for b in basis:
                    w2 = w2 - np.dot(w2, b) * b
                nrm2 = np.linalg.norm(w2)
                if nrm2 > 1e-10:
                    basis.append(w2 / nrm2)
                    break
    return np.array(basis)

def method_rosenbrock_unconstrained(x0, r, tol=1e-4, max_iter=200):
    """Метод вращающихся координат (алгоритм Розенброка 0-го порядка)"""
    x = np.array(x0, dtype=float)
    n = len(x)
    S = np.eye(n)
    
    for k in range(max_iter):
        x_start = x.copy()
        lambdas = np.zeros(n)

        for i in range(n):
            lam = line_search(x, S[i], r, tol=tol)
            lambdas[i] = lam
            x = x + lam * S[i]

        if np.linalg.norm(x - x_start) < tol:
            break

        A = np.array([sum(lambdas[j] * S[j] for j in range(i, n)) for i in range(n)])
        if np.linalg.norm(A[0]) < 1e-10:
            S = np.eye(n)
            continue

        S_new = gram_schmidt(A)
        if S_new.shape == (n, n): S = S_new
        else: S = np.eye(n)

    return x

def method_cg_unconstrained(x0, r, tol=1e-4, max_iter=200):
    """Метод сопряженных градиентов (Флетчер-Ривс)"""
    x = np.array(x0, dtype=float)
    n = len(x)
    
    grad = grad_Q(x, r)
    S = -grad
    
    for k in range(max_iter):
        if np.linalg.norm(grad) < tol: 
            break
            
        if k > 0 and k % n == 0:
            S = -grad
            
        lam = line_search(x, S, r, tol=tol)
        x_new = x + lam * S
        
        grad_new = grad_Q(x_new, r)
        omega = np.dot(grad_new, grad_new) / (np.dot(grad, grad) + 1e-16)
        
        S = -grad_new + omega * S
        x = x_new
        grad = grad_new
        
    return x

# --- 5. Метод штрафных функций ---

def penalty_method(x0, unconstrained_solver, r0=1.0, C=10.0, tol=1e-4, max_outer_iter=20):
    x = np.array(x0, dtype=float)
    r = r0
    history = [x.copy()]
    
    for k in range(max_outer_iter):
        # Решаем задачу безусловной оптимизации
        x_new = unconstrained_solver(x, r)
        history.append(x_new.copy())
        
        # Проверка критерия останова: ограничение выполнено и шаг мал
        penalty_val = 0.5 * (g(x_new) + abs(g(x_new)))
        if penalty_val < tol and np.linalg.norm(x_new - x) < tol:
            break
            
        x = x_new
        r *= C # Увеличиваем штраф
        
    return x, f(x), history

# --- 6. Визуализация ---

def plot_penalty(history, title, filename):
    plt.figure(figsize=(10, 8))
    
    # Сетка для изолиний
    x1 = np.linspace(-4, 12, 400)
    x2 = np.linspace(-2, 16, 400)
    X1, X2 = np.meshgrid(x1, x2)
    Z = f([X1, X2])
    
    # Линии уровня исходной функции
    levels = np.logspace(0, 6, 40)
    plt.contour(X1, X2, Z, levels=levels, cmap='viridis', alpha=0.5)
    
    # Отрисовка ограничения 5x1 + 3x2 <= 30
    x_line = np.linspace(-4, 12, 100)
    y_line = (30 - 5*x_line) / 3
    plt.plot(x_line, y_line, 'k-', linewidth=2, label='Граница: 5x1 + 3x2 = 30')
    plt.fill_between(x_line, y_line, 16, color='red', alpha=0.1, label='Недопустимая область (g > 0)')
    
    # Траектория
    hist_arr = np.array(history)
    plt.plot(hist_arr[:, 0], hist_arr[:, 1], 'r.-', linewidth=2, markersize=8, label='Траектория (внешние итерации)')
    plt.plot(hist_arr[0, 0], hist_arr[0, 1], 'go', markersize=10, label='Старт [0, 0]')
    
    # Аналитический минимум
    opt_x =[-75/113, 12 + 262/339]
    plt.plot(opt_x[0], opt_x[1], 'b*', markersize=15, markeredgecolor='black', label=f'Оптимум (-1.66, 12.77)\nf = {f(opt_x):.2f}')
    
    plt.title(title, fontsize=14, fontweight='bold')
    plt.xlabel('x1', fontsize=12)
    plt.ylabel('x2', fontsize=12)
    plt.legend()
    plt.grid(True, linestyle='--', alpha=0.5)
    
    plt.xlim([-4, 10])
    plt.ylim([-2, 16])
    
    save_path = os.path.join(SAVE_DIR, filename)
    plt.savefig(save_path, dpi=150, bbox_inches='tight')
    plt.close()
    print(f"Сохранен график: {filename}")

# --- 7. Запуск ---

def run_lab4():
    print("=== Метод штрафных функций ===")
    x0 = [0.0, 0.0]
    
    print("\n1. Вспомогательный метод: Вращающиеся координаты (Розенброк)")
    res_rosen, f_rosen, hist_rosen = penalty_method(x0, method_rosenbrock_unconstrained)
    print(f"Оптимальная точка: {res_rosen}")
    print(f"Значение функции: {f_rosen:.4f}")
    plot_penalty(hist_rosen, "Метод штрафных функций (Внутренний: Вращающиеся координаты)", "penalty_rosenbrock.png")
    
    print("\n2. Вспомогательный метод: Сопряженные градиенты")
    res_cg, f_cg, hist_cg = penalty_method(x0, method_cg_unconstrained)
    print(f"Оптимальная точка: {res_cg}")
    print(f"Значение функции: {f_cg:.4f}")
    plot_penalty(hist_cg, "Метод штрафных функций (Внутренний: Сопряженные градиенты)", "penalty_cg.png")

if __name__ == "__main__":
    run_lab4()