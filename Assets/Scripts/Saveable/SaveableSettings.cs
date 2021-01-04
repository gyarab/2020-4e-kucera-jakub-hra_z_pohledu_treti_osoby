using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SaveableSettings
{
    public float xSensitivity, ySensitivity;

    public SaveableSettings()
    {
        xSensitivity = 30f;
        ySensitivity = 30f;
    }

    public SaveableSettings(float xSensitivity, float ySensitivity)
    {
        this.xSensitivity = xSensitivity;
        this.ySensitivity = ySensitivity;
    }
}
