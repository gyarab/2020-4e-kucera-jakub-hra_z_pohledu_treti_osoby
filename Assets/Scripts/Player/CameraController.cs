using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public bool StopRotatingCamera { get; set; }

    [Header("General")]
    [SerializeField]
    private Transform _playerTransform;
    [SerializeField]
    private LayerMask _collisionLayer;

    [Header("Offset"), SerializeField]
    private Vector3 _cameraOffset;
    [SerializeField]
    private Vector2 _pitchMinMax;
    [SerializeField]
    private float _distanceFromTarget, _rotationSmoothTime, _cameraClippingOffset, _automaticCameraRotationSpeed;
    
    private Transform _cameraLockedTarget;
    private bool _lockedOnTarget, _shouldRotate, _forceLockOn;
    private Vector3 _rotationSmoothVelocity, _currentRotation, _velocityRotation;
    private float _yaw, _pitch;
    private Vector2 _lookInput;
    private float _cameraSensitivityX, _cameraSensitivityY;

    // Nastaví citlivost při otáčení kamerou
    public void SetSensitivity(float x, float y)
    {
        _cameraSensitivityX = x;
        _cameraSensitivityY = y;
    }

    // Tato metoda je volána z Input Manageru, který ji uživatelská vstup a další nutné informace
    public void SetInput(Vector3 input, Vector3 velocityRotation, bool rotateManually)
    {
        _lookInput = input;
        _velocityRotation = velocityRotation;
        _shouldRotate = rotateManually;
    }

    // nastaví cíl, na který se má kamera zaměřit
    public void SetTarget(Transform target)
    {
        _cameraLockedTarget = target;
        _lockedOnTarget = true;
    }

    // Metoda je volána herním enginem tědně před vykreslením smímku
    void LateUpdate()
    {
        Vector3 newCamPos;
        RaycastHit hit;

        // Kamera se otáčí, když je registrovaný dotek ovládající kameru
        if (_shouldRotate && !_forceLockOn)
        {
            LookAround();
            _lockedOnTarget = false;
        }
        else if (_lockedOnTarget) // Když je kamera zaměřená na nepřítele, tak se otáčí na něj
        {
            if (_cameraLockedTarget == null)
            {
                _lockedOnTarget = false;
            }
            else
            {
                _currentRotation = new Vector3(_currentRotation.x, Mathf.LerpAngle(_currentRotation.y, Quaternion.LookRotation(_cameraLockedTarget.position - _playerTransform.position).eulerAngles.y, _automaticCameraRotationSpeed * Time.deltaTime));
                transform.eulerAngles = _currentRotation;
            }
        }
        else if (_velocityRotation != Vector3.zero) // Když nejou předchozí podmínky splňeny, tak se kamera otáčí podle směru pohybu hráče
        {
            _currentRotation = new Vector3(_currentRotation.x, Mathf.LerpAngle(_currentRotation.y, Quaternion.LookRotation(_velocityRotation).eulerAngles.y, _automaticCameraRotationSpeed * Time.deltaTime));
            transform.eulerAngles = _currentRotation;
        }

        newCamPos = _playerTransform.position - transform.forward * _distanceFromTarget + _cameraOffset;

        // Přiblíží kameru k hráčově postavě, pokud by měla projít zdí
        Ray ray = new Ray(_playerTransform.position + _cameraOffset, newCamPos - (_playerTransform.position + _cameraOffset));
        if (Physics.Raycast(ray, out hit, _distanceFromTarget, _collisionLayer))
        {
            transform.position = hit.point + transform.forward * _cameraClippingOffset;
        }
        else
        {
            transform.position = newCamPos;
        }

        _shouldRotate = false;
    }

    // Otáčí kamerou, když je ovládána dotekem obrazovky
    private void LookAround()
    {
        // Pohybuje kamerou
        _yaw += _lookInput.x * _cameraSensitivityX;
        _pitch -= _lookInput.y * _cameraSensitivityY;
        _pitch = Mathf.Clamp(_pitch, _pitchMinMax.x, _pitchMinMax.y);

        // Otáčí kameru za hráčem
        _currentRotation = Vector3.SmoothDamp(_currentRotation, new Vector3(_pitch, _yaw), ref _rotationSmoothVelocity, _rotationSmoothTime);
        transform.eulerAngles = _currentRotation;
    }
}
