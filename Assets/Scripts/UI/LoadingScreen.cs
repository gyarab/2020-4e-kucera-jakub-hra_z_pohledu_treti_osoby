using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LoadingScreen : MonoBehaviour
{
    private Canvas _loadingCanvas;

    // Inicializace a schová Canvas
    private void Start()
    {
        _loadingCanvas = GetComponent<Canvas>();
        HideLoadingScreen();
    }

    // Skryje Canvas k načítání
    public void HideLoadingScreen()
    {
        _loadingCanvas.enabled = false;
    }

    // Zobrazí Canvas k načítaní
    public void ShowLoadingScreen()
    {
        _loadingCanvas.enabled = true;
    }
}
