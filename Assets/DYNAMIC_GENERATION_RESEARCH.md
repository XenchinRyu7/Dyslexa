# 🧠 DYNAMIC QUESTION GENERATION SYSTEM

## 📚 RESEARCH FOUNDATION

### **Visual Dyslexia Patterns (Reading Problems)**

#### 1️⃣ **Mirror/Reflection (Horizontal Flip)**
Penderita disleksia sering **membalik huruf horizontal**:
- **b ↔ d** (PALING UMUM - 80% penderita!)
- **p ↔ q**
- **u ↔ n** (vertical mirror)

**Contoh Error:**
- "bola" dibaca "dola"
- "pintu" dibaca "qintu"

---

#### 2️⃣ **Rotation 180° (Putaran Penuh)**
Huruf yang **dirotasi 180°** terlihat sama:
- **b ↔ q** (b dirotasi jadi q)
- **d ↔ p** (d dirotasi jadi p)
- **m ↔ w** (m dirotasi jadi w)
- **n ↔ u** (n dirotasi jadi u)
- **6 ↔ 9** (angka juga!)

**Contoh Error:**
- "minum" dibaca "winum"
- "mana" dibaca "wana"

---

#### 3️⃣ **Similar Shapes (Bentuk Mirip)**
Huruf dengan **bentuk visual serupa**:
- **h ↔ n** (batang vertikal + lengkungan)
- **c ↔ o ↔ e** (bentuk bulat)
- **v ↔ w ↔ y** (bentuk angular)
- **i ↔ l ↔ 1** (batang vertikal)
- **s ↔ z ↔ 5** (bentuk S)

**Contoh Error:**
- "hati" dibaca "nati"
- "ibu" dibaca "lbu"

---

#### 4️⃣ **Sequence Reversal (Urutan Terbalik)**
Kata yang **terbalik urutannya**:
- **saw ↔ was**
- **top ↔ pot**
- **tap ↔ pat**
- **no ↔ on**
- **god ↔ dog**

**Contoh Error (Bahasa Indonesia):**
- "kita" dibaca "atik"
- "suka" dibaca "akus"

---

### **Phonological Dyslexia Patterns (Listening Problems)**

#### 1️⃣ **Voiced ↔ Voiceless (Bersuara vs Tidak)**
Kesulitan **membedakan suara** yang beda voicing:
- **b ↔ p** (bilabial: bola/pola)
- **d ↔ t** (alveolar: dadu/tadu)
- **g ↔ k** (velar: gula/kula)

**Contoh Error:**
- Mendengar "bola" tapi tulis "pola"
- Mendengar "kaki" tapi tulis "gagi"

---

#### 2️⃣ **Nasal Place Confusion (Posisi Hidung)**
Kesulitan **membedakan posisi nasal**:
- **m ↔ n** (bilabial vs alveolar)
- **n ↔ ng** (alveolar vs velar)

**Contoh Error:**
- "makan" → "nakan"
- "bukan" → "bukam"

---

#### 3️⃣ **Liquid Confusion (L/R - SANGAT UMUM DI INDONESIA!)**
Kesulitan **membedakan L dan R**:
- **l ↔ r** (lateral vs rhotic)

**Contoh Error:**
- "lari" → "rari"
- "rumah" → "lumah"

---

#### 4️⃣ **Similar Articulation**
Suara yang **artikulasinya mirip**:
- **s ↔ z** (sibilants)
- **f ↔ v** (labiodental)

---

## 🎯 DYNAMIC GENERATION ALGORITHM

### **Visual Questions (Reading-based)**

```
Difficulty 1: Single Letter
├─ Pilih dari: mirrorHorizontal (b,d,p,q) atau rotation180 (m,w,n,u)
├─ Generate 3 distractors dari confusion group
└─ Shuffle options

Difficulty 2: Simple Words
├─ Pilih kata: "bola", "dadu", "buku", "mana"
├─ Generate distractors dengan letter substitution (b→d, p→q)
└─ Contoh: "bola" → ["bola", "dola", "pola", "qola"]

Difficulty 3-5: Complex Words
├─ Multiple letter substitutions
├─ Sequence reversals
└─ Heavy confusion patterns
```

---

### **Phonology Questions (Listening-based)**

```
Difficulty 1: Single Letter
├─ Pilih dari available audio: b, d, m, n, p, q, w
├─ Generate distractors dari:
│   ├─ Voiced/Voiceless pairs (b↔p, d↔t)
│   ├─ Nasal confusion (m↔n)
│   └─ Place confusion
└─ Contoh: Audio "b" → options ["B", "P", "D", "M"]

Difficulty 2+: Words
├─ Pilih dari: "bola", "buku", "gula"
├─ Generate distractors dengan consonant substitution
├─ Contoh: Audio "bola" → options ["BOLA", "POLA", "DOLA", "BODA"]
└─ Pattern: B→P/D, L→R/N
```

---

## 🔧 IMPLEMENTATION

### **Usage:**

```csharp
// Initialize
DynamicQuestionGenerator dynamicGen = GetComponent<DynamicQuestionGenerator>();

// Generate visual question
Question visualQ = dynamicGen.GenerateVisualQuestion(difficulty: 3);
// Result: "Pilih kata 'bangku'" 
// Options: ["bangku", "dangku", "paugku", "banqpu"]

// Generate phonology question
Question phonoQ = dynamicGen.GeneratePhonologyQuestion(difficulty: 2);
// Result: "🔊 Dengarkan kata yang disebutkan"
// Audio: "Audio/sound_bola"
// Options: ["BOLA", "POLA", "DOLA", "BODA"]
```

---

## 📊 ADVANTAGES (Dynamic vs JSON)

### ✅ **Dynamic Generation:**
- **Infinite variasi** - tidak pernah habis soal
- **Scalable** - mudah tambah difficulty
- **Adaptive** - bisa adjust pattern berdasarkan error tracking
- **File size kecil** - tidak perlu ratusan JSON entries
- **Research-based** - pattern langsung dari teori disleksia

### ⚠️ **JSON Generation:**
- Fixed content - perlu manual update
- File besar kalau banyak soal
- Tidak adaptive
- ✅ Predictable untuk testing
- ✅ Mudah di-review manual

---

## 🎮 NEXT STEPS

1. **Test Dynamic Generator**
   ```
   - Unity: Create GameObject → Add DynamicQuestionGenerator
   - Console: Lihat generated questions
   - Verify: Confusion patterns benar?
   ```

2. **Hybrid Approach** (RECOMMENDED!)
   ```
   - Difficulty 1-2: JSON (predictable, untuk warmup)
   - Difficulty 3-5: Dynamic (infinite variasi)
   ```

3. **Add to QuestionGenerator.cs**
   ```csharp
   if (useDynamicGeneration && difficulty >= 3)
   {
       return dynamicGenerator.GenerateVisualQuestion(difficulty);
   }
   else
   {
       return LoadFromJSON(difficulty);
   }
   ```

---

## 📚 REFERENCES

**Research Papers:**
- Shaywitz, S. (2003). *Overcoming Dyslexia*
- Ramus, F. (2003). Developmental dyslexia: specific phonological deficit
- Vellutino, F. R. (1979). Visual vs phonological deficits

**Indonesian Context:**
- L/R confusion prevalent in Indonesian speakers
- b/p, d/t, k/g confusion common in Bahasa

---

**READY TO TEST?** 🚀
