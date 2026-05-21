using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelSelectManager : MonoBehaviour
{
    [SerializeField] private LevelCatalog levelCatalog;
    [SerializeField] private Transform buttonContainer;
    [SerializeField] private LevelSelectButton buttonTemplate;
    [SerializeField] private string unknownCoinTotalText = "?";

    private void Start()
    {
        BuildLevelButtons();
    }

    public void BuildLevelButtons()
    {
        if (levelCatalog == null || buttonContainer == null || buttonTemplate == null)
        {
            return;
        }

        buttonTemplate.gameObject.SetActive(false);

        ClearGeneratedButtons();

        foreach (LevelCatalogEntry level in levelCatalog.Levels)
        {
            LevelSelectButton button = Instantiate(buttonTemplate, buttonContainer);
            button.gameObject.SetActive(true);
            button.SetLevel(level, GetProgressText(level), IsLevelCompleted(level), LoadLevel);
        }
    }

    private void ClearGeneratedButtons()
    {
        for (int i = buttonContainer.childCount - 1; i >= 0; i--)
        {
            Transform child = buttonContainer.GetChild(i);

            if (child == buttonTemplate.transform)
            {
                continue;
            }

            Destroy(child.gameObject);
        }
    }

    private string GetProgressText(LevelCatalogEntry level)
    {
        int collectedCount = 0;
        int coinTotal = 0;

        if (GameProgressStore.TryGetLevel(level.LevelId, out LevelProgressSave progress))
        {
            collectedCount = progress.CollectedCoinCount;
            coinTotal = progress.coinTotal;
        }

        string totalText = coinTotal > 0 ? coinTotal.ToString() : unknownCoinTotalText;
        return $"{collectedCount}/{totalText}";
    }

    private bool IsLevelCompleted(LevelCatalogEntry level)
    {
        return GameProgressStore.TryGetLevel(level.LevelId, out LevelProgressSave progress) &&
            progress.isCompleted;
    }

    private void LoadLevel(LevelCatalogEntry level)
    {
        if (string.IsNullOrWhiteSpace(level.SceneName))
        {
            return;
        }

        SceneManager.LoadScene(level.SceneName);
    }
}
