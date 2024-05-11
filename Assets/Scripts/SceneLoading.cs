using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoading : MonoBehaviour
{
    public List<SceneAsset> ScenePool { get => _scenePool; }

    [SerializeField] private List<SceneAsset> _scenePool;
    private string _loadedScene = "";

    private void Start()
    {
        //_loadedScene = _loadableScenes[0];
        //AsyncOperation sceneUnload = SceneManager.UnloadSceneAsync(_loadedScene);
        //sceneUnload.completed += (async) => SceneManager.LoadScene(_loadableScenes[0], LoadSceneMode.Additive);
    }

    public void UnloadScene(string sceneName)
    {
        if (string.IsNullOrEmpty(_loadedScene))
            return;

        if (_loadedScene == sceneName)
        {
            SceneManager.UnloadSceneAsync(sceneName);
            _loadedScene = "";
        }
    }

    public void LoadScene(string sceneName)
    {
        if (_loadedScene == sceneName)
            return;

        SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
        _loadedScene = sceneName;
    }
}
