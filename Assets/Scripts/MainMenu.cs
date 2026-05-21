using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Главное меню. Вешается на любой объект в сцене MainMenu.
/// </summary>
public class MainMenu : MonoBehaviour
{
    [Header("Имя игровой сцены")]
    public string gameSceneName = "GameScene";

    public void StartGame()
    {
        // Заблокировать курсор для игры
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        SceneManager.LoadScene(gameSceneName);
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
