using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class ReturnPortal : FloatingButton
{
    public static Action OnMazeExit;

    // Inicializace
    void Start()
    {
        FloatingButtonStart();
    }

    // Tato metoda je zavolána pomocí grafického rozhraní; vyvolá akci On Maze Exit, která navrátí hráče zpátky do mapy s výběrem levelů
    public void InteractGUI()
    {
        OnMazeExit?.Invoke();
    }
}
