#pragma once

#include <QObject>
#include <QString>
#include <cstdint>

class GeneratorANSI : public QObject {
  Q_OBJECT
 public:
  explicit GeneratorANSI(QObject* parent = nullptr) noexcept;

  // Файловые операции
  Q_INVOKABLE QString readFileContent(const QString& filePath) const noexcept;
  Q_INVOKABLE bool saveFileContent(const QString& filePath,
                                   const QString& content) const noexcept;

  // Генерация ПСП
  Q_INVOKABLE QString generateANSI(const QString& k1Hex, const QString& k2Hex,
                                   const QString& s0Hex, int m) noexcept;

  // Статистические тесты
  Q_INVOKABLE QString runTests(const QString& bitSequence) noexcept;

 private:
  // Математика DES / 3DES
  uint64_t desEncrypt(uint64_t block, uint64_t key) noexcept;
  uint64_t desDecrypt(uint64_t block, uint64_t key) noexcept;
  uint64_t encrypt3DES(uint64_t data, uint64_t k1, uint64_t k2) noexcept;

  // Тесты
  QString frequencyTest(const QString& seq) noexcept;
  QString runTest(const QString& seq) noexcept;
  QString extendedDeviationTest(const QString& seq) noexcept;
};