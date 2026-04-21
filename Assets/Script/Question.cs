using System.Collections.Generic;

public enum QuestionType
{
    Phonology,
    Visual
}

[System.Serializable]
public class Question
{
    public QuestionType type;
    public string stimulus;
    public string correctAnswer;
    public List<string> options;
    public string audioClipName; // NEW: for phonology audio

    public Question(QuestionType type, string stimulus, string correctAnswer, List<string> options, string audioClipName = "")
    {
        this.type = type;
        this.stimulus = stimulus;
        this.correctAnswer = correctAnswer;
        this.options = options;
        this.audioClipName = audioClipName;
    }
}
