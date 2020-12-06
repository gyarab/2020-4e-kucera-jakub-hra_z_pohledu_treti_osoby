﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SaveableGameState
{
    public int highestLevelUnlocked;
    public bool firstTime;

    public SaveableGameState(int highestLevelUnlocked, bool firstTime)
    {
        this.highestLevelUnlocked = highestLevelUnlocked;
        this.firstTime = firstTime;
    }
}