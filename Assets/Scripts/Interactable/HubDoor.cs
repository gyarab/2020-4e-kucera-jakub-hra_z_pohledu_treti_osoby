using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HubDoor : MonoBehaviour, IDoor // TODO rework to button press?
{
    [SerializeField]
    private MazeSettingsSO _mazeSettings;

    // Particle System začíná pozastavený
    private void Start()
    {
        GetComponentInChildren<ParticleSystem>().Stop(false, ParticleSystemStopBehavior.StopEmittingAndClear);
    }

    // Po vstupu do portálu zavolá metodu v Game Manageru a předá mu nastavení úrovně
    public void Entered()
    {
        GameManager.Instance.EnterMaze(_mazeSettings);
    }

    // Spustí Particle System
    public void Enabled()
    {
        GetComponentInChildren<ParticleSystem>().Play();
    }
}