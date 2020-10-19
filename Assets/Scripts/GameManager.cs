using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour // Should not be singleton
{
    private static GameManager _instance;
    // TODO change to properties? or move
    public float pickUpRange;
    public ItemDatabaseObject itemObjectDatabase;
    public GameObject player; // TODO
    public bool loadingScene;

    public Pathfinding Pathfinding { get; set; }

    [Header("Save Path")]
    [SerializeField]
    private string _savePath;

    public string CurrentSavePath { get; set; }

    public static GameManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = (GameManager)FindObjectOfType(typeof(GameManager));

                if (_instance == null)
                {
                    // Need to create a new GameObject to attach the singleton to.
                    GameObject gameObject = new GameObject("_GameManager");
                    _instance = gameObject.AddComponent<GameManager>();
                    DontDestroyOnLoad(gameObject);
                }
            }

            return _instance;
        }
    }

    public ItemObject GetItemObjectByID(int _id)
    {
        return itemObjectDatabase.getItem[_id];
    }

    // TODO prob change to normal class - singleton is useless
    #region Save Managment

    public List<string> GetSaves()
    {
        return LoadManager.Instance.ReturnSubdirectories(Path.Combine(Application.persistentDataPath, _savePath));
    }

    public void CreateNewSave(string _path)
    {
        LoadManager.Instance.CreateSave(Path.Combine(Application.persistentDataPath, _savePath, _path, "hello.json")); // TODO player stats, location and inventory; items for each chunk
    }

    public void DeleteSave(string _path)
    {
        LoadManager.Instance.DeleteDirectory(Path.Combine(Application.persistentDataPath, _savePath, _path));
    }

    public void LoadGame(string _path)
    {
        // TODO 
        Debug.Log("Loading");
        loadingScene = true;
        StartCoroutine(LoadScenesAsync("Chunk1", "Player")); // TODO
    }

    public void UnloadScene(string _name)
    {
        StartCoroutine(UnloadSceneAsync(_name));
    }

    #endregion

    // Loads game scene
    #region Enumerators

    IEnumerator LoadScenesAsync(string locationSceneName, string playerSceneName)
    {
        List<AsyncOperation> sceneLoads = new List<AsyncOperation>();

        AsyncOperation locationSceneLoadingTask = SceneManager.LoadSceneAsync(locationSceneName, LoadSceneMode.Additive);
        locationSceneLoadingTask.allowSceneActivation = false;

        AsyncOperation playerSceneLoadingTask = SceneManager.LoadSceneAsync(playerSceneName, LoadSceneMode.Additive);
        playerSceneLoadingTask.allowSceneActivation = false;

        while (locationSceneLoadingTask.progress < 0.9f && playerSceneLoadingTask.progress < 0.9f) { yield return null; }

        locationSceneLoadingTask.allowSceneActivation = true;
        while (!locationSceneLoadingTask.isDone) { yield return null; }

        // TODO Spawn items and enemies

        playerSceneLoadingTask.allowSceneActivation = true;
        while (!playerSceneLoadingTask.isDone) { yield return null; }

        SceneManager.SetActiveScene(SceneManager.GetSceneByName(locationSceneName));
        loadingScene = false; // TODO move; idk

        Debug.Log("Loading Done");
    }

    IEnumerator UnloadSceneAsync(string sceneName)
    {
        if (!sceneName.Equals(SceneManager.GetActiveScene().name))
        {
            AsyncOperation sceneUnloadingTask = SceneManager.UnloadSceneAsync(sceneName); // TODO move somewhere else
            while (!sceneUnloadingTask.isDone) { yield return null; }
        }
    }

    #endregion
}
