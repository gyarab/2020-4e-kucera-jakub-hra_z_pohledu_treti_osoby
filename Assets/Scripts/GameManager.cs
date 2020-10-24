using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    private const string SAVESFOLDER = "Saves";
    private const string PLAYERFILE = "player.stats";
    private const string GAMEFILE = "game.info";

    private static GameManager _instance;
    public GameObject Player { get; set; }
    public HubManager CurrentHubManager { get; set; }
    public Pathfinding Pathfinding { get; set; }
    public MazeGenerator MazeGen { get; set; }
    public Vector3 Spawnpoint { get; set; }

    private string _currentSavePath; // TODO remove useless?

    private ItemDatabaseObject _itemObjectDatabase;

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

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        _itemObjectDatabase = Resources.Load<ItemDatabaseObject>("ItemDatabase");
    }

    public ItemObject GetItemObjectByID(int id)
    {
        return _itemObjectDatabase.getItem[id];
    }

    #region Save Managment

    public List<string> GetSaves()
    {
        return LoadManager.ReturnSubdirectories(Path.Combine(Application.persistentDataPath, SAVESFOLDER));
    }

    public void CreateNewSave(string path)
    {
        string fullPath = Path.Combine(Application.persistentDataPath, SAVESFOLDER, path);
        LoadManager.CreateFolder(fullPath);
        LoadManager.SaveFile(Path.Combine(fullPath, GAMEFILE), new SaveableGameState(1, true));

        // player stats  PLAYERFILE
        // shop inventory
    }

    public void DeleteSave(string path)
    {
        LoadManager.DeleteDirectory(Path.Combine(Application.persistentDataPath, SAVESFOLDER, path));
    }

    public void LoadGame(string path)
    {
        _currentSavePath = path;
        // TODO do something w path; load inventory
        StartCoroutine(LoadHubAsync("Hub", "Player"));
    }

    public void LoadMaze(MazeSettingsSO mazeSettings)
    {
        // TODO loading screen in player canvas
        Player.SetActive(false);
        StartCoroutine(LoadMazeAsync("Maze", mazeSettings));
    }

    public void UnloadScene(string name)
    {
        StartCoroutine(UnloadSceneAsync(name));
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

        Player.GetComponent<PlayerController>().GetPlayerInventory().Load(Path.Combine(Application.persistentDataPath, SAVESFOLDER, _currentSavePath, "player.inv")); // TODO hardcoded

        CurrentHubManager.EnablePlayerDependantObjects(Player.transform, Path.Combine(Application.persistentDataPath, SAVESFOLDER, _currentSavePath, "shop.inv"));
        UnloadScene("Menu");
    }

    IEnumerator LoadMazeAsync(string locationSceneName, MazeSettingsSO mazeSettings)
    {
        AsyncOperation locationSceneLoadingTask = SceneManager.LoadSceneAsync(locationSceneName, LoadSceneMode.Additive);

        while (!locationSceneLoadingTask.isDone) { yield return null; }

        SceneManager.SetActiveScene(SceneManager.GetSceneByName(locationSceneName));

        while (MazeGen == null) { yield return null; }

        MazeGen.GenerateMaze(mazeSettings);

        // TODO Change (mazeGen); impossible?
        //CurrentHubManager.EnablePlayerDependantObjects(Player.transform);

        CurrentHubManager = null;
        UnloadScene("Hub");

        Player.SetActive(true);
        // TODO hide player loading screen
    }

    IEnumerator UnloadSceneAsync(string sceneName)
    {
        if (!sceneName.Equals(SceneManager.GetActiveScene().name))
        {
            AsyncOperation sceneUnloadingTask = SceneManager.UnloadSceneAsync(sceneName);
            while (!sceneUnloadingTask.isDone) { yield return null; }
        }
    }

    #endregion
}
