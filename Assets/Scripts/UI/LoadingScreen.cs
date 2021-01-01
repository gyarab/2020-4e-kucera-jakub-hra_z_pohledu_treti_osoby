using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LoadingScreen : MonoBehaviour
{
    private Canvas _loadingCanvas;

    private void Start()
    {
        _loadingCanvas = GetComponent<Canvas>();
        HideLoadingScreen();
    }

    public void HideLoadingScreen()
    {
        _loadingCanvas.enabled = false;
    }

    public void ShowLoadingScreen()
    {
        _loadingCanvas.enabled = true;
    }
}
