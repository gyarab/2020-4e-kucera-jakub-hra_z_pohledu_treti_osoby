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

    // Aktivuje odemčené portály a nastaví cestu k ukládání v obchodě
    public void EnablePlayerDependantObjects(Transform target, Transform cameraTransform, string shopInventoryPath)
    {
        FloatingButton.SetTransforms(target, cameraTransform);

        for (int i = 0; i < _gameState.highestLevelUnlocked; i++)
        {
            _portals[i].enabled = true;
        }

        for (int i = 0; i < _shopkeepers.Length; i++)
        {
            _shopkeepers[i].enabled = true;
            _shopkeepers[i].LoadShopInventory(shopInventoryPath);
        }
    }

    // Načte ze souboru postup hráče
    public void LoadState(string path)
    {
        _savePath = path;
        _gameState = LoadManager.ReadFile<SaveableGameState>(_savePath);

        GameManager.Instance.QuestUI.QueueMessage("Welcome, visit a shop on your left before you go to battle."); // TODO hardcoded
        GameManager.Instance.QuestUI.QueueMessage("Once you're ready enter a lit portal.");
        _gameState.firstTime = false;
        Save();
    }

    // Odemkne další portál, pokud je to možné, a uloží postup
    public void UnlockNextLevel()
    {
        if(_gameState.highestLevelUnlocked < _levelCount)
        {
            _portals[_gameState.highestLevelUnlocked].enabled = true;
            _gameState.highestLevelUnlocked++;
            
            Save();
        }
    }

    // Zavolá metodu v Load Manageru, která uloží postup do souboru
    private void Save()
    {
        LoadManager.SaveFile(_savePath, _gameState);
    }
}
