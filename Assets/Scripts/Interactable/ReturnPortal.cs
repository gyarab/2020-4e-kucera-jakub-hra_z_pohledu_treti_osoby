using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class ReturnPortal : FloatingButton
{
    public static Action OnMazeExit;

    void Start()
    {
        FloatingButtonStart();
    }

    public void InteractGUI()
    {
        OnMazeExit?.Invoke();
    }
}
