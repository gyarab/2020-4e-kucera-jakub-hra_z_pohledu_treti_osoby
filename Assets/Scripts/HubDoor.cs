using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HubDoor : MonoBehaviour, IDoor // TODO rework to button press?
{
    [SerializeField]
    private MazeSettingsSO _mazeSettings;

    public void Entered()
    {
        GameManager.Instance.LoadMaze(_mazeSettings);
    }
}