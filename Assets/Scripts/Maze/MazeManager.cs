using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class MazeManager : MonoBehaviour
{
    private bool _completedWinCondition;
    private int _coinsCollected;
    private string[] _winConditionMessages;

    public void Awake()
    {
        _completedWinCondition = false;
        _coinsCollected = 0;
    }

    // Předá svůj odkaz Game Manageru, aby věděl, že už je skript načtený a mohl z něj zavolat metodu k vytvoření mapy
    public void Start()
    {
        GameManager.Instance.CurrentMazeManager = this;
    }

    // Přidá jeden komponent implementující interface Win Condition podle nastavení v Maze Settings
    public string CreateMaze(MazeSettingsSO mazeSettings)
    {
        IWinCondition winCondition;

        switch (mazeSettings.mazeWinCondition)
        {
            case WinConditionType.Boss:
                winCondition = gameObject.AddComponent<FindKey>();
                break;
            case WinConditionType.ClearLocation:
                winCondition = gameObject.AddComponent<ClearLocation>();
                break;
            case WinConditionType.CollectItems:
                winCondition = gameObject.AddComponent<CollectArtefacts>();
                break;
            default:
                throw new System.Exception("Can't generate maze without win condition");
        }

        winCondition.OnCompleted += WinConditionCompleted;

        MazeGenerator mazeGenerator = GetComponent<MazeGenerator>();
        PathfindingNode[] nodes = mazeGenerator.GenerateMaze(mazeSettings, winCondition, out int nodeCount);
        Pathfinding<PathfindingNode> pathfinding = new Pathfinding<PathfindingNode>(nodes, nodeCount);
        EnemyController.Pathfinder = pathfinding;

        _winConditionMessages = winCondition.GetMessages();
        return _winConditionMessages[0];
    }

    // Metoda je zavolána z Win Condition, když je úkol k dokončení úrovně splňen
    private void WinConditionCompleted()
    {
        _completedWinCondition = true;
        GameManager.Instance.QuestUI.QueueMessage(_winConditionMessages[1]);
    }

    // Zavolá metodu v Game Manageru, která načte scńu s výběrem úrovní
    private void ReturnToHub()
    {
        GameManager.Instance.ReturnToHub(_completedWinCondition, _coinsCollected);
    }

    // Vždy, když je nepřítel poražen, "dostane" hráč 1 peníz
    private void GotCoin(Vector3 position)
    {
        _coinsCollected++;
    }

    // Nastaví odebírání metod
    private void OnEnable()
    {
        ReturnPortal.OnMazeExit += ReturnToHub;
        EnemyController.OnEnemyDeath += GotCoin;
    }

    // Zruší odebírání metod
    private void OnDisable()
    {
        ReturnPortal.OnMazeExit -= ReturnToHub;
        EnemyController.OnEnemyDeath -= GotCoin;
    }
}