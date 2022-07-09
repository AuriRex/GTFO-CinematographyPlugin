﻿using CinematographyPlugin.Cinematography.Networking;
using CinematographyPlugin.UI.Enums;
using GTFO.API;
using Player;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace CinematographyPlugin.UI
{
    public class CinemaUIManager : MonoBehaviour
    {
        public static CinemaUIManager Current;
        public static event Action OnUIStart;

        private readonly KeyCode UIOpenKey = ConfigManager.MenuKey;
        private const string PrefabPath = "Assets/UI/CinemaUI.prefab";

        internal Dictionary<UIOption, Option> Options { get; set; }
        internal Dictionary<UIOption, ToggleOption> Toggles; 
        internal Dictionary<UIOption, SliderOption> Sliders;

        internal CursorLockMode CursorLockLastMode { get; set; }
        internal bool CursorLastVisible { get; set; }
        internal bool MenuOpen { get; set; }
        private bool _init;
        private bool _hideTargetingText;

        private TMP_Text _centerText;
        private GameObject _centerTextWindow;
        private GameObject _window;
        private GameObject _cinematicBars;
        private GameObject _cinemaUIgo;
        private LocalPlayerAgent _playerAgent;

        public void Awake()
        {
            Current = this;
            var loadedAsset = AssetAPI.GetLoadedAsset(PrefabPath);
            
            var instantiate = Instantiate(loadedAsset);
            _cinemaUIgo = instantiate.TryCast<GameObject>();
            
            if (_cinemaUIgo is null)
            {
                CinematographyCore.log.LogWarning(
                    "Cinematography plugin could not load UI prefab. Asset either not loaded or not in Bepinex/Config/Assets/AssetBundles folder");
            }
            else
            {
                // CinemaUI/Canvas/Window
                var canvas = _cinemaUIgo.transform.GetChild(0);
                _cinematicBars = canvas.GetChild(0).gameObject;
                _window = canvas.GetChild(1).gameObject;
                _centerTextWindow = canvas.GetChild(2).gameObject;
                _centerText = _centerTextWindow.GetComponentInChildren<TMP_Text>();

                _window.gameObject.AddComponent<UIWindow>();
                
                var resetButton = _cinemaUIgo.GetComponentInChildren<Button>();;
                resetButton.onClick.AddListener((UnityAction) OnCloseButton);
                
                Options = UIFactory.BuildOptions(_cinemaUIgo);
                Toggles = UIFactory.GetToggles(Options);
                Sliders = UIFactory.GetSliders(Options);

                _window.active = false;
                _centerTextWindow.active = false;
                _cinematicBars.active = false;
                _cinemaUIgo.active = true;
                _init = true;
            }
        }

        public void Start()
        {
            CinemaNetworkingManager.OnFreeCamEnableOrDisable += OnFreeCamEnableOrDisable;
            CinemaNetworkingManager.OnTimeScaleEnableOrDisable += OnTimeScaleEnableOrDisable;
            CinemaPluginPatches.OnLocalPlayerDieOrRevive += OnFreeCamEnableOrDisable;
            
            Toggles[UIOption.ToggleFreeCamera].OnValueChanged += OnFreeCamSetActive;
            Toggles[UIOption.ToggleTargetingVisibility].OnValueChanged += SetHideTextOnScreen;

            _playerAgent = FindObjectOfType<LocalPlayerAgent>();

            OnUIStart?.Invoke();
        }

        public void Update()
        {
            if (!_init) return;
            if (Input.GetKeyDown(UIOpenKey))
            {
                if (MenuOpen)
                {
                    CloseUI();
                }
                else
                {
                    OpenUI();
                }
            }
        }

        public void OnCloseButton()
        {
            CloseUI();
        }

        public GameObject GetCinematicBars()
        {
            return _cinematicBars;
        }
        
        public void ShowTextOnScreen(string text)
        {
            if (_hideTargetingText) return;
            
            _centerText.SetText($"[{text}]");
            _centerTextWindow.gameObject.active = true;
        }
        
        public void ShowNoTargetTextOnScreen()
        {
            if (_hideTargetingText) return;
            
            _centerText.SetText("[NO TARGET]");
            _centerTextWindow.gameObject.active = true;
        }

        public void HideTextOnScreen()
        {
            _centerTextWindow.gameObject.active = false;
        }

        private void SetHideTextOnScreen(bool value)
        {
            _hideTargetingText = value;
        }

        private void OnFreeCamEnableOrDisable(bool enable)
        {
            var option = Toggles[UIOption.ToggleFreeCamera];
            if (enable)
            {
                option.Enable(option.Toggle.isOn);
            }
            else
            {
                option.Disable(false);
            }
        }

        private void OnFreeCamSetActive(bool active)
        {
            if (active)
            {
                _playerAgent.Sync.WantsToWieldSlot(InventorySlot.GearMelee);
            }
        }
        
        private void OnTimeScaleEnableOrDisable(bool enable)
        {
            var option = Toggles[UIOption.ToggleTimeScale];
            if (enable)
            {
                option.Enable(option.Toggle.isOn);
            }
            else
            {
                option.Disable(false);
            }
        }

        public void OpenUI()
        {
            if (!CinemaNetworkingManager.AssertAllPlayersHasPlugin()) return;

            CursorLockLastMode = Cursor.lockState;
            CursorLastVisible = Cursor.visible;
            
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            _window.active = true;
            MenuOpen = true;
        }
        
        public void CloseUI()
        {
            Cursor.lockState = CursorLockLastMode;
            Cursor.visible = CursorLastVisible;
            _window.active = false;
            MenuOpen = false;
        }

        public void OnDestroy()
        {
            CinemaNetworkingManager.OnFreeCamEnableOrDisable -= OnFreeCamEnableOrDisable;
            CinemaNetworkingManager.OnTimeScaleEnableOrDisable -= OnTimeScaleEnableOrDisable;
            CinemaPluginPatches.OnLocalPlayerDieOrRevive -= OnFreeCamEnableOrDisable;
            
            Toggles[UIOption.ToggleTargetingVisibility].OnValueChanged -= SetHideTextOnScreen;

            if (_cinemaUIgo != null)
            {
                Destroy(_cinemaUIgo);
            }
        }
    }
}