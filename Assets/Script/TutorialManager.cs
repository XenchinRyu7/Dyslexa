using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;

[Serializable]
public class TutorialStep
{
    public RectTransform targetRect;
    public RectTransform dragTargetRect; // Jika diisi, tangan akan animasi drag dari target ke sini
    public string text;
    public bool requiresExactClick; // Jika true, anak harus ngeklik targetRect
    public Action onStepComplete;
}

public class TutorialManager : MonoBehaviour
{
    public static TutorialManager Instance;

    [Header("Visual Settings")]
    [Tooltip("Geser posisi tangan (X,Y) dari Inspector biar pas di ujung jari. Contoh: X=50, Y=-50")]
    public Vector2 pointerOffset = Vector2.zero;

    private GameObject darkOverlay;
    private GameObject foregroundOverlay;
    private GameObject handPointer;
    private GameObject textBubble;
    private Button overlayButton; // Tombol gaib buat nangkep klik

    private Canvas addedCanvas;
    private GraphicRaycaster addedRaycaster;

    private List<TutorialStep> currentSequence;
    private int currentStepIndex = 0;
    private string currentPrefsKey = "";

    public bool IsPlaying
    {
        get { return darkOverlay != null && darkOverlay.activeSelf; }
    }

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public bool IsTutorialCompleted(string baseKey)
    {
        return PlayerPrefs.GetInt(GetProfileScopedKey(baseKey), 0) == 1;
    }

    private string GetProfileScopedKey(string baseKey)
    {
        if (PlayerProfileManager.Instance != null && PlayerProfileManager.Instance.ActiveProfile != null)
        {
            return PlayerProfileManager.Instance.ActiveProfile.profileId + "_" + baseKey;
        }
        return baseKey; // Fallback kalau lagi ngetest tanpa ProfileManager
    }

    /// <summary>
    /// Memulai antrian tutorial.
    /// prefsKey: kunci database (misal "Tutorial_ChooseMode")
    /// Jika nilainya sudah 1, tutorial akan di-skip.
    /// </summary>
    public void StartSequence(string prefsKey, List<TutorialStep> steps)
    {
        string scopedKey = GetProfileScopedKey(prefsKey);
        
        if (PlayerPrefs.GetInt(scopedKey, 0) == 1) return; // Sudah pernah
        if (steps == null || steps.Count == 0) return;

        currentPrefsKey = scopedKey;
        currentSequence = steps;
        currentStepIndex = 0;

        ShowStep(currentSequence[0]);
    }

    private void ShowStep(TutorialStep step)
    {
        if (step.targetRect == null)
        {
            NextStep();
            return;
        }

        // 1. Buat Dark Overlay
        if (darkOverlay == null)
        {
            darkOverlay = new GameObject("TutorialDarkOverlay");
            Canvas canvas = darkOverlay.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 90; // Di bawah target

            darkOverlay.AddComponent<GraphicRaycaster>();

            Image bg = darkOverlay.AddComponent<Image>();
            bg.color = new Color(0, 0, 0, 0.7f); // Layar meredup
        }
        darkOverlay.SetActive(true);

        // 1b. Buat Foreground Overlay
        if (foregroundOverlay == null)
        {
            foregroundOverlay = new GameObject("TutorialForegroundOverlay");
            Canvas fgCanvas = foregroundOverlay.AddComponent<Canvas>();
            fgCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            fgCanvas.sortingOrder = 110; 

            foregroundOverlay.AddComponent<GraphicRaycaster>();
        }
        foregroundOverlay.SetActive(true);

        // 2. Angkat Target biar terang
        // HANCURKAN RAYCASTER DULU SEBELUM CANVAS BIAR GAK ERROR
        if (addedRaycaster != null) Destroy(addedRaycaster);
        if (addedCanvas != null) Destroy(addedCanvas);

        addedCanvas = step.targetRect.gameObject.AddComponent<Canvas>();
        addedCanvas.overrideSorting = true;
        addedCanvas.sortingOrder = 100;
        addedRaycaster = step.targetRect.gameObject.AddComponent<GraphicRaycaster>();

        // 3. Pasang Hand Pointer
        if (handPointer == null)
        {
            handPointer = new GameObject("TutorialHandPointer");
            handPointer.transform.SetParent(foregroundOverlay.transform, false);
            Image handImg = handPointer.AddComponent<Image>();
            Sprite handSprite = Resources.Load<Sprite>("Image/Tutorial/hand_pointer");
            if (handSprite != null) handImg.sprite = handSprite;
            else handImg.color = Color.yellow; 

            RectTransform handRt = handPointer.GetComponent<RectTransform>();
            handRt.pivot = new Vector2(0.5f, 0.5f); // Balikin ke tengah normal
            handRt.sizeDelta = new Vector2(200, 200); // Diperbesar
        }
        handPointer.SetActive(true);

        StopAllCoroutines();
        if (step.dragTargetRect != null)
        {
            // Animasi Tarik & Lepas
            StartCoroutine(AnimateDragAndDrop(step.targetRect, step.dragTargetRect));
        }
        else
        {
            // Animasi Biasa (Nunjuk maju mundur)
            StartCoroutine(AnimateHand(step.targetRect));
        }

        // 4. Pasang Bubble Text
        ShowTextBubble(step.targetRect, step.text);

        // 5. Siapkan Tombol Penangkap Klik
        if (overlayButton == null)
        {
            GameObject btnObj = new GameObject("TutorialOverlayButton");
            btnObj.transform.SetParent(foregroundOverlay.transform, false);
            Image btnImg = btnObj.AddComponent<Image>();
            btnImg.color = new Color(0, 0, 0, 0); // Transparan
            overlayButton = btnObj.AddComponent<Button>();
        }
        overlayButton.gameObject.SetActive(true);

        RectTransform overlayRt = overlayButton.GetComponent<RectTransform>();
        
        // Kalau butuh klik pas di objek, sesuaikan ukuran overlay button.
        // Kalau nggak, penuhi layar (klik sembarang tempat).
        if (step.requiresExactClick)
        {
            overlayRt.anchorMin = new Vector2(0.5f, 0.5f);
            overlayRt.anchorMax = new Vector2(0.5f, 0.5f);
            
            Vector3[] corners = new Vector3[4];
            step.targetRect.GetWorldCorners(corners);
            Vector3 centerPos = (corners[0] + corners[2]) / 2f;

            overlayRt.position = centerPos;
            
            // Lebar dan tinggi asli dari elemen UI
            float width = Vector3.Distance(corners[0], corners[3]);
            float height = Vector3.Distance(corners[0], corners[1]);
            overlayRt.sizeDelta = new Vector2(width, height);
        }
        else
        {
            overlayRt.anchorMin = Vector2.zero;
            overlayRt.anchorMax = Vector2.one;
            overlayRt.sizeDelta = Vector2.zero;
            overlayRt.anchoredPosition = Vector2.zero;
        }

        overlayButton.onClick.RemoveAllListeners();
        overlayButton.onClick.AddListener(() =>
        {
            step.onStepComplete?.Invoke();
            NextStep();
        });
    }

    private void ShowTextBubble(RectTransform targetRect, string textMsg)
    {
        if (textBubble == null)
        {
            textBubble = new GameObject("TutorialTextBubble");
            textBubble.transform.SetParent(foregroundOverlay.transform, false);
            Image bubbleBg = textBubble.AddComponent<Image>();
            
            Sprite bubbleSprite = Resources.Load<Sprite>("Image/Tutorial/bubble");
            if (bubbleSprite != null) 
            {
                bubbleBg.sprite = bubbleSprite;
                bubbleBg.type = Image.Type.Sliced; // Biar bisa melar bagus kalau spritenya 9-slice
            }
            else 
            {
                // Fallback kalau nggak ada gambar bubble, pakai kotak putih biasa
                bubbleBg.color = new Color(1f, 1f, 1f, 0.95f);
            }

            GameObject textObj = new GameObject("BubbleText");
            textObj.transform.SetParent(textBubble.transform, false);
            
            // Pake TextMeshProUGUI otomatis pakai default font asset lu (LilitaOne)
            TextMeshProUGUI txt = textObj.AddComponent<TextMeshProUGUI>();
            txt.color = Color.black;
            txt.fontSize = 48; // Font dikecilin biar nggak lebay
            txt.alignment = TextAlignmentOptions.Center;
            txt.enableWordWrapping = true;

            RectTransform bubbleRt = textBubble.GetComponent<RectTransform>();
            // Ukuran awal dihapus karena nanti bakal dihitung otomatis di bawah

            RectTransform textRt = textObj.GetComponent<RectTransform>();
            textRt.anchorMin = Vector2.zero;
            textRt.anchorMax = Vector2.one;
            // Kasih margin/padding sedikit biar teks nggak nabrak pinggir kotak
            textRt.offsetMin = new Vector2(20, 20);
            textRt.offsetMax = new Vector2(-20, -20);
        }

        textBubble.SetActive(true);
        TextMeshProUGUI bubbleTxt = textBubble.GetComponentInChildren<TextMeshProUGUI>();
        bubbleTxt.text = textMsg;

        // --- SISTEM FLEKSIBEL (AUTO RESIZE BUBBLE) ---
        bubbleTxt.ForceMeshUpdate();
        // Lebar maksimal bubble 600 pixel biar nggak kepanjangan dan lebih cepat pindah baris
        Vector2 textSize = bubbleTxt.GetPreferredValues(textMsg, 600f, 0f); 
        
        RectTransform rt = textBubble.GetComponent<RectTransform>();
        // Ukuran bubble = ukuran teks + padding 60 pixel biar nggak mepet pinggir
        rt.sizeDelta = new Vector2(textSize.x + 60f, textSize.y + 60f);

        // Posisikan bubble statis di bagian bawah-tengah layar (biar gak nutupin objek game)
        rt.anchorMin = new Vector2(0.5f, 0f); // Titik anchor di tengah bawah
        rt.anchorMax = new Vector2(0.5f, 0f);
        rt.pivot = new Vector2(0.5f, 0f);

        // Skala manual berdasarkan tinggi layar
        float myScale = Screen.height / 1080f;
        textBubble.transform.localScale = new Vector3(myScale, myScale, 1f);

        // Posisi Y kasih margin sedikit dari bawah biar gak mentok layar banget
        rt.anchoredPosition = new Vector2(0, 50f * myScale); 
    }

    private IEnumerator AnimateHand(RectTransform targetRect)
    {
        RectTransform handRt = handPointer.GetComponent<RectTransform>();
        float myScale = Screen.height / 1080f;
        handRt.localScale = new Vector3(myScale, myScale, 1f);

        Canvas targetCanvas = targetRect.GetComponentInParent<Canvas>();
        Camera cam = (targetCanvas != null && targetCanvas.renderMode != RenderMode.ScreenSpaceOverlay) ? 
                     (targetCanvas.worldCamera != null ? targetCanvas.worldCamera : Camera.main) : null;

        while (darkOverlay != null && darkOverlay.activeSelf)
        {
            if (targetRect == null) yield break;

            // CARI TITIK TENGAH OBJEK YANG ASLI MENGGUNAKAN CORNER (Mengabaikan Pivot)
            Vector3[] corners = new Vector3[4];
            targetRect.GetWorldCorners(corners);
            Vector3 worldCenter = (corners[0] + corners[2]) / 2f;
            Vector2 screenPos = RectTransformUtility.WorldToScreenPoint(cam, worldCenter);

            // Terapkan offset dari Inspector, dikali rasio layar biar konsisten
            Vector3 offsetPixels = new Vector3(pointerOffset.x * myScale, pointerOffset.y * myScale, 0);

            Vector3 startPos = new Vector3(screenPos.x + (100f * myScale), screenPos.y - (100f * myScale), 0) + offsetPixels;
            Vector3 endPos = new Vector3(screenPos.x, screenPos.y, 0) + offsetPixels;
            
            float t = Mathf.PingPong(Time.time * 2f, 1f);
            float smoothT = t * t * (3f - 2f * t); 
            handRt.position = Vector3.Lerp(startPos, endPos, smoothT);
            yield return null;
        }
    }

    private IEnumerator AnimateDragAndDrop(RectTransform sourceRect, RectTransform destRect)
    {
        RectTransform handRt = handPointer.GetComponent<RectTransform>();
        float myScale = Screen.height / 1080f;
        handRt.localScale = new Vector3(myScale, myScale, 1f);

        Canvas targetCanvas = sourceRect.GetComponentInParent<Canvas>();
        Camera cam = (targetCanvas != null && targetCanvas.renderMode != RenderMode.ScreenSpaceOverlay) ? 
                     (targetCanvas.worldCamera != null ? targetCanvas.worldCamera : Camera.main) : null;
        
        while (darkOverlay != null && darkOverlay.activeSelf)
        {
            if (sourceRect == null || destRect == null) yield break;

            // CARI TITIK TENGAH ASLI UNTUK SOURCE DAN DEST
            Vector3[] srcCorners = new Vector3[4];
            sourceRect.GetWorldCorners(srcCorners);
            Vector3 srcWorldCenter = (srcCorners[0] + srcCorners[2]) / 2f;

            Vector3[] dstCorners = new Vector3[4];
            destRect.GetWorldCorners(dstCorners);
            Vector3 dstWorldCenter = (dstCorners[0] + dstCorners[2]) / 2f;

            Vector2 srcScreen = RectTransformUtility.WorldToScreenPoint(cam, srcWorldCenter);
            Vector2 dstScreen = RectTransformUtility.WorldToScreenPoint(cam, dstWorldCenter);

            // Terapkan offset dari Inspector
            Vector3 offsetPixels = new Vector3(pointerOffset.x * myScale, pointerOffset.y * myScale, 0);

            Vector3 startPos = new Vector3(srcScreen.x, srcScreen.y, 0) + offsetPixels;
            Vector3 endPos = new Vector3(dstScreen.x, dstScreen.y, 0) + offsetPixels;

            float duration = 1.5f;
            float timer = 0;

            while (timer < duration)
            {
                timer += Time.deltaTime;
                float t = timer / duration;
                float smoothT = t * t * (3f - 2f * t); 
                handRt.position = Vector3.Lerp(startPos, endPos, smoothT);
                yield return null;
            }

            // Step 2: Menghilang sekilas lalu ngulang
            handPointer.SetActive(false);
            yield return new WaitForSeconds(0.2f);
            handPointer.SetActive(true);
        }
    }

    private void NextStep()
    {
        currentStepIndex++;
        if (currentStepIndex < currentSequence.Count)
        {
            ShowStep(currentSequence[currentStepIndex]);
        }
        else
        {
            FinishSequence();
        }
    }

    private void FinishSequence()
    {
        PlayerPrefs.SetInt(currentPrefsKey, 1);
        PlayerPrefs.Save();

        // HANCURKAN RAYCASTER DULU SEBELUM CANVAS BIAR GAK ERROR
        if (addedRaycaster != null) Destroy(addedRaycaster);
        if (addedCanvas != null) Destroy(addedCanvas);
        if (darkOverlay != null) darkOverlay.SetActive(false);
        if (foregroundOverlay != null) foregroundOverlay.SetActive(false);
    }
}
