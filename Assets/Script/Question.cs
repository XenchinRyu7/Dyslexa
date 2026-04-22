using System.Collections.Generic;

public enum QuestionType
{
    // === VISUAL MODE ===
    VisualLetterRecognition,  // Lihat huruf → pilih huruf yang sama
    VisualSpacing,            // Lihat kata → pilih ejaan spasi yang benar

    // === FONOLOGIS MODE ===
    PhonologyBlending,        // Dengar audio suku kata → pilih gambar benda
    PhonologySegmenting       // Lihat gambar benda → drag suku kata ke slot urutan
}

// Mapping helper: mode string → question types yang relevan
public static class QuestionTypeHelper
{
    public static bool IsFonologis(QuestionType type)
        => type == QuestionType.PhonologyBlending || type == QuestionType.PhonologySegmenting;

    public static bool IsVisual(QuestionType type)
        => type == QuestionType.VisualLetterRecognition || type == QuestionType.VisualSpacing;
}

[System.Serializable]
public class Question
{
    public QuestionType type;

    // Stimulus utama (teks atau path gambar)
    public string stimulus;          // Teks/huruf untuk Visual, path gambar untuk Segmenting
    public string stimulusImagePath; // Path gambar stimulus (untuk Blending & Segmenting)

    // Jawaban
    public string correctAnswer;     // Jawaban benar (teks)
    public List<string> options;     // Pilihan jawaban (teks)

    // Untuk Blending: opsi berupa gambar
    public List<string> imageOptions; // Path gambar tiap opsi (untuk Blending)

    // Untuk Segmenting: suku kata yang benar secara berurutan
    public string[] correctSyllables; // Urutan suku kata benar, misal: ["BO","LA"]
    public string[] allSyllables;     // Semua suku kata (termasuk distraktor), misal: ["BO","LA","LU"]

    // Audio
    public string audioClipName;     // Path audio clip (untuk Blending & Segmenting per suku kata)
    public string[] syllableAudios;  // Audio per suku kata di Segmenting, misal: ["Audio/bo","Audio/la","Audio/lu"]

    // Constructor untuk Visual (Letter Recognition & Spacing)
    public Question(QuestionType type, string stimulus, string correctAnswer, List<string> options)
    {
        this.type = type;
        this.stimulus = stimulus;
        this.correctAnswer = correctAnswer;
        this.options = options;
        this.imageOptions = new List<string>();
        this.audioClipName = "";
    }

    // Constructor untuk Blending (audio stimulus → pilih gambar)
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

    // Constructor untuk Segmenting (gambar → drag suku kata)
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
