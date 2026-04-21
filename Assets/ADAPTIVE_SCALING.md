# 🎯 Adaptive Difficulty Scaling System

## 📊 Konsep Global Difficulty

System ini menggunakan **GLOBAL PERSISTENT DIFFICULTY** yang berkembang seiring progress player di seluruh game.

### Difficulty Level (1-5)

| Level | Phonology | Visual | Target Player |
|-------|-----------|--------|---------------|
| **1** | Single letter (B, A, M) | Clear contrast (O vs I) | Pemula/kesulitan tinggi |
| **2** | Simple syllables (BA, KA) | Similar shapes (P vs R) | Pemula berkembang |
| **3** | Words (BOLA, KAKI) | Confusing pairs (b vs d) | Menengah |
| **4** | Blends (KRAN, PRIA) | Rotations (M vs W) | Menengah-mahir |
| **5** | Complex words (KONSTRUKSI) | Multi-distractor | Mahir |

---

## 🔄 Cara Kerja Scaling

### 1️⃣ First Session (Node 0)
```
Player mulai game
  ↓
Global Difficulty = 1 (default)
Phonology Weight = 0.5 (50%)
Visual Weight = 0.5 (50%)
  ↓
Generate 15 questions:
  - 7-8 Phonology level 1
  - 7-8 Visual level 1
  ↓
Player selesai dengan accuracy 90%
  ↓
RuleEngine evaluasi:
  ✓ Accuracy >= 85% → Difficulty NAIK
  ✓ Global Difficulty = 2
  ✓ Save ke ProgressManager
```

### 2️⃣ Second Session (Node 1)
```
Player klik Node 1
  ↓
Load Global Difficulty = 2 (dari session sebelumnya!)
Phonology Weight = 0.5
Visual Weight = 0.5
  ↓
Generate 15 questions:
  - 7-8 Phonology level 2 (suku kata)
  - 7-8 Visual level 2 (similar shapes)
  ↓
Player selesai dengan accuracy 50%
  ↓
RuleEngine evaluasi:
  ✗ Accuracy < 60% → Difficulty TURUN
  ✓ Global Difficulty = 1
  ✓ Save ke ProgressManager
```

### 3️⃣ Third Session (Node 2)
```
Player klik Node 2
  ↓
Load Global Difficulty = 1 (turun dari session sebelumnya)
  ↓
Generate 15 questions level 1 lagi
  ↓
Player selesai accuracy 70%
  ↓
RuleEngine evaluasi:
  ○ 60% < Accuracy < 85% → Difficulty TETAP
  ✓ Global Difficulty = 1 (tidak berubah)
```

---

## 🎲 Content Weight Adaptation

Selain difficulty, system juga menyesuaikan **proporsi jenis soal** berdasarkan error pattern:

### Scenario A: Phonology Errors > Visual Errors
```
Session metrics:
  - Kesalahan Fonologis: 5
  - Kesalahan Visual: 2
  ↓
RuleEngine adjustment:
  Phonology Weight: 0.5 → 0.6 (+0.1)
  Visual Weight: 0.5 → 0.4
  ↓
Next session:
  - 9 Phonology questions (60%)
  - 6 Visual questions (40%)
```

### Scenario B: Visual Errors > Phonology Errors
```
Session metrics:
  - Kesalahan Fonologis: 1
  - Kesalahan Visual: 6
  ↓
RuleEngine adjustment:
  Phonology Weight: 0.5 → 0.4
  Visual Weight: 0.5 → 0.6 (+0.1)
  ↓
Next session:
  - 6 Phonology questions (40%)
  - 9 Visual questions (60%)
```

Weights selalu **normalized** agar total = 1.0

---

## 💾 Persistence System

### ProgressManager (Singleton)
Menyimpan global state di `game_progress.json`:
```json
{
  "currentDifficulty": 3,
  "phonologyWeight": 0.6,
  "visualWeight": 0.4,
  "currentUnlockedNode": 5,
  "totalSessionsCompleted": 12,
  "overallAccuracy": 0.78
}
```

**Persistence berarti:**
- Player tutup game → difficulty TETAP tersimpan
- Buka lagi → lanjut dari difficulty terakhir
- Node unlock TETAP tersimpan
- Overall accuracy tracked untuk research

---

## 🧠 Decision Rules

### Difficulty Adjustment
```csharp
if (accuracy >= 85% && hint_rate < 20%)
    difficulty++  // NAIK
else if (accuracy < 60%)
    difficulty--  // TURUN
else
    difficulty = difficulty  // TETAP

// Clamp antara 1-5
difficulty = Clamp(difficulty, 1, 5)
```

### Node Unlock
```csharp
if (accuracy >= 80%)
    unlock_next_node()
```

### Weight Adjustment
```csharp
if (fonologis_errors > visual_errors)
    phonology_weight += 0.1
else if (visual_errors > fonologis_errors)
    visual_weight += 0.1

// Normalize
total = phonology_weight + visual_weight
phonology_weight /= total
visual_weight /= total
```

---

## 📈 Progression Path Example

| Session | Node | Difficulty | Accuracy | Action | Next Difficulty |
|---------|------|------------|----------|--------|-----------------|
| 1 | 0 | 1 | 90% | Naik | 2 |
| 2 | 1 | 2 | 87% | Naik | 3 |
| 3 | 2 | 3 | 75% | Tetap | 3 |
| 4 | 2 (retry) | 3 | 50% | Turun | 2 |
| 5 | 3 | 2 | 88% | Naik | 3 |
| 6 | 4 | 3 | 92% | Naik | 4 |
| 7 | 5 | 4 | 85% | Naik | 5 |
| 8 | 6 | 5 | 80% | Tetap | 5 |

Player **tidak bisa застряться** di level terlalu sulit karena ada **auto-tuning**.

---

## 🔥 Key Advantages

### ✅ Player Centric
- Tidak ada frustasi karena terlalu sulit
- Tidak ada bosan karena terlalu mudah
- Auto-adjust ke kemampuan individual

### ✅ Research Ready
- Semua data terlog (JSON)
- Track progression individual
- Identifikasi pola error (fonologis vs visual)
- Baseline untuk ML comparison

### ✅ Scalable
- Mudah tambah difficulty level
- Mudah tambah jenis soal
- Mudah adjust parameter (threshold, weight increment)

---

## 📝 Implementation Checklist

- [x] Question banks (JSON) - 5 levels per type
- [x] QuestionGenerator - Load dari JSON
- [x] RuleEngine - Adaptive logic
- [x] ProgressManager - Global persistence
- [x] Logger - Track semua metrics
- [x] LevelMapGenerator - Unlock logic
- [x] GameSessionManager - Session loop

---

## 🚀 Next Steps

1. **Test di Unity Editor**
   - Verify JSON loading
   - Test session flow
   - Check difficulty progression

2. **Tune Parameters**
   - Adjust threshold (saat ini 85% / 60%)
   - Adjust weight increment (saat ini 0.1)
   - Test dengan user sebenarnya

3. **Add Features**
   - Hint system
   - Time pressure variation
   - Visual feedback improvements

4. **Research Phase**
   - Collect data dari 30 anak
   - Analyze confusion patterns
   - Prepare ML training dataset
