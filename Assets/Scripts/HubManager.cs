using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HubManager : MonoBehaviour
{
    [SerializeField]
    private int _levelCount;
    [SerializeField]
    private Door[] _portals;
    [SerializeField]
    private Shopkeeper[] _shopkeepers;

    private SaveableGameState _gameState;
    private string _savePath;

    // Start is called before the first frame update
    void Start()
    {
        GameManager.Instance.CurrentHubManager = this;
    }

    public void EnablePlayerDependantObjects(Transform target, Transform cameraTransform, string shopInventoryPath)
    {
        FloatingButton.SetTransforms(target, cameraTransform);

        for (int i = 0; i < _gameState.highestLevelUnlocked; i++)
        {
            _portals[i].enabled = true;
            // TODO visual portal change?
        }

        for (int i = 0; i < _shopkeepers.Length; i++)
        {
            _shopkeepers[i].enabled = true;
            _shopkeepers[i].LoadShopInventory(shopInventoryPath);
        }
    }

    public void LoadState(string path)
    {
        _savePath = path;
        _gameState = LoadManager.ReadFile<SaveableGameState>(_savePath);

        // TODO check if first time and show how to play screen
    }

    public void UnlockNextLevel()
    {
        if(_gameState.highestLevelUnlocked < _levelCount)
        {
            _portals[_gameState.highestLevelUnlocked].enabled = true;
            _gameState.highestLevelUnlocked++;
            
            Save();
        }
    }

    private void Save()
    {
        LoadManager.SaveFile(_savePath, _gameState);
    }
}
