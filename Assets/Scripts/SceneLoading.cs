using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoading : MonoBehaviour
{
    private string _loadedScene = "";

    private void Start()
    {
        //_loadedScene = _loadableScenes[0];
        //AsyncOperation sceneUnload = SceneManager.UnloadSceneAsync(_loadedScene);
        //sceneUnload.completed += (async) => SceneManager.LoadScene(_loadableScenes[0], LoadSceneMode.Additive);
    }

    public void UnloadScene()
    {
        if (_loadedScene != null)
        {
            SceneManager.UnloadSceneAsync(_loadedScene);
            _loadedScene = null;
        }
    }

    public void LoadScene(string sceneName)
    {
        if (sceneName == _loadedScene)
            return;

        UnloadScene();

        SceneManager.LoadScene(sceneName, LoadSceneMode.Additive);
        _loadedScene = sceneName;
    }
}
