using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class InGameMenuManager : MonoBehaviour
{
    // Référence au panneau UI du menu pause
    [Header("Références UI")]
    public GameObject pauseMenuPanel;

    // Référence au manager de votre cube pour désactiver l'input
    [Header("Références Jeu")]
    public RubiksManager rubiksManager;
    public MonoBehaviour rubiksInput; // Référence au script RubiksInput (CubeRotateSystemeBlabla.cs)
    public MonoBehaviour cameraOrbit; // Référence à votre script de caméra (ex: CameraOrbit.cs)

    private bool isPaused = false;
    private const string MAIN_MENU_SCENE_NAME = "MainMenu"; // Vérifiez que ce nom correspond

    void Start()
    {
        // S'assurer que le jeu commence non-pausé et que le panneau est caché
        SetPaused(false);
    }

    // Fonction principale pour basculer entre pause et jeu
    public void TogglePauseMenu()
    {
        // On inverse l'état actuel de la pause
        SetPaused(!isPaused);
    }

    void Update()
    {
        // Détection de l'appui sur ECHAP
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            TogglePauseMenu();
        }
    }

    public void SetPaused(bool pauseState)
    {
        isPaused = pauseState;

        if (isPaused)
        {
            // --- Mode Pause (TimeScale = 0) ---
            Time.timeScale = 0f;
            pauseMenuPanel.SetActive(true);

            // Désactiver les inputs du joueur
            if (rubiksManager != null) rubiksManager.enabled = false;
            if (rubiksInput != null) rubiksInput.enabled = false;
            if (cameraOrbit != null) cameraOrbit.enabled = false;

            // Afficher le curseur
            //Cursor.lockState = CursorLockMode.None;
            //Cursor.visible = true;
        }
        else
        {
            // --- Mode Jeu (TimeScale = 1) ---
            Time.timeScale = 1f;
            pauseMenuPanel.SetActive(false);

            // Réactiver les inputs du joueur
            if (rubiksManager != null) rubiksManager.enabled = true;
            if (rubiksInput != null) rubiksInput.enabled = true;
            if (cameraOrbit != null) cameraOrbit.enabled = true;

            // Masquer le curseur
            //Cursor.lockState = CursorLockMode.Locked; // Si vous l'utilisez pour la caméra
            //Cursor.visible = false;
        }
    }

    // --- Fonctions appelées par les boutons UI ---

    public void OnClick_Resume()
    {
        SetPaused(false);
    }

    public void OnClick_QuitToMainMenu()
    {
        // CRUCIAL : Réinitialiser Time.timeScale avant de changer de scène
        Time.timeScale = 1f;
        SceneManager.LoadScene(MAIN_MENU_SCENE_NAME);
    }
}