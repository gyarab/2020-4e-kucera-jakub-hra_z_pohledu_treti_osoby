using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class InputManager : MonoBehaviour
{
    [Header("General")]
    [SerializeField]
    private Joystick _joystick;
    [SerializeField]
    private PlayerController _playerController;
    [SerializeField]
    private CameraController _cameraController;
    [SerializeField]
    private InventoryMonoBehaviour _inventory;

    [Header("Target Lock"), Range(0f, 1f), SerializeField]
    private float _targetLockTimeWindow;
    [SerializeField]
    public float _targetLockMaxFingerDistance, _targetLockRayDistance;
    [Range(1, 10), SerializeField]
    private int _recordedTouchesLimit;
    [SerializeField]
    private LayerMask _excludeUILayer;

    private int _cameraFingerId;
    private Dictionary<int, float> _fingerTouchTimeDictionary;
    private RaycastHit _raycastHit;
    private Ray _ray;

    // Nastaví id doteku, který má pohybovat s kamerou na -1 (znamená, že není zatím id přiřazeno) a vytvoří slovník pro uložení doteků obrazovky
    private void Awake()
    {
        _cameraFingerId = -1;
        _fingerTouchTimeDictionary = new Dictionary<int, float>(_recordedTouchesLimit);
        _excludeUILayer = ~_excludeUILayer;
    }

    // Předá odkaz na sebe Game Manageru, aby se mohlo načítání pokračovat
    private void Start()
    {
        GameManager.Instance.InputManager = this;
    }

    // Každý snímek zavolá metodu Get Input, která kontroluje vstup
    private void Update()
    {
        GetInput();
    }

    // Kontroluje uživatelský vstup spojený s otáčením kamery, pohybem a uzamčení otáčení obrazovky na nepřátelích
    private void GetInput()
    {
        // Projde všechny doteky obrazovky
        for (int i = 0; i < Input.touchCount; i++)
        {
            Touch t = Input.GetTouch(i);

            switch (t.phase)
            {
                case TouchPhase.Began:
                    // Zkontroluje jestli prst se dotkl prvku grafického rozhraní
                    if (!EventSystem.current.IsPointerOverGameObject(t.fingerId))
                    {
                        if (_cameraFingerId == -1)
                        {
                            // Považuje tento dotek za dotek pohybující kamerou
                            _cameraFingerId = t.fingerId;
                        }

                        if (_fingerTouchTimeDictionary.Count < _recordedTouchesLimit)
                        {
                            // Sleduje i další doteky, protože pomocí nich může hráč zaměřit kameru na protivníka
                            _fingerTouchTimeDictionary.Add(t.fingerId, 0);
                        }
                    }
                    break;

                case TouchPhase.Ended:
                case TouchPhase.Canceled:
                    // Přestane sledovat dotek, když skončil
                    if (t.fingerId == _cameraFingerId)
                    {
                        _cameraFingerId = -1;
                    }

                    if (_fingerTouchTimeDictionary.ContainsKey(t.fingerId))
                    {
                        _fingerTouchTimeDictionary.Remove(t.fingerId);
                        _ray = _cameraController.GetComponent<Camera>().ScreenPointToRay(t.position);
                        if (Physics.Raycast(_ray, out _raycastHit, _targetLockRayDistance, _excludeUILayer))
                        {
                            if (_raycastHit.transform.CompareTag("Damageable"))
                            {
                                _cameraController.SetTarget(_raycastHit.transform);
                            }
                        }
                    }
                    break;

                case TouchPhase.Moved:
                    // Získání vstupu pro rozhlížení
                    if (t.fingerId == _cameraFingerId)
                    {
                        _cameraController.SetInput(t.deltaPosition * Time.deltaTime, _playerController.GetRotationVelocity(), true);
                    }

                    if (_fingerTouchTimeDictionary.ContainsKey(t.fingerId))
                    {
                        _fingerTouchTimeDictionary[t.fingerId] += t.deltaTime;
                        if (Vector2.SqrMagnitude(t.deltaPosition) > _targetLockMaxFingerDistance || _fingerTouchTimeDictionary[t.fingerId] > _targetLockTimeWindow)
                        {
                            _fingerTouchTimeDictionary.Remove(t.fingerId);
                        }
                    }
                    break;

                case TouchPhase.Stationary:
                    // Vstup je 0, když dotek je na stejném místě
                    if (t.fingerId == _cameraFingerId)
                    {
                        _cameraController.SetInput(Vector2.zero, _playerController.GetRotationVelocity(), true);
                    }

                    if (_fingerTouchTimeDictionary.ContainsKey(t.fingerId))
                    {
                        _fingerTouchTimeDictionary[t.fingerId] += t.deltaTime;
                        if (_fingerTouchTimeDictionary[t.fingerId] > _targetLockTimeWindow)
                        {
                            _fingerTouchTimeDictionary.Remove(t.fingerId);
                        }
                    }
                    break;
            }
        }

        if(_cameraFingerId == -1)
        {
            _cameraController.SetInput(Vector2.zero, _playerController.GetRotationVelocity(), false);
        }
        _playerController.SetJoystickInput(_joystick.Horizontal, _joystick.Vertical, _cameraController.transform.rotation.eulerAngles.y);
    }

    #region Button Activated Methods

    // Tato metoda je aktivována tlačítkem pro útočení
    public void AttackUI()
    {
        _playerController.SendInput(PlayerActionType.Attack);
    }

    // Tato metoda je zavolána tlačítkem pro vyhýbaní se
    public void RollUI()
    {
        _playerController.SendInput(PlayerActionType.Roll);
    }

    // Tato metoda je aktivována tlačítkem pro skákání
    public void JumpUI()
    {
        _playerController.SendInput(PlayerActionType.Jump);
    }

    // Tato metoda je zavolána pomocí tlačítka pro pozastavení otevžení menu
    public void OpenMenuUI()
    {
        _inventory.ShowInventory(false);
    }

    #endregion

    // Vrací Transform kamery, která sleduje hráčovu postavu
    public Transform GetCameraTransform()
    {
        return _cameraController.transform;
    }
}
