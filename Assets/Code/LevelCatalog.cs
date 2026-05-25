using System;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

[CreateAssetMenu(fileName = "LevelCatalog", menuName = "Summer/Level Catalog")]
public class LevelCatalog : ScriptableObject
{
    [SerializeField] private List<LevelCatalogEntry> levels = new List<LevelCatalogEntry>();

    public IReadOnlyList<LevelCatalogEntry> Levels => levels;

    public bool TryGetLevel(string levelId, out LevelCatalogEntry level)
    {
        for (int i = 0; i < levels.Count; i++)
        {
            if (levels[i].LevelId == levelId)
            {
                level = levels[i];
                return true;
            }
        }

        level = null;
        return false;
    }

    private void OnValidate()
    {
        for (int i = 0; i < levels.Count; i++)
        {
            levels[i].Validate();
        }
    }
}

[Serializable]
public class LevelCatalogEntry
{
    [SerializeField] private string levelId;
    [SerializeField] private string displayName;
    [SerializeField] private string modifiedLevelId;
    [SerializeField] private string modifiedDisplayName;

#if UNITY_EDITOR
    [SerializeField] private SceneAsset scene;
    [SerializeField] private SceneAsset modifiedScene;
#endif

    [SerializeField, HideInInspector] private string sceneName;
    [SerializeField, HideInInspector] private string scenePath;
    [SerializeField, HideInInspector] private string modifiedSceneName;
    [SerializeField, HideInInspector] private string modifiedScenePath;

    public string LevelId => levelId;
    public string DisplayName => string.IsNullOrWhiteSpace(displayName) ? levelId : displayName;
    public string SceneName => sceneName;
    public string ScenePath => scenePath;
    public string ModifiedLevelId
    {
        get
        {
            if (!string.IsNullOrWhiteSpace(modifiedLevelId))
            {
                return modifiedLevelId;
            }

            return !string.IsNullOrWhiteSpace(modifiedSceneName) ? modifiedSceneName : $"{levelId}_modified";
        }
    }
    public string ModifiedDisplayName => string.IsNullOrWhiteSpace(modifiedDisplayName) ? DisplayName : modifiedDisplayName;
    public string ModifiedSceneName => modifiedSceneName;
    public string ModifiedScenePath => modifiedScenePath;
    public bool HasModifiedLevel => !string.IsNullOrWhiteSpace(ModifiedSceneName);

    public string GetLevelId(bool useModifiedLevel)
    {
        return useModifiedLevel ? ModifiedLevelId : LevelId;
    }

    public string GetDisplayName(bool useModifiedLevel)
    {
        return useModifiedLevel ? ModifiedDisplayName : DisplayName;
    }

    public string GetSceneName(bool useModifiedLevel)
    {
        return useModifiedLevel ? ModifiedSceneName : SceneName;
    }

    public void Validate()
    {
#if UNITY_EDITOR
        if (scene != null)
        {
            scenePath = AssetDatabase.GetAssetPath(scene);
            sceneName = System.IO.Path.GetFileNameWithoutExtension(scenePath);
        }
        else
        {
            scenePath = string.Empty;
            sceneName = string.Empty;
        }

        if (modifiedScene != null)
        {
            modifiedScenePath = AssetDatabase.GetAssetPath(modifiedScene);
            modifiedSceneName = System.IO.Path.GetFileNameWithoutExtension(modifiedScenePath);
        }
        else
        {
            modifiedScenePath = string.Empty;
            modifiedSceneName = string.Empty;
        }
#endif
    }
}
