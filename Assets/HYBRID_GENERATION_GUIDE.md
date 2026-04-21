# 🎮 HYBRID QUESTION GENERATION SYSTEM

## 🎯 CARA KERJA

### **Mode 1: JSON Generation (Difficulty 1-2)**
- **Predictable** - soal tetap dari JSON
- **Controlled** - mudah di-review manual
- **Warmup phase** - untuk pengenalan awal

### **Mode 2: Dynamic Generation (Difficulty 3-5)**  
- **Infinite variability** - tidak pernah habis soal
- **Research-based** - pakai confusion patterns
- **Scalable** - bisa tambah complexity

---

## ⚙️ SETUP DI UNITY

### **Step 1: Add Component**
```
GameSession GameObject:
├─ GameSessionManager (existing)
├─ QuestionGenerator (existing)
└─ DynamicQuestionGenerator (BARU - auto-added)
```

### **Step 2: Configure QuestionGenerator**
Di Inspector:
```
QuestionGenerator:
├─ Use Dynamic Generation: ✅ TRUE
└─ Dynamic Generation Threshold: 3
```

**Artinya:**
- Difficulty 1-2 → JSON (file phonology_questions.json, visual_questions.json)
- Difficulty 3-5 → Dynamic (algorithmic generation)

### **Step 3: Test!**
```
1. Play game
2. Console akan show:
   "[QuestionGenerator] Using DYNAMIC visual generation (difficulty 3)"
3. Lihat variasi soal - setiap session beda!
```

---

## 📊 COMPARISON

### **JSON Mode:**
```
✅ Predictable content
✅ Manual review possible
✅ Good for testing
❌ Limited questions (40 per type)
❌ Repetitive after many sessions
❌ Fixed distractors
```

### **Dynamic Mode:**
```
✅ Infinite questions
✅ Never runs out
✅ Research-validated patterns
✅ Adaptive to errors
❌ Harder to predict
❌ Requires testing patterns
```

---

## 🧪 TESTING CHECKLIST

### **Test Dynamic Visual:**
```
1. Set difficulty = 3
2. Generate 10 visual questions
3. Check:
   ✓ Confusion patterns benar? (b/d/p/q, m/w/n/u)
   ✓ Options tidak duplicate?
   ✓ CorrectAnswer ada di options?
   ✓ Stimulus jelas?
```

### **Test Dynamic Phonology:**
```
1. Set difficulty = 3
2. Generate 10 phonology questions
3. Check:
   ✓ Audio file exists?
   ✓ Distractors phonologically similar?
   ✓ Pattern: b/p, d/t, k/g?
   ✓ Audio plays correctly?
```

---

## 🔧 CUSTOMIZATION

### **Change Threshold:**
```csharp
// Di Unity Inspector:
dynamicGenerationThreshold = 2; // Start dynamic dari difficulty 2

// Atau di code:
questionGenerator.dynamicGenerationThreshold = 4; // Only use dynamic for difficulty 4-5
```

### **Disable Dynamic:**
```csharp
// Pure JSON mode:
questionGenerator.useDynamicGeneration = false;
```

### **Pure Dynamic Mode:**
```csharp
// No JSON, all dynamic:
questionGenerator.useDynamicGeneration = true;
questionGenerator.dynamicGenerationThreshold = 1; // Semua difficulty dynamic
```

---

## 📈 PHASE 2: ADAPTIVE PATTERNS

Nanti bisa dikembangkan:

```csharp
// Track player errors
if (player.errorPattern == "b/d confusion")
{
    // Generate MORE b/d questions
    dynamicGenerator.IncreaseBDConfusion();
}

if (player.errorPattern == "phonology weak")
{
    // Generate phonology with specific patterns
    dynamicGenerator.FocusOnVoicedVoiceless();
}
```

---

## 🎯 RECOMMENDATIONS

### **Untuk Research:**
```
Difficulty 1-2: JSON (consistent baseline)
Difficulty 3-5: Dynamic (test adaptability)
```

### **Untuk Production:**
```
All Dynamic (infinite content)
```

### **Untuk Testing:**
```
All JSON (predictable qa)
```

---

## 🚀 NEXT ACTIONS

1. **Test di Unity:**
   ```
   - Play session dengan difficulty 1 → lihat JSON questions
   - Naik ke difficulty 3 → lihat dynamic questions
   - Console: verify generation mode
   ```

2. **Validate Patterns:**
   ```
   - Visual: b→d substitution benar?
   - Phonology: b→p confusion benar?
   - No crashes, no nulls?
   ```

3. **Collect Data:**
   ```
   - Log semua generated questions
   - Analyze: pattern distribution correct?
   - Adjust weights if needed
   ```

---

**READY TO TEST!** 🎮

Cek Console untuk debug messages:
```
[QuestionGenerator] Using DYNAMIC visual generation (difficulty 3)
[QuestionGenerator] Using JSON phonology generation (difficulty 1)
```
