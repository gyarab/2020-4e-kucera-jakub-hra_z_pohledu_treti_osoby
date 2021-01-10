using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SaveableSettings
{
    public float xSensitivity, ySensitivity;

    // Konstruktor s pevně danými hodnotami
    public SaveableSettings()
    {
        xSensitivity = 30f;
        ySensitivity = 30f;
    }

    // Konstruktor se všemi parametry
    public SaveableSettings(float xSensitivity, float ySensitivity)
    {
        this.xSensitivity = xSensitivity;
        this.ySensitivity = ySensitivity;
    }
}
