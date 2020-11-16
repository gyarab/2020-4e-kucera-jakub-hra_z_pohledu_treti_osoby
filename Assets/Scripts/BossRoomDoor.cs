using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class BossRoomDoor : MonoBehaviour, IDoor
{
    public static Action OnDoorsOpened { get; set; }

    public void Entered()
    {
        // TODO fog + close doors, add listenet to boss for activation?
        OnDoorsOpened?.Invoke();
    }
}
