using UnityEngine;
using UnityEngine.SceneManagement; // Essentiel pour changer de scène

public class MainMenuManager : MonoBehaviour
{
    // Nom ou index de la scène de jeu à charger
    public string gameSceneName = "GameScene";

    public void OnClick_PlayGame()
    {
        // Charge la scène du jeu. Ici, nous utilisons l'index 1 (GameScene)
        // en supposant que MainMenu est l'index 0.
        SceneManager.LoadScene(gameSceneName);
        // OU SceneManager.LoadScene(1);
    }

    public void OnClick_QuitGame()
    {
        Debug.Log("Quitter l'application");
        Application.Quit();

        // Ceci est uniquement pour quitter dans l'éditeur Unity :
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}