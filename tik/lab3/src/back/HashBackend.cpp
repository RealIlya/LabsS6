#include "HashBackend.hpp"

#include <QDebug>
#include <QFile>
#include <QTextStream>

// Константы из методички
const uint32_t R1[80] = {
    0,  1, 2,  3, 4,  5,  6, 7,  8, 9,  10, 11, 12, 13, 14, 15, 7,  4,  13, 1,
    10, 6, 15, 3, 12, 0,  9, 5,  2, 14, 11, 8,  3,  10, 14, 4,  9,  15, 8,  1,
    2,  7, 0,  6, 13, 11, 5, 12, 1, 9,  11, 10, 0,  8,  12, 4,  13, 3,  7,  15,
    14, 5, 6,  2, 4,  0,  5, 9,  7, 12, 2,  10, 14, 1,  3,  8,  11, 6,  15, 13};
const uint32_t R2[80] = {
    5,  14, 7,  0,  9,  2,  11, 4,  13, 6, 15, 8, 1,  10, 3,  12, 6, 11, 3, 7,
    0,  13, 5,  10, 14, 15, 8,  12, 4,  9, 1,  2, 15, 5,  1,  3,  7, 14, 6, 9,
    11, 8,  12, 2,  10, 0,  4,  13, 8,  6, 4,  1, 3,  11, 15, 0,  5, 12, 2, 13,
    9,  7,  10, 14, 12, 15, 10, 4,  1,  5, 8,  7, 6,  2,  13, 14, 0, 3,  9, 11};
const uint32_t S1[80] = {11, 14, 15, 12, 5,  8,  7,  9,  11, 13, 14, 15, 6,  7,
                         9,  8,  7,  6,  8,  13, 11, 9,  7,  15, 7,  12, 15, 9,
                         11, 7,  13, 12, 11, 13, 6,  7,  14, 9,  13, 15, 14, 8,
                         13, 6,  5,  12, 7,  5,  11, 12, 14, 15, 14, 15, 9,  8,
                         9,  14, 5,  6,  8,  6,  5,  12, 9,  15, 5,  11, 6,  8,
                         13, 12, 5,  12, 13, 14, 11, 8,  5,  6};
const uint32_t S2[80] = {8,  9,  9,  11, 13, 15, 15, 5,  7,  7,  8,  11, 14, 14,
                         12, 6,  9,  13, 15, 7,  12, 8,  9,  11, 7,  7,  12, 7,
                         6,  15, 13, 11, 9,  7,  15, 11, 8,  6,  6,  14, 12, 13,
                         5,  14, 13, 13, 7,  5,  15, 5,  8,  11, 14, 14, 6,  14,
                         6,  9,  12, 9,  12, 5,  15, 8,  8,  5,  12, 9,  12, 5,
                         14, 6,  8,  13, 6,  5,  15, 13, 11, 11};

uint32_t f(int j, uint32_t x, uint32_t y, uint32_t z) {
  if (j <= 15) return x ^ y ^ z;
  if (j <= 31) return (x & y) | (~x & z);
  if (j <= 47) return (x | ~y) ^ z;
  if (j <= 63) return (x & z) | (y & ~z);
  return x ^ (y | ~z);
}

uint32_t K1(int j) {
  if (j <= 15) return 0x00000000;
  if (j <= 31) return 0x5a827999;
  if (j <= 47) return 0x6ed9eba1;
  if (j <= 63) return 0x8f1bbcdc;
  return 0xa953fd4e;
}

uint32_t K2(int j) {
  if (j <= 15) return 0x50a28be6;
  if (j <= 31) return 0x5c4dd124;
  if (j <= 47) return 0x6d703ef3;
  if (j <= 63) return 0x7a6d76e9;
  return 0x00000000;
}

HashBackend::HashBackend(QObject* parent) noexcept : QObject(parent) {}

QString HashBackend::readFileContent(const QString& filePath) const noexcept {
  QFile inputFile(filePath);
  if (!inputFile.open(QIODevice::ReadOnly | QIODevice::Text)) {
    return "Ошибка: невозможно открыть файл.";
  }
  QTextStream in(&inputFile);
  QString content = in.readAll();
  inputFile.close();
  return content;
}

bool HashBackend::saveFileContent(const QString& filePath,
                                  const QString& content) const noexcept {
  QFile outputFile(filePath);
  if (!outputFile.open(QIODevice::WriteOnly | QIODevice::Text)) {
    return false;
  }
  QTextStream out(&outputFile);
  out << content;
  outputFile.close();
  return true;
}

std::vector<uint8_t> HashBackend::padMessage(const QString& input) {
  QByteArray data = input.toUtf8();
  uint64_t bitLength = data.size() * 8;

  std::vector<uint8_t> padded(data.begin(), data.end());
  padded.push_back(0x80);  // Добавляем 1 бит

  while (padded.size() % 64 != 56) padded.push_back(0x00);  // Добавляем нули

  for (int i = 0; i < 8; i++) padded.push_back((bitLength >> (i * 8)) & 0xFF);
  return padded;
}

void HashBackend::processBlock(const uint8_t* block, uint32_t* H) {
  uint32_t X[16];
  std::memcpy(X, block, 64);

  uint32_t A1 = H[0], B1 = H[1], C1 = H[2], D1 = H[3], E1 = H[4];
  uint32_t A2 = H[5], B2 = H[6], C2 = H[7], D2 = H[8], E2 = H[9];

  for (int j = 0; j < 80; ++j) {
    uint32_t T1 =
        std::rotl(A1 + f(j, B1, C1, D1) + X[R1[j]] + K1(j), S1[j]) + E1;
    A1 = E1;
    E1 = D1;
    D1 = std::rotl(C1, 10);
    C1 = B1;
    B1 = T1;

    uint32_t T2 =
        std::rotl(A2 + f(79 - j, B2, C2, D2) + X[R2[j]] + K2(j), S2[j]) + E2;
    A2 = E2;
    E2 = D2;
    D2 = std::rotl(C2, 10);
    C2 = B2;
    B2 = T2;

    if (j == 15) std::swap(B1, B2);
    if (j == 31) std::swap(D1, D2);
    if (j == 47) std::swap(A1, A2);
    if (j == 63) std::swap(C1, C2);
    if (j == 79) std::swap(E1, E2);
  }

  uint32_t T = H[1] + C1 + D2;
  H[1] = H[2] + D1 + E2;
  H[2] = H[3] + E1 + A2;
  H[3] = H[4] + A1 + B2;
  H[4] = H[0] + B1 + C2;
  H[0] = T;

  T = H[6] + C2 + D1;
  H[6] = H[7] + D2 + E1;
  H[7] = H[8] + E2 + A1;
  H[8] = H[9] + A2 + B1;
  H[9] = H[5] + B2 + C1;
  H[5] = T;
}

void HashBackend::processBlockWithTrace(
    const uint8_t* block, std::vector<std::vector<uint32_t>>& trace) {
  uint32_t X[16];
  std::memcpy(X, block, 64);

  // Инициализация по методичке RIPEMD-320
  uint32_t A1 = 0x67452301, B1 = 0xefcdab89, C1 = 0x98badcfe, D1 = 0x10325476,
           E1 = 0xc3d2e1f0;
  uint32_t A2 = 0x76543210, B2 = 0xfedcba98, C2 = 0x89abcdef, D2 = 0x01234567,
           E2 = 0x3c2d1e0f;
  for (int j = 0; j < 80; ++j) {
    uint32_t T1 =
        std::rotl(A1 = f(j, B1, C1, D1) + X[R1[j]] + K1(j), S1[j]) + E1;
    A1 = E1;
    E1 = D1;
    D1 = std::rotl(C1, 10);
    C1 = B1;
    B1 = T1;
    uint32_t T2 =
        std::rotl(A2 = f(79 - j, B2, C2, D2) + X[R2[j]] + K2(j), S2[j]) + E2;
    A2 = E2;
    E2 = D2;
    D2 = std::rotl(C2, 10);
    C2 = B2;
    B2 = T2;

    if (j == 15) std::swap(B1, B2);
    if (j == 31) std::swap(D1, D2);
    if (j == 47) std::swap(A1, A2);
    if (j == 63) std::swap(C1, C2);
    if (j == 79) std::swap(E1, E2);
    trace.push_back({A1, B1, C1, D1, E1, A2, B2, C2, D2, E2});
  }
}

QString HashBackend::calculateHash(const QString& input) noexcept {
  std::vector<uint8_t> padded = padMessage(input);

  uint32_t H[10] = {0x67452301, 0xefcdab89, 0x98badcfe, 0x10325476, 0xc3d2e1f0,
                    0x76543210, 0xfedcba98, 0x89abcdef, 0x01234567, 0x3c2d1e0f};

  for (size_t i = 0; i < padded.size(); i += 64) processBlock(&padded[i], H);
  std::stringstream ss;
  for (int i = 0; i < 10; ++i) {
    uint8_t* bytes = reinterpret_cast<uint8_t*>(&H[i]);
    for (int b = 0; b < 4; ++b)
      ss << std::hex << std::setfill('0') << std::setw(2) << (int)bytes[b];
  }
  return QString::fromStdString(ss.str());
}

int HashBackend::calculateHammingDistance(const std::vector<uint32_t>& state1,
                                          const std::vector<uint32_t>& state2) {
  int distance = 0;
  for (size_t i = 0; i < 10; ++i)
    distance += std::popcount(state1[i] ^ state2[i]);
  return distance;
}

QVariantList HashBackend::analyzeAvalanche(const QString& input,
                                           int bitIndexToFlip) noexcept {
  QVariantList results;
  std::vector<uint8_t> block1 = padMessage(input);
  block1.resize(64);
  auto block2 = block1;
  if (bitIndexToFlip >= 0 && bitIndexToFlip < 512) {
    int byteIndex = bitIndexToFlip / 8;
    int bitInByte = 7 - (bitIndexToFlip % 8);
    block2[byteIndex] ^= (1 << bitInByte);
  }
  std::vector<std::vector<uint32_t>> trace1;
  std::vector<std::vector<uint32_t>> trace2;
  processBlockWithTrace(&block1[0], trace1);
  processBlockWithTrace(&block2[0], trace2);
  for (int j = 0; j < 80; ++j)
    results.append(calculateHammingDistance(trace1[j], trace2[j]));
  return results;
}