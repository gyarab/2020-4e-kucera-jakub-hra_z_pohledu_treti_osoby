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

    private int _rightFingerId;
    private Dictionary<int, float> _fingerTouchTimeDictionary;

    private void Awake()
    {
        _rightFingerId = -1;
        _fingerTouchTimeDictionary = new Dictionary<int, float>(_recordedTouchesLimit);
    }

    private void Start()
    {
        GameManager.Instance.InputManager = this;
    }

    private void Update()
    {
        GetInput();
    }

    private void GetInput()
    {
        // Tracking the finger that controlls the camera
        for (int i = 0; i < Input.touchCount; i++)
        {
            Touch t = Input.GetTouch(i);

            switch (t.phase)
            {
                case TouchPhase.Began:
                    // Didn't touch UI
                    if (!EventSystem.current.IsPointerOverGameObject(t.fingerId))
                    {
                        if (_rightFingerId == -1)
                        {
                            // Start tracking the rightfinger if it was not previously being tracked
                            _rightFingerId = t.fingerId;
                        }

                        if (_fingerTouchTimeDictionary.Count < _recordedTouchesLimit)
                        {
                            // and if it hits enemy; maybe not
                            _fingerTouchTimeDictionary.Add(t.fingerId, 0);
                        }
                    }

                    break;
                case TouchPhase.Ended:
                case TouchPhase.Canceled:
                    if (t.fingerId == _rightFingerId)
                    {
                        // Stop tracking the right finger
                        _rightFingerId = -1;
                        //Debug.Log("Stopped tracking right finger");
                    }

                    if (_fingerTouchTimeDictionary.ContainsKey(t.fingerId))
                    {
                        _fingerTouchTimeDictionary.Remove(t.fingerId);

                        Ray ray = _cameraController.GetComponent<Camera>().ScreenPointToRay(t.position);
                        RaycastHit hit;
                        if (Physics.Raycast(ray, out hit, _targetLockRayDistance, _excludeUILayer))
                        {
                            if (hit.transform.tag == "Damageable")
                            {
                                Debug.Log("Enemy Lock; dst: " + hit.distance);
                                // TODO pass data to camera; move to camera
                                _cameraController.SetTarget(hit.transform);
                            }
                        }
                    }
                    break;
                case TouchPhase.Moved:
                    // Get input for looking around
                    if (t.fingerId == _rightFingerId)
                    {
                        _cameraController.SetInput(t.deltaPosition * Time.deltaTime, _playerController.GetRotationVelocity());
                        _cameraController.RotateCamera();
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
                    // Set the look input to zero if the finger is still
                    if (t.fingerId == _rightFingerId)
                    {
                        _cameraController.SetInput(Vector2.zero, _playerController.GetRotationVelocity());
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

        _playerController.SetJoystickInput(_joystick.Horizontal, _joystick.Vertical, _cameraController.transform.rotation.eulerAngles.y);
    }

    #region Button Activated Methods

    public void AttackUI()
    {
        _playerController.SendInput(PlayerActionType.Attack);
    }

    public void RollUI()
    {
        _playerController.SendInput(PlayerActionType.Roll);
    }

    public void JumpUI()
    {
        _playerController.SendInput(PlayerActionType.Jump);
    }

    public void PauseGameUI()
    {
        _inventory.ShowInventory(false);
    }

    #endregion

    public Transform GetCameraTransform()
    {
        return _cameraController.transform;
    }
}
