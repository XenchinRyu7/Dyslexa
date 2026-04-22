using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Generator soal untuk 4 mode gameplay.
/// Bank soal built-in — tidak perlu JSON.
///
/// VISUAL MODE:
///   - VisualLetterRecognition: 5 grup huruf mirip, pilih huruf yang sama
///   - VisualSpacing: 15 kata, generate variasi spasi salah secara algoritmik
///
/// FONOLOGIS MODE:
///   - PhonologyBlending: audio kata → pilih gambar
///   - PhonologySegmenting: gambar → drag suku kata ke slot
/// </summary>
public class QuestionGenerator : MonoBehaviour
{
    // =============================================
    // BANK SOAL — VISUAL LETTER RECOGNITION
    // 5 grup huruf yang sering tertukar pada disleksia
    // Stimulus = huruf besar, opsi = 4 huruf dalam grup (kecil)
    // =============================================
    private static readonly string[][] letterGroups = new string[][]
    {
        new string[] { "b", "d", "p", "q" },   // Grup 1 — rotasi horizontal & vertikal
        new string[] { "m", "n", "w", "u" },   // Grup 2 — bentuk mirip
        new string[] { "s", "z", "5", "2" },   // Grup 3 — huruf & angka mirip
        new string[] { "k", "h", "t", "r" },   // Grup 4 — konsisten height
        new string[] { "a", "e", "o", "d" },   // Grup 5 — vokal + d yang sering tertukar
    };

    // =============================================
    // BANK SOAL — VISUAL SPACING AWARENESS
    // Variasi spasi salah digenerate otomatis dari kata
    // =============================================
    private static readonly string[] spacingWords = new string[]
    {
        "BOLA", "BUKU", "RUMAH", "KUCING", "IKAN",
        "PINTU", "KAKI", "MATA", "ROTI", "KURSI",
        "MEJA", "BAJU", "SAPU", "AYAM", "PISANG"
    };

    // =============================================
    // BANK SOAL — FONOLOGIS BLENDING
    // { audioPath, correctImagePath, distractor1, distractor2, distractor3 }
    // gambar di: Resources/Image/Blending/
    // audio ada: sound_buku, sound_bola, sound_gula (sisanya "" untuk sementara)
    // =============================================
    private static readonly string[][] blendingBank = new string[][]
    {
        // Format: { audio, correct, wrong1, wrong2, wrong3 }
        new string[] { "Audio/sound_buku",  "Image/Blending/book",        "Image/Blending/table",       "Image/Blending/dice",      "Image/Blending/shirt"      }, // BUKU
        new string[] { "",                  "Image/Blending/bread",       "Image/Blending/sugar",       "Image/Blending/hayballe",  "Image/Blending/shirt"      }, // ROTI
        new string[] { "",                  "Image/Blending/cow",         "Image/Blending/horse",       "Image/Blending/deer",      "Image/Blending/spider"     }, // SAPI
        new string[] { "",                  "Image/Blending/deer",        "Image/Blending/horse",       "Image/Blending/cow",       "Image/Blending/spider"     }, // RUSA
        new string[] { "",                  "Image/Blending/dice",        "Image/Blending/book",        "Image/Blending/soccer_ball","Image/Blending/table"     }, // DADU
        new string[] { "",                  "Image/Blending/earth",       "Image/Blending/hayballe",    "Image/Blending/book",      "Image/Blending/table"      }, // BUMI
        new string[] { "",                  "Image/Blending/eyes",        "Image/Blending/tooth",       "Image/Blending/shirt",     "Image/Blending/book"       }, // MATA
        new string[] { "",                  "Image/Blending/hayballe",    "Image/Blending/earth",       "Image/Blending/book",      "Image/Blending/table"      }, // JERAMI
        new string[] { "",                  "Image/Blending/horse",       "Image/Blending/cow",         "Image/Blending/deer",      "Image/Blending/spider"     }, // KUDA
        new string[] { "",                  "Image/Blending/shirt",       "Image/Blending/book",        "Image/Blending/eyes",      "Image/Blending/tooth"      }, // BAJU
        new string[] { "Audio/sound_bola",  "Image/Blending/soccer_ball", "Image/Blending/dice",        "Image/Blending/earth",     "Image/Blending/table"      }, // BOLA
        new string[] { "",                  "Image/Blending/spider",      "Image/Blending/cow",         "Image/Blending/horse",     "Image/Blending/deer"       }, // LABA-LABA
        new string[] { "Audio/sound_gula",  "Image/Blending/sugar",       "Image/Blending/bread",       "Image/Blending/shirt",     "Image/Blending/book"       }, // GULA
        new string[] { "",                  "Image/Blending/table",       "Image/Blending/book",        "Image/Blending/dice",      "Image/Blending/earth"      }, // MEJA
        new string[] { "",                  "Image/Blending/tooth",       "Image/Blending/eyes",        "Image/Blending/shirt",     "Image/Blending/sugar"      }, // GIGI
    };

    // =============================================
    // BANK SOAL — FONOLOGIS SEGMENTING
    // { imagePath, correct[], allSyllables[], audioPerSuku[] }
    // gambar di: Resources/Image/Segmenting/
    // hanya kata 2 suku kata (panel punya 2 slot)
    // =============================================
    private static readonly object[][] segmentingBank = new object[][]
    {
        // { "Image/Segmenting/file", correct[], all[] (correct+distractor), audio[] }
        new object[] { "Image/Segmenting/soccer_ball", new string[]{"BO","LA"},   new string[]{"BO","LA","MA"},   new string[]{"","",""} }, // BOLA
        new object[] { "Image/Segmenting/bread",       new string[]{"RO","TI"},   new string[]{"RO","TI","BI"},   new string[]{"","",""} }, // ROTI
        new object[] { "Image/Segmenting/home",        new string[]{"RU","MAH"},  new string[]{"RU","MAH","BAH"}, new string[]{"","",""} }, // RUMAH
        new object[] { "Image/Segmenting/cat",         new string[]{"KU","CING"}, new string[]{"KU","CING","SING"},new string[]{"","",""} }, // KUCING
        new object[] { "Image/Segmenting/fish",        new string[]{"I","KAN"},   new string[]{"I","KAN","PAN"},  new string[]{"","",""} }, // IKAN
        new object[] { "Image/Segmenting/door",        new string[]{"PIN","TU"},  new string[]{"PIN","TU","DU"},  new string[]{"","",""} }, // PINTU
        new object[] { "Image/Segmenting/eyes",        new string[]{"MA","TA"},   new string[]{"MA","TA","DA"},   new string[]{"","",""} }, // MATA
        new object[] { "Image/Segmenting/shirt",       new string[]{"BA","JU"},   new string[]{"BA","JU","MU"},   new string[]{"","",""} }, // BAJU
        new object[] { "Image/Segmenting/armchair",    new string[]{"KUR","SI"},  new string[]{"KUR","SI","NI"},  new string[]{"","",""} }, // KURSI
        new object[] { "Image/Segmenting/table",       new string[]{"ME","JA"},   new string[]{"ME","JA","MI"},   new string[]{"","",""} }, // MEJA
        new object[] { "Image/Segmenting/broom",       new string[]{"SA","PU"},   new string[]{"SA","PU","BU"},   new string[]{"","",""} }, // SAPU
        new object[] { "Image/Segmenting/chicken",     new string[]{"A","YAM"},   new string[]{"A","YAM","LAM"},  new string[]{"","",""} }, // AYAM
        new object[] { "Image/Segmenting/hat",         new string[]{"TO","PI"},   new string[]{"TO","PI","BI"},   new string[]{"","",""} }, // TOPI
        new object[] { "Image/Segmenting/horse",       new string[]{"KU","DA"},   new string[]{"KU","DA","MA"},   new string[]{"","",""} }, // KUDA
        new object[] { "Image/Segmenting/car",         new string[]{"MO","BIL"},  new string[]{"MO","BIL","NIL"}, new string[]{"","",""} }, // MOBIL
    };

    // =============================================
    // PUBLIC: GENERATE QUESTION SET
    // =============================================
    public List<Question> GenerateQuestionSet(int totalQuestions, int difficulty, string selectedMode)
    {
        List<Question> questions = new List<Question>();

        if (selectedMode == "Fonologis")
        {
            int blendingCount   = Mathf.RoundToInt(totalQuestions * 0.5f);
            int segmentingCount = totalQuestions - blendingCount;

            for (int i = 0; i < blendingCount; i++)
                questions.Add(GenerateBlendingQuestion(difficulty));
            for (int i = 0; i < segmentingCount; i++)
                questions.Add(GenerateSegmentingQuestion(difficulty));
        }
        else // "Visual" default
        {
            int letterCount  = Mathf.RoundToInt(totalQuestions * 0.5f);
            int spacingCount = totalQuestions - letterCount;

            for (int i = 0; i < letterCount; i++)
                questions.Add(GenerateLetterRecognitionQuestion(difficulty));
            for (int i = 0; i < spacingCount; i++)
                questions.Add(GenerateSpacingQuestion(difficulty));
        }

        ShuffleList(questions);
        Debug.Log($"[QuestionGenerator] {questions.Count} soal, mode: {selectedMode}, difficulty: {difficulty}");
        return questions;
    }

    // Legacy overload untuk backward compat
    public List<Question> GenerateQuestionSet(int totalQuestions, int difficulty, float phonologyW, float visualW)
    {
        string mode = phonologyW >= visualW ? "Fonologis" : "Visual";
        return GenerateQuestionSet(totalQuestions, difficulty, mode);
    }

    // =============================================
    // GENERATOR: VISUAL LETTER RECOGNITION
    // Pilih 1 grup berdasarkan difficulty, acak 1 huruf jadi stimulus
    // Semua 4 huruf dalam grup jadi opsi
    // =============================================
    private Question GenerateLetterRecognitionQuestion(int difficulty)
    {
        // Difficulty 1-2 → grup mudah (bdpq, mnwu)
        // Difficulty 3   → semua grup diacak
        // Difficulty 4-5 → semua grup + bisa mix antar grup (lebih banyak variasi)
        int maxGroup = difficulty <= 2 ? Mathf.Min(difficulty, letterGroups.Length)
                                       : letterGroups.Length;

        int groupIndex  = Random.Range(0, maxGroup);
        string[] group  = letterGroups[groupIndex];
        string correct  = group[Random.Range(0, group.Length)];

        List<string> options = new List<string>(group);
        ShuffleList(options);

        // Stimulus ditampilkan UPPERCASE, opsi lowercase
        return new Question(QuestionType.VisualLetterRecognition, correct.ToUpper(), correct, options);
    }

    // =============================================
    // GENERATOR: VISUAL SPACING AWARENESS
    // Generate 2 variasi spasi salah secara algoritmik dari kata
    // =============================================
    private Question GenerateSpacingQuestion(int difficulty)
    {
        // Difficulty menentukan pool kata (panjang kata bertambah seiring difficulty)
        int poolEnd;
        switch (difficulty)
        {
            case 1:  poolEnd = 4;  break;  // BOLA, BUKU, RUMAH, KUCING (4 huruf)
            case 2:  poolEnd = 7;  break;  // + IKAN, PINTU, KAKI
            case 3:  poolEnd = 10; break;  // + MATA, ROTI, KURSI
            default: poolEnd = spacingWords.Length; break; // semua
        }

        string word = spacingWords[Random.Range(0, Mathf.Min(poolEnd, spacingWords.Length))];

        // Generate variasi spasi salah secara algoritmik
        List<string> wrongs = GenerateSpacingVariants(word);

        List<string> options = new List<string> { word };
        options.AddRange(wrongs);
        ShuffleList(options);

        return new Question(QuestionType.VisualSpacing, word, word, options);
    }

    /// <summary>
    /// Buat 2 variasi spasi salah dari sebuah kata.
    /// Contoh: "BOLA" (4 huruf) → "BO LA" (split 2), "BOL A" (split 3)
    /// Contoh: "RUMAH" (5 huruf) → "RU MAH" (split 2), "RUM AH" (split 3)
    /// </summary>
    private List<string> GenerateSpacingVariants(string word)
    {
        List<string> variants = new List<string>();
        int len = word.Length;

        // Pilih 2 posisi split yang berbeda
        // Posisi split: mulai dari 1/3 sampai 2/3 panjang kata
        int split1 = Mathf.Max(1, len / 3);        // misal 4/3=1, 6/3=2
        int split2 = Mathf.Max(split1 + 1, len / 2); // misal 4/2=2, 6/2=3

        if (split1 < len)
            variants.Add(word.Substring(0, split1) + " " + word.Substring(split1));

        if (split2 < len && split2 != split1)
            variants.Add(word.Substring(0, split2) + " " + word.Substring(split2));

        // Pastikan selalu ada 2 variasi
        while (variants.Count < 2)
        {
            int sp = Random.Range(1, len);
            string v = word.Substring(0, sp) + " " + word.Substring(sp);
            if (!variants.Contains(v) && v != word)
                variants.Add(v);
        }

        return variants;
    }

    // =============================================
    // GENERATOR: FONOLOGIS BLENDING
    // =============================================
    private Question GenerateBlendingQuestion(int difficulty)
    {
        // Difficulty tinggi → pilih dari semua bank, rendah → beberapa pertama
        int poolEnd = difficulty <= 2 ? 4 : blendingBank.Length;
        string[] entry = blendingBank[Random.Range(0, Mathf.Min(poolEnd, blendingBank.Length))];

        string audioPath  = entry[0];
        string correctImg = entry[1];

        List<string> imageOptions = new List<string> { correctImg, entry[2], entry[3], entry[4] };
        ShuffleList(imageOptions);

        return new Question(QuestionType.PhonologyBlending, audioPath, correctImg, imageOptions);
    }

    // =============================================
    // GENERATOR: FONOLOGIS SEGMENTING
    // =============================================
    private Question GenerateSegmentingQuestion(int difficulty)
    {
        int poolEnd = difficulty <= 2 ? 5 : segmentingBank.Length;
        object[] entry = segmentingBank[Random.Range(0, Mathf.Min(poolEnd, segmentingBank.Length))];

        string   imagePath        = (string)entry[0];
        string[] correctSyllables = (string[])entry[1];
        string[] allSyllables     = (string[])entry[2];
        string[] syllableAudios   = (string[])entry[3];

        List<string> shuffled = new List<string>(allSyllables);
        ShuffleList(shuffled);

        return new Question(
            QuestionType.PhonologySegmenting,
            imagePath,
            correctSyllables,
            shuffled.ToArray(),
            syllableAudios
        );
    }

    // =============================================
    // HELPER
    // =============================================
    private void ShuffleList<T>(List<T> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            T   temp        = list[i];
            int randomIndex = Random.Range(i, list.Count);
            list[i]         = list[randomIndex];
            list[randomIndex] = temp;
        }
    }
}