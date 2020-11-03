using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MazeManager : MonoBehaviour
{
    public Pathfinding Pathfinding { get; set; }
    private static MazeManager _instance;

    public static MazeManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = (MazeManager)FindObjectOfType(typeof(MazeManager));

                if (_instance == null)
                {
                    // Create a new GameObject to attach the singleton to.
                    GameObject gameObject = new GameObject("_MazeManager");
                    _instance = gameObject.AddComponent<MazeManager>();
                }
            }

            return _instance;
        }
    }

    public void CreateMaze(MazeSettingsSO mazeSettings) // TODO IWIN
    {
        IWinCondition winCondition = new CollectArtefacts(); // TODO add properly
        MazeGenerator mazeGenerator = gameObject.AddComponent<MazeGenerator>();
        int nodeCount;
        PathfindingNode[] nodes = mazeGenerator.GenerateMaze(mazeSettings, winCondition, out nodeCount);
        Pathfinding = new Pathfinding(nodes, nodeCount);
    }
}
