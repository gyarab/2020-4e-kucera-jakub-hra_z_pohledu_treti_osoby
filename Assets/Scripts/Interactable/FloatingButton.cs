using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FloatingButton : MonoBehaviour
{
    [SerializeField]
    private float _interactableDistance = 2f;

    private static Transform playerTransform;
    private static Transform playerCameraTransform;

    private Canvas _canvas;
    private bool _canvasEnabled;

    private void Start()
    {
        FloatingButtonStart();
    }

    protected void FloatingButtonStart()
    {
        _canvas = GetComponentInChildren<Canvas>();
        _canvas.enabled = false;

        if (playerCameraTransform == null)
        {
            playerCameraTransform = Camera.main.transform;
        }
        if (playerTransform == null)
        {
            playerTransform = GameManager.Instance.Player.transform;
        }
    }

    public static void SetTransforms(Transform player, Transform camera)
    {
        playerTransform = player;
        playerCameraTransform = camera;
    }

    private void Update()
    {
        LookAtCamera(playerCameraTransform);
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
