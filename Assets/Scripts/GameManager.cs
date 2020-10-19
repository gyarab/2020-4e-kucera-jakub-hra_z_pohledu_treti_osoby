using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    private static GameManager _instance;
    // TODO change to properties? or move + 1
    public float pickUpRange;
    public ItemDatabaseObject itemObjectDatabase; // TODO load in start
    public GameObject Player { get; set; }
    public HubManager CurrentHubManager { get; set; }
    public Pathfinding Pathfinding { get; set; }
    public MazeGenerator MazeGen { get; set; }
    public Vector3 Spawnpoint { get; set; }

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
                    // Create a new GameObject to attach the singleton to.
                    GameObject gameObject = new GameObject("_GameManager");
                    _instance = gameObject.AddComponent<GameManager>();
                }
            }

            return _instance;
        }
    }

    private void Start()
    {
        DontDestroyOnLoad(gameObject);
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
        LoadManager.Instance.CreateSave(Path.Combine(Application.persistentDataPath, _savePath, _path, "hello.save")); // TODO player stats, location and inventory; items for each chunk; rework to json?
    }

    public void DeleteSave(string _path)
    {
        LoadManager.Instance.DeleteDirectory(Path.Combine(Application.persistentDataPath, _savePath, _path));
    }

    public void LoadGame(string _path)
    {
        // TODO do something w path
        StartCoroutine(LoadHubAsync("Hub", "Player"));
    }

    public void LoadMaze(MazeSettingsSO mazeSettings)
    {
        // TODO loading screen in player canvas
        Player.SetActive(false);
        StartCoroutine(LoadMazeAsync("Maze", mazeSettings));
    }

    public void UnloadScene(string _name)
    {
        StartCoroutine(UnloadSceneAsync(_name));
    }

    #endregion

    // Loads game scene
    #region Enumerators

    IEnumerator LoadHubAsync(string locationSceneName, string playerSceneName)
    {
        AsyncOperation locationSceneLoadingTask = SceneManager.LoadSceneAsync(locationSceneName, LoadSceneMode.Additive);
        locationSceneLoadingTask.allowSceneActivation = false;

        AsyncOperation playerSceneLoadingTask = SceneManager.LoadSceneAsync(playerSceneName, LoadSceneMode.Additive);
        playerSceneLoadingTask.allowSceneActivation = false;

        while (locationSceneLoadingTask.progress < 0.9f && playerSceneLoadingTask.progress < 0.9f) { yield return null; }

        locationSceneLoadingTask.allowSceneActivation = true;
        while (!locationSceneLoadingTask.isDone) { yield return null; }

        playerSceneLoadingTask.allowSceneActivation = true;
        while (!playerSceneLoadingTask.isDone) { yield return null; }

        SceneManager.SetActiveScene(SceneManager.GetSceneByName(locationSceneName));

        while (Player == null || CurrentHubManager == null) { yield return null; }

        CurrentHubManager.EnablePlayerDependantObjects(Player.transform);
        UnloadScene("Menu");
    }

    IEnumerator LoadMazeAsync(string locationSceneName, MazeSettingsSO mazeSettings)
    {
        AsyncOperation locationSceneLoadingTask = SceneManager.LoadSceneAsync(locationSceneName, LoadSceneMode.Additive);

        while (!locationSceneLoadingTask.isDone) { yield return null; }

        SceneManager.SetActiveScene(SceneManager.GetSceneByName(locationSceneName));

        while (MazeGen == null) { yield return null; }

        MazeGen.GenerateMaze(mazeSettings);

        CurrentHubManager.EnablePlayerDependantObjects(Player.transform); // TODO Change
        CurrentHubManager = null;
        UnloadScene("Hub");

        Player.SetActive(true);
        // TODO hide player loading screen
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
