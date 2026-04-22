# 📁 File Structure — Dyslexa

> Terakhir diupdate: Setelah refactor GameSession ke 4 Mode (April 2026)

---

## Scripts (.cs)

```
Script/
│
│── [CORE DATA]
├── Question.cs              ✅ Question model & 4 QuestionType enum
├── SessionState.cs          ✅ Session state machine enum
├── PlayerProfile.cs         ✅ Data model profil anak + PlayerProfileData
│
│── [MANAGERS - Singleton / DontDestroyOnLoad]
├── PlayerProfileManager.cs  ✅ Save/load profil (JSON), ActiveProfile state
├── ProgressManager.cs       ✅ Global persistent state (difficulty, weights, unlocked)
├── DataExportManager.cs     ✅ Export CSV (Excel) & HTML Report (PDF-ready)
│
│── [GAME SESSION]
├── GameSessionManager.cs    ✅ Orkestrasi sesi, routing ke 4 panel, metrics
├── QuestionGenerator.cs     ✅ Built-in bank soal untuk 4 mode gameplay
├── RuleEngine.cs            ✅ Adaptive difficulty logic + SessionMetrics
├── Logger.cs                ✅ JSON logging (question_logs, session_logs)
│
│── [GAME SESSION — PANELS]
├── VisualLetterPanel.cs     ✅ Panel Visual Letter Recognition
├── VisualSpacingPanel.cs    ✅ Panel Visual Spacing Awareness
├── FonologisBlendingPanel.cs   ✅ Panel Fonologis Blending (audio → gambar)
├── FonologisSegmentingPanel.cs ✅ Panel Fonologis Segmenting (gambar → drag suku kata)
│
│── [GAME SESSION — DRAG & DROP]
├── DraggableSyllable.cs     ✅ Suku kata yang bisa di-drag (IBeginDrag/IDrag/IEndDrag)
├── SyllableDropSlot.cs      ✅ Slot penerima suku kata (IDropHandler)
│
│── [LEVEL MAP]
├── LevelMapGenerator.cs     ✅ Generate node map + unlock logic
├── LevelNode.cs             ✅ Node state (Locked/Unlocked) + nomor urut
│
│── [UI / NAVIGATION]
├── MainNavigation.cs        ✅ Scene navigation umum
├── ChooseModeManager.cs     ✅ Scene ChooseMode — display profile, pilih mode
├── ProfileLoader.cs         ✅ Scene ContinueGame — list profil + confirm delete
├── SettingsWindowManager.cs ✅ HomeScreen — Settings window (export Excel/PDF)
│
│── [DEBUG]
└── DebugHelper.cs           ✅ Debug keyboard shortcut (P/R/U/M key)
```

### ⛔ Script yang Dihapus (Setelah Refactor)
```
DynamicQuestionGenerator.cs  ❌ HAPUS — tidak digunakan setelah bank soal pindah ke built-in
QuestionData.cs              ❌ HAPUS — tidak digunakan setelah JSON bank dihilangkan
```

---

## Resources

```
Resources/
├── Images/                  ⚠️ Tambahkan gambar benda (meja, bola, kuda, dll)
│   └── [nama_benda].png     → format: "Images/meja", "Images/bola", dst
├── Audio/                   ⚠️ Tambahkan audio kata & suku kata
│   ├── meja.mp3             → Audio/meja (untuk Blending)
│   └── suku/bo.mp3          → Audio/suku/bo (untuk Segmenting per suku)
├── Background/              (existing sprites)
├── Fonts/                   (existing fonts)
└── Sprite/                  (existing UI sprites)
```

> **Catatan**: `phonology_questions.json` dan `visual_questions.json` sudah tidak digunakan.  
> Bank soal sekarang **built-in** di `QuestionGenerator.cs`.

---

## Saved Data (Runtime — Auto generated)

```
Application.persistentDataPath/
├── player_profiles.json     → Semua profil anak (nama, umur, gender)
├── game_progress.json       → Global progress (difficulty, unlocked nodes)
├── question_logs.json       → Log per soal
├── session_logs.json        → Log per sesi
├── Dyslexa_DataExport.csv   → Export Excel (dipicu dari Settings)
└── Dyslexa_Report.html      → Export PDF-ready (dipicu dari Settings)
```

---

## 🎮 Alur Navigasi Scene

```
HomeScreen
 ├── [New Game] → NewGame → OnboardingAge → OnboardingGender → ChooseMode
 ├── [Continue] → ContinueGame (list profil) → ChooseMode
 └── [Settings] → Settings Window (Export Excel / PDF)

ChooseMode
 └── [Fonologis / Visual] → LevelMap

LevelMap
 └── [Klik Node] → GameSession

GameSession (1 scene, 4 panel)
 ├── Mode Fonologis → mix Blending + Segmenting (15 soal)
 └── Mode Visual    → mix Letter Recognition + Spacing (15 soal)
     └── Selesai → RuleEngine → ProgressManager → kembali ke LevelMap
```

---

## 🎮 Game Session — 4 Mode Gameplay

| Mode | Stimulus | Cara Jawab | Panel Script |
|---|---|---|---|
| Visual Letter Recognition | Huruf teks besar | Klik huruf yang sama | `VisualLetterPanel` |
| Visual Spacing Awareness | Kata utuh (BUKU) | Klik ejaan spasi benar | `VisualSpacingPanel` |
| Fonologis Blending | Audio suku kata auto-play | Klik gambar benda | `FonologisBlendingPanel` |
| Fonologis Segmenting | Gambar benda | Drag suku kata ke slot urutan | `FonologisSegmentingPanel` |

---

## 🔧 Unity Scene Setup — GameSession

```
GameSession Scene
├── Canvas
│   ├── [Shared] ProgressContainer  → slotPrefab
│   ├── [Shared] QuizTitle          → quizTitleText (TMP)
│   ├── [Shared] TimerText          → timerText (TMP)
│   ├── [Shared] FeedbackPanel      → feedbackPanel + feedbackText
│   ├── [Shared] BackToMapButton
│   │
│   ├── Panel_VisualLetter          → VisualLetterPanel.cs
│   ├── Panel_VisualSpacing         → VisualSpacingPanel.cs
│   ├── Panel_Blending              → FonologisBlendingPanel.cs
│   └── Panel_Segmenting            → FonologisSegmentingPanel.cs
│       ├── SyllableContainer       → tempat suku kata draggable
│       └── SlotContainer           → tempat slot urutan
│
└── Managers (Empty GameObject)
    └── GameSessionManager.cs (assign semua 4 panel di Inspector)
```

### Prefabs yang Dibutuhkan
```
Prefabs/
├── ProgressSlot.prefab      ✅ (existing) — progress bar
├── AnswerButton.prefab      ⚠️ (buat) — tombol teks jawaban (Visual)
├── ImageButton.prefab       ⚠️ (buat) — tombol gambar jawaban (Blending)
├── SyllablePrefab.prefab    ⚠️ (buat) — suku kata draggable (Segmenting)
├── SlotPrefab.prefab        ⚠️ (buat) — slot kosong (Segmenting)
└── LevelNode.prefab         ✅ (existing) — level map node
```

---

## 🛠️ Debug Commands (DebugHelper.cs)

```
P key → Show current progress di Console
R key → Reset semua progress ke default
U key → Unlock semua 10 node
M key → Set difficulty ke max (5)
```

---

## 📱 Build Settings (Android)

```
Scenes in Build:
 0. HomeScreen
 1. NewGame
 2. OnboardingAge
 3. OnboardingGender
 4. ContinueGame
 5. ChooseMode
 6. LevelMap
 7. GameSession

Platform: Android
Min API Level: 21
Target API Level: 33
Scripting Backend: IL2CPP
Architecture: ARM64
Write Permission: External Storage
```
