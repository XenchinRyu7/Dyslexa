using UnityEngine;
using UnityEngine.UI;
using TMPro;

public enum NodeState
{
    Locked,
    Unlocked
}

public class LevelNode : MonoBehaviour
{
    private Button button;
    private Image image;

    [Header("Node Label")]
    public TextMeshProUGUI nodeNumberText; // Assign di Prefab Inspector

    [Header("UI Bintang")]
    public GameObject starContainer; // Objek StarContainer di dalam Node Prefab
    public Unity.VectorGraphics.SVGImage[] starImages; // Array 3 bintang
    public Sprite starFilled;
    public Sprite starEmpty;

    public Color unlockedColor = new Color(0.2f, 0.8f, 0.3f);
    public Color lockedColor = new Color(0.8f, 0.8f, 0.8f);

    void Awake()
    {
        button = GetComponent<Button>();
        image = GetComponent<Image>();
    }

    // Dipanggil oleh LevelMapGenerator saat generate node
    public void SetNodeNumber(int number)
    {
        if (nodeNumberText != null)
            nodeNumberText.text = number.ToString();
        else
            Debug.LogWarning($"[LevelNode] nodeNumberText belum di-assign di prefab! Node: {gameObject.name}");
    }

    public void SetState(NodeState state)
    {
        if (state == NodeState.Locked)
        {
            image.color = lockedColor;
            button.interactable = false;
            if (starContainer != null) starContainer.SetActive(false); // Sembunyikan bintang kalau kekunci
        }
        else
        {
            image.color = unlockedColor;
            button.interactable = true;
            if (starContainer != null) starContainer.SetActive(true); // Munculkan bintang
        }
    }

    // Dipanggil dari LevelMapGenerator buat ngisi bintang di Node ini
    public void SetStars(int starCount)
    {
        if (starImages != null && starImages.Length >= 3)
        {
            for (int i = 0; i < 3; i++)
            {
                if (starImages[i] != null)
                {
                    starImages[i].sprite = (i < starCount) ? starFilled : starEmpty;
                }
            }
        }
    }
}