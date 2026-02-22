#pragma once

#include <QMap>
#include <QObject>
#include <QString>
#include <random>

class Factorizer : public QObject {
  Q_OBJECT
 private:
  std::mt19937_64 rng;

  // Математические утилиты
  long long randomInRange(long long min, long long max) noexcept;
  long long gcd(long long a, long long b) noexcept;

  // Быстрое возведение в степень с использованием __int128 для предотвращения
  // переполнений
  long long modPow(long long base, long long exp, long long mod) noexcept;

  // Тест простоты Лемана (из ЛР №6)
  bool isPrime(long long n, int iterations);

  // Метод (p-1) Полларда для поиска одного делителя
  long long pollardPminus1(long long n, long long& iterations);

  // Рекурсивная функция для полного разложения числа
  void factorizeRecursive(long long n, QMap<long long, int>& factors,
                          long long& totalIterations);

 public:
  explicit Factorizer(QObject* parent = nullptr) noexcept;

  Q_INVOKABLE QString readFileContent(const QString& filePath) const noexcept;
  Q_INVOKABLE QString factorize(const QString& input_number) noexcept;
};