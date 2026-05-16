using System.Collections.Generic;
using System.IO;
using System.Text;
using System;
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
        tempName = name.Trim();
        Debug.Log($"[ProfileManager] Temp Name set to: {tempName}");
    }

    public bool IsProfileNameTaken(string name)
    {
        if (string.IsNullOrWhiteSpace(name)) return false;

        string normalizedName = name.Trim();
        return allProfiles.Exists(profile =>
            string.Equals(profile.playerName?.Trim(), normalizedName, StringComparison.OrdinalIgnoreCase));
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

        // Inisialisasi progress kosong untuk profil baru
        ProgressManager.Instance.LoadForProfile(newProfile.profileId);

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

            // Load progress khusus profil ini
            ProgressManager.Instance.LoadForProfile(ActiveProfile.profileId);
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
            string msg = "Tidak ada profil untuk diekspor.";
            Debug.LogWarning($"[ProfileManager] {msg}");
            ShowAndroidToast(msg);
            return;
        }

        // Simpan ke Persistent Data Path (Bisa dibaca di PC / Smart TV lewat File Explorer)
        string fileName = "Laporan_Dyslexa_" + System.DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".csv";
        string csvPath = Path.Combine(Application.persistentDataPath, fileName);

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
            string successMsg = $"Data berhasil diekspor ke:\n{csvPath}";
            Debug.Log($"[ProfileManager] {successMsg}");
            ShowAndroidToast("Export Sukses! File tersimpan.");

            #if UNITY_EDITOR || UNITY_STANDALONE
            // Di PC / Mac / Smart TV (Standalone), langsung buka foldernya atau filenya
            Application.OpenURL("file://" + csvPath);
            #elif UNITY_ANDROID
            // --- CARA MENGGUNAKAN NATIVE SHARE (ANDROID) ---
            // 1. Download plugin gratis "Native Share for Android & iOS" dari Unity Asset Store
            // 2. Import ke dalam project Unity Anda
            // 3. Hapus tanda komentar (//) pada 2 baris kode di bawah ini:
            
            new NativeShare().AddFile(csvPath).SetSubject("Laporan Dyslexa").SetText("Berikut lampiran data CSV.").Share();
            ShowAndroidToast("Membuka menu Share Android...");
            #endif
        }
        catch (System.Exception ex)
        {
            string errorMsg = $"Gagal mengekspor CSV: {ex.Message}";
            Debug.LogError($"[ProfileManager] {errorMsg}");
            ShowAndroidToast("Error: " + ex.Message);
        }
    }

    // --- ANDROID TOAST HELPER ---
    public void ShowAndroidToast(string message)
    {
        #if UNITY_ANDROID && !UNITY_EDITOR
        try
        {
            AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            AndroidJavaObject currentActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
            AndroidJavaClass toastClass = new AndroidJavaClass("android.widget.Toast");
            
            currentActivity.Call("runOnUiThread", new AndroidJavaRunnable(() =>
            {
                // Instantiate the Toast
                AndroidJavaObject toastObject = toastClass.CallStatic<AndroidJavaObject>("makeText", currentActivity, message, 1);
                toastObject.Call("show");
            }));
        }
        catch (System.Exception e)
        {
            Debug.LogError("[ProfileManager] Gagal memunculkan Android Toast: " + e.Message);
        }
        #endif
    }
}
