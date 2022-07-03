﻿using UnityEngine;

namespace CinematographyPlugin.UI
{
    public abstract class Option
    {
        private GameObject Root { get; }
        private bool StartActive { get; }
        private string Name { get; }

        internal List<Option> SubOptions { get; } = new ();

        internal Dictionary<Option, bool> StateByDisableOnSelectOptions { get; } = new ();

        protected Option(GameObject root, bool startActive)
        {
            Root = root;
            StartActive = startActive;
            Name = root.name;

            root.active = startActive;
        }

        public void SetActive(bool state)
        {
            Root.active = state;
        }

        public abstract void Disable(bool state);
        public abstract void Enable(bool state);

        public abstract void OnReset();
    }
}