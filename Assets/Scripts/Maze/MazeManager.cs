using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class MazeManager : MonoBehaviour
{
    private bool _completedWinCondition;
    private int _coinsCollected;

    public void Awake()
    {
        _completedWinCondition = false;
        _coinsCollected = 0;
    }

    public void Start()
    {
        GameManager.Instance.CurrentMazeManager = this; // why?
    }

    public void CreateMaze(MazeSettingsSO mazeSettings) // TODO IWIN
    {
        IWinCondition winCondition = gameObject.AddComponent<FindKey>(); // TODO add properly
        winCondition.OnCompleted += WinConditionCompleted;

        MazeGenerator mazeGenerator = GetComponent<MazeGenerator>();
        PathfindingNode[] nodes = mazeGenerator.GenerateMaze(mazeSettings, winCondition, out int nodeCount);
        Pathfinding<PathfindingNode> pathfinding = new Pathfinding<PathfindingNode>(nodes, nodeCount);
        EnemyController.Pathfinder = pathfinding;
    }

    private void WinConditionCompleted()
    {
        Debug.Log("mission accomplished");
        // TODO something
        _completedWinCondition = true;
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