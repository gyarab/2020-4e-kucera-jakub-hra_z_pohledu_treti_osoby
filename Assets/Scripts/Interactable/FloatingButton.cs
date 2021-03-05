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

    // Inicializace
    private void Start()
    {
        FloatingButtonStart();
    }

    // Získá odkaz ke Canvasu, kameře a hráči
    protected void FloatingButtonStart()
    {
        _canvas = GetComponentInChildren<Canvas>();
        _canvas.enabled = false;
    }

    // Předá odkaty na hráče a kameru
    public static void SetTransforms(Transform player, Transform camera)
    {
        playerTransform = player;
        playerCameraTransform = camera;
    }

    // Natočí UI směrem ke kameře
    private void Update()
    {
        LookAtCamera(playerCameraTransform);
    }

    // Zobrazí nebo schová UI v závislosti na vzdálenosti od hráče
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

    // Zobrazí grafické rozhraní
    private void ShowButton()
    {
        if (!_canvasEnabled)
        {
            _canvas.enabled = true;
            _canvasEnabled = true;
        }
    }

    // Skryje grafické rozhraní
    private void HideButton()
    {
        if (_canvasEnabled)
        {
            _canvas.enabled = false;
            _canvasEnabled = false;
        }
    }

    // Otočí UI směrem ke kameře
    private void LookAtCamera(Transform _cameraTransform)
    {
        if (_canvasEnabled)
        {
            _canvas.transform.LookAt(_cameraTransform);
            _canvas.transform.Rotate(0, 180, 0);
        }
    }
}
