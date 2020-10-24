using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GroundItem : FloatingButton
{
    // TODO remove or uncomment if it doesnt work
    /*[SerializeField]
    private float pickUpRange;

    private static Transform playerTransform;
    private static Transform cameraTransform;*/
    
    private ItemObject _item;

    /*private Canvas pickUpCanvas;
    private bool canvasEnabled;*/

    private void Start()
    {
        FloatingButtonStart();

        /*pickUpCanvas = GetComponentInChildren<Canvas>();
        pickUpCanvas.enabled = false;

        if(cameraTransform == null)
        {
            cameraTransform = Camera.main.transform;
        }
        if (playerTransform == null)
        {
            playerTransform = GameManager.Instance.Player.transform;
        }*/
    }

    /*private void Update()
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
    }*/

    public void SetVariables(ItemObject item)
    {
        _item = item;
    }

    /*private void ShowButton()
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
    }*/

        
    public void PickUpGUI()
    {
        GameManager.Instance.Player.GetComponent<PlayerController>().GetPlayerInventory().AddItem(_item, 1);
        Destroy(gameObject);
    }
}
