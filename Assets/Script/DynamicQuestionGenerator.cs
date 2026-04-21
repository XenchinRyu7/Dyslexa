using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Dynamic Question Generator - Generate visual & phonological questions on-the-fly
/// Tidak pake JSON, full algorithmic generation berdasarkan confusion patterns
/// </summary>
public class DynamicQuestionGenerator : MonoBehaviour
{
    // ========== VISUAL CONFUSION PATTERNS ==========
    
    // Mirror horizontal (b↔d, p↔q)
    private static readonly Dictionary<char, char[]> mirrorHorizontal = new Dictionary<char, char[]>
    {
        {'b', new[] {'d', 'p', 'q'}},
        {'d', new[] {'b', 'q', 'p'}},
        {'p', new[] {'q', 'b', 'd'}},
        {'q', new[] {'p', 'd', 'b'}},
    };

    // Rotation 180° (m↔w, n↔u)
    private static readonly Dictionary<char, char[]> rotation180 = new Dictionary<char, char[]>
    {
        {'m', new[] {'w', 'n', 'v'}},
        {'w', new[] {'m', 'v', 'u'}},
        {'n', new[] {'u', 'h', 'm'}},
        {'u', new[] {'n', 'v', 'w'}},
    };

    // Similar shapes
    private static readonly Dictionary<char, char[]> similarShapes = new Dictionary<char, char[]>
    {
        {'h', new[] {'n', 'b', 'm'}},
        {'v', new[] {'w', 'u', 'y'}},
        {'c', new[] {'o', 'e', 'g'}},
        {'o', new[] {'c', 'q', 'g'}},
        {'i', new[] {'l', 'j', '1'}},
        {'s', new[] {'z', '5', '2'}},
    };

    // Kata-kata untuk reversal testing
    private static readonly string[] reversibleWords = new[]
    {
        "no", "on",      // reversal
        "saw", "was",
        "top", "pot",
        "tap", "pat",
        "tar", "rat",
        "god", "dog",
        "bud", "dub",
    };

    // ========== PHONOLOGICAL CONFUSION PATTERNS ==========
    
    // Voiced ↔ Voiceless
    private static readonly Dictionary<string, string[]> voicedVoiceless = new Dictionary<string, string[]>
    {
        {"B", new[] {"P", "M", "D"}},
        {"D", new[] {"T", "N", "L"}},
        {"G", new[] {"K", "H", "NG"}},
        {"P", new[] {"B", "M", "F"}},
        {"T", new[] {"D", "N", "S"}},
        {"K", new[] {"G", "H", "KH"}},
    };

    // Nasal confusion
    private static readonly Dictionary<string, string[]> nasalConfusion = new Dictionary<string, string[]>
    {
        {"M", new[] {"N", "NG", "B"}},
        {"N", new[] {"M", "NG", "D"}},
        {"NG", new[] {"N", "M", "G"}},
    };

    // Liquid confusion (sangat umum!)
    private static readonly Dictionary<string, string[]> liquidConfusion = new Dictionary<string, string[]>
    {
        {"L", new[] {"R", "N", "D"}},
        {"R", new[] {"L", "W", "Y"}},
    };

    // Audio files yang available
    private static readonly string[] availableAudioFiles = new[]
    {
        "b", "d", "m", "n", "p", "q", "w",
        "bola", "buku", "gula"
    };

    // ========== WORD BANKS PER DIFFICULTY ==========
    
    private static readonly Dictionary<int, string[]> wordBanksByDifficulty = new Dictionary<int, string[]>
    {
        {1, new[] {"b", "d", "p", "q", "m", "n", "u", "w"}},
        {2, new[] {"bola", "dola", "pola", "buku", "duku", "puku", "mana", "wana"}},
        {3, new[] {"bangku", "mobil", "pintu", "minum", "dunia", "warna"}},
        {4, new[] {"pembuat", "penumpang", "berdua", "dengan", "mungkin"}},
        {5, new[] {"mendapatkan", "pembuangan", "pengumpulan", "membicarakan", "dibandingkan"}},
    };
    
    // ========== MAIN GENERATION FUNCTIONS ==========

    /// <summary>
    /// Generate dynamic visual question (reading-based, confusion patterns)
    /// </summary>
    public Question GenerateVisualQuestion(int difficulty)
    {
        if (difficulty == 1)
        {
            // Single letter dengan mirror/rotation confusion
            return GenerateSingleLetterVisual();
        }
        else if (difficulty == 2)
        {
            // Simple words dengan letter substitution
            return GenerateSimpleWordVisual();
        }
        else if (difficulty == 3)
        {
            // Words dengan multiple confusions
            return GenerateWordConfusionVisual(difficulty);
        }
        else if (difficulty == 4)
        {
            // Longer words + reversal
            return GenerateComplexWordVisual(difficulty);
        }
        else // difficulty 5
        {
            // Complex words dengan heavy confusion
            return GenerateAdvancedWordVisual(difficulty);
        }
    }

    /// <summary>
    /// Generate dynamic phonology question (listening-based, audio)
    /// </summary>
    public Question GeneratePhonologyQuestion(int difficulty)
    {
        // Pilih random audio yang available
        string audioFile = availableAudioFiles[Random.Range(0, availableAudioFiles.Length)];
        string audioClipName = $"Audio/sound_{audioFile}";

        // Determine correct answer
        string correctAnswer = audioFile.ToUpper();
        string stimulus = "🔊 Dengarkan kata yang disebutkan";

        // Generate distractors based on phonological confusion
        List<string> options = new List<string> { correctAnswer };
        
        if (audioFile.Length == 1)
        {
            // Single letter phonology
            options.AddRange(GeneratePhonologicalDistractors(audioFile.ToUpper(), 3));
        }
        else
        {
            // Word phonology (e.g., BOLA → POLA, DOLA, BODA)
            options.AddRange(GenerateWordPhonologicalDistractors(correctAnswer, 3));
        }

        options = options.Distinct().Take(4).ToList();
        ShuffleList(options);

        return new Question(QuestionType.Phonology, stimulus, correctAnswer, options, audioClipName);
    }

    // ========== VISUAL GENERATION HELPERS ==========

    private Question GenerateSingleLetterVisual()
    {
        // Pilih dari mirror atau rotation group
        Dictionary<char, char[]> chosenGroup = Random.value > 0.5f ? mirrorHorizontal : rotation180;
        var entry = chosenGroup.ElementAt(Random.Range(0, chosenGroup.Count));
        
        char correctLetter = entry.Key;
        string correctAnswer = correctLetter.ToString();
        string stimulus = $"Pilih huruf '{correctLetter}'";
        
        // Options: correct + confusions
        List<string> options = new List<string> { correctAnswer };
        foreach (char distractor in entry.Value)
        {
            options.Add(distractor.ToString());
            if (options.Count >= 4) break;
        }
        
        options = options.Take(4).ToList();
        ShuffleList(options);
        
        return new Question(QuestionType.Visual, stimulus, correctAnswer, options, "");
    }

    private Question GenerateSimpleWordVisual()
    {
        string[] words = wordBanksByDifficulty[2];
        string correctWord = words[Random.Range(0, words.Length)];
        
        string correctAnswer = correctWord;
        string stimulus = $"Pilih kata '{correctWord}'";
        
        // Generate distractors dengan substitution
        List<string> options = new List<string> { correctWord };
        options.AddRange(GenerateVisualWordDistractors(correctWord, 3));
        
        options = options.Distinct().Take(4).ToList();
        ShuffleList(options);
        
        return new Question(QuestionType.Visual, stimulus, correctAnswer, options, "");
    }

    private Question GenerateWordConfusionVisual(int difficulty)
    {
        string[] words = wordBanksByDifficulty.ContainsKey(difficulty) 
            ? wordBanksByDifficulty[difficulty] 
            : wordBanksByDifficulty[3];
            
        string correctWord = words[Random.Range(0, words.Length)];
        
        string correctAnswer = correctWord;
        string stimulus = $"Pilih kata '{correctWord}'";
        
        // Generate heavy distractors
        List<string> options = new List<string> { correctWord };
        options.AddRange(GenerateVisualWordDistractors(correctWord, 3));
        
        options = options.Distinct().Take(4).ToList();
        ShuffleList(options);
        
        return new Question(QuestionType.Visual, stimulus, correctAnswer, options, "");
    }

    private Question GenerateComplexWordVisual(int difficulty)
    {
        return GenerateWordConfusionVisual(difficulty);
    }

    private Question GenerateAdvancedWordVisual(int difficulty)
    {
        return GenerateWordConfusionVisual(difficulty);
    }

    /// <summary>
    /// Generate visual distractors untuk kata dengan letter substitution
    /// </summary>
    private List<string> GenerateVisualWordDistractors(string word, int count)
    {
        List<string> distractors = new List<string>();
        
        for (int i = 0; i < count; i++)
        {
            string distractor = SubstituteConfusableLetter(word);
            if (distractor != word && !distractors.Contains(distractor))
            {
                distractors.Add(distractor);
            }
        }
        
        // Fill remaining dengan random substitutions
        while (distractors.Count < count)
        {
            string distractor = SubstituteConfusableLetter(word);
            if (!distractors.Contains(distractor) && distractor != word)
            {
                distractors.Add(distractor);
            }
        }
        
        return distractors;
    }

    /// <summary>
    /// Substitute huruf di kata dengan confusion pattern
    /// </summary>
    private string SubstituteConfusableLetter(string word)
    {
        if (string.IsNullOrEmpty(word)) return word;
        
        char[] chars = word.ToCharArray();
        int position = Random.Range(0, chars.Length);
        char original = char.ToLower(chars[position]);
        
        // Cari di confusion dictionaries
        if (mirrorHorizontal.ContainsKey(original))
        {
            char replacement = mirrorHorizontal[original][Random.Range(0, mirrorHorizontal[original].Length)];
            chars[position] = char.IsUpper(word[position]) ? char.ToUpper(replacement) : replacement;
        }
        else if (rotation180.ContainsKey(original))
        {
            char replacement = rotation180[original][Random.Range(0, rotation180[original].Length)];
            chars[position] = char.IsUpper(word[position]) ? char.ToUpper(replacement) : replacement;
        }
        else if (similarShapes.ContainsKey(original))
        {
            char replacement = similarShapes[original][Random.Range(0, similarShapes[original].Length)];
            chars[position] = char.IsUpper(word[position]) ? char.ToUpper(replacement) : replacement;
        }
        
        return new string(chars);
    }

    // ========== PHONOLOGY GENERATION HELPERS ==========

    /// <summary>
    /// Generate phonological distractors untuk single letter
    /// </summary>
    private List<string> GeneratePhonologicalDistractors(string letter, int count)
    {
        List<string> distractors = new List<string>();
        
        // Try voiced/voiceless
        if (voicedVoiceless.ContainsKey(letter))
        {
            distractors.AddRange(voicedVoiceless[letter]);
        }
        
        // Try nasal
        if (nasalConfusion.ContainsKey(letter))
        {
            distractors.AddRange(nasalConfusion[letter]);
        }
        
        // Try liquid
        if (liquidConfusion.ContainsKey(letter))
        {
            distractors.AddRange(liquidConfusion[letter]);
        }
        
        return distractors.Distinct().Take(count).ToList();
    }

    /// <summary>
    /// Generate phonological distractors untuk word (substitute consonants)
    /// </summary>
    private List<string> GenerateWordPhonologicalDistractors(string word, int count)
    {
        List<string> distractors = new List<string>();
        
        for (int i = 0; i < count * 2; i++) // Generate lebih banyak, ambil yang unik
        {
            string distractor = SubstitutePhonologicalConsonant(word);
            if (distractor != word && !distractors.Contains(distractor))
            {
                distractors.Add(distractor);
            }
            
            if (distractors.Count >= count) break;
        }
        
        return distractors.Take(count).ToList();
    }

    /// <summary>
    /// Substitute consonant di kata dengan phonological confusion
    /// </summary>
    private string SubstitutePhonologicalConsonant(string word)
    {
        if (string.IsNullOrEmpty(word)) return word;
        
        char[] chars = word.ToCharArray();
        
        // Cari consonant untuk di-substitute
        for (int attempt = 0; attempt < 10; attempt++)
        {
            int position = Random.Range(0, chars.Length);
            string original = chars[position].ToString().ToUpper();
            
            List<string> possibleReplacements = new List<string>();
            
            if (voicedVoiceless.ContainsKey(original))
                possibleReplacements.AddRange(voicedVoiceless[original]);
            if (nasalConfusion.ContainsKey(original))
                possibleReplacements.AddRange(nasalConfusion[original]);
            if (liquidConfusion.ContainsKey(original))
                possibleReplacements.AddRange(liquidConfusion[original]);
            
            if (possibleReplacements.Count > 0)
            {
                string replacement = possibleReplacements[Random.Range(0, possibleReplacements.Count)];
                chars[position] = char.IsUpper(word[position]) 
                    ? replacement[0] 
                    : char.ToLower(replacement[0]);
                break;
            }
        }
        
        return new string(chars);
    }

    // ========== UTILITY ==========

    private void ShuffleList<T>(List<T> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            T temp = list[i];
            list[i] = list[j];
            list[j] = temp;
        }
    }
}
