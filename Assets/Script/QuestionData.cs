using System.Collections.Generic;

[System.Serializable]
public class QuestionData
{
    public string stimulus;
    public string correctAnswer;
    public List<string> options;
    public string audioClipName; // NEW: audio file name (without extension)
}

[System.Serializable]
public class QuestionBank
{
    public List<QuestionData> difficulty_1;
    public List<QuestionData> difficulty_2;
    public List<QuestionData> difficulty_3;
    public List<QuestionData> difficulty_4;
    public List<QuestionData> difficulty_5;

    public List<QuestionData> GetQuestionsByDifficulty(int difficulty)
    {
        switch (difficulty)
        {
            case 1: return difficulty_1;
            case 2: return difficulty_2;
            case 3: return difficulty_3;
            case 4: return difficulty_4;
            case 5: return difficulty_5;
            default: return difficulty_1;
        }
    }
}
