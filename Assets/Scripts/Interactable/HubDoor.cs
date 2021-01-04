using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HubDoor : MonoBehaviour, IDoor // TODO rework to button press?
{
    [SerializeField]
    private MazeSettingsSO _mazeSettings;

    private void Start()
    {
        GetComponentInChildren<ParticleSystem>().Stop();
    }

    public void Entered()
    {
        GameManager.Instance.EnterMaze(_mazeSettings);
    }

    public void Enabled()
    {
        GetComponentInChildren<ParticleSystem>().Play();
    }
}