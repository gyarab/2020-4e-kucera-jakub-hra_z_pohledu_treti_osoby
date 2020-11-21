using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class BossRoomDoor : MonoBehaviour, IDoor
{
    public static Action OnDoorsOpened { get; set; }
    private bool _opened;

    [SerializeField]
    private GameObject _door;

    private void Awake()
    {
        _opened = false;
    }

    public void Entered()
    {
        if (!_opened)
        {
            OnDoorsOpened?.Invoke();
            _opened = true;
            Close();
        }
    }

    public void Open()
    {
        _door.SetActive(false);
    }

    public void Close()
    {
        _door.SetActive(true);
    }
}
