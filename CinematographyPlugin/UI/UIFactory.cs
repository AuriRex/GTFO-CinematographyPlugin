﻿using CinematographyPlugin.Cinematography;
using CinematographyPlugin.Cinematography.Settings;
using CinematographyPlugin.UI.Enums;
using MonoMod.Utils;
using UnityEngine;

namespace CinematographyPlugin.UI
{
    public static class UIFactory
    {
        
        public static Dictionary<UIOption, Option> BuildOptions(GameObject cinemaUI)
        {
            var options = new Dictionary<UIOption, Option>
            {
                { UIOption.ToggleUI, new ToggleOption(GetOptionObj(cinemaUI, UIOption.ToggleUI), true, true) },
                { UIOption.ToggleBio, new ToggleOption(GetOptionObj(cinemaUI, UIOption.ToggleBio), false, false, true) },
                { UIOption.ToggleAspectRatio, new ToggleOption(GetOptionObj(cinemaUI, UIOption.ToggleAspectRatio), false, true) },
                { UIOption.AspectRatioSlider, new SliderOption(GetOptionObj(cinemaUI, UIOption.AspectRatioSlider), false, 1.777f, 0, 5) },
                { UIOption.ToggleBody, new ToggleOption(GetOptionObj(cinemaUI, UIOption.ToggleBody), true, true) },
                { UIOption.ToggleFreeCamera, new ToggleOption(GetOptionObj(cinemaUI, UIOption.ToggleFreeCamera), false, true) },
                { UIOption.MovementSpeedSlider, new SliderOption(GetOptionObj(cinemaUI, UIOption.MovementSpeedSlider), false, CinCamSettings.MovementSpeedDefault, CinCamSettings.MovementSpeedMin, CinCamSettings.MovementSpeedMax) },
                { UIOption.MovementSmoothingSlider, new SliderOption(GetOptionObj(cinemaUI, UIOption.MovementSmoothingSlider), false, CinCamSettings.MovementSmoothTimeDefault, CinCamSettings.MovementSmoothTimeMin, CinCamSettings.MovementSmoothTimeMax) },
                { UIOption.RotationSpeedSlider, new SliderOption(GetOptionObj(cinemaUI, UIOption.RotationSpeedSlider), false, CinCamSettings.RotationSpeedDefault, CinCamSettings.RotationSpeedMin, CinCamSettings.RotationSpeedMax) },
                { UIOption.RotationSmoothingSlider, new SliderOption(GetOptionObj(cinemaUI, UIOption.RotationSmoothingSlider), false, CinCamSettings.RotationSmoothTimeDefault, CinCamSettings.RotationSmoothTimeMin, CinCamSettings.RotationSmoothTimeMax) },
                { UIOption.ZoomSpeedSlider, new SliderOption(GetOptionObj(cinemaUI, UIOption.ZoomSpeedSlider), false, CinCamSettings.ZoomSpeedDefault, CinCamSettings.ZoomSpeedMin, CinCamSettings.ZoomSpeedMax) },
                { UIOption.ZoomSmoothingSlider, new SliderOption(GetOptionObj(cinemaUI, UIOption.ZoomSmoothingSlider), false, CinCamSettings.ZoomSmoothTimeDefault, CinCamSettings.ZoomSmoothTimeMin, CinCamSettings.ZoomSmoothTimeMax) },
                { UIOption.ToggleDynamicRoll, new ToggleOption(GetOptionObj(cinemaUI, UIOption.ToggleDynamicRoll), false, false) },
                { UIOption.DynamicRollIntensitySlider, new SliderOption(GetOptionObj(cinemaUI, UIOption.DynamicRollIntensitySlider), false, CinCamSettings.DynamicRotationDefault, CinCamSettings.DynamicRotationMin, CinCamSettings.DynamicRotationMax) },
                { UIOption.ToggleAlignPitchAxisWCam, new ToggleOption(GetOptionObj(cinemaUI, UIOption.ToggleAlignPitchAxisWCam), true, false) },
                { UIOption.ToggleAlignRollAxisWCam, new ToggleOption(GetOptionObj(cinemaUI, UIOption.ToggleAlignRollAxisWCam), false, false) },
                { UIOption.ToggleFpsLookSmoothing, new ToggleOption(GetOptionObj(cinemaUI, UIOption.ToggleFpsLookSmoothing), false, true) },
                { UIOption.FpsLookSmoothingSlider, new SliderOption(GetOptionObj(cinemaUI, UIOption.FpsLookSmoothingSlider), false, CinCamSettings.LookSmoothDefault, CinCamSettings.LookSmoothMin, CinCamSettings.LookSmoothMax) },
                { UIOption.ToggleDoF, new ToggleOption(GetOptionObj(cinemaUI, UIOption.ToggleDoF), false, true) },
                { UIOption.FocusDistanceSlider, new SliderOption(GetOptionObj(cinemaUI, UIOption.FocusDistanceSlider), false, CinCamSettings.FocusDistanceDefault, CinCamSettings.FocusDistanceMin, CinCamSettings.FocusDistanceMax) },
                { UIOption.ApertureSlider, new SliderOption(GetOptionObj(cinemaUI, UIOption.ApertureSlider), false, CinCamSettings.ApertureDefault, CinCamSettings.ApertureMin, CinCamSettings.ApertureMax) },
                { UIOption.FocalLenghtSlider, new SliderOption(GetOptionObj(cinemaUI, UIOption.FocalLenghtSlider), false, CinCamSettings.FocalLenghtDefault, CinCamSettings.FocalLenghtMin, CinCamSettings.FocalLenghtMax) },
                { UIOption.ToggleVignette, new ToggleOption(GetOptionObj(cinemaUI, UIOption.ToggleVignette), true, true) },
                { UIOption.ToggleAmbientParticles, new ToggleOption(GetOptionObj(cinemaUI, UIOption.ToggleAmbientParticles), true, true) },
                { UIOption.ToggleTimeScale, new ToggleOption(GetOptionObj(cinemaUI, UIOption.ToggleTimeScale), false, true) },
                { UIOption.TimeScaleSlider, new SliderOption(GetOptionObj(cinemaUI, UIOption.TimeScaleSlider), false, CinCamSettings.TimeScaleDefault, CinCamSettings.TimeScaleMin, CinCamSettings.TimeScaleMax) }
            };

            // Add sub options
            options[UIOption.ToggleFreeCamera].SubOptions.AddRange(new []
            {
                options[UIOption.MovementSpeedSlider],
                options[UIOption.MovementSmoothingSlider],
                options[UIOption.RotationSpeedSlider],
                options[UIOption.RotationSmoothingSlider],
                options[UIOption.ZoomSpeedSlider],
                options[UIOption.ZoomSmoothingSlider],
                options[UIOption.ToggleDynamicRoll],
                options[UIOption.ToggleAlignPitchAxisWCam],
                options[UIOption.ToggleAlignRollAxisWCam],
            });
            
            options[UIOption.ToggleDynamicRoll].SubOptions.Add(options[UIOption.DynamicRollIntensitySlider]);
            
            options[UIOption.ToggleAspectRatio].SubOptions.Add(options[UIOption.AspectRatioSlider]);
            
            options[UIOption.ToggleFpsLookSmoothing].SubOptions.Add(options[UIOption.FpsLookSmoothingSlider]);
            
            options[UIOption.ToggleTimeScale].SubOptions.Add(options[UIOption.TimeScaleSlider]);
            
            options[UIOption.ToggleUI].SubOptions.Add(options[UIOption.ToggleBio]);
            
            options[UIOption.ToggleDoF].SubOptions.AddRange(new []
            {
                options[UIOption.FocusDistanceSlider],
                options[UIOption.ApertureSlider],
                options[UIOption.FocalLenghtSlider]
            });
            
            // Add options to disable on select
            options[UIOption.ToggleFreeCamera].StateByDisableOnSelectOptions.Add(options[UIOption.ToggleUI], false);
            options[UIOption.ToggleFreeCamera].StateByDisableOnSelectOptions.Add(options[UIOption.ToggleBody], false);
            options[UIOption.ToggleFreeCamera].StateByDisableOnSelectOptions.Add(options[UIOption.ToggleFpsLookSmoothing], false);
            
            options[UIOption.ToggleTimeScale].StateByDisableOnSelectOptions.Add(options[UIOption.ToggleFpsLookSmoothing], false);
            
            return options;
        }

        public static Dictionary<UIOption, ToggleOption> GetToggles(Dictionary<UIOption, Option> options)
        {
            return options.Where(o => o.Value is ToggleOption).ToDictionary(o => o.Key, o => o.Value as ToggleOption);
        }
        
        public static Dictionary<UIOption, SliderOption> GetSliders(Dictionary<UIOption, Option> options)
        {
            return options.Where(o => o.Value is SliderOption).ToDictionary(o => o.Key, o => o.Value as SliderOption);
        }

        private static GameObject GetOptionObj(GameObject cinemaUI, UIOption option)
        {
            // CinemaUI/Canvas/Window/ViewPort
            var windowViewPort = cinemaUI.transform.GetChild(0).GetChild(1).GetChild(0);
            // ViewPort/Body/ViewPort
            var bodyViewPort = windowViewPort.GetChild(1).GetChild(0);
            var gameObject = bodyViewPort.transform.Find(option.ToString()).gameObject;
            return gameObject;
        }
    }
}