using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance { get; private set; }

    [SerializeField] private string levelId;
    [SerializeField] private string displayName;
    [SerializeField] private int coinTotal;

    public string LevelId => levelId;
    public string DisplayName => string.IsNullOrWhiteSpace(displayName) ? levelId : displayName;
    public int CoinTotal => coinTotal;
    public LevelProgressSave Progress => GameProgressStore.GetLevel(levelId);

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
            return;
        }

        Instance = this;
        EnsureLevelId();
        RefreshCoinTotalFromScene();
        GameProgressStore.SetCoinTotal(levelId, coinTotal);
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    public bool IsCoinCollected(string coinId)
    {
        return GameProgressStore.IsCoinCollected(levelId, coinId);
    }

    public bool MarkCoinCollected(string coinId)
    {
        return GameProgressStore.MarkCoinCollected(levelId, coinId);
    }

    public void MarkCompleted()
    {
        GameProgressStore.MarkLevelCompleted(levelId);
    }

    public void RefreshCoinTotal(int total)
    {
        coinTotal = Mathf.Max(0, total);
        GameProgressStore.SetCoinTotal(levelId, coinTotal);
    }

    public void RefreshCoinTotalFromScene()
    {
        CollectableCoin[] coins = FindObjectsByType<CollectableCoin>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        coinTotal = coins.Length;
    }

    private void EnsureLevelId()
    {
        if (!string.IsNullOrWhiteSpace(levelId))
        {
            return;
        }

        levelId = SceneManager.GetActiveScene().name;
    }
}
