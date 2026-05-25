using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LevelSelectManager : MonoBehaviour
{
    [SerializeField] private LevelCatalog levelCatalog;
    [SerializeField] private Transform buttonContainer;
    [SerializeField] private LevelSelectButton buttonTemplate;
    [SerializeField] private Button modifiedModeButton;
    [SerializeField] private Image modifiedModeImage;
    [SerializeField] private Sprite normalModeSprite;
    [SerializeField] private Sprite normalModeHoveredSprite;
    [SerializeField] private Sprite modifiedModeSprite;
    [SerializeField] private Sprite modifiedModeHoveredSprite;
    [SerializeField] private Color normalTitleColor = Color.white;
    [SerializeField] private Color modifiedTitleColor = new Color(1f, 0.15f, 0.12f, 1f);
    [SerializeField] private string unknownCoinTotalText = "?";
    [SerializeField] private string lockedProgressText = "--";

    private bool useModifiedLevels;
    private bool isHoveringModifiedModeButton;

    private void Start()
    {
        if (modifiedModeButton != null)
        {
            modifiedModeButton.onClick.AddListener(ToggleModifiedMode);
        }

        BuildLevelButtons();
    }

    private void Update()
    {
        UpdateModifiedModeButtonHover();
    }

    private void OnDestroy()
    {
        if (modifiedModeButton != null)
        {
            modifiedModeButton.onClick.RemoveListener(ToggleModifiedMode);
        }
    }

    private void ToggleModifiedMode()
    {
        useModifiedLevels = !useModifiedLevels;
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

        RefreshModifiedModeButton();

        foreach (LevelCatalogEntry level in levelCatalog.Levels)
        {
            bool isUnlocked = IsLevelUnlocked(level, useModifiedLevels);
            bool canLoadLevel = isUnlocked && HasScene(level, useModifiedLevels);
            LevelSelectButton button = Instantiate(buttonTemplate, buttonContainer);
            button.gameObject.SetActive(true);
            button.SetLevel(
                level,
                level.GetDisplayName(useModifiedLevels),
                isUnlocked ? GetProgressText(level, useModifiedLevels) : lockedProgressText,
                GetTitleColor(),
                isUnlocked && IsLevelCompleted(level, useModifiedLevels),
                canLoadLevel,
                LoadLevel);
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

    private string GetProgressText(LevelCatalogEntry level, bool useModifiedLevel)
    {
        int collectedCount = 0;
        int coinTotal = 0;
        string levelId = level.GetLevelId(useModifiedLevel);

        if (GameProgressStore.TryGetLevel(levelId, out LevelProgressSave progress))
        {
            collectedCount = progress.CollectedCoinCount;
            coinTotal = progress.coinTotal;
        }

        string totalText = coinTotal > 0 ? coinTotal.ToString() : unknownCoinTotalText;
        return $"{collectedCount}/{totalText}";
    }

    private bool IsLevelCompleted(LevelCatalogEntry level, bool useModifiedLevel)
    {
        return GameProgressStore.TryGetLevel(level.GetLevelId(useModifiedLevel), out LevelProgressSave progress) &&
            progress.isCompleted;
    }

    private bool IsLevelUnlocked(LevelCatalogEntry level, bool useModifiedLevel)
    {
        return !useModifiedLevel || IsLevelCompleted(level, false);
    }

    private bool HasScene(LevelCatalogEntry level, bool useModifiedLevel)
    {
        return !string.IsNullOrWhiteSpace(level.GetSceneName(useModifiedLevel));
    }

    private bool IsModifiedModeActive()
    {
        return useModifiedLevels;
    }

    private Color GetTitleColor()
    {
        return useModifiedLevels ? modifiedTitleColor : normalTitleColor;
    }

    private void RefreshModifiedModeButton()
    {
        if (modifiedModeImage != null)
        {
            modifiedModeImage.sprite = GetModifiedModeSprite();
        }
    }

    private Sprite GetModifiedModeSprite()
    {
        if (useModifiedLevels)
        {
            return isHoveringModifiedModeButton && modifiedModeHoveredSprite != null ?
                modifiedModeHoveredSprite :
                modifiedModeSprite;
        }

        return isHoveringModifiedModeButton && normalModeHoveredSprite != null ?
            normalModeHoveredSprite :
            normalModeSprite;
    }

    private void UpdateModifiedModeButtonHover()
    {
        if (modifiedModeButton == null || modifiedModeImage == null || Mouse.current == null)
        {
            return;
        }

        RectTransform rectTransform = modifiedModeButton.transform as RectTransform;
        if (rectTransform == null)
        {
            return;
        }

        Canvas canvas = modifiedModeButton.GetComponentInParent<Canvas>();
        Camera uiCamera = canvas != null && canvas.renderMode != RenderMode.ScreenSpaceOverlay ? canvas.worldCamera : null;
        bool isHovering = RectTransformUtility.RectangleContainsScreenPoint(
            rectTransform,
            Mouse.current.position.ReadValue(),
            uiCamera);

        if (isHoveringModifiedModeButton == isHovering)
        {
            return;
        }

        isHoveringModifiedModeButton = isHovering;
        RefreshModifiedModeButton();
    }

    private void LoadLevel(LevelCatalogEntry level)
    {
        string sceneName = level.GetSceneName(IsModifiedModeActive());

        if (string.IsNullOrWhiteSpace(sceneName))
        {
            return;
        }

        SceneManager.LoadScene(sceneName);
    }
}
