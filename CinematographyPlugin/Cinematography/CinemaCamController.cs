﻿using CinematographyPlugin.UI;
using CinematographyPlugin.UI.Enums;
using CinematographyPlugin.UI.UiInput;
using CinematographyPlugin.Util;
using Player;
using UnityEngine;

namespace CinematographyPlugin.Cinematography
{
    public class CinemaCamController : MonoBehaviour
    {
        private const float FastSpeedScale = 2;
        private const float SlowSpeedScale = 1f/2f;
        
        public const float MovementSpeedDefault = 0.5f;
        public const float MovementSpeedMin = 0f;
        public const float MovementSpeedMax = 1f;
        private const float MovementSpeedScale = 10;

        public const float MovementSmoothTimeDefault = 0.2f;
        public const float MovementSmoothTimeMin = 0f;
        public const float MovementSmoothTimeMax = 1f;

        private const float SensitivityScaling = 100f;
        private static float _rotationSpeedDefault = .65f;
        public const float RotationSpeedMin = 0f;
        public const float RotationSpeedMax = 1f;
        private const float RotationDiffMax = 90f;

        public const float RotationSmoothTimeDefault = 0.2f;
        public const float RotationSmoothTimeMin = 0f;
        public const float RotationSmoothTimeMax = 1f;

        private static float _zoomDefault;
        private const float ZoomMin = 1f;
        private const float ZoomMax = 160f;

        private const float ZoomScaling = 100f;
        public const float ZoomSpeedDefault = .65f;
        public const float ZoomSpeedMin = 0f;
        public const float ZoomSpeedMax = 1f;

        public const float ZoomSmoothTimeDefault = 0.2f;
        public const float ZoomSmoothTimeMin = 0f;
        public const float ZoomSmoothTimeMax = 1f;

        public const float DynamicRotationDefault = 1f;
        public const float DynamicRotationMin = 0f;
        public const float DynamicRotationMax = 2f;
        private const float DynamicRotationSpeedScale = 10f;
        private const float DynamicRotationSmoothFactor = 0.4f;
        private const float DynamicRotationRollMax = 180f;
        
        private bool _alignPitchAxisWCam = true;
        private bool _alignRollAxisWCam = false;
        private bool _dynamicRotation = true;
        
        private float _movementSpeed = MovementSpeedDefault;
        private float _rotationSpeed = _rotationSpeedDefault;
        private float _movementSmoothFactor = MovementSmoothTimeDefault;
        private float _rotationSmoothFactor = RotationSmoothTimeDefault;
        private float _targetZoom = _zoomDefault;
        private float _currZoom = _zoomDefault;
        private float _zoomSpeed = ZoomSpeedDefault;
        private float _zoomSmoothFactor = ZoomSmoothTimeDefault;
        private float _dynamicRotationSpeed = DynamicRotationDefault;
        
        private FPSCamera _fpsCamera;
        private PlayerAgent _playerAgent;
        private Transform _rotTrans;
        private Vector3 _targetPos = Vector3.zero;
        private Vector3 _prevPos = Vector3.zero;
        private Vector3 _movementVelocity = Vector3.zero;
        private Quaternion _targetWorldRot = Quaternion.identity;
        private Quaternion _targetLocalRot = Quaternion.identity;

        private void Awake()
        {
            CinemaUIManager.Sliders[UIOption.MovementSpeedSlider].OnValueChanged += SetMovementSpeed;
            CinemaUIManager.Sliders[UIOption.MovementSmoothingSlider].OnValueChanged += SetMovementSmoothTime;
            
            CinemaUIManager.Sliders[UIOption.RotationSpeedSlider].OnValueChanged += SetRotationSpeed;
            CinemaUIManager.Sliders[UIOption.RotationSmoothingSlider].OnValueChanged += SetRotationSmoothTime;

            CinemaUIManager.Sliders[UIOption.ZoomSpeedSlider].OnValueChanged += SetZoomSpeed;
            CinemaUIManager.Sliders[UIOption.ZoomSmoothingSlider].OnValueChanged += SetZoomSmoothTime;

            CinemaUIManager.Toggles[UIOption.ToggleDynamicRoll].OnValueChanged += SetDynamicRotation;
            CinemaUIManager.Sliders[UIOption.DynamicRollIntensitySlider].OnValueChanged += SetDynamicRotationSpeed;
            CinemaUIManager.Toggles[UIOption.ToggleAlignPitchAxisWCam].OnValueChanged += SetAlignPitchAxisWCam;
            CinemaUIManager.Toggles[UIOption.ToggleAlignRollAxisWCam].OnValueChanged += SetAlignRollAxisWCam;

            _playerAgent = PlayerManager.GetLocalPlayerAgent();
            
            _fpsCamera = FindObjectOfType<FPSCamera>();
            _rotTrans = transform.GetChild(0);
            
            // Get default sensitivity 
            _rotationSpeedDefault = CellSettingsManager.GetFloatValue(eCellSettingID.Gameplay_LookSpeed);
            _rotationSpeed = _rotationSpeedDefault;
            
            // Get default FoV
            _zoomDefault = GetDefaultZoom();
            _targetZoom = _zoomDefault;
            _currZoom = _zoomDefault;
        }

        // Positions and Rotations must be synced before parenting to align with camera transform    
        public void SyncWithCameraTransform()
        {
            var fpsCamRot = _fpsCamera.m_orgParent.eulerAngles;
            var worldRot = Quaternion.Euler(0, fpsCamRot.y, 0);
            var localRot = Quaternion.Euler(fpsCamRot.x, 0, 0);

            transform.localRotation = _targetWorldRot = worldRot;
            _rotTrans.localRotation = _targetLocalRot = localRot;
            _targetPos = transform.position = _fpsCamera.Position;
        }

        private static float GetDefaultZoom()
        {
            if (_zoomDefault < 0.001)
            {
                _zoomDefault = CellSettingsManager.GetIntValue(eCellSettingID.Video_WorldFOV);
            }

            return _zoomDefault;
        }
        
        public static float GetDefaultRotationSpeed()
        {
            if (_rotationSpeedDefault < 0.001)
            {
                _rotationSpeedDefault = CellSettingsManager.GetFloatValue(eCellSettingID.Gameplay_LookSpeed);
            }

            return _rotationSpeedDefault;
        }

        private void Update()
        {
            if (Cursor.lockState != CursorLockMode.Locked) return;
            
            UpdateRotation();
            UpdatePosition();
            UpdateCuller();
            UpdateZoom();
            CheckReset();

            if (!_dynamicRotation) return;
            
            CalculateDynamicRotationDelta();
        }

        private void UpdatePosition()
        {
            var independentDeltaTime = IndependentDeltaTimeManager.GetDeltaTime();

            // get directional vectors
            var x = InputManager.GetAxis(AxisName.PosX);
            var y = InputManager.GetAxis(AxisName.PosY);
            var z = InputManager.GetAxis(AxisName.PosZ);
                
            var delta = Vector3.zero;

            // calculate speed and smoothing time
            var speedAxis = InputManager.GetAxis(AxisName.Speed);
            var speedScale = MovementSpeedScale * (speedAxis > 0 ? FastSpeedScale : speedAxis < 0 ? SlowSpeedScale : 1);
            var speed = _movementSpeed * speedScale;

            var right = _alignRollAxisWCam ? _rotTrans.right : FlatRight();
            var forward = _alignPitchAxisWCam ? _rotTrans.forward : FlatForward();
            var up = _alignPitchAxisWCam ? _rotTrans.up : Vector3.up;

            // calculate translation delta with smoothing
            delta += independentDeltaTime * speed * x * right;
            delta += independentDeltaTime * speed * y * up;
            delta += independentDeltaTime * speed * z * forward;
            
            _targetPos += delta;
            
            var currPos = transform.position;
            var t = 1.0f - Mathf.Pow(_movementSmoothFactor, independentDeltaTime);
            var newPos = Vector3.Lerp(currPos, _targetPos, t);

            _movementVelocity = (newPos - _prevPos)/independentDeltaTime;
            _prevPos = newPos;
            
            transform.position = newPos;
        }
        
        private void UpdateRotation()
        {
            var independentDeltaTime = IndependentDeltaTimeManager.GetDeltaTime();
            
            var worldTrans = transform;
            var localTrans = _rotTrans;
            
            // get directional vectors
            var pitch = InputManager.GetAxis(AxisName.RotX);
            var yaw = InputManager.GetAxis(AxisName.RotY);
            var roll = InputManager.GetAxis(AxisName.RotZ);

            var upsideDown = Math.Sign(Vector3.Dot(localTrans.up, Vector3.up));
            yaw *= upsideDown; // invert yaw controls when upside down to keep mose directions consistent

            // calculate speed and smoothing time
            var speed = _rotationSpeed * SensitivityScaling;

            var deltaEuler = new Vector3(pitch, yaw, roll);
            deltaEuler *= independentDeltaTime * speed;
            
            var deltaWorld = Quaternion.Euler(0, deltaEuler.y, deltaEuler.z);
            var deltaLocal = Quaternion.Euler(deltaEuler.x, 0, 0);
            
            // calculate rotation delta with smoothing
            if (!IsYawRotationFlippedByNewRotation(deltaWorld))
            {
                _targetWorldRot *= deltaWorld;
            }
            if (!IsPitchRotationFlippedByNewRotation(deltaLocal))
            {
                _targetLocalRot *= deltaLocal;
            }

            var t = 1.0f - Mathf.Pow(_rotationSmoothFactor, independentDeltaTime);
            var newWorldRot = Quaternion.Slerp(worldTrans.localRotation, _targetWorldRot, t);
            var newLocalRot = Quaternion.Slerp(localTrans.localRotation, _targetLocalRot, t);

            worldTrans.localRotation = newWorldRot;
            localTrans.localRotation = newLocalRot;
        }

        // Prevents slerp from sudden direction change when shortest angle flips around to opposite dir 
        private bool IsYawRotationFlippedByNewRotation(Quaternion delta)
        {
            var preTargetForward = _targetWorldRot * Vector3.fwd;
            var aftTargetForward = _targetWorldRot * delta * Vector3.fwd;
            var currRot = transform.localRotation * Vector3.fwd;
            
            var rotAxis = Vector3.Cross(_fpsCamera.Forward, preTargetForward);
            var preAngle = Vector3.SignedAngle(currRot, preTargetForward, rotAxis);
            var aftAngle = Vector3.SignedAngle(currRot, aftTargetForward, rotAxis);

            return Math.Abs(preAngle) >= RotationDiffMax && Math.Sign(preAngle) != Math.Sign(aftAngle);
        }
        
        private bool IsPitchRotationFlippedByNewRotation(Quaternion delta)
        {
            var preTargetForward = _targetLocalRot * Vector3.fwd;
            var aftTargetForward = _targetLocalRot * delta * Vector3.fwd;
            var currRot = _rotTrans.localRotation * Vector3.fwd;
            
            var rotAxis = Vector3.Cross(_fpsCamera.Forward, preTargetForward);
            var preAngle = Vector3.SignedAngle(currRot, preTargetForward, rotAxis);
            var aftAngle = Vector3.SignedAngle(currRot, aftTargetForward, rotAxis);

            return Math.Abs(preAngle) >= RotationDiffMax && Math.Sign(preAngle) != Math.Sign(aftAngle);
        }
        
        // Pitch causes more trouble than good so it is commented out
        private void CalculateDynamicRotationDelta()
        {
            var independentDeltaTime = IndependentDeltaTimeManager.GetDeltaTime();
            
            var worldTrans = transform;
            var localTrans = _rotTrans;
            
            var up = _alignPitchAxisWCam ? localTrans.up : Vector3.up;
            var right = _alignPitchAxisWCam ? localTrans.right : FlatRight();
            // var forward = _mouseCtrlAltitude ? localTrans.forward : FlatForward();
            
            var velocityXZ = Vector3.ProjectOnPlane(_movementVelocity, up);
            var vector = velocityXZ * (_dynamicRotationSpeed * DynamicRotationSpeedScale);

            // var pitchDir = Mathf.Sign(Vector3.Dot(vector, forward));
            var rollDir = Mathf.Sign(Vector3.Dot(vector, right));

            // var pitch = Mathf.Clamp(Vector3.Project(vector, forward).magnitude, 0, DynamicRotationPitchMax) * pitchDir;
            var roll = Mathf.Clamp(Vector3.Project(vector, right).magnitude, 0, DynamicRotationRollMax) * rollDir;
            // var targetPitch = worldTrans.rotation * Quaternion.Euler(pitch, 0 , 0);
            var targetRoll = worldTrans.rotation * Quaternion.Euler(0, 0 , roll);

            var t = 1.0f - Mathf.Pow(DynamicRotationSmoothFactor, independentDeltaTime);
            // var newLocalRot = Quaternion.Slerp(localTrans.localRotation, targetPitch, t);
            var newWorldRot = Quaternion.Slerp(worldTrans.localRotation, targetRoll, t);

            // localTrans.localRotation = newLocalRot;
            worldTrans.localRotation = newWorldRot;
        }

        private void UpdateZoom()
        {
            var independentDeltaTime = IndependentDeltaTimeManager.GetDeltaTime();

            var dir = InputManager.GetAxis(AxisName.Zoom);
            var speed = _zoomSpeed * ZoomScaling;
            
            _targetZoom = Mathf.Clamp(_targetZoom + independentDeltaTime * speed * dir, ZoomMin, ZoomMax);
            
            var t = 1.0f - Mathf.Pow(_zoomSmoothFactor, independentDeltaTime);
            var newZoom = Mathf.Lerp(_currZoom, _targetZoom, t);
            
            _fpsCamera.m_camera.fieldOfView = newZoom;
            // Have separate zoom since accessing the field seems to reset it
            _currZoom = newZoom;
        }
        
        private void UpdateCuller()
        {
            var currPosition = transform.position;
            var raycastOrigWithOffset = currPosition + FlatForward();
            var newCullPos = Physics.Raycast(raycastOrigWithOffset, Vector3.down, out var hit) ? hit.point : currPosition;
            
            _fpsCamera.m_owner.m_movingCuller.UpdatePosition(_playerAgent.m_dimensionIndex, newCullPos);
        }

        private void CheckReset()
        {
            if (InputManager.GetReset())
            {
                OnReset();
            }
        }
        
        private void OnReset()
        {
            _targetZoom = _zoomDefault;
            _targetWorldRot = Quaternion.Euler(0, _targetWorldRot.eulerAngles.y, 0);
        }

        private Vector3 FlatForward()
        {
            return Vector3.ProjectOnPlane(_rotTrans.forward, Vector3.up).normalized;
        }
        
        private Vector3 FlatRight()
        {
            return Vector3.ProjectOnPlane(_rotTrans.right, Vector3.up).normalized;
        }

        private void SetMovementSpeed(float value)
        {
            _movementSpeed = value;
        }
        
        private void SetMovementSmoothTime(float value)
        {
            _movementSmoothFactor = value;
        }
        
        private void SetRotationSpeed(float value)
        {
            _rotationSpeed = value;
        }
        
        private void SetRotationSmoothTime(float value)
        {
            _rotationSmoothFactor = value;
        }
        
        private void SetZoomSpeed(float value)
        {
            _zoomSpeed = value;
        }
        
        private void SetZoomSmoothTime(float value)
        {
            _zoomSmoothFactor = value;
        }
        
        private void SetAlignPitchAxisWCam(bool value)
        {
            _alignPitchAxisWCam = value;
        }
        
        private void SetAlignRollAxisWCam(bool value)
        {
            _alignRollAxisWCam = value;
        }
        
        private void SetDynamicRotation(bool value)
        {
            _dynamicRotation = value;
        }

        private void SetDynamicRotationSpeed(float value)
        {
            _dynamicRotationSpeed = value;
        }

        private void OnDestroy()
        {
            CinemaUIManager.Sliders[UIOption.MovementSpeedSlider].OnValueChanged -= SetMovementSpeed;
            CinemaUIManager.Sliders[UIOption.MovementSmoothingSlider].OnValueChanged -= SetMovementSmoothTime;
            
            CinemaUIManager.Sliders[UIOption.RotationSpeedSlider].OnValueChanged -= SetRotationSpeed;
            CinemaUIManager.Sliders[UIOption.RotationSmoothingSlider].OnValueChanged -= SetRotationSmoothTime;

            CinemaUIManager.Sliders[UIOption.ZoomSpeedSlider].OnValueChanged -= SetZoomSpeed;
            CinemaUIManager.Sliders[UIOption.ZoomSmoothingSlider].OnValueChanged -= SetZoomSmoothTime;
            
            CinemaUIManager.Toggles[UIOption.ToggleAlignPitchAxisWCam].OnValueChanged -= SetAlignPitchAxisWCam;
            CinemaUIManager.Toggles[UIOption.ToggleAlignRollAxisWCam].OnValueChanged -= SetAlignRollAxisWCam;
            CinemaUIManager.Toggles[UIOption.ToggleDynamicRoll].OnValueChanged -= SetDynamicRotation;
            CinemaUIManager.Sliders[UIOption.DynamicRollIntensitySlider].OnValueChanged -= SetDynamicRotationSpeed;
        }
    }
}