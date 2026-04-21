using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class ProfileLoader : MonoBehaviour
{
    [Header("Referensi UI")]
    public GameObject profileButtonPrefab; // Prefab tombol (DockName)
    public Transform container; // ContainerLayout yang ada Vertical Layout Group-nya

    [Header("Confirm Delete Window")]
    public GameObject confirmDeleteWindow; // Panel konfirmasi hapus (show/hide)
    public Button btnYes;                 // Tombol "Ya, Hapus"
    public Button btnNo;                  // Tombol "Batal"

    // Menyimpan ID profil yang sedang menunggu konfirmasi hapus
    private string pendingDeleteProfileId = null;

    void Start()
    {
        // Pastikan window konfirmasi tersembunyi di awal
        if (confirmDeleteWindow != null)
            confirmDeleteWindow.SetActive(false);

        // Assign listener ke BtnYes dan BtnNo
        if (btnYes != null)
            btnYes.onClick.AddListener(OnConfirmDelete);

        if (btnNo != null)
            btnNo.onClick.AddListener(CloseConfirmWindow);

        LoadAndDisplayProfiles();
    }

    public void LoadAndDisplayProfiles()
    {
        // 1. Bersihkan dulu isi container (jika ada tombol dummy/bekas)
        foreach (Transform child in container)
        {
            Destroy(child.gameObject);
        }

        // 2. Ambil semua data profil dari Manager
        List<PlayerProfile> profiles = PlayerProfileManager.Instance.GetAllProfiles();

        if (profiles.Count == 0)
        {
            Debug.Log("[ProfileLoader] Belum ada profil yang tersimpan.");
            return;
        }

        // 3. Looping untuk membuat tombol sebanyak profil yang ada
        foreach (PlayerProfile profile in profiles)
        {
            // Buat copy dari prefab
            GameObject newButton = Instantiate(profileButtonPrefab, container);

            // Ubah Teks-nya menjadi nama anak saja
            TextMeshProUGUI nameText = newButton.GetComponentInChildren<TextMeshProUGUI>();
            if (nameText != null)
            {
                nameText.text = profile.playerName;
            }

            // Tambahkan fungsi klik (Event Listener) ke tombol utama (Pilih Profil)
            Button mainBtn = newButton.GetComponent<Button>();
            if (mainBtn != null)
            {
                string savedProfileId = profile.profileId;
                mainBtn.onClick.AddListener(() =>
                {
                    OnProfileClicked(savedProfileId);
                });
            }

            // CARI TOMBOL DELETE (nama child harus 'BtnDelete' di dalam Prefab)
            Transform deleteBtnTransform = newButton.transform.Find("BtnDelete");
            if (deleteBtnTransform != null)
            {
                Button deleteBtn = deleteBtnTransform.GetComponent<Button>();
                if (deleteBtn != null)
                {
                    string savedProfileId = profile.profileId;
                    deleteBtn.onClick.AddListener(() =>
                    {
                        // Tampilkan window konfirmasi, bukan langsung hapus
                        OpenConfirmWindow(savedProfileId);
                    });
                }
            }
        }
    }

    // -------------------------------------------
    // CONFIRM DELETE WINDOW
    // -------------------------------------------

    // Buka window konfirmasi dan simpan ID profil yang mau dihapus
    private void OpenConfirmWindow(string profileId)
    {
        pendingDeleteProfileId = profileId;

        if (confirmDeleteWindow != null)
        {
            confirmDeleteWindow.SetActive(true);
            Debug.Log($"[ProfileLoader] Konfirmasi hapus untuk profil ID: {profileId}");
        }
        else
        {
            Debug.LogWarning("[ProfileLoader] confirmDeleteWindow belum di-assign di Inspector!");
        }
    }

    // Tutup window konfirmasi — dipanggil BtnNo ATAU setelah BtnYes selesai hapus
    public void CloseConfirmWindow()
    {
        pendingDeleteProfileId = null;

        if (confirmDeleteWindow != null)
            confirmDeleteWindow.SetActive(false);

        Debug.Log("[ProfileLoader] Window konfirmasi ditutup.");
    }

    // Dipanggil BtnYes: eksekusi hapus profil, lalu tutup window & refresh list
    private void OnConfirmDelete()
    {
        if (!string.IsNullOrEmpty(pendingDeleteProfileId))
        {
            PlayerProfileManager.Instance.DeleteProfile(pendingDeleteProfileId);
            Debug.Log($"[ProfileLoader] Profil dihapus: {pendingDeleteProfileId}");
        }

        CloseConfirmWindow();
        LoadAndDisplayProfiles();
    }

    // -------------------------------------------
    // NAVIGASI
    // -------------------------------------------

    // Fungsi yang dipanggil saat tombol profil diklik
    private void OnProfileClicked(string profileId)
    {
        Debug.Log($"[ProfileLoader] Memilih profil dengan ID: {profileId}");
        PlayerProfileManager.Instance.SelectProfile(profileId);
        SceneManager.LoadScene("ChooseMode");
    }
}
