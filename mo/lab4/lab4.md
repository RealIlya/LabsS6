# Лабораторная работа 4

**Выполнили:** Веселый Д. А.; Ворончук И.И.; Лыкова М.Р.

**Группа:** ПМИ-32

**Вариант:** 2

## **Цель:**
Ознакомиться с методом штрафных функций (методом внешней точки) для решения задач нелинейного программирования с ограничениями. Реализовать сведение задачи условной оптимизации к последовательности задач безусловной оптимизации и сравнить эффективность методов вращающихся координат и сопряженных градиентов на промежуточных этапах.

## **Формулировка задания.**

1. С использованием метода штрафных функций решить следующую задачу условной минимизации:
   $$ \min \left\{ 5(6x_1 + 5x_2 - 60)^2 + (x_1 - 2x_2 - 4)^2 \mid 5x_1 + 3x_2 - 30 \le 0 \right\} $$
   
2. Использовать вспомогательную функцию вида:
   $$ Q(\bar{x}, r) = 5(6x_1 + 5x_2 - 60)^2 + (x_1 - 2x_2 - 4)^2 + r \left\{ \frac{1}{2} \left[ g(\bar{x}) + |g(\bar{x})| \right] \right\}^2 $$
   где $g(\bar{x}) = 5x_1 + 3x_2 - 30$.

3. Для решения последовательности задач безусловной минимизации $\min Q(\bar{x}, r)$ использовать:
   * **Метод вращающихся координат** (нулевого порядка);
   * **Метод сопряженных градиентов** (первого порядка).

4. Проиллюстрировать сходимость реализованных алгоритмов, оценить результаты. Сравнить с аналитически известным оптимумом: $\bar{x}^* = \left( -1\frac{75}{113}, 12\frac{262}{339} \right) \approx (-1.6637, 12.7728)$, значение функции $f^* \approx 1161.178$.

---

## 1. Исследование методов при решении задачи методом штрафных функций
*(Начальная точка: $\bar{x}^0 = [0, 0]^T$, начальный коэффициент штрафа $r_0 = 1$, множитель $C = 10$)*

### Вспомогательная безусловная минимизация: Метод вращающихся координат
![alt text](plots_lab4/penalty_rosenbrock.png)

### Вспомогательная безусловная минимизация: Метод сопряженных градиентов (Флетчер-Ривс)
![alt text](plots_lab4/penalty_cg.png)


## 2. Анализ траекторий поиска и сравнение методов

*В ходе выполнения лабораторной работы было замечено, что траектория сходимости алгоритмов не достигает точки $(-1.6637, 12.7728)$, предложенной в задании в качестве аналитического оптимума. Аналитическое решение методом подстановки ограничения показало, что предложенная в методических указаниях точка не является точкой минимума. Истинный условный минимум достигается в точке $\bar{x}^* = (-19/69, 2165/207) \approx (-0.275, 10.459)$, где значение функции равно $f^* \approx 1072.51$, что меньше значения в точке из задания ($1161.178$). Программная реализация метода штрафных функций успешно и с высокой точностью сошлась именно к истинному минимуму.*

**Механика работы метода штрафных функций:**
На графиках четко видно два этапа оптимизации:
1. **Свободный поиск:** На первых итерациях (пока штраф $r$ мал) алгоритм стремится из начала координат к точке глобального (безусловного) минимума функции $f(x)$, которая находится правее допустимой области: $x_{безусл} \approx [8.23, 2.11]$.
2. **Движение вдоль границы:** По мере увеличения параметра штрафа $r$ нарастает «стена» за пределами допустимой области. Траектория отталкивается от безусловного минимума и скользит вдоль границы $5x_1 + 3x_2 - 30 = 0$ вверх и влево, пока не достигает истинного условного минимума $\bar{x}^* \approx (-1.66, 12.77)$.

**Сравнение внутренних методов минимизации:**

* **Метод вращающихся координат (алгоритм Розенброка 0-го порядка):** 
  * *Поведение:* Алгоритм строит ортогональный базис, адаптируясь к форме линий уровня функции $Q(\bar{x}, r)$. По мере роста $r$ функция штрафа формирует резкий «хребет» (овраг) вдоль границы ограничения. 
  * *Эффективность:* Метод вращающихся координат хорошо справляется с овражными структурами. Поскольку он не использует производные (которые терпят разрыв в точке $g(\bar{x})=0$), он сохраняет высокую стабильность, хотя и требует большего числа вычислений самой функции.

* **Метод сопряженных градиентов:** 
  * *Поведение:* Метод опирается на градиент вспомогательной функции $Q(\bar{x}, r)$. При больших значениях штрафа $r$ задача становится плохо обусловленной (производные по нормали к ограничению становятся огромными по сравнению с касательными).
  * *Эффективность:* Вдали от ограничения метод быстро доходит до минимума. Однако вблизи границы, где градиент терпит излом (из-за взятия модуля в функции штрафа), метод сопряженных градиентов начинает совершать зигзаги и может требовать частых рестартов направления. Точность одномерного поиска становится критически важной.

## 3. Достоинства и недостатки методов

### Метод штрафных функций (внешней точки)
**Достоинства:**
* Позволяет использовать мощный и хорошо изученный арсенал методов безусловной оптимизации для решения задач с ограничениями.
* Точка начального приближения $\bar{x}^0$ может лежать как внутри, так и вне допустимой области (в отличие от метода барьерных функций).
* Простая алгоритмическая реализация функции штрафа для ограничений типа неравенств.

**Недостатки:**
* По мере роста коэффициента $r$ матрица Гессе функции $Q(\bar{x}, r)$ становится все более плохо обусловленной. Это резко снижает эффективность градиентных методов (в т.ч. сопряженных градиентов).
* Промежуточные решения всегда лежат вне допустимой области (нарушают ограничения). Если алгоритм прервать досрочно, мы получим недопустимое решение.

### Внутренние методы в условиях штрафных функций
* **Метод вращающихся координат:** Идеально подходит для финальных этапов метода штрафов, так как не нуждается в непрерывности производных и хорошо отслеживает кривизну границы допустимой области. Недостаток — низкая скорость на гладких участках.
* **Метод сопряженных градиентов:** Очень быстр на начальных этапах (при малых $r$), но "спотыкается" на поздних этапах из-за плохой обусловленности штрафной функции. Требует высокоточного одномерного поиска.

## 4. Выводы
1. Метод штрафных функций успешно свел поставленную задачу условной минимизации к последовательности задач безусловной оптимизации. Обе реализации успешно достигли аналитического оптимума не, заявленного в методичке $f^* \approx 1161.178$ в точке $(-1.66, 12.77)$, а истиного оптимума $f^* \approx 1072.4534$ в точке $(-0.273, 10.455)$.
2. Показано, что для безусловной оптимизации плохо обусловленных функций $Q(\bar{x}, r)$ с «изломом» на границе методы нулевого порядка (вращающихся координат) могут быть надежнее методов первого порядка (сопряженных градиентов), так как градиентные методы чувствительны к резким перепадам производных.
3. Практическая сходимость метода сильно зависит от закона изменения параметра штрафа $r$. Слишком быстрое увеличение $r$ приводит к ранней «овражности» и остановке алгоритма, а слишком медленное — к чрезмерному количеству внешних итераций.

## 5. Приложение
Код на python:
```python
import numpy as np
import matplotlib.pyplot as plt
from scipy.optimize import minimize_scalar
import os

SCRIPT_DIR = os.path.dirname(os.path.abspath(__file__))
SAVE_DIR = os.path.join(SCRIPT_DIR, 'plots_lab4')
os.makedirs(SAVE_DIR, exist_ok=True)

def f(x):
    return 5 * (6*x[0] + 5*x[1] - 60)**2 + (x[0] - 2*x[1] - 4)**2

def grad_f(x):
    df_dx0 = 60 * (6*x[0] + 5*x[1] - 60) + 2 * (x[0] - 2*x[1] - 4)
    df_dx1 = 50 * (6*x[0] + 5*x[1] - 60) - 4 * (x[0] - 2*x[1] - 4)
    return np.array([df_dx0, df_dx1])

def g(x):
    """Ограничение g(x) <= 0"""
    return 5*x[0] + 3*x[1] - 30

def grad_g(x):
    return np.array([5.0, 3.0])

def Q(x, r):
    penalty = 0.5 * (g(x) + abs(g(x))) # max(0, g(x))
    return f(x) + r * penalty**2

def grad_Q(x, r):
    penalty = 0.5 * (g(x) + abs(g(x)))
    if penalty > 0:
        return grad_f(x) + 2 * r * penalty * grad_g(x)
    return grad_f(x)

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
    d_norm = np.linalg.norm(d)
    if d_norm < 1e-12:
        return 0.0
        
    d_normalized = d / d_norm
    
    def phi(lam): 
        return Q(x + lam * d_normalized, r)
        
    bracket = bracket_unimodal(phi, start_step=1e-3)
    res = minimize_scalar(phi, method='bounded', bounds=bracket, options={'xatol': tol})
    
    return res.x / d_norm

def gram_schmidt(vecs):
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

def method_cg_unconstrained(x0, r, tol=1e-4, max_iter=1000):
    x = np.array(x0, dtype=float)
    n = len(x)
    
    grad = grad_Q(x, r)
    S = -grad
    
    for k in range(max_iter):
        if np.linalg.norm(grad) < tol: 
            break
            
        lam = line_search(x, S, r, tol=tol)
        x_new = x + lam * S
        
        grad_new = grad_Q(x_new, r)
        
        # Формула Полака-Рибьера (гораздо надежнее Флетчера-Ривса на штрафных функциях)
        omega = np.dot(grad_new, grad_new - grad) / (np.dot(grad, grad) + 1e-16)
        omega = max(0.0, omega) # Если метод заклинило, он сбросится до градиентного спуска
        
        if k > 0 and k % n == 0:
            S = -grad_new
        else:
            S = -grad_new + omega * S
            
        x = x_new
        grad = grad_new
        
    return x

def penalty_method(x0, unconstrained_solver, r0=1.0, C=10.0, tol=1e-4, max_outer_iter=20):
    x = np.array(x0, dtype=float)
    r = r0
    history = [x.copy()]
    
    for k in range(max_outer_iter):
        x_new = unconstrained_solver(x, r)
        history.append(x_new.copy())
        
        penalty_val = 0.5 * (g(x_new) + abs(g(x_new)))
        if penalty_val < tol and np.linalg.norm(x_new - x) < tol:
            break
            
        x = x_new
        r *= C
        
    return x, f(x), history

def plot_penalty(history, title, filename):
    plt.figure(figsize=(10, 8))
    
    x1 = np.linspace(-4, 12, 400)
    x2 = np.linspace(-2, 16, 400)
    X1, X2 = np.meshgrid(x1, x2)
    Z = f([X1, X2])
    
    levels = np.logspace(0, 6, 40)
    plt.contour(X1, X2, Z, levels=levels, cmap='viridis', alpha=0.5)
    
    x_line = np.linspace(-4, 12, 100)
    y_line = (30 - 5*x_line) / 3
    plt.plot(x_line, y_line, 'k-', linewidth=2, label='Граница: 5x1 + 3x2 = 30')
    plt.fill_between(x_line, y_line, 16, color='red', alpha=0.1, label='Недопустимая область (g > 0)')
    
    hist_arr = np.array(history)
    plt.plot(hist_arr[:, 0], hist_arr[:, 1], 'r.-', linewidth=2, markersize=8, label='Траектория (внешние итерации)')
    plt.plot(hist_arr[0, 0], hist_arr[0, 1], 'go', markersize=10, label='Старт [0, 0]')
    
    opt_x = hist_arr[-1, :]
    plt.plot(opt_x[0], opt_x[1], 'b*', markersize=15, markeredgecolor='black', label=f'Оптимум ({opt_x[0]:.2f}, {opt_x[1]:.2f})\nf = {f(opt_x):.2f}')
    
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

def run_lab4():
    print("=== Метод штрафных функций ===")
    x0 =[0.0, 0.0]
    
    print("\n1. Вспомогательный метод: Вращающиеся координаты (Розенброк)")
    res_rosen, f_rosen, hist_rosen = penalty_method(x0, method_rosenbrock_unconstrained)
    print(f"Оптимальная точка: {res_rosen}")
    print(f"Значение функции: {f_rosen:.4f}")
    print(hist_rosen.__len__())
    plot_penalty(hist_rosen, "Метод штрафных функций (Внутренний: Вращающиеся координаты)", "penalty_rosenbrock.png")
    
    print("\n2. Вспомогательный метод: Сопряженные градиенты (Полак-Рибьер)")
    res_cg, f_cg, hist_cg = penalty_method(x0, method_cg_unconstrained)
    print(f"Оптимальная точка: {res_cg}")
    print(f"Значение функции: {f_cg:.4f}")
    print(hist_cg.__len__())
    plot_penalty(hist_cg, "Метод штрафных функций (Внутренний: Сопряженные градиенты)", "penalty_cg.png")

if __name__ == "__main__":
    run_lab4()
```