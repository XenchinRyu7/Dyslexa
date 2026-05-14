using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class LevelMapGenerator : MonoBehaviour
{
    public GameObject nodePrefab;
    public Transform content;

    public int totalNodes = 10;
    public float xSpacing = 600f;  
    public float waveHeight = 200f;
    public float waveFrequency = 0.5f;
    public float startOffset = 200f;

    private LevelNode[] levelNodes;
    private int currentUnlockedNode = 0;

    void Start()
    {
        LoadProgress();
        GenerateMap();
        UpdateNodeStates();
    }

    void GenerateMap()
    {
        RectTransform contentRT = content.GetComponent<RectTransform>();
        levelNodes = new LevelNode[totalNodes];

        for (int i = 0; i < totalNodes; i++)
        {
            GameObject node = Instantiate(nodePrefab, content);
            RectTransform rt = node.GetComponent<RectTransform>();

            float x = startOffset + i * xSpacing;
            float y = Mathf.Sin(i * waveFrequency) * waveHeight;

            rt.anchoredPosition = new Vector2(x, y);

            // Get LevelNode component and setup button
            LevelNode levelNode = node.GetComponent<LevelNode>();
            levelNodes[i] = levelNode;

            // Set nomor urut node (1-based: 1, 2, 3, ...)
            if (levelNode != null)
                levelNode.SetNodeNumber(i + 1);

            // Setup button click
            Button btn = node.GetComponent<Button>();
            int nodeIndex = i; // Capture for closure
            btn.onClick.AddListener(() => OnNodeClicked(nodeIndex));
        }

        float totalWidth = startOffset + (totalNodes - 1) * xSpacing + 400f;
        contentRT.sizeDelta = new Vector2(totalWidth, 800f);

        ScrollRect sr = content.GetComponentInParent<ScrollRect>();
        sr.horizontalNormalizedPosition = 0f;
        sr.velocity = Vector2.zero;
        Canvas.ForceUpdateCanvases();
    }

    void UpdateNodeStates()
    {
        string mode = PlayerPrefs.GetString("SelectedGameMode", "Visual");

        for (int i = 0; i < levelNodes.Length; i++)
        {
            if (levelNodes[i] != null)
            {
                // Unlock current node and all previous nodes
                if (i <= currentUnlockedNode)
                {
                    levelNodes[i].SetState(NodeState.Unlocked);

                    // Ambil dan set bintang dari Database
                    int earnedStars = ProgressManager.Instance.GetStarsForNode(mode, i);
                    levelNodes[i].SetStars(earnedStars);
                }
                else
                {
                    levelNodes[i].SetState(NodeState.Locked);
                }
            }
        }
    }

    void OnNodeClicked(int nodeIndex)
    {
        if (nodeIndex <= currentUnlockedNode)
        {
            Debug.Log($"[LevelMap] Starting session for node {nodeIndex}");
            
            // Save which node was selected
            PlayerPrefs.SetInt("SelectedNodeIndex", nodeIndex);
            PlayerPrefs.Save();

            // Log current global difficulty
            int globalDifficulty = ProgressManager.Instance.GetCurrentDifficulty();
            Debug.Log($"[LevelMap] Current global difficulty: {globalDifficulty}");

            // Load GameSession scene
            SceneManager.LoadScene("GameSession");
        }
        else
        {
            Debug.Log($"[LevelMap] Node {nodeIndex} is locked!");
        }
    }

    void LoadProgress()
    {
        // Load from ProgressManager (persistent global state)
        currentUnlockedNode = ProgressManager.Instance.GetCurrentUnlockedNode();
        Debug.Log($"[LevelMap] Current unlocked node: {currentUnlockedNode}");
    }

    public void UnlockNextNode()
    {
        if (currentUnlockedNode < totalNodes - 1)
        {
            currentUnlockedNode++;
            ProgressManager.Instance.SetCurrentUnlockedNode(currentUnlockedNode);
            
            UpdateNodeStates();
            
            Debug.Log($"[LevelMap] Unlocked node {currentUnlockedNode}");
        }
    }

    public static void CheckAndUnlockNode(SessionMetrics metrics)
    {
        // Mastery rule: If accuracy >= 0.5 (2 Bintang), unlock next node
        if (metrics.accuracy >= 0.5f)
        {
            int currentUnlocked = ProgressManager.Instance.GetCurrentUnlockedNode();
            int totalNodes = 10; // Should match the map

            if (currentUnlocked < totalNodes - 1)
            {
                currentUnlocked++;
                ProgressManager.Instance.SetCurrentUnlockedNode(currentUnlocked);

                Debug.Log($"[LevelMap] Node unlocked! New unlocked node: {currentUnlocked}");
            }
        }
    }
}