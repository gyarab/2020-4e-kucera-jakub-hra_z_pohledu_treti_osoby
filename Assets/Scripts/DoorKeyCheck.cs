using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorKeyCheck : MonoBehaviour, IDoor
{
    [SerializeField]
    private BossRoomDoor _doorToOpen;
    [SerializeField]
    private int _keyID;

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
            if (GameManager.Instance.Player.GetComponent<PlayerController>().GetPlayerInventory().RemoveItemIfInInventory(_keyID))
            {
                _doorToOpen.Open();
                _activated = true;
            }
        }
    }

    public void Enabled()
    {

    }
}
