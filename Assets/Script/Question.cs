using System.Collections.Generic;

public enum QuestionType
{
    VisualLetterRecognition,
    VisualSpacing,
    PhonologyBlending,
    PhonologySegmenting,
    WorkingMemoryNumbers,
    WorkingMemoryImages
}

public static class QuestionTypeHelper
{
    public static bool IsFonologis(QuestionType type)
        => type == QuestionType.PhonologyBlending || type == QuestionType.PhonologySegmenting;

    public static bool IsVisual(QuestionType type)
        => type == QuestionType.VisualLetterRecognition || type == QuestionType.VisualSpacing;

    public static bool IsWorkingMemory(QuestionType type)
        => type == QuestionType.WorkingMemoryNumbers || type == QuestionType.WorkingMemoryImages;
}

[System.Serializable]
public class Question
{
    public QuestionType type;

    public string stimulus;
    public string stimulusImagePath;

    public string correctAnswer;
    public List<string> options;

    public List<string> imageOptions;

    public string[] correctSyllables;
    public string[] allSyllables;

    public string audioClipName;
    public string[] syllableAudios;

    public Question(QuestionType type, string stimulus, string correctAnswer, List<string> options)
    {
        this.type = type;
        this.stimulus = stimulus;
        this.correctAnswer = correctAnswer;
        this.options = options;
        this.imageOptions = new List<string>();
        this.audioClipName = "";
    }

    public Question(QuestionType type, string audioClipName, string correctAnswer,
                    List<string> imageOptions, string stimulus = "")
    {
        this.type = type;
        this.stimulus = stimulus;
        this.audioClipName = audioClipName;
        this.correctAnswer = correctAnswer;
        this.imageOptions = imageOptions;
        this.options = new List<string>();
    }

    public Question(QuestionType type, string stimulusImagePath, string[] correctSyllables,
                    string[] allSyllables, string[] syllableAudios)
    {
        this.type = type;
        this.stimulusImagePath = stimulusImagePath;
        this.stimulus = stimulusImagePath;
        this.correctSyllables = correctSyllables;
        this.allSyllables = allSyllables;
        this.syllableAudios = syllableAudios;
        this.correctAnswer = string.Join("-", correctSyllables);
        this.options = new List<string>();
        this.imageOptions = new List<string>();
        this.audioClipName = "";
    }
}
