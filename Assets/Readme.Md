Dyslexia Therapy Game – Rule-Based Architecture (Phase 1)
🎯 Project Goal

Build a scalable 2D Android educational game for dyslexia therapy using:

Mixed question sessions (Phonological + Visual)

Rule-based adaptive difficulty (prototype phase)

Local logging system

Single GameSession scene (dynamic content)

Node-based progression system

This phase focuses strictly on:

Rule-based adaptive engine
Clean architecture
Working code before ML integration

🧠 Core Design Principles

Single GameSession scene

Node = Session container (15 mixed questions)

Adaptive = Rule-based (no ML yet)

Difficulty adjustable

Content weight adjustable (phonology vs visual)

Local logging first (JSON)

UI and logic separated

🏗️ Scene Architecture
Scenes
MainMenu
LevelMap
GameSession
Result (optional)
🎮 GameSession Scene Structure
GameSession
 ├── Canvas
 │    ├── Background
 │    ├── ProgressContainer
 │    ├── QuizTitle
 │    ├── QuestionText (TMP)
 │    ├── AnswerContainer
 │    └── FeedbackPanel
 │
 └── Managers
      ├── GameSessionManager
      ├── QuestionGenerator
      ├── RuleEngine
      └── Logger
🧩 Core System Architecture
1️⃣ Question Model
public enum QuestionType
{
    Phonology,
    Visual
}

public class Question
{
    public QuestionType type;
    public string stimulus;
    public string correctAnswer;
    public List<string> options;
}
2️⃣ Session Flow
StartSession()
 → GenerateQuestionSet()
 → Loop 15 Questions:
      ShowQuestion()
      WaitAnswer()
      LogResponse()
 → EndSession()
 → CalculateMetrics()
 → RuleEngine.Evaluate()
 → SaveResults()
🎲 Question Generation (Mixed by Design)

Each session must ALWAYS be mixed.

Default distribution:

7 phonology

8 visual

Adaptive distribution:

If phonology errors > visual errors:

Increase phonology weight next session

If visual errors > phonology:

Increase visual weight next session

QuestionGenerator Responsibilities

Generate N phonology questions

Generate N visual questions

Shuffle list

Respect difficulty level

📈 Rule-Based Adaptive Engine
Input Metrics

From session:

jumlah_benar
jumlah_salah
rata_waktu_respons
kesalahan_fonologis
kesalahan_visual
penggunaan_hint
total_soal
Derived Metrics
accuracy = benar / total
error_rate = salah / total
hint_rate = hint / total
fonologis_rate = kesalahan_fonologis / salah
visual_rate = kesalahan_visual / salah
Difficulty Adjustment Rules
if (accuracy >= 0.85f && hint_rate < 0.2f)
    difficulty++;
else if (accuracy < 0.6f)
    difficulty--;
else
    difficulty = difficulty;

Clamp difficulty between 1–5.

Content Weight Adjustment
if (kesalahan_fonologis > kesalahan_visual)
    phonologyWeight += 0.1f;
else if (kesalahan_visual > kesalahan_fonologis)
    visualWeight += 0.1f;

Normalize so total weight = 1.

📊 Logging System (Phase 1)
Per Question Log
{
  "nodeIndex": 2,
  "difficulty": 1,
  "questionType": "Phonology",
  "correct": true,
  "responseTime": 3.2,
  "usedHint": false,
  "timestamp": "2026-02-21T10:32:21"
}
Per Session Log
{
  "accuracy": 0.8,
  "error_rate": 0.2,
  "phonology_errors": 2,
  "visual_errors": 1,
  "difficulty_before": 1,
  "difficulty_after": 2
}

Save locally to:

Application.persistentDataPath
🎯 Difficulty Definition

Difficulty affects:

Level	Phonology	Visual
1	Single letter	Clear contrast
2	Simple syllables	Similar shapes
3	Words	Confusing pairs
4	Blends	Rotations
5	Complex words	Multi-distractor
🎮 Node System Logic

Node does NOT represent domain.

Node represents:

Session milestone

Difficulty tier

Unlock rule:

If mastery >= 0.8 → unlock next node

Mastery calculated from rolling average accuracy.

🧠 Session State Machine
public enum SessionState
{
    Loading,
    ShowingQuestion,
    WaitingAnswer,
    ShowingFeedback,
    Finished
}

No input allowed during feedback.

🎨 Background System

Random background per session

Not per question (avoid distraction)

Minimal design

High contrast center area

📦 Development Phases
Phase 1 – Core

Question class

Generator

Session loop

Progress bar segmented

Logging basic JSON

RuleEngine basic

Phase 2 – Stability

Prevent spam input

Feedback animation

Better logging structure

Phase 3 – ML Preparation

Structured dataset export

CSV export

Confusion matrix tracking

❌ What NOT To Do

Do NOT create separate scenes per domain

Do NOT hardcode 15 questions in scene

Do NOT mix UI logic with rule engine

Do NOT change background per question

Do NOT overcomplicate before alpha test

🎓 Research Positioning

This system qualifies as:

Rule-based adaptive learning system prototype
Baseline adaptive mechanism before ML integration

ML phase will use collected dataset from 30 children.

🚀 Immediate Next Implementation Steps

Implement Question class

Implement QuestionGenerator

Implement Session loop (15 questions)

Implement basic RuleEngine

Implement JSON logging

Connect to LevelMap unlock logic

🔥 Final Goal of This Phase

Have a fully playable:

Mixed session

Adaptive difficulty

Logging enabled

Stable for alpha testing

Before ML.