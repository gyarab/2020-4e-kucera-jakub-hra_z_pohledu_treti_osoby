using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FloatingButton : MonoBehaviour
{
    [SerializeField]
    private float _interactableDistance;

    private static Transform playerTransform;
    private static Transform cameraTransform;

    private Canvas _canvas;
    private bool _canvasEnabled;

    private void Start()
    {
        FloatingButtonStart();
    }

    public void FloatingButtonStart()
    {
        _canvas = GetComponentInChildren<Canvas>();
        _canvas.enabled = false;

        if (cameraTransform == null)
        {
            cameraTransform = Camera.main.transform;
        }
        if (playerTransform == null)
        {
            playerTransform = GameManager.Instance.Player.transform;
        }
    }

    private void Update()
    {
        LookAtCamera(cameraTransform);
    }

    private void FixedUpdate()
    {
        if (Vector3.Distance(transform.position, playerTransform.position) < _interactableDistance)
        {
            ShowButton();
        }
        else
        {
            HideButton();
        }
        Debug.Log("Floating button update");
    }

    private void ShowButton()
    {
        if (!_canvasEnabled)
        {
            _canvas.enabled = true;
            _canvasEnabled = true;
        }
    }

    private void HideButton()
    {
        if (_canvasEnabled)
        {
            _canvas.enabled = false;
            _canvasEnabled = false;
        }
    }

    private void LookAtCamera(Transform _cameraTransform)
    {
        if (_canvasEnabled)
        {
            _canvas.transform.LookAt(_cameraTransform);
            _canvas.transform.Rotate(0, 180, 0);
        }
    }
}
