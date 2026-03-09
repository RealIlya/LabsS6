#pragma once

#include <QMap>
#include <QObject>
#include <QString>
#include <QVariantList>
#include <cstdint>
#include <vector>

class HashBackend : public QObject {
  Q_OBJECT
 private:
  std::vector<uint8_t> padMessage(const QString& input);
  void processBlock(const uint8_t* block, uint32_t* hashState);
  void processBlockWithTrace(const uint8_t* block,
                             std::vector<std::vector<uint32_t>>& trace);
  int calculateHammingDistance(const std::vector<uint32_t>& state1,
                               const std::vector<uint32_t>& state2);

 public:
  explicit HashBackend(QObject* parent = nullptr) noexcept;

  Q_INVOKABLE QString readFileContent(const QString& filePath) const noexcept;
  Q_INVOKABLE bool saveFileContent(const QString& filePath,
                                   const QString& content) const noexcept;
  Q_INVOKABLE QString calculateHash(const QString& input) noexcept;

  Q_INVOKABLE QVariantList analyzeAvalanche(const QString& input,
                                            int bitIndexToFlip) noexcept;
};