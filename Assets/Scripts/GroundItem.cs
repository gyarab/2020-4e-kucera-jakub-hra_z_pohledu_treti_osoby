using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GroundItem : MonoBehaviour
{
    public static Transform playerTransform;
    public static Transform cameraTransform;
    public static float pickUpRange;
    
    public ItemObject item;

    private Canvas pickUpCanvas;
    private bool canvasEnabled;

    private void Start()
    {
        pickUpCanvas = GetComponentInChildren<Canvas>();
        pickUpCanvas.enabled = false;
    }

    private void Update()
    {
        LookAtCamera(cameraTransform);
    }

    private void FixedUpdate()
    {
        if (Vector3.Distance(transform.position, playerTransform.position) < pickUpRange)
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
        if (!canvasEnabled)
        {
            pickUpCanvas.enabled = true;
            canvasEnabled = true;
        }
    }

    private void HideButton()
    {
        if (canvasEnabled)
        {
            pickUpCanvas.enabled = false;
            canvasEnabled = false;
        }
    }

    private void LookAtCamera(Transform _cameraTransform)
    {
        if (canvasEnabled)
        {
            pickUpCanvas.transform.LookAt(_cameraTransform);
            pickUpCanvas.transform.Rotate(0, 180, 0);
        }
    }

    public void PickUpGUI()
    {
        GameManager.Instance.Player.GetComponent<PlayerController>().inventory.AddItem(item, 1);
        // TODO
        //GameManager.Instance.activeChunkManager.RemoveGroundItem(this);
        Destroy(gameObject);
    }
}
