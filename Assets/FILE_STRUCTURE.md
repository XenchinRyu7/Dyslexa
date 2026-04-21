# 📁 File Structure Lengkap

## Scripts (.cs)
```
Script/
├── Question.cs              ✅ Question model & enum
├── QuestionData.cs          ✅ JSON deserialization model
├── SessionState.cs          ✅ Session state machine enum
├── QuestionGenerator.cs     ✅ Generate questions from JSON
├── RuleEngine.cs            ✅ Adaptive difficulty logic
├── Logger.cs                ✅ JSON logging system
├── ProgressManager.cs       ✅ Global persistent state (Singleton)
├── GameSessionManager.cs    ✅ Main session controller
├── LevelMapGenerator.cs     ✅ Level map with unlock logic
├── LevelNode.cs            ✅ Node state management
├── MainNavigation.cs       ✅ Scene navigation (existing)
└── DebugHelper.cs          ✅ Debug tools untuk testing
```

## Resources (JSON Question Banks)
```
Resources/
├── phonology_questions.json  ✅ 8 soal × 5 difficulty levels
├── visual_questions.json     ✅ 8 soal × 5 difficulty levels
├── Background/              (existing sprites)
├── Fonts/                   (existing fonts)
└── Sprite/                  (existing UI sprites)
```

## Documentation
```
Assets/
├── Readme.Md                ✅ Main specification
├── ADAPTIVE_SCALING.md      ✅ Scaling system explanation
└── FILE_STRUCTURE.md        ✅ This file
```

## Saved Data (Runtime - Auto generated)
```
Application.persistentDataPath/
├── game_progress.json       → Global progress (difficulty, weights, unlocked)
├── question_logs.json       → Per-question logs
└── session_logs.json        → Per-session logs
```

---

## 🎮 Data Flow

### Startup
```
1. ProgressManager (Singleton) loads game_progress.json
2. MainMenu → LevelMap
3. LevelMap loads current unlocked nodes from ProgressManager
```

### Session Flow
```
1. Player clicks Node X
2. LevelMap saves nodeIndex to PlayerPrefs
3. Load GameSession scene
4. GameSessionManager:
   ├── Get nodeIndex from PlayerPrefs
   ├── Get global difficulty from ProgressManager
   ├── QuestionGenerator loads JSON banks (Resources)
   ├── Generate 15 mixed questions
   ├── Loop: Show → Answer → Log → Feedback
   ├── End: Calculate metrics
   ├── RuleEngine: Evaluate & adapt global difficulty
   ├── Logger: Save question_logs.json & session_logs.json
   ├── ProgressManager: Update & save game_progress.json
   └── Check unlock next node
5. Back to LevelMap
```

---

## 📊 JSON Structures

### game_progress.json
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

### phonology_questions.json / visual_questions.json
```json
{
  "difficulty_1": [
    {
      "stimulus": "Pilih huruf 'B'",
      "correctAnswer": "B",
      "options": ["B", "D", "P", "G"]
    }
  ],
  "difficulty_2": [...],
  ...
}
```

### question_logs.json
```json
{
  "questions": [
    {
      "nodeIndex": 2,
      "difficulty": 3,
      "questionType": "Phonology",
      "correct": true,
      "responseTime": 3.2,
      "usedHint": false,
      "timestamp": "2026-02-25T10:32:21"
    }
  ]
}
```

### session_logs.json
```json
{
  "sessions": [
    {
      "nodeIndex": 2,
      "accuracy": 0.8,
      "error_rate": 0.2,
      "phonology_errors": 2,
      "visual_errors": 1,
      "difficulty_before": 2,
      "difficulty_after": 3,
      "avg_response_time": 4.5,
      "total_hints_used": 0,
      "timestamp": "2026-02-25T10:35:00"
    }
  ]
}
```

---

## 🔧 Unity Setup Requirements

### GameSession Scene Hierarchy
```
GameSession
├── Canvas
│   ├── Background (Image)
│   ├── ProgressContainer (HorizontalLayoutGroup)
│   ├── QuizTitle (TextMeshProUGUI)
│   ├── QuestionText (TextMeshProUGUI)
│   ├── AnswerContainer (VerticalLayoutGroup)
│   └── FeedbackPanel (Panel)
│       └── FeedbackText (TextMeshProUGUI)
└── Managers (Empty GameObject)
    └── GameSessionManager (with all manager components)
```

### Prefabs Needed
```
Prefabs/
├── ProgressSlot.prefab      ✅ (existing) - untuk progress bar
├── AnswerButton.prefab      ⚠️ (buat baru) - untuk pilihan jawaban
└── LevelNode.prefab         ✅ (existing) - untuk level map
```

### Components Assignment (GameSessionManager)
```
GameSessionManager:
  - slotPrefab: ProgressSlot.prefab
  - progressContainer: ProgressContainer transform
  - quizTitleText: QuizTitle TMP
  - questionText: QuestionText TMP
  - answerContainer: AnswerContainer transform
  - answerButtonPrefab: AnswerButton.prefab
  - feedbackPanel: FeedbackPanel gameobject
  - feedbackText: FeedbackText TMP
  - totalQuestions: 15
```

---

## 🎯 Testing Checklist

### Before First Run
- [ ] Create AnswerButton.prefab (Button + TMP child)
- [ ] Assign all references in GameSessionManager
- [ ] Check Resources folder has JSON files
- [ ] Verify TextMeshPro installed

### Test Sequence
1. **First Session**
   - [ ] Start game, difficulty should be 1
   - [ ] Complete with high accuracy (>85%)
   - [ ] Check logs: difficulty should increase to 2

2. **Second Session**
   - [ ] Questions should be difficulty 2
   - [ ] Complete with low accuracy (<60%)
   - [ ] Difficulty should decrease back to 1

3. **Node Unlock**
   - [ ] Complete session with accuracy >80%
   - [ ] Next node should unlock
   - [ ] Check ProgressManager (Debug P key)

4. **Persistence**
   - [ ] Close game
   - [ ] Reopen game
   - [ ] Difficulty & unlocked nodes should persist

### Debug Commands
```
P key = Show current progress
R key = Reset all progress
U key = Unlock all nodes
M key = Set difficulty to max
```

---

## 📱 Build Settings

### Scenes in Build
```
0. MainMenu
1. LevelMap
2. GameSession
(3. Result - optional)
```

### Platform Settings (Android)
```
- Minimum API Level: 21
- Target API Level: 33
- IL2CPP
- ARM64
- Write permission: External Storage (SD Card)
```

---

## 🚀 Ready for Implementation!

All files created ✅  
System architecture complete ✅  
Documentation ready ✅  

**Next:** Setup Unity scene & test!
