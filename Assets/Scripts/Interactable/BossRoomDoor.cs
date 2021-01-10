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

    // Inicializace proměnných
    private void Awake()
    {
        _opened = false;
    }

    // Metoda vyvolá akci On Doors Opened; a zavře dveře
    public void Entered()
    {
        if (!_opened)
        {
            OnDoorsOpened?.Invoke();
            _opened = true;
            Close();
        }
    }

    // Spustí Coroutine k otevření dveří
    public void Open()
    {
        StartCoroutine(OpenDoor());
    }

    // Spustí Coroutine k uzavření dveří
    public void Close()
    {
        StartCoroutine(CloseDoor());
    }

    // Coroutine otevírající dveře tím, že je otáčí okol osy Y
    private IEnumerator OpenDoor()
    {
        float degreesRotated = 0;
        float degreesToRotate;
        while (true)
        {
            degreesToRotate = degreesToOpen * Time.deltaTime / timeToOpen;

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

    // Coroutine uzavírající dveře tím, že je otáčí okol osy Y
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

    // Není potřeba nic udělat při aktivaci (metoda z interface IDoor)
    public void Enabled()
    {

    }
}
