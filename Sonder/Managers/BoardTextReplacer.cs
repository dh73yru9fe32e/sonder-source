using System;
using Sonder.Core;
using TMPro;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Sonder.Managers
{
    // Replaces in-game code-of-conduct board text with the Sonder welcome paragraph.
    public class BoardTextReplacer : MonoBehaviour
    {
        public static BoardTextReplacer Instance { get; private set; }

        private const string Welcome = "Welcome to Sonder. This menu is your gateway to a world of mods and tweaks. Explore the categories, customize your experience, and enjoy your time in Gorilla Tag. Please use responsibly and respect other players.";

        private bool _done;
        private float _nextScan;

        private void Awake()
        {
            Instance = this;
        }

        private void OnDestroy()
        {
            if (Instance == this)
                Instance = null;
        }

        private void Update()
        {
            if (_done)
                return;

            if (Time.time < _nextScan)
                return;

            _nextScan = Time.time + 2f;
            TryReplace();
        }

        private void TryReplace()
        {
            bool replaced = false;

            TextMeshPro[] tmps = Resources.FindObjectsOfTypeAll<TextMeshPro>();
            for (int i = 0; i < tmps.Length; i++)
            {
                TextMeshPro t = tmps[i];
                if (t == null)
                    continue;
                string text = t.text;
                if (string.IsNullOrEmpty(text))
                    continue;
                if (text.IndexOf("conduct", StringComparison.OrdinalIgnoreCase) >= 0 ||
                    text.IndexOf("code of", StringComparison.OrdinalIgnoreCase) >= 0 ||
                    text.IndexOf("rules", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    t.text = Welcome;
                    replaced = true;
                }
            }

            TextMeshProUGUI[] tmpu = Resources.FindObjectsOfTypeAll<TextMeshProUGUI>();
            for (int i = 0; i < tmpu.Length; i++)
            {
                TextMeshProUGUI t = tmpu[i];
                if (t == null)
                    continue;
                string text = t.text;
                if (string.IsNullOrEmpty(text))
                    continue;
                if (text.IndexOf("conduct", StringComparison.OrdinalIgnoreCase) >= 0 ||
                    text.IndexOf("code of", StringComparison.OrdinalIgnoreCase) >= 0 ||
                    text.IndexOf("rules", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    t.text = Welcome;
                    replaced = true;
                }
            }

            if (replaced)
            {
                _done = true;
                RuntimeLog.Write("board text replaced");
            }
        }
    }
}