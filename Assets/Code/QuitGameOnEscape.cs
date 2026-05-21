using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class QuitGameOnEscape : MonoBehaviour
{
    [SerializeField] private string levelSelectSceneName = "Level Select";

    private void Update()
    {
        if (Keyboard.current == null || !Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            return;
        }

        HandleEscape();
    }

    private void HandleEscape()
    {
        if (LevelManager.Instance != null)
        {
            ReturnToLevelSelect();
            return;
        }

        QuitGame();
    }

    private void ReturnToLevelSelect()
    {
        Time.timeScale = 1f;
        LevelManager.Instance.RefreshCoinTotalFromScene();
        GameProgressStore.SaveToPlayerPrefs();

        if (!string.IsNullOrWhiteSpace(levelSelectSceneName))
        {
            SceneManager.LoadScene(levelSelectSceneName);
        }
    }

    private void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
