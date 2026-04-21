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
        }
        else
        {
            image.color = unlockedColor;
            button.interactable = true;
        }
    }
}