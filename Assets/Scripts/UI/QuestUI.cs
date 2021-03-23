using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class QuestUI : MonoBehaviour
{
    private TextMeshProUGUI _questTMPT;
    private Canvas _questCanvas;
    private Queue<string> _messageQueue;
    private bool _isDisplaying;

    // Inicializace proměnných
    void Start()
    {
        _questCanvas = GetComponent<Canvas>();
        _questTMPT = GetComponentInChildren<TextMeshProUGUI>();
        _questCanvas.enabled = false;
        _messageQueue = new Queue<string>();
        _isDisplaying = false;
    }

    // Přidá zprávu do fronty
    public void QueueMessage(string message)
    {
        _messageQueue.Enqueue(message);
        TryToDisplayNext();
    }

    // Vyčistí frontu a skryje Canvas
    public void ClearQueueAndHideCanvas()
    {
        _messageQueue.Clear();
        HideCanvas();
    }

    // Zobrazí další zprávu, pokud už se nějaká nezobrazuje nebo fronta je prázdná
    private void TryToDisplayNext()
    {
        if (!_isDisplaying && _messageQueue.Count > 0)
        {
            string message = _messageQueue.Dequeue();
            _questTMPT.text = message;
            _questCanvas.enabled = true;
            _isDisplaying = true;
            StartCoroutine(HideCanvasAfterDelay(4f));
        }
    }

    // Skryje Canvas se zprávou a pokusí se zobrazit další
    private IEnumerator HideCanvasAfterDelay(float delay)
    {
        yield return new WaitForSecondsRealtime(delay);
        HideCanvas();
        TryToDisplayNext();
    }

    // Skryje Canvas
    private void HideCanvas()
    {
        _questCanvas.enabled = false;
        _isDisplaying = false;
    }
}
