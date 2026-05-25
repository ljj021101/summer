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
    [SerializeField] private Graphic[] tintTargets;
    [SerializeField] private Color unlockedTint = Color.white;
    [SerializeField] private Color lockedTint = new Color(0.35f, 0.35f, 0.35f, 1f);

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
        string displayName,
        string coinProgress,
        Color titleColor,
        bool isCompleted,
        bool isInteractable,
        Action<LevelCatalogEntry> clicked)
    {
        this.level = level;
        this.clicked = clicked;

        if (levelNameText != null)
        {
            levelNameText.text = displayName;
            levelNameText.color = titleColor;
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
            button.interactable = isInteractable;
            button.onClick.RemoveListener(HandleClick);
            button.onClick.AddListener(HandleClick);
        }

        ApplyTint(isInteractable);
    }

    private void HandleClick()
    {
        clicked?.Invoke(level);
    }

    private void ApplyTint(bool isInteractable)
    {
        Color tint = isInteractable ? unlockedTint : lockedTint;

        if (tintTargets != null && tintTargets.Length > 0)
        {
            for (int i = 0; i < tintTargets.Length; i++)
            {
                if (tintTargets[i] != null)
                {
                    tintTargets[i].color = tint;
                }
            }

            return;
        }

        if (button != null && button.targetGraphic != null)
        {
            button.targetGraphic.color = tint;
        }
    }
}
