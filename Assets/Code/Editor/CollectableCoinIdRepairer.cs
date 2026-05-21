using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

[InitializeOnLoad]
public static class CollectableCoinIdRepairer
{
    private static bool repairQueued;

    static CollectableCoinIdRepairer()
    {
        QueueRepair();
        EditorApplication.hierarchyChanged += QueueRepair;
        EditorSceneManager.sceneOpened += (_, _) => QueueRepair();
        ObjectFactory.componentWasAdded += HandleComponentAdded;
    }

    private static void HandleComponentAdded(Component component)
    {
        if (component is CollectableCoin)
        {
            QueueRepair();
        }
    }

    [MenuItem("Tools/Summer/Repair Coin IDs")]
    public static void RepairOpenScenes()
    {
        bool changed = false;
        HashSet<string> usedIds = new HashSet<string>();

        for (int i = 0; i < SceneManager.sceneCount; i++)
        {
            Scene scene = SceneManager.GetSceneAt(i);

            if (!scene.isLoaded)
            {
                continue;
            }

            foreach (GameObject root in scene.GetRootGameObjects())
            {
                CollectableCoin[] coins = root.GetComponentsInChildren<CollectableCoin>(true);

                for (int coinIndex = 0; coinIndex < coins.Length; coinIndex++)
                {
                    CollectableCoin coin = coins[coinIndex];
                    string coinId = coin.CoinId;

                    if (!string.IsNullOrWhiteSpace(coinId) && usedIds.Add(coinId))
                    {
                        continue;
                    }

                    Undo.RecordObject(coin, "Repair Coin ID");
                    coin.RegenerateId();
                    usedIds.Add(coin.CoinId);
                    EditorUtility.SetDirty(coin);
                    EditorSceneManager.MarkSceneDirty(scene);
                    changed = true;
                }
            }
        }

        if (changed)
        {
            Debug.Log("Repaired duplicate or missing collectable coin IDs.");
        }
    }

    private static void QueueRepair()
    {
        if (repairQueued)
        {
            return;
        }

        repairQueued = true;
        EditorApplication.delayCall += () =>
        {
            repairQueued = false;
            RepairOpenScenes();
        };
    }
}
