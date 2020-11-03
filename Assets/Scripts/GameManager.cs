using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    private const string SAVES_FOLDER = "Saves";
    private const string PLAYER_INVENTORY = "player.inv";
    private const string PLAYER_STATS = "player.stats";
    private const string SHOP_INVENTORY = "shop.inv";
    private const string GAME_FILE = "game.info";

    private static GameManager _instance;
    public GameObject Player { get; set; }
    public HubManager CurrentHubManager { get; set; }
    public Vector3 Spawnpoint { get; set; }

    private string _currentSavePath; // TODO remove useless?

    //private ItemDatabaseObject _itemObjectDatabase;
    private ItemDatabase _itemDatabase;

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
        _itemDatabase = Instantiate(Resources.Load<GameObject>("_ItemDatabase"), transform).GetComponent<ItemDatabase>();
    }

    private void Start()
    {
        //_itemObjectDatabase = Resources.Load<ItemDatabaseObject>("ItemDatabase");
    }

    public ItemObject GetItemObjectByID(int id)
    {
        return _itemDatabase.GetItem[id];
    }

    #region Save Managment

    public List<string> GetSaves()
    {
        return LoadManager.ReturnSubdirectories(Path.Combine(Application.persistentDataPath, SAVES_FOLDER));
    }

    public void CreateNewSave(string path)
    {
        string fullPath = Path.Combine(Application.persistentDataPath, SAVES_FOLDER, path);

        LoadManager.CreateFolder(Path.Combine(Application.persistentDataPath, SAVES_FOLDER), path);
        LoadManager.SaveFile(Path.Combine(fullPath, GAME_FILE), new SaveableGameState(1, true));
        LoadManager.SaveFile(Path.Combine(fullPath, PLAYER_INVENTORY), new SaveableInventory());

        // TODO player stats
        // TODO shop inventory
        LoadManager.SaveFile(Path.Combine(fullPath, SHOP_INVENTORY), new SaveableInventory(Resources.Load<NewGameInventorySO>(Path.Combine("NewGame", "ShopkeeperInventory")).itemIDs));
    }

    public void DeleteSave(string path)
    {
        LoadManager.DeleteDirectory(Path.Combine(Application.persistentDataPath, SAVES_FOLDER, path));
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

        Player.GetComponent<PlayerController>().GetPlayerInventory().Load(Path.Combine(Application.persistentDataPath, SAVES_FOLDER, _currentSavePath, "player.inv")); // TODO hardcoded

        CurrentHubManager.EnablePlayerDependantObjects(Player.transform, Player.GetComponent<PlayerController>().GetPlayerCameraTransform(), Path.Combine(Application.persistentDataPath, SAVES_FOLDER, _currentSavePath, "shop.inv"));
        UnloadScene("Menu");
    }

    IEnumerator LoadMazeAsync(string locationSceneName, MazeSettingsSO mazeSettings)
    {
        AsyncOperation locationSceneLoadingTask = SceneManager.LoadSceneAsync(locationSceneName, LoadSceneMode.Additive);

        while (!locationSceneLoadingTask.isDone) { yield return null; }

        SceneManager.SetActiveScene(SceneManager.GetSceneByName(locationSceneName));

        MazeManager.Instance.CreateMaze(mazeSettings); // TODO set spawnPoint?

        // TODO Change (mazeGen); impossible?
        //CurrentHubManager.EnablePlayerDependantObjects(Player.transform); ?

        CurrentHubManager = null;
        UnloadScene("Hub");

        Player.SetActive(true);
        // TODO hide player loading screen; add loading screen to GameManager?
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
