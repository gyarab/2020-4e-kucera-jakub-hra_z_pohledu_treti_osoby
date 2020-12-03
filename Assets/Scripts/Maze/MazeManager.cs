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

    public void Start()
    {
        GameManager.Instance.CurrentMazeManager = this;
    }

    public string CreateMaze(MazeSettingsSO mazeSettings) // TODO IWIN
    {
        IWinCondition winCondition;

        switch (mazeSettings.mazeWinCondition)
        {
            case WinConditionType.Boss:
                winCondition = gameObject.AddComponent<FindKey>();
                break;
            case WinConditionType.ClearLocation:
                winCondition = gameObject.AddComponent<FindKey>();
                break;
            case WinConditionType.CollectItems:
                winCondition = gameObject.AddComponent<FindKey>();
                break;
            default:
                throw new System.Exception("Can't generate maze without win condition"); // or maybe yes? - to farm gold
        }

        winCondition.OnCompleted += WinConditionCompleted;

        MazeGenerator mazeGenerator = GetComponent<MazeGenerator>();
        PathfindingNode[] nodes = mazeGenerator.GenerateMaze(mazeSettings, winCondition, out int nodeCount);
        Pathfinding<PathfindingNode> pathfinding = new Pathfinding<PathfindingNode>(nodes, nodeCount);
        EnemyController.Pathfinder = pathfinding;

        _winConditionMessages = winCondition.GetMessages();
        return _winConditionMessages[0];
    }

    private void WinConditionCompleted()
    {
        _completedWinCondition = true;
        GameManager.Instance.QuestUI.QueueMessage(_winConditionMessages[1]);
    }

    private void ReturnToHub()
    {
        GameManager.Instance.LoadHub(_completedWinCondition, _coinsCollected);
    }

    private void GotCoin(Vector3 position)
    {
        _coinsCollected++;
    }

    private void OnEnable()
    {
        ReturnPortal.OnMazeExit += ReturnToHub;
        EnemyController.OnEnemyDeath += GotCoin;
    }

    private void OnDisable()
    {
        ReturnPortal.OnMazeExit -= ReturnToHub;
        EnemyController.OnEnemyDeath -= GotCoin;
    }
}