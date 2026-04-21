using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

[System.Serializable]
public class QuestionLog
{
    public int nodeIndex;
    public int difficulty;
    public string questionType;
    public bool correct;
    public float responseTime;
    public bool usedHint;
    public string timestamp;

    public QuestionLog(int nodeIndex, int difficulty, QuestionType qType, bool correct, float responseTime, bool usedHint)
    {
        this.nodeIndex = nodeIndex;
        this.difficulty = difficulty;
        this.questionType = qType.ToString();
        this.correct = correct;
        this.responseTime = responseTime;
        this.usedHint = usedHint;
        this.timestamp = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss");
    }
}

[System.Serializable]
public class SessionLog
{
    public int nodeIndex;
    public float accuracy;
    public float error_rate;
    public int phonology_errors;
    public int visual_errors;
    public int difficulty_before;
    public int difficulty_after;
    public float avg_response_time;
    public float waktu_penyelesaian; // NEW: Total session time
    public int total_hints_used;
    public string timestamp;

    public SessionLog(int nodeIndex, SessionMetrics metrics, int diffBefore, int diffAfter)
    {
        this.nodeIndex = nodeIndex;
        this.accuracy = metrics.accuracy;
        this.error_rate = metrics.error_rate;
        this.phonology_errors = metrics.kesalahan_fonologis;
        this.visual_errors = metrics.kesalahan_visual;
        this.difficulty_before = diffBefore;
        this.difficulty_after = diffAfter;
        this.avg_response_time = metrics.rata_waktu_respons;
        this.waktu_penyelesaian = metrics.waktu_penyelesaian; // NEW
        this.total_hints_used = metrics.penggunaan_hint;
        this.timestamp = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss");
    }
}

[System.Serializable]
public class QuestionLogList
{
    public List<QuestionLog> questions = new List<QuestionLog>();
}

[System.Serializable]
public class SessionLogList
{
    public List<SessionLog> sessions = new List<SessionLog>();
}

public class Logger : MonoBehaviour
{
    private string questionLogPath;
    private string sessionLogPath;

    private QuestionLogList questionLogs = new QuestionLogList();
    private SessionLogList sessionLogs = new SessionLogList();

    void Awake()
    {
        string dataPath = Application.persistentDataPath;
        questionLogPath = Path.Combine(dataPath, "question_logs.json");
        sessionLogPath = Path.Combine(dataPath, "session_logs.json");

        LoadLogs();
    }

    public void LogQuestion(int nodeIndex, int difficulty, QuestionType qType, bool correct, float responseTime, bool usedHint)
    {
        QuestionLog log = new QuestionLog(nodeIndex, difficulty, qType, correct, responseTime, usedHint);
        questionLogs.questions.Add(log);

        SaveQuestionLogs();

        Debug.Log($"[Logger] Question logged: Node={nodeIndex}, Type={qType}, Correct={correct}");
    }

    public void LogSession(int nodeIndex, SessionMetrics metrics, int diffBefore, int diffAfter)
    {
        SessionLog log = new SessionLog(nodeIndex, metrics, diffBefore, diffAfter);
        sessionLogs.sessions.Add(log);

        SaveSessionLogs();

        Debug.Log($"[Logger] Session logged: Node={nodeIndex}, Accuracy={metrics.accuracy:F2}");
    }

    private void SaveQuestionLogs()
    {
        try
        {
            string json = JsonUtility.ToJson(questionLogs, true);
            File.WriteAllText(questionLogPath, json);
        }
        catch (Exception e)
        {
            Debug.LogError($"[Logger] Failed to save question logs: {e.Message}");
        }
    }

    private void SaveSessionLogs()
    {
        try
        {
            string json = JsonUtility.ToJson(sessionLogs, true);
            File.WriteAllText(sessionLogPath, json);
        }
        catch (Exception e)
        {
            Debug.LogError($"[Logger] Failed to save session logs: {e.Message}");
        }
    }

    private void LoadLogs()
    {
        // Load question logs
        if (File.Exists(questionLogPath))
        {
            try
            {
                string json = File.ReadAllText(questionLogPath);
                questionLogs = JsonUtility.FromJson<QuestionLogList>(json);
                Debug.Log($"[Logger] Loaded {questionLogs.questions.Count} question logs");
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[Logger] Failed to load question logs: {e.Message}");
                questionLogs = new QuestionLogList();
            }
        }

        // Load session logs
        if (File.Exists(sessionLogPath))
        {
            try
            {
                string json = File.ReadAllText(sessionLogPath);
                sessionLogs = JsonUtility.FromJson<SessionLogList>(json);
                Debug.Log($"[Logger] Loaded {sessionLogs.sessions.Count} session logs");
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[Logger] Failed to load session logs: {e.Message}");
                sessionLogs = new SessionLogList();
            }
        }
    }

    public string GetLogPath()
    {
        return Application.persistentDataPath;
    }
}
