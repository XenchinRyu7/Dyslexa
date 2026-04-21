using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

public class PlayerProfileManager : MonoBehaviour
{
    public static PlayerProfileManager Instance { get; private set; }

    private string saveFilePath;
    private List<PlayerProfile> allProfiles = new List<PlayerProfile>();

    public PlayerProfile ActiveProfile { get; private set; }

    // Temporary variables to hold data between scenes during Onboarding
    private string tempName;
    private int tempAge;
    private string tempGender;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            saveFilePath = Path.Combine(Application.persistentDataPath, "player_profiles.json");
            LoadProfiles();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // --- ONBOARDING FLOW (MULTI-SCENE) ---

    // 1. Call this when pressing OK on "NewGame" (Input Name) scene
    public void SetTempName(string name)
    {
        tempName = name;
        Debug.Log($"[ProfileManager] Temp Name set to: {tempName}");
    }

    // 2. Call this when pressing OK on "OnboardingAge" scene
    public void SetTempAge(int age)
    {
        tempAge = age;
        Debug.Log($"[ProfileManager] Temp Age set to: {tempAge}");
    }

    // 3. Call this when pressing OK on "OnboardingGender" scene
    // This will finalize the creation and save to JSON
    public void SetTempGenderAndSave(string gender)
    {
        tempGender = gender;
        Debug.Log($"[ProfileManager] Temp Gender set to: {tempGender}");

        // Create the new profile
        PlayerProfile newProfile = new PlayerProfile(tempName, tempAge, tempGender);
        allProfiles.Add(newProfile);

        // Set as active profile
        ActiveProfile = newProfile;

        // Save to JSON
        SaveProfiles();
        
        Debug.Log($"[ProfileManager] New profile created and saved! ID: {newProfile.profileId}");
    }

    // --- SAVE / LOAD SYSTEM (JSON) ---

    public void SaveProfiles()
    {
        PlayerProfileData data = new PlayerProfileData();
        data.profiles = allProfiles.ToArray();

        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(saveFilePath, json);
        Debug.Log($"[ProfileManager] Saved {allProfiles.Count} profiles to {saveFilePath}");
    }

    public void LoadProfiles()
    {
        if (File.Exists(saveFilePath))
        {
            string json = File.ReadAllText(saveFilePath);
            PlayerProfileData data = JsonUtility.FromJson<PlayerProfileData>(json);
            
            if (data != null && data.profiles != null)
            {
                allProfiles = new List<PlayerProfile>(data.profiles);
                Debug.Log($"[ProfileManager] Loaded {allProfiles.Count} profiles.");
            }
        }
        else
        {
            Debug.Log("[ProfileManager] No save file found. Starting fresh.");
        }
    }

    public List<PlayerProfile> GetAllProfiles()
    {
        return allProfiles;
    }

    // Call this from "ChooseAccount" / "ContinueGame" scene
    public void SelectProfile(string profileId)
    {
        ActiveProfile = allProfiles.Find(p => p.profileId == profileId);
        if (ActiveProfile != null)
        {
            Debug.Log($"[ProfileManager] Active profile selected: {ActiveProfile.playerName}");
        }
    }

    // Call this to delete a profile and save the changes
    public void DeleteProfile(string profileId)
    {
        PlayerProfile profileToDelete = allProfiles.Find(p => p.profileId == profileId);
        if (profileToDelete != null)
        {
            allProfiles.Remove(profileToDelete);
            SaveProfiles();
            Debug.Log($"[ProfileManager] Profile deleted: {profileToDelete.playerName}");
        }
    }

    // --- EXPORT TO EXCEL (CSV) ---

    public void ExportToCSV()
    {
        if (allProfiles.Count == 0)
        {
            Debug.LogWarning("[ProfileManager] No profiles to export.");
            return;
        }

        // We will save the CSV in the persistent data path
        string csvPath = Path.Combine(Application.persistentDataPath, "ExportedData_Dyslexa.csv");

        // Use StringBuilder to build the CSV string
        StringBuilder sb = new StringBuilder();
        
        // Header
        sb.AppendLine("ProfileID,Name,Age,Gender,CreationDate");

        // Data
        foreach (var profile in allProfiles)
        {
            // Simple comma separation
            sb.AppendLine($"{profile.profileId},{profile.playerName},{profile.age},{profile.gender},{profile.creationDate}");
        }

        try
        {
            File.WriteAllText(csvPath, sb.ToString());
            Debug.Log($"[ProfileManager] Successfully exported data to CSV at: {csvPath}");
            
            // On Windows/Mac editor, this will help you find the file easily
            #if UNITY_EDITOR
            UnityEditor.EditorUtility.RevealInFinder(csvPath);
            #endif
        }
        catch (IOException ex)
        {
            Debug.LogError($"[ProfileManager] Failed to export CSV: {ex.Message}");
        }
    }
}
