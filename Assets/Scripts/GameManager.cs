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
    private const string SETTINGS_FILE = "settings.conf";

    private const string MENU_SCENE_NAME = "Menu";
    private const string HUB_SCENE_NAME = "Hub";
    private const string PLAYER_SCENE_NAME = "Player";
    private const string MAP_SCENE_NAME = "Maze";

    private static GameManager _instance;
    public GameObject Player { get; set; }
    public InputManager InputManager { get; set; }
    public HubManager CurrentHubManager { get; set; }
    public Vector3 Spawnpoint { get; set; }
    public MazeManager CurrentMazeManager { get; set; }
    public QuestUI QuestUI { get; set; }

    private string _currentSavePath;
    private bool _loading;

    private ItemDatabase _itemDatabase;
    private LoadingScreen _loadingScreen;

    // Vrátí jedinou instanci třídy GameManager
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

    // Načte ze složky se zdroji databázi předmětů, načítací obrazovku a grafické rozhraní pro zadávání úkolů hráči
    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
        _itemDatabase = Instantiate(Resources.Load<GameObject>("_ItemDatabase"), transform).GetComponent<ItemDatabase>();
        _loadingScreen = Instantiate(Resources.Load<GameObject>("LoadingScreen"), transform).GetComponent<LoadingScreen>();
        QuestUI = Instantiate(Resources.Load<GameObject>("QuestCanvas"), transform).GetComponent<QuestUI>();
    }

    // Najde předmět v databáze předmětů podle id
    public ItemObject GetItemObjectByID(int id)
    {
        return _itemDatabase.GetItem[id];
    }

    #region Save Managment

    // Najde uložené hry
    public List<string> GetSaves()
    {
        return LoadManager.ReturnSubdirectories(Path.Combine(Application.persistentDataPath, SAVES_FOLDER));
    }

    // Vytvoří složku pro uložení nové hry a vytvoří v ní nutné soubory
    public void CreateNewSave(string path)
    {
        string fullPath = Path.Combine(Application.persistentDataPath, SAVES_FOLDER, path);

        LoadManager.CreateFolder(Path.Combine(Application.persistentDataPath, SAVES_FOLDER), path);
        LoadManager.SaveFile(Path.Combine(fullPath, GAME_FILE), new SaveableGameState(1, true));
        LoadManager.SaveFile(Path.Combine(fullPath, PLAYER_INVENTORY), new SaveableInventory(10));
        LoadManager.SaveFile(Path.Combine(fullPath, SHOP_INVENTORY), new SaveableInventory(Resources.Load<NewGameInventorySO>(Path.Combine("NewGame", "ShopkeeperInventory")).itemIDs));
    }

    // Smaže složku s uloženým postupem
    public void DeleteSave(string path)
    {
        LoadManager.DeleteDirectory(Path.Combine(Application.persistentDataPath, SAVES_FOLDER, path));
    }

    // Uloží nastavení
    public void SaveSettings(SaveableSettings settings)
    {
        LoadManager.SaveFile<SaveableSettings>(Path.Combine(Application.persistentDataPath, SETTINGS_FILE), settings);
    }

    // Načte nastavení, případně vytvoří nové
    public SaveableSettings LoadSettings()
    {
        string fullPath = Path.Combine(Application.persistentDataPath, SETTINGS_FILE);

        if (LoadManager.FileExists(fullPath)) {
            return LoadManager.ReadFile<SaveableSettings>(fullPath);
        } else
        {
            SaveableSettings settings = new SaveableSettings();
            LoadManager.SaveFile<SaveableSettings>(fullPath, settings);
            return settings;
        }
    }

    #endregion

    #region Scene Managment

    // Přesvědčí se, zda se už něco nenačítá a poté zavolá Coroutine, která načte scénu s postavou a druhou scénu s výběrem úrovní
    public void LoadGame(string path, SaveableSettings playerSettings)
    {
        if (_loading)
        {
            return;
        }

        _currentSavePath = path;
        StartCoroutine(LoadGameAsync(HUB_SCENE_NAME, PLAYER_SCENE_NAME, playerSettings));
    }

    // Přesvědčí se, zda se už něco nenačítá a poté zavolá Coroutine, která načte scénu, která vygeneruje mapu
    public void EnterMaze(MazeSettingsSO mazeSettings)
    {
        if (_loading)
        {
            return;
        }

        Player.GetComponent<PlayerController>().ResetStatsAndCleanInventory();

        StartCoroutine(LoadMazeAsync(MAP_SCENE_NAME, mazeSettings));
    }

    // Zjistí se, zda se už něco nenačítá a poté zavolá Coroutine, která načte scénu s výběrem úrovní
    public void ReturnToHub(bool success, int coinsUnlocked)
    {
        if (_loading)
        {
            return;
        }

        Player.GetComponent<PlayerController>().GetPlayerInventory().AddCoinsToPlayer(coinsUnlocked);
        Player.GetComponent<PlayerController>().ResetStatsAndCleanInventory();
        Player.GetComponent<PlayerController>().GetPlayerInventory().Save();

        int completedLevelIndex = 0;
        if (success)
        {
            completedLevelIndex = CurrentMazeManager.LevelNumber;
        }

        StartCoroutine(LoadHubAsync(HUB_SCENE_NAME, completedLevelIndex));
    }

    // Přesvědčí se, zda se už něco nenačítá a poté zavolá Coroutine, která načte úvodní obrazovku
    public void ReturnToMenu()
    {
        if (_loading)
        {
            return;
        }

        QuestUI.ClearQueueAndHideCanvas();
        StartCoroutine(LoadMenu(MENU_SCENE_NAME));
    }

    // Odnačte scénu podle názvu
    public void UnloadScene(string name)
    {
        StartCoroutine(UnloadSceneAsync(name));
    }

    #endregion

    #region Enumerators

    //  Načte scénu s postavou a výběrem levelů, poté předá portálům a prodavačovi odkaz na hráčovu pozici a také cestu, kam ukládat postup
    IEnumerator LoadGameAsync(string locationSceneName, string playerSceneName, SaveableSettings playerSettings)
    {
        _loading = true;
        _loadingScreen.ShowLoadingScreen();

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

        while (Player == null || CurrentHubManager == null || InputManager == null) { yield return null; }

        Player.GetComponent<PlayerController>().GetPlayerInventory().Load(Path.Combine(Application.persistentDataPath, SAVES_FOLDER, _currentSavePath, PLAYER_INVENTORY));
        InputManager.GetCameraTransform().GetComponent<CameraController>().SetSensitivity(playerSettings.xSensitivity, playerSettings.ySensitivity);

        CurrentHubManager.LoadState(Path.Combine(Application.persistentDataPath, SAVES_FOLDER, _currentSavePath, GAME_FILE));
        CurrentHubManager.EnablePlayerDependantObjects(Player.transform, InputManager.GetCameraTransform(), Path.Combine(Application.persistentDataPath, SAVES_FOLDER, _currentSavePath, "shop.inv"));

        UnloadScene(MENU_SCENE_NAME);
        _loading = false;
        _loadingScreen.HideLoadingScreen();
    }

    // Načte scénu s výběrem levelů (scéna s postavou je načtena) a odnačte scénu s bludištěm
    IEnumerator LoadHubAsync(string locationSceneName, int completedLevelIndex)
    {
        _loading = true;
        _loadingScreen.ShowLoadingScreen();
        Player.SetActive(false);
        CurrentHubManager = null;
        CurrentMazeManager = null;
        AsyncOperation locationSceneLoadingTask = SceneManager.LoadSceneAsync(locationSceneName, LoadSceneMode.Additive);

        while (!locationSceneLoadingTask.isDone) { yield return null; }

        SceneManager.SetActiveScene(SceneManager.GetSceneByName(locationSceneName));

        while (CurrentHubManager == null) { yield return null; }

        UnloadScene(MAP_SCENE_NAME);

        Player.SetActive(true);
        Player.transform.position = new Vector3(0, 2, 0); // hardcoded

        CurrentHubManager.LoadState(Path.Combine(Application.persistentDataPath, SAVES_FOLDER, _currentSavePath, GAME_FILE));
        CurrentHubManager.EnablePlayerDependantObjects(Player.transform, InputManager.GetCameraTransform(), Path.Combine(Application.persistentDataPath, SAVES_FOLDER, _currentSavePath, SHOP_INVENTORY));

        CurrentHubManager.UnlockNextLevel(completedLevelIndex);

        _loading = false;
        _loadingScreen.HideLoadingScreen();
    }

    // Načte scénu s generátorem mapy (scéna s postavou je načtena) a odnačte scénu s výběrem úrovní
    IEnumerator LoadMazeAsync(string locationSceneName, MazeSettingsSO mazeSettings)
    {
        _loading = true;
        _loadingScreen.ShowLoadingScreen();
        Player.SetActive(false);
        CurrentHubManager = null;
        CurrentMazeManager = null;
        AsyncOperation locationSceneLoadingTask = SceneManager.LoadSceneAsync(locationSceneName, LoadSceneMode.Additive);

        while (!locationSceneLoadingTask.isDone) { yield return null; }

        SceneManager.SetActiveScene(SceneManager.GetSceneByName(locationSceneName));

        while (CurrentMazeManager == null) { yield return null; }

        string message = CurrentMazeManager.CreateMaze(mazeSettings);
        QuestUI.QueueMessage(message);

        CurrentHubManager = null;
        UnloadScene(HUB_SCENE_NAME);

        Player.SetActive(true);
        _loading = false;
        _loadingScreen.HideLoadingScreen();
    }

    // Načte menu
    IEnumerator LoadMenu(string menuSceneName)
    {
        _loading = true;
        _loadingScreen.ShowLoadingScreen();
        Player.SetActive(false);
        Player = null;
        CurrentHubManager = null;
        InputManager = null;
        CurrentHubManager = null;
        CurrentMazeManager = null;

        string[] loadedScenes = GetNamesOfActiveScenes();

        AsyncOperation menuSceneLoadingTask = SceneManager.LoadSceneAsync(menuSceneName, LoadSceneMode.Additive);
        while (!menuSceneLoadingTask.isDone) { yield return null; }
        SceneManager.SetActiveScene(SceneManager.GetSceneByName(menuSceneName));

        yield return null;

        for (int i = 0; i < loadedScenes.Length; i++)
        {
            UnloadScene(loadedScenes[i]);
        }

        _loading = false;
        _loadingScreen.HideLoadingScreen();
    }

    // Odnačte scénu podle názvu
    IEnumerator UnloadSceneAsync(string sceneName)
    {
        if (!sceneName.Equals(SceneManager.GetActiveScene().name))
        {
            AsyncOperation sceneUnloadingTask = SceneManager.UnloadSceneAsync(sceneName);
            while (!sceneUnloadingTask.isDone) { yield return null; }
        }
    }

    private string[] GetNamesOfActiveScenes()
    {
        int sceneCount = SceneManager.sceneCount;
        string[] loadedScenes = new string[sceneCount];

        for (int i = 0; i < sceneCount; i++)
        {
            loadedScenes[i] = SceneManager.GetSceneAt(i).name;
        }

        return loadedScenes;
    }

    #endregion
}
