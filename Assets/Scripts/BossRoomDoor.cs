using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class BossRoomDoor : MonoBehaviour, IDoor
{
    public static Action OnDoorsOpened { get; set; }
    private bool _opened;

    [Header("Opening and closing"), SerializeField]
    private float degreesToOpen;
    [SerializeField]
    private float timeToOpen, timeToClose;
    [SerializeField]
    private Transform _rightDoor, _leftDoor;

    private void Awake()
    {
        _opened = false;
    }

    public void Entered()
    {
        if (!_opened)
        {
            OnDoorsOpened?.Invoke();
            _opened = true;
            Close();
        }
    }

    public void Open()
    {
        StartCoroutine(OpenDoor());
    }

    public void Close()
    {
        StartCoroutine(CloseDoor());
    }

    private IEnumerator OpenDoor()
    {
        float degreesRotated = 0;
        float degreesToRotate;
        while (true)
        {
            degreesToRotate = degreesToOpen * Time.deltaTime / timeToOpen;
            Debug.Log("degrees: " + degreesToRotate);

            if (degreesToOpen <= degreesToRotate + degreesRotated)
            {
                degreesToRotate = degreesToOpen - degreesRotated;
                _rightDoor.Rotate(0, -degreesToRotate, 0);
                _leftDoor.Rotate(0, degreesToRotate, 0);
                break;
            }

            _rightDoor.Rotate(0, -degreesToRotate, 0);
            _leftDoor.Rotate(0, degreesToRotate, 0);
            degreesRotated += degreesToRotate;

            yield return new WaitForEndOfFrame();
        }
    }

    private IEnumerator CloseDoor()
    {
        float degreesRotated = 0;
        float degreesToRotate;
        while (true)
        {
            degreesToRotate = degreesToOpen * Time.deltaTime / timeToOpen;

            if(degreesToOpen <= degreesToRotate + degreesRotated)
            {
                degreesToRotate = degreesToOpen - degreesRotated;
                _rightDoor.Rotate(0, degreesToRotate, 0);
                _leftDoor.Rotate(0, -degreesToRotate, 0);
                break;
            }

            _rightDoor.Rotate(0, degreesToRotate, 0);
            _leftDoor.Rotate(0, -degreesToRotate, 0);
            degreesRotated += degreesToRotate;

            yield return new WaitForEndOfFrame();
        }
    }
}
