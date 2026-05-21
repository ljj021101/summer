using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LevelSelectButton : MonoBehaviour
{
    [SerializeField] private Button button;
    [SerializeField] private TextMeshProUGUI levelNameText;
    [SerializeField] private TextMeshProUGUI coinProgressText;
    [SerializeField] private GameObject completedCheckmark;

    private LevelCatalogEntry level;
    private Action<LevelCatalogEntry> clicked;

    private void Awake()
    {
        if (button == null)
        {
            button = GetComponent<Button>();
        }

        if (levelNameText == null || coinProgressText == null)
        {
            TextMeshProUGUI[] texts = GetComponentsInChildren<TextMeshProUGUI>(true);

            if (levelNameText == null && texts.Length > 0)
            {
                levelNameText = texts[0];
            }

            if (coinProgressText == null && texts.Length > 1)
            {
                coinProgressText = texts[1];
            }
        }
    }

    public void SetLevel(
        LevelCatalogEntry level,
        string coinProgress,
        bool isCompleted,
        Action<LevelCatalogEntry> clicked)
    {
        this.level = level;
        this.clicked = clicked;

        if (levelNameText != null)
        {
            levelNameText.text = level.DisplayName;
        }

        if (coinProgressText != null)
        {
            coinProgressText.text = coinProgress;
        }

        if (completedCheckmark != null)
        {
            completedCheckmark.SetActive(isCompleted);
        }

        if (button != null)
        {
            button.onClick.RemoveListener(HandleClick);
            button.onClick.AddListener(HandleClick);
        }
    }

    private void HandleClick()
    {
        clicked?.Invoke(level);
    }
}
