using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorKeyCheck : MonoBehaviour, IDoor
{
    [SerializeField]
    private BossRoomDoor _doorToOpen;
    private bool _activated;

    private void Awake()
    {
        _activated = false;
    }

    public void Entered()
    {
        // TODO check for key
        if(!_activated)
        {
            // TODO remove key;
            _doorToOpen.Open();
            _activated = true;
        }
    }
}
