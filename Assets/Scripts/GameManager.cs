﻿using System.Collections;
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
    // TODO in a different way?
    public MazeManager CurrentMazeManager { get; set; }

    private string _currentSavePath; // TODO remove useless?

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
        StartCoroutine(LoadGameAsync("Hub", "Player"));
    }

    public void LoadMaze(MazeSettingsSO mazeSettings)
    {
        Debug.Log("loading maze");

        // TODO loading screen
        StartCoroutine(LoadMazeAsync("Maze", mazeSettings));
    }

    public void LoadHub(bool success, int coinsUnlocked)
    {
        Debug.Log("loading hub");

        if (success)
        {
            // TODO transfer player coins to inv && unlock next level if possible
        }

        // TODO laoding screen
        StartCoroutine(LoadHubAsync("Hub"));
    }

    public void UnloadScene(string name)
    {
        StartCoroutine(UnloadSceneAsync(name));
    }

    #endregion

    // TODO add check if its already loadings something
    #region Enumerators

    IEnumerator LoadGameAsync(string locationSceneName, string playerSceneName)
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

        Player.GetComponent<PlayerController>().GetPlayerInventory().Load(Path.Combine(Application.persistentDataPath, SAVES_FOLDER, _currentSavePath, PLAYER_INVENTORY));

        CurrentHubManager.EnablePlayerDependantObjects(Player.transform, Player.GetComponent<PlayerController>().GetPlayerCameraTransform(), Path.Combine(Application.persistentDataPath, SAVES_FOLDER, _currentSavePath, "shop.inv"));
        UnloadScene("Menu");
    }

    IEnumerator LoadHubAsync(string locationSceneName)
    {
        Player.SetActive(false);
        AsyncOperation locationSceneLoadingTask = SceneManager.LoadSceneAsync(locationSceneName, LoadSceneMode.Additive);

        while (!locationSceneLoadingTask.isDone) { yield return null; }

        SceneManager.SetActiveScene(SceneManager.GetSceneByName(locationSceneName));

        while (CurrentHubManager == null) { yield return null; }

        CurrentHubManager = null;
        UnloadScene("Maze");

        Player.SetActive(true);
        // TODO hide player loading screen; add loading screen to GameManager?
    }

    IEnumerator LoadMazeAsync(string locationSceneName, MazeSettingsSO mazeSettings)
    {
        Player.SetActive(false);
        AsyncOperation locationSceneLoadingTask = SceneManager.LoadSceneAsync(locationSceneName, LoadSceneMode.Additive);

        while (!locationSceneLoadingTask.isDone) { yield return null; }

        SceneManager.SetActiveScene(SceneManager.GetSceneByName(locationSceneName));

        while (CurrentMazeManager == null) { yield return null; }

        // TODO Change (mazeGen); impossible?
        CurrentMazeManager.CreateMaze(mazeSettings);

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
