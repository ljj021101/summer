using UnityEngine;

public static class GameProgressStore
{
    private const string SaveKey = "summer.game.progress.v1";

    private static GameProgressSave cachedSave;

    public static GameProgressSave Save
    {
        get
        {
            if (cachedSave == null)
            {
                cachedSave = Load();
            }

            return cachedSave;
        }
    }

    public static LevelProgressSave GetLevel(string levelId)
    {
        return Save.GetOrCreateLevel(levelId);
    }

    public static bool TryGetLevel(string levelId, out LevelProgressSave level)
    {
        for (int i = 0; i < Save.levels.Count; i++)
        {
            if (Save.levels[i].levelId == levelId)
            {
                level = Save.levels[i];
                return true;
            }
        }

        level = null;
        return false;
    }

    public static void SetCoinTotal(string levelId, int coinTotal)
    {
        LevelProgressSave level = GetLevel(levelId);
        level.coinTotal = Mathf.Max(0, coinTotal);
        SaveToPlayerPrefs();
    }

    public static bool IsCoinCollected(string levelId, string coinId)
    {
        return GetLevel(levelId).HasCollectedCoin(coinId);
    }

    public static bool MarkCoinCollected(string levelId, string coinId)
    {
        LevelProgressSave level = GetLevel(levelId);
        bool changed = level.AddCollectedCoin(coinId);

        if (changed)
        {
            SaveToPlayerPrefs();
        }

        return changed;
    }

    public static void MarkLevelCompleted(string levelId)
    {
        LevelProgressSave level = GetLevel(levelId);

        if (level.isCompleted)
        {
            return;
        }

        level.isCompleted = true;
        SaveToPlayerPrefs();
    }

    public static void SaveToPlayerPrefs()
    {
        string json = JsonUtility.ToJson(Save);
        PlayerPrefs.SetString(SaveKey, json);
        PlayerPrefs.Save();
    }

    public static void ResetAllProgress()
    {
        cachedSave = new GameProgressSave();
        PlayerPrefs.DeleteKey(SaveKey);
        PlayerPrefs.Save();
    }

    private static GameProgressSave Load()
    {
        string json = PlayerPrefs.GetString(SaveKey, string.Empty);

        if (string.IsNullOrWhiteSpace(json))
        {
            return new GameProgressSave();
        }

        try
        {
            GameProgressSave save = JsonUtility.FromJson<GameProgressSave>(json);
            return save ?? new GameProgressSave();
        }
        catch
        {
            return new GameProgressSave();
        }
    }
}
