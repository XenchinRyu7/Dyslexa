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
    // Format: { correctImagePath, string[] syllableAudios, distractor1, distractor2, distractor3 }
    // Gambar di:  Resources/Image/Segmenting/
    // Audio di:   Resources/Audio/Blending/{Kata}/{suku}.mp3
    // =============================================
    private static readonly object[][] blendingBank = new object[][]
    {
        new object[] { "Image/Segmenting/soccer_ball", new string[]{"Audio/Blending/Bola/bo",  "Audio/Blending/Bola/la"},                                "Image/Segmenting/hat",      "Image/Segmenting/horse",   "Image/Segmenting/table"    }, // BOLA
        new object[] { "Image/Segmenting/bread",       new string[]{"Audio/Blending/Roti/ro",  "Audio/Blending/Roti/ti"},                                "Image/Segmenting/broom",    "Image/Segmenting/hat",     "Image/Segmenting/armchair" }, // ROTI
        new object[] { "Image/Segmenting/horse",       new string[]{"Audio/Blending/Kuda/ku",  "Audio/Blending/Kuda/da"},                                "Image/Segmenting/cat",      "Image/Segmenting/chicken", "Image/Segmenting/fish"     }, // KUDA
        new object[] { "Image/Segmenting/eyes",        new string[]{"Audio/Blending/Mata/ma",  "Audio/Blending/Mata/ta"},                                "Image/Segmenting/door",     "Image/Segmenting/fish",    "Image/Segmenting/hat"      }, // MATA
        new object[] { "Image/Segmenting/table",       new string[]{"Audio/Blending/Meja/me",  "Audio/Blending/Meja/ja"},                                "Image/Segmenting/soccer_ball","Image/Segmenting/bread", "Image/Segmenting/armchair" }, // MEJA
        new object[] { "Image/Segmenting/shirt",       new string[]{"Audio/Blending/Baju/ba",  "Audio/Blending/Baju/ju"},                                "Image/Segmenting/hat",      "Image/Segmenting/broom",   "Image/Segmenting/bread"    }, // BAJU
        new object[] { "Image/Segmenting/home",        new string[]{"Audio/Blending/Rumah/rum","Audio/Blending/Rumah/ah"},                               "Image/Segmenting/door",     "Image/Segmenting/armchair","Image/Segmenting/table"    }, // RUMAH
        new object[] { "Image/Segmenting/fish",        new string[]{"Audio/Blending/Ikan/ik",  "Audio/Blending/Ikan/an"},                                "Image/Segmenting/chicken",  "Image/Segmenting/cat",     "Image/Segmenting/horse"    }, // IKAN
        new object[] { "Image/Segmenting/armchair",    new string[]{"Audio/Blending/Kursi/kur","Audio/Blending/Kursi/si"},                               "Image/Segmenting/table",    "Image/Segmenting/door",    "Image/Segmenting/home"     }, // KURSI
        new object[] { "Image/Segmenting/car",         new string[]{"Audio/Blending/Mobil/mo", "Audio/Blending/Mobil/bil"},                              "Image/Segmenting/horse",    "Image/Segmenting/soccer_ball","Image/Segmenting/door"  }, // MOBIL
        new object[] { "Image/Segmenting/door",        new string[]{"Audio/Blending/Pintu/pin","Audio/Blending/Pintu/tu"},                               "Image/Segmenting/home",     "Image/Segmenting/armchair","Image/Segmenting/table"    }, // PINTU
        new object[] { "Image/Segmenting/broom",       new string[]{"Audio/Blending/Sapu/sa",  "Audio/Blending/Sapu/pu"},                                "Image/Segmenting/bread",    "Image/Segmenting/shirt",   "Image/Segmenting/hat"      }, // SAPU
        new object[] { "Image/Segmenting/chicken",     new string[]{"Audio/Blending/Ayam/ay",  "Audio/Blending/Ayam/yam"},                               "Image/Segmenting/fish",     "Image/Segmenting/cat",     "Image/Segmenting/horse"    }, // AYAM
        new object[] { "Image/Segmenting/hat",         new string[]{"Audio/Blending/Topi/to",  "Audio/Blending/Topi/pi"},                                "Image/Segmenting/shirt",    "Image/Segmenting/broom",   "Image/Segmenting/bread"    }, // TOPI
        new object[] { "Image/Segmenting/cat",         new string[]{"Audio/Blending/Kucing/ku","Audio/Blending/Kucing/ci","Audio/Blending/Kucing/ng"},   "Image/Segmenting/horse",    "Image/Segmenting/chicken", "Image/Segmenting/fish"     }, // KUCING
        new object[] { "Image/Segmenting/camera",      new string[]{"Audio/Blending/Kamera/ka","Audio/Blending/Kamera/me","Audio/Blending/Kamera/ra"},  "Image/Segmenting/car",      "Image/Segmenting/home",    "Image/Segmenting/table"    }, // KAMERA
    };



    // =============================================
    // BANK SOAL — FONOLOGIS SEGMENTING
    // { imagePath, correct[], allSyllables[], audioPerSuku[] }
    // gambar di: Resources/Image/Segmenting/
    // audio di:  Resources/Audio/Segmenting/{KATA}/{suku}.mp3
    // =============================================
    private static readonly object[][] segmentingBank = new object[][]
    {
        // Format: { imagePath, correct[], allSyllables_hard[], audioPerSuku_hard[], easyDistractor, easyDistractorAudio }
        // allSyllables_hard  = correct + distraktor fonetis mirip (diff 3-5)
        // easyDistractor     = suku kata beda jauh (diff 1-2), audio reuse dari kata lain

        // === KATA DENGAN AUDIO ===
        new object[] { "Image/Segmenting/soccer_ball", new string[]{"BO","LA"}, new string[]{"BO","LA","LU"}, new string[]{"Audio/Segmenting/BOLA/bo","Audio/Segmenting/BOLA/la","Audio/Segmenting/BOLA/lu"}, "TA", "Audio/Segmenting/MATA/ta"  }, // BOLA
        new object[] { "Image/Segmenting/bread",       new string[]{"RO","TI"}, new string[]{"RO","TI","TU"}, new string[]{"Audio/Segmenting/ROTI/ro","Audio/Segmenting/ROTI/ti","Audio/Segmenting/ROTI/tu"}, "MA", "Audio/Segmenting/MATA/ma"  }, // ROTI
        new object[] { "Image/Segmenting/horse",       new string[]{"KU","DA"}, new string[]{"KU","DA","DO"}, new string[]{"Audio/Segmenting/KUDA/ku","Audio/Segmenting/KUDA/da","Audio/Segmenting/KUDA/do"}, "RO", "Audio/Segmenting/ROTI/ro"  }, // KUDA
        new object[] { "Image/Segmenting/eyes",        new string[]{"MA","TA"}, new string[]{"MA","TA","TU"}, new string[]{"Audio/Segmenting/MATA/ma","Audio/Segmenting/MATA/ta","Audio/Segmenting/MATA/tu"}, "BO", "Audio/Segmenting/BOLA/bo"  }, // MATA
        new object[] { "Image/Segmenting/table",       new string[]{"ME","JA"}, new string[]{"ME","JA","JO"}, new string[]{"Audio/Segmenting/MEJA/me","Audio/Segmenting/MEJA/ja","Audio/Segmenting/MEJA/jo"}, "KU", "Audio/Segmenting/KUDA/ku"  }, // MEJA
        new object[] { "Image/Segmenting/shirt",       new string[]{"BA","JU"}, new string[]{"BA","JU","JA"}, new string[]{"Audio/Segmenting/BAJU/ba","Audio/Segmenting/BAJU/ju","Audio/Segmenting/BAJU/ja"}, "ME", "Audio/Segmenting/MEJA/me"  }, // BAJU

        // === KATA TANPA AUDIO (audio menyusul) ===
        new object[] { "Image/Segmenting/home",     new string[]{"RU","MAH"},  new string[]{"RU","MAH","BAH"},  new string[]{"","",""}, "TA",  "" }, // RUMAH
        new object[] { "Image/Segmenting/cat",      new string[]{"KU","CING"}, new string[]{"KU","CING","SING"},new string[]{"","",""}, "BO",  "" }, // KUCING
        new object[] { "Image/Segmenting/fish",     new string[]{"I","KAN"},   new string[]{"I","KAN","PAN"},   new string[]{"","",""}, "MA",  "" }, // IKAN
        new object[] { "Image/Segmenting/door",     new string[]{"PIN","TU"},  new string[]{"PIN","TU","DU"},   new string[]{"","",""}, "BO",  "" }, // PINTU
        new object[] { "Image/Segmenting/armchair", new string[]{"KUR","SI"},  new string[]{"KUR","SI","NI"},   new string[]{"","",""}, "MA",  "" }, // KURSI
        new object[] { "Image/Segmenting/broom",    new string[]{"SA","PU"},   new string[]{"SA","PU","BU"},    new string[]{"","",""}, "TA",  "" }, // SAPU
        new object[] { "Image/Segmenting/chicken",  new string[]{"A","YAM"},   new string[]{"A","YAM","LAM"},  new string[]{"","",""}, "BO",  "" }, // AYAM
        new object[] { "Image/Segmenting/hat",      new string[]{"TO","PI"},   new string[]{"TO","PI","BI"},    new string[]{"","",""}, "MA",  "" }, // TOPI
        new object[] { "Image/Segmenting/car",      new string[]{"MO","BIL"},  new string[]{"MO","BIL","NIL"},  new string[]{"","",""}, "TA",  "" }, // MOBIL
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

        // Generate variasi spasi salah — kesulitan mempengaruhi posisi split
        List<string> wrongs = GenerateSpacingVariants(word, difficulty);

        List<string> options = new List<string> { word };
        options.AddRange(wrongs);
        ShuffleList(options);

        return new Question(QuestionType.VisualSpacing, word, word, options);
    }

    /// <summary>
    /// Buat 2 variasi spasi salah dari sebuah kata — adaptif berdasarkan difficulty.
    ///
    /// Difficulty 1-2 (mudah): split di posisi ekstrim (1 dan len-1)
    ///   → Salah lebih OBVIOUS, misal "B OLA" dan "BOL A" — mudah dibedakan
    ///
    /// Difficulty 3 (sedang): split di 1/3 dan 1/2 kata
    ///   → Distraksi sedang, misal "BO LA" dan "BOL A"
    ///
    /// Difficulty 4-5 (sulit): split di tengah dan tengah+1
    ///   → Salah lebih SUBTLE, dua opsi salah sangat mirip satu sama lain
    ///   → misal "BO LA" dan "BOL A" untuk kata 4 huruf
    /// </summary>
    private List<string> GenerateSpacingVariants(string word, int difficulty)
    {
        List<string> variants = new List<string>();
        int len = word.Length;

        if (difficulty <= 2)
        {
            // Mudah: split di posisi paling ekstrim → paling obvious
            string v1 = word.Substring(0, 1) + " " + word.Substring(1);
            string v2 = word.Substring(0, len - 1) + " " + word.Substring(len - 1);
            if (!variants.Contains(v1)) variants.Add(v1);
            if (!variants.Contains(v2) && v2 != v1) variants.Add(v2);
        }
        else if (difficulty == 3)
        {
            // Sedang: split di 1/3 dan 1/2
            int split1 = Mathf.Max(1, len / 3);
            int split2 = Mathf.Max(split1 + 1, len / 2);
            if (split1 < len)
                variants.Add(word.Substring(0, split1) + " " + word.Substring(split1));
            if (split2 < len && split2 != split1)
                variants.Add(word.Substring(0, split2) + " " + word.Substring(split2));
        }
        else
        {
            // Sulit: split di tengah dan tengah+1 → paling mirip, paling sulit dibedakan
            int mid  = Mathf.Max(1, len / 2);
            int mid2 = Mathf.Min(mid + 1, len - 1);
            string v1 = word.Substring(0, mid) + " " + word.Substring(mid);
            string v2 = word.Substring(0, mid2) + " " + word.Substring(mid2);
            if (!variants.Contains(v1)) variants.Add(v1);
            if (!variants.Contains(v2) && v2 != v1) variants.Add(v2);
        }

        // Pastikan selalu ada 2 variasi (fallback)
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
        int poolEnd = difficulty <= 2 ? 6 : blendingBank.Length;
        object[] entry = blendingBank[Random.Range(0, Mathf.Min(poolEnd, blendingBank.Length))];

        string   correctImg    = (string)entry[0];
        string[] syllableAudio = (string[])entry[1];
        string   distractor1   = (string)entry[2];
        string   distractor2   = (string)entry[3];
        string   distractor3   = (string)entry[4];

        List<string> imageOptions = new List<string> { correctImg, distractor1, distractor2, distractor3 };
        ShuffleList(imageOptions);

        // PENTING: pass 5 arg agar C# pakai constructor Blending, bukan Visual!
        // Kalau 4 arg → C# pakai constructor Visual (exact match) → imageOptions kosong!
        Question q = new Question(QuestionType.PhonologyBlending, "", correctImg, imageOptions, "");
        q.syllableAudios = syllableAudio;
        return q;
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
        string[] allSyllables     = (string[])entry[2];  // hard distractor (default)
        string[] syllableAudios   = (string[])entry[3];

        // Diff 1-2 → pakai easy distraktor (suku kata beda jauh, gampang dibedakan)
        // Diff 3-5 → pakai hard distraktor (suku kata mirip fonetis, perlu fokus)
        if (difficulty <= 2 && entry.Length > 4)
        {
            string easyDist      = (string)entry[4];
            string easyDistAudio = entry.Length > 5 ? (string)entry[5] : "";

            // Bangun array baru: 2 suku kata benar + 1 easy distraktor
            allSyllables   = new string[] { allSyllables[0], allSyllables[1], easyDist };
            syllableAudios = new string[] { syllableAudios[0], syllableAudios[1], easyDistAudio };
        }

        // Shuffle index BERSAMA supaya text dan audio tetap sinkron
        List<int> indices = new List<int>();
        for (int i = 0; i < allSyllables.Length; i++) indices.Add(i);
        ShuffleList(indices);

        string[] shuffledSyl   = new string[allSyllables.Length];
        string[] shuffledAudio = new string[syllableAudios.Length];
        for (int i = 0; i < indices.Count; i++)
        {
            shuffledSyl[i]   = allSyllables[indices[i]];
            shuffledAudio[i] = syllableAudios[indices[i]];
        }

        return new Question(
            QuestionType.PhonologySegmenting,
            imagePath,
            correctSyllables,
            shuffledSyl,
            shuffledAudio
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