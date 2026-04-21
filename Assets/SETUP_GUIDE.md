# 🚀 SETUP GUIDE - Start Here!

## Status Project
- ✅ Semua script C# sudah dibuat (12 files)
- ✅ JSON question banks sudah siap (80 soal)
- ✅ Scenes sudah ada (MainMenu, LevelMap, GameSession)
- ⚠️ Tinggal setup references di Unity Editor

---

## 📋 STEP-BY-STEP SETUP

### STEP 1: Buat Prefab yang Kurang

#### 1.1 Create AnswerButton Prefab
```
1. Klik kanan di Prefabs folder → Create → UI → Button - TextMeshPro
2. Rename jadi "AnswerButton"
3. Struktur:
   AnswerButton (GameObject)
   ├── Button component
   ├── Image component (background)
   └── Text (TMP) - child object untuk teks jawaban

4. Setup Button:
   - Width: 400
   - Height: 80
   - Colors: Normal=white, Highlighted=light blue, Pressed=blue

5. Setup Text (TMP child):
   - Anchor: Stretch (fill parent)
   - Font Size: 24
   - Alignment: Center Middle
   - Color: Black
   - Text: "Option"

6. Drag ke Prefabs folder
7. Delete dari Hierarchy
```

---

### STEP 2: Setup GameSession Scene

**Priority: TERTINGGI - Scene paling penting!**

#### 2.1 Open GameSession Scene
```
Assets/Scenes/GameSession.unity
```

#### 2.2 Create UI Hierarchy
```
Canvas (sudah ada?)
├── Background (Image)
│   └── Set sprite dari Resources/Background
│
├── QuizTitle (TextMeshProUGUI)
│   - Anchor: Top Center
│   - PosY: -50
│   - Font Size: 36
│   - Text: "Session 1"
│
├── ProgressContainer (GameObject + HorizontalLayoutGroup)
│   - Anchor: Top Center
│   - PosY: -120
│   - Width: 800, Height: 40
│   - HorizontalLayoutGroup:
│     • Spacing: 10
│     • Child Alignment: Middle Center
│     • Child Control Size: Width & Height
│
├── QuestionText (TextMeshProUGUI)
│   - Anchor: Middle Center
│   - PosY: 100
│   - Width: 700, Height: 200
│   - Font Size: 32
│   - Alignment: Center Middle
│   - Text: "Pertanyaan akan muncul di sini"
│
├── AnswerContainer (GameObject + VerticalLayoutGroup)
│   - Anchor: Middle Center
│   - PosY: -100
│   - Width: 500, Height: 400
│   - VerticalLayoutGroup:
│     • Spacing: 15
│     • Child Alignment: Middle Center
│     • Child Control Size: Width
│
└── FeedbackPanel (Panel/Image)
    - Anchor: Stretch All
    - Color: Semi-transparent black (0,0,0,200)
    - Active: FALSE (hidden by default)
    └── FeedbackText (TextMeshProUGUI)
        - Anchor: Middle Center
        - Font Size: 48
        - Alignment: Center Middle
        - Text: "Benar! ✓"
```

#### 2.3 Create Managers GameObject
```
1. Hierarchy klik kanan → Create Empty
2. Rename: "Managers"
3. Add component: GameSessionManager
4. GameSessionManager akan auto-add:
   - QuestionGenerator
   - RuleEngine
   - Logger
```

#### 2.4 Assign References di GameSessionManager
```
Inspector → GameSessionManager:

[Progress Bar]
• Slot Prefab: Drag "Prefabs/ProgressSlot"
• Progress Container: Drag "Canvas/ProgressContainer"

[UI Elements]
• Quiz Title Text: Drag "Canvas/QuizTitle"
• Question Text: Drag "Canvas/QuestionText"
• Answer Container: Drag "Canvas/AnswerContainer"
• Answer Button Prefab: Drag "Prefabs/AnswerButton"
• Feedback Panel: Drag "Canvas/FeedbackPanel"
• Feedback Text: Drag "Canvas/FeedbackPanel/FeedbackText"

[Session Settings]
• Total Questions: 15
• Node Index: 0 (auto-set saat runtime)
```

#### 2.5 TEST GameSession
```
1. Play scene
2. Cek Console:
   ✓ "[QuestionGenerator] Loaded phonology question bank"
   ✓ "[QuestionGenerator] Loaded visual question bank"
   ✓ "[GameSession] Generated 15 questions"
   ✓ Progress bar muncul (15 slot)
   ✓ Pertanyaan muncul
   ✓ 4 tombol jawaban muncul

3. Klik jawaban:
   ✓ Feedback muncul (Benar/Salah)
   ✓ Progress bar isi
   ✓ Next question muncul

4. Selesai 15 soal:
   ✓ Muncul hasil (Benar: X/15, Akurasi: XX%)
   ✓ Console log difficulty adjustment
```

**JIKA ERROR:** Cek bagian Troubleshooting di bawah

---

### STEP 3: Setup LevelMap Scene

#### 3.1 Open LevelMap Scene
```
Assets/Scenes/LevelMap.unity
```

#### 3.2 Verify Hierarchy
```
Canvas
└── ScrollView
    └── Viewport
        └── Content
            - LevelMapGenerator component attached?
            - Reference to LevelNode prefab?
```

#### 3.3 Setup LevelMapGenerator
```
Inspector → LevelMapGenerator:
• Node Prefab: Drag "Prefabs/LevelNode"
• Content: Drag "ScrollView/Viewport/Content"
• Total Nodes: 10
• X Spacing: 600
• Wave Height: 200
• Wave Frequency: 0.5
• Start Offset: 200
```

#### 3.4 Verify LevelNode Prefab
```
Prefabs/LevelNode harus punya:
✓ Button component
✓ Image component
✓ LevelNode script
```

#### 3.5 TEST LevelMap
```
1. Play scene
2. Cek:
   ✓ 10 nodes muncul dalam pattern wave
   ✓ Node pertama (0) warna hijau (unlocked)
   ✓ Node lainnya abu-abu (locked)

3. Klik node 0:
   ✓ Load ke GameSession scene
   ✓ Session berjalan normal
   ✓ Setelah selesai, kembali ke LevelMap

4. Selesai session dengan accuracy >80%:
   ✓ Node berikutnya unlock (warna hijau)
```

---

### STEP 4: Setup MainMenu Scene (Optional)

#### 4.1 Simple Setup
```
Canvas
├── Title (TMP) - "Dyslexia Therapy Game"
├── ButtonPlay (Button + TMP)
│   - OnClick: MainNavigation.LoadLevelMap()
└── ButtonExit (Button + TMP)
    - OnClick: Application.Quit()
```

#### 4.2 Add MainNavigation Script
```
1. Create Empty GameObject: "Navigation"
2. Add component: MainNavigation
3. Assign buttons
```

---

### STEP 5: Add Debug Helper (PENTING!)

#### 5.1 Add to MainMenu or LevelMap
```
1. Create Empty GameObject: "DebugHelper"
2. Add component: DebugHelper
3. Simpan scene
```

#### 5.2 Test Debug Commands
```
Play game, tekan keyboard:
• P key = Show current progress (Console)
• R key = Reset semua progress
• U key = Unlock all nodes
• M key = Max difficulty

Gunakan ini untuk testing!
```

---

### STEP 6: Build Settings

```
File → Build Settings

Scenes in Build (drag in order):
0. MainMenu
1. LevelMap  
2. GameSession

Platform: Android
- Switch Platform
- Player Settings:
  • Company: UNIKU
  • Product: Dyslexa
  • Minimum API: 21
  • Target API: 33
```

---

## 🐛 TROUBLESHOOTING

### Error: "Failed to load phonology_questions.json"
```
✓ Pastikan file ada di: Assets/Resources/phonology_questions.json
✓ HARUS di folder "Resources" (case-sensitive!)
✓ File extension: .json (bukan .json.txt)
```

### Error: "NullReferenceException in GameSessionManager"
```
✓ Semua references di Inspector sudah assigned?
✓ Prefabs (ProgressSlot, AnswerButton) sudah ada?
✓ UI elements (QuestionText, AnswerContainer) sudah dibuat?
```

### Questions tidak muncul / kosong
```
✓ Check Console log saat start
✓ Harus ada: "Loaded phonology/visual question bank"
✓ Harus ada: "Generated 15 questions"
✓ Jika tidak, JSON loading gagal
```

### Progress tidak tersimpan antar session
```
✓ ProgressManager adalah Singleton
✓ Harus persistent antar scene
✓ Check: Application.persistentDataPath di Console
✓ File game_progress.json harusnya auto-create
```

### Button klik tidak merespon
```
✓ Canvas punya GraphicRaycaster?
✓ Ada EventSystem di scene?
✓ Button Interactable = TRUE?
```

---

## 📱 TEST PATH (Lengkap End-to-End)

### Test Sequence
```
1. Start MainMenu
2. Klik Play → LevelMap
3. Tekan P → lihat difficulty = 1
4. Klik Node 0 → GameSession
5. Jawab semua BENAR → accuracy >85%
6. Lihat Console: "Difficulty: 1 → 2"
7. Kembali LevelMap
8. Node 1 seharusnya unlock (hijau)
9. Klik Node 1 → GameSession
10. Pertanyaan harusnya lebih sulit (level 2)
11. Tekan P → difficulty = 2
12. ✓ ADAPTIVE WORKING!
```

---

## 📊 Data Location

Saat runtime, cek di Console:
```
[Logger] Log path: C:/Users/[User]/AppData/LocalLow/UNIKU/Dyslexa
```

File yang akan dibuat:
```
game_progress.json     ← Global state
question_logs.json     ← Detail setiap jawaban
session_logs.json      ← Summary per session
```

---

## ✅ CHECKLIST FINAL

Sebelum testing ke user:
- [ ] GameSession scene fully setup
- [ ] LevelMap scene fully setup  
- [ ] MainMenu scene setup
- [ ] DebugHelper added
- [ ] Test adaptive difficulty (naik/turun)
- [ ] Test node unlock (accuracy >80%)
- [ ] Test persistence (tutup-buka game)
- [ ] JSON logs ter-generate
- [ ] Build Android berhasil

---

## 🎯 Priority Order

**Hari ini:** Focus ke GameSession (paling penting!)
```
1. Buat AnswerButton prefab
2. Setup GameSession scene UI
3. Assign references
4. TEST - harus bisa main 1 session lengkap
```

**Besok:** LevelMap & Testing
```
1. Setup LevelMap
2. Test full flow end-to-end
3. Test adaptive
4. Collect sample data
```

**Lusa:** Polish & Build
```
1. UI improvements
2. Build Android
3. Test di device
4. Ready for alpha
```

---

## 💡 TIPS

1. **Sering Save Scene** (Ctrl+S)
2. **Check Console terus** - semua log informatif
3. **Pakai Debug Helper** - P/R/U/M keys sangat membantu
4. **Test incremental** - jangan setup semua sekaligus
5. **JSON sudah siap** - tinggal load aja

---

## 🚀 MULAI DARI SINI:

**SEKARANG:** Buka Unity → GameSession scene → Follow Step 2!

Questions? Stuck? Ask me! 💪
