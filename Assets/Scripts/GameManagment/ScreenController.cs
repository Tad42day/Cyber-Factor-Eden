using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class ScreenController : Singleton<ScreenController> {

    public enum Scenes
    {
        SplashScreen,
        MenuInicial,
        MenuOpcoes,
        FaseGalpao,
        FaseEscritorio,
        FaseFinal,
        GameOver,
        Creditos
    }

    [System.Serializable]
    public class SceneSettings
    {
        public int currentSceneIndex;
        public Scenes currentScene;
    }

    public SceneSettings sceneSettings;

    void Awake()
    {
        sceneSettings.currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
    }

    void Update()
    {
        switch (sceneSettings.currentSceneIndex)
        {
            case 0:
                sceneSettings.currentScene = Scenes.SplashScreen;
                break;
            case 1:
                sceneSettings.currentScene = Scenes.MenuInicial;
                break;
            case 2:
                sceneSettings.currentScene = Scenes.MenuOpcoes;
                break;
            case 3:
                sceneSettings.currentScene = Scenes.FaseGalpao;
                break;
            case 4:
                sceneSettings.currentScene = Scenes.FaseEscritorio;
                break;
            case 5:
                sceneSettings.currentScene = Scenes.FaseFinal;
                break;
            case 6:
                sceneSettings.currentScene = Scenes.GameOver;
                break;
            case 7:
                sceneSettings.currentScene = Scenes.Creditos;
                break;
        }
    }

    public void LoadScene(string scene)
    {
        SceneManager.LoadScene(scene);
        sceneSettings.currentSceneIndex = SceneManager.GetSceneByName(scene).buildIndex;
    }

    public void Sair()
    {
        Application.Quit();
    }
}
