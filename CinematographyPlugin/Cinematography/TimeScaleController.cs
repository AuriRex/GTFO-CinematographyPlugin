﻿using CinematographyPlugin.Cinematography.Networking;
using CinematographyPlugin.UI;
using CinematographyPlugin.UI.Enums;
using CinematographyPlugin.UI.UiInput;
using CinematographyPlugin.Util;
using UnityEngine;

namespace CinematographyPlugin.Cinematography
{
    public class TimeScaleController : MonoBehaviour
    {
        public const float TimeScaleDefault = 1;
        public const float TimeScaleMin = 0.01f;
        public const float TimeScaleMax = 1;
        private const float TimeChangeSpeed = 1f;

        private static bool _paused;
        private static float _prevTimeScale = 1f;
        private float _targetTimeScale = TimeScaleDefault;
        
        private ToggleOption _timeScaleToggle;
        private SliderOption _timeScaleSlider;

        private void Start()
        {
            _timeScaleToggle = (ToggleOption) CinemaUIManager.Options[UIOption.ToggleTimeScale];
            _timeScaleSlider = (SliderOption) CinemaUIManager.Options[UIOption.TimeScaleSlider];
            _timeScaleSlider.OnValueChanged += OnTimeScaleChange;
            CinemaNetworkingManager.OnTimeScaleChangedByOtherPlayer += OnTimeScaleChange;
        }

        private void Update()
        {
            if (CinemaCamManager.Current.FreeCamEnabled())
            {
                UpdateTimeScaleFromKeyBinds();
            }
        }

        public static void ResetTimeScale()
        {
            Time.timeScale = 1;
        }

        private void OnTimeScaleChange(float value)
        {
            Time.timeScale = value;
        }

        private void UpdateTimeScaleFromKeyBinds()
        {
            _targetTimeScale = Mathf.Clamp(_targetTimeScale + InputManager.GetTimeScaleInput(), TimeScaleMin, TimeScaleMax);

            if (InputManager.GetTimeScalePausePlay())
            {
                TogglePausePlay();
            }
            
            if (Math.Abs(_targetTimeScale - Time.timeScale) > 0.001)
            {
                var newTimeScale = Mathf.MoveTowards(Time.timeScale, _targetTimeScale, IndependentDeltaTimeManager.GetDeltaTime() * TimeChangeSpeed);
                _timeScaleSlider.OnSliderChange(newTimeScale);
            }
            
            // Automatically turn on the time scale toggle if it's off and are using the key binds
            if (Math.Abs(_targetTimeScale - TimeScaleMax) > 0.001 && !_timeScaleToggle.Toggle.isOn)
            {
                _timeScaleToggle.Toggle.Set(true);
            }
        }
        
        private void TogglePausePlay()
        {
            var newTimeScale = Math.Abs(_targetTimeScale - TimeScaleMin) > 0.01 ? TimeScaleMin : TimeScaleMax;
            _targetTimeScale = newTimeScale;
            _timeScaleSlider.OnSliderChange(newTimeScale);
        }

        private void OnDestroy()
        {
            Time.timeScale = 1;
            ((SliderOption) CinemaUIManager.Options[UIOption.TimeScaleSlider]).OnValueChanged -=  OnTimeScaleChange;
            CinemaNetworkingManager.OnTimeScaleChangedByOtherPlayer -= OnTimeScaleChange;
        }
    }
}