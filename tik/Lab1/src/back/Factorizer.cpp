#include "Factorizer.hpp"

#include <QDebug>
#include <QFile>
#include <QTextStream>
#include <chrono>

Factorizer::Factorizer(QObject* parent) noexcept : QObject(parent) {
  std::random_device rd;
  rng.seed(rd());
}

QString Factorizer::readFileContent(const QString& filePath) const noexcept {
  QFile inputFile(filePath);
  if (!inputFile.open(QIODevice::ReadOnly | QIODevice::Text)) {
    return "Ошибка: невозможно открыть файл.";
  }
  QTextStream in(&inputFile);
  QString content = in.readLine().trimmed();
  inputFile.close();
  return content;
}

long long Factorizer::randomInRange(long long min, long long max) noexcept {
  std::uniform_int_distribution<long long> dist(min, max);
  return dist(rng);
}

long long Factorizer::gcd(long long a, long long b) noexcept {
  while (b) {
    a %= b;
    std::swap(a, b);
  }
  return a;
}

long long Factorizer::modPow(long long base, long long exp,
                             long long mod) noexcept {
  long long result = 1;
  base %= mod;
  while (exp > 0) {
    if (exp % 2 == 1) {
      // Приведение к __int128 защищает от переполнения при умножении больших
      // чисел
      result = (long long)((__int128)result * base % mod);
    }
    base = (long long)((__int128)base * base % mod);
    exp /= 2;
  }
  return result;
}

bool Factorizer::isPrime(long long n, int iterations) {
  if (n <= 1) return false;
  if (n == 2 || n == 3) return true;
  if (n % 2 == 0) return false;

  long long exponent = (n - 1) / 2;
  for (int i = 0; i < iterations; i++) {
    long long a = randomInRange(2, n - 1);
    long long result = modPow(a, exponent, n);
    if (result != 1 && result != (n - 1)) {
      return false;  // точно составное
    }
  }
  return true;  // вероятно простое
}

long long Factorizer::pollardPminus1(long long n, long long& iterations) {
  // Базы (c), которые мы пробуем, если с 2 не получилось
  long long bases[] = {2, 3, 5, 7, 11};

  for (long long c : bases) {
    long long m = c;
    long long i = 2;
    long long limit = 500000;  // Ограничитель итераций, чтобы не зависнуть

    while (i <= limit) {
      iterations++;
      // Вычисляем m_i = (m_{i-1})^i mod n
      m = modPow(m, i, n);
      long long d = gcd(m > 0 ? m - 1 : n - 1 + m, n);

      if (d > 1 && d < n) {
        return d;  // Нашли нетривиальный делитель!
      }
      if (d == n) {
        break;  // Переходим к следующей базе c
      }
      i++;
    }
  }
  return 0;  // Метод не смог найти делитель
}

void Factorizer::factorizeRecursive(long long n, QMap<long long, int>& factors,
                                    long long& totalIterations) {
  if (n <= 1) return;

  // Если число простое - добавляем в список и выходим
  if (isPrime(n, 20)) {
    factors[n]++;
    return;
  }

  // Отделяем двойки (метод Полларда плохо работает с четными числами)
  if (n % 2 == 0) {
    factors[2]++;
    factorizeRecursive(n / 2, factors, totalIterations);
    return;
  }

  // Пытаемся найти один делитель методом Полларда
  long long d = pollardPminus1(n, totalIterations);

  if (d > 1) {
    // Если нашли делитель, рекурсивно раскладываем обе части
    factorizeRecursive(d, factors, totalIterations);
    factorizeRecursive(n / d, factors, totalIterations);
  } else {
    // Запасной план (перебор), если Поллард не справился (для малых чисел)
    for (long long i = 3; i * i <= n; i += 2) {
      totalIterations++;
      if (n % i == 0) {
        factorizeRecursive(i, factors, totalIterations);
        factorizeRecursive(n / i, factors, totalIterations);
        return;
      }
    }
    factors[n]++;  // Теоретически до сюда не дойдет
  }
}

QString Factorizer::factorize(const QString& input_number) noexcept {
  bool ok;
  long long n = input_number.trimmed().toLongLong(&ok);

  if (!ok || n <= 1) {
    return "Ошибка: Введите корректное составное целое число > 1.";
  }

  // Сначала проверяем на простоту (требование задания)
  if (isPrime(n, 20)) {
    return "Число " + QString::number(n) +
           " является простым (согласно тесту Лемана).\nФакторизация не "
           "требуется.";
  }

  auto start = std::chrono::high_resolution_clock::now();

  long long totalIterations = 0;
  QMap<long long, int> factors;  // Ключ - простой множитель, Значение - его
                                 // степень (сколько раз встречается)

  // Выполняем рекурсивное разложение
  factorizeRecursive(n, factors, totalIterations);

  auto end = std::chrono::high_resolution_clock::now();
  std::chrono::duration<double, std::milli> time_ms = end - start;

  // Формируем красивый вывод результатов
  QString result = "Составное число: " + QString::number(n) + "\n\n";
  result += "Разложение: ";
  QStringList factorsStr;

  for (auto it = factors.begin(); it != factors.end(); ++it) {
    if (it.value() == 1) {
      factorsStr << QString::number(it.key());
    } else {
      factorsStr << QString("%1^%2").arg(it.key()).arg(it.value());
    }
  }

  result += factorsStr.join(" * ") + "\n\n";
  result += "Затраченное время: " + QString::number(time_ms.count()) + " мс\n";
  result +=
      "Кол-во итераций метода Полларда: " + QString::number(totalIterations);

  return result;
}