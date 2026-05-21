using System;
using System.Collections.Generic;

[Serializable]
public class GameProgressSave
{
    public List<LevelProgressSave> levels = new List<LevelProgressSave>();

    public LevelProgressSave GetOrCreateLevel(string levelId)
    {
        for (int i = 0; i < levels.Count; i++)
        {
            if (levels[i].levelId == levelId)
            {
                return levels[i];
            }
        }

        LevelProgressSave level = new LevelProgressSave(levelId);
        levels.Add(level);
        return level;
    }
}

[Serializable]
public class LevelProgressSave
{
    public string levelId;
    public bool isCompleted;
    public int coinTotal;
    public List<string> collectedCoinIds = new List<string>();

    public LevelProgressSave(string levelId)
    {
        this.levelId = levelId;
    }

    public int CollectedCoinCount => collectedCoinIds.Count;

    public bool HasCollectedCoin(string coinId)
    {
        return collectedCoinIds.Contains(coinId);
    }

    public bool AddCollectedCoin(string coinId)
    {
        if (string.IsNullOrWhiteSpace(coinId) || collectedCoinIds.Contains(coinId))
        {
            return false;
        }

        collectedCoinIds.Add(coinId);
        return true;
    }
}
