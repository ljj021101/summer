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

#if UNITY_EDITOR
    [SerializeField] private SceneAsset scene;
#endif

    [SerializeField, HideInInspector] private string sceneName;
    [SerializeField, HideInInspector] private string scenePath;

    public string LevelId => levelId;
    public string DisplayName => string.IsNullOrWhiteSpace(displayName) ? levelId : displayName;
    public string SceneName => sceneName;
    public string ScenePath => scenePath;

    public void Validate()
    {
#if UNITY_EDITOR
        if (scene != null)
        {
            scenePath = AssetDatabase.GetAssetPath(scene);
            sceneName = System.IO.Path.GetFileNameWithoutExtension(scenePath);
        }
#endif
    }
}
