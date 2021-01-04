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

    public void SetSensitivity(float x, float y)
    {
        _cameraSensitivityX = x;
        _cameraSensitivityY = y;
    }

    public void SetInput(Vector3 input, Vector3 velocityRotation, bool rotateManually)
    {
        _lookInput = input;
        _velocityRotation = velocityRotation;
        _shouldRotate = rotateManually;
    }

    public void SetTarget(Transform target)
    {
        _cameraLockedTarget = target;
        _lockedOnTarget = true;
    }

    void LateUpdate()
    {
        Vector3 newCamPos;
        RaycastHit hit;

        if (_shouldRotate && !_forceLockOn)
        {
            // Look around if the right finger is being tracked
            LookAround();
            _lockedOnTarget = false;
        }
        else if (_lockedOnTarget)
        {
            _currentRotation = new Vector3(_currentRotation.x, Mathf.LerpAngle(_currentRotation.y, Quaternion.LookRotation(_cameraLockedTarget.position - _playerTransform.position).eulerAngles.y, _automaticCameraRotationSpeed * Time.deltaTime));
            transform.eulerAngles = _currentRotation;
        }
        else if (_velocityRotation != Vector3.zero) // TODO CHANGE x and z != 0; should I?
        {
            // Rotates camera so it faces player movement direction  
            _currentRotation = new Vector3(_currentRotation.x, Mathf.LerpAngle(_currentRotation.y, Quaternion.LookRotation(_velocityRotation).eulerAngles.y, _automaticCameraRotationSpeed * Time.deltaTime));
            transform.eulerAngles = _currentRotation;
        }

        newCamPos = _playerTransform.position - transform.forward * _distanceFromTarget + _cameraOffset;

        // Camera Collision Check
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

    private void LookAround()
    {
        // Moving camera
        _yaw += _lookInput.x * _cameraSensitivityX;
        _pitch -= _lookInput.y * _cameraSensitivityY;
        _pitch = Mathf.Clamp(_pitch, _pitchMinMax.x, _pitchMinMax.y);

        // Rotating camera
        _currentRotation = Vector3.SmoothDamp(_currentRotation, new Vector3(_pitch, _yaw), ref _rotationSmoothVelocity, _rotationSmoothTime);
        transform.eulerAngles = _currentRotation;

        // change rotation based on movement; prob useless; TODO remove
        /*Vector3 e = cameraTransform.eulerAngles;
        e.x = 0;

        transform.eulerAngles = e;*/
    }
}
