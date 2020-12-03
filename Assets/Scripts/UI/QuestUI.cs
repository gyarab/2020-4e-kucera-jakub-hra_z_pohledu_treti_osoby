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

    // Start is called before the first frame update
    void Start()
    {
        _questCanvas = GetComponent<Canvas>();
        _questTMPT = GetComponentInChildren<TextMeshProUGUI>();
        _questCanvas.enabled = false;
        _messageQueue = new Queue<string>();
        _isDisplaying = false;

        Debug.Log(_questCanvas);
        Debug.Log(_questTMPT);
    }

    public void QueueMessage(string message)
    {
        _messageQueue.Enqueue(message);
        TryToDisplayNext();
    }

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

    private IEnumerator HideCanvasAfterDelay(float delay)
    {
        yield return new WaitForSecondsRealtime(delay);
        _questCanvas.enabled = false;
        _isDisplaying = false;
        TryToDisplayNext();
    }
}
