using System.Collections.Generic;
using System.Text;
using Sonder.Core;
using TMPro;
using UnityEngine;
using Object = UnityEngine.Object;
using UnityEngine.UI;

namespace Sonder.Managers
{
    public class ModListOverlay : MonoBehaviour
    {
        public static ModListOverlay Instance { get; private set; }

        private GameObject _root;
        private TextMeshProUGUI _text;
        private float _nextUpdate;

        private void Awake()
        {
            Instance = this;
            Build();
        }

        private void OnDestroy()
        {
            if (Instance == this)
                Instance = null;
            if (_root != null)
                Object.Destroy(_root);
        }

        private void Build()
        {
            _root = new GameObject("Sonder_ModListOverlay");
            Canvas canvas = _root.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 4999;
            CanvasScaler scaler = _root.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);

            GameObject panel = new GameObject("Panel");
            panel.transform.SetParent(_root.transform, false);
            RectTransform pr = panel.AddComponent<RectTransform>();
            pr.anchorMin = new Vector2(1f, 1f);
            pr.anchorMax = new Vector2(1f, 1f);
            pr.pivot = new Vector2(1f, 1f);
            pr.anchoredPosition = new Vector2(-20f, -20f);
            pr.sizeDelta = new Vector2(320f, 600f);

            _text = panel.AddComponent<TextMeshProUGUI>();
            _text.fontSize = 14;
            _text.color = new Color(0.95f, 0.85f, 0.60f, 0.85f);
            _text.alignment = TextAlignmentOptions.TopRight;
            _text.text = "";
            Object.DontDestroyOnLoad(_root);
        }

        private void Update()
        {
            if (Time.time < _nextUpdate)
                return;
            _nextUpdate = Time.time + 1f;

            if (_text == null)
                return;

            StringBuilder sb = new StringBuilder();
            sb.AppendLine("<b>ENABLED MODS</b>");
            try
            {
                List<string> titles = ModManager.ActiveTitles();
                if (titles.Count == 0)
                {
                    sb.Append("(none)");
                }
                else
                {
                    for (int i = 0; i < titles.Count; i++)
                    {
                        sb.AppendLine("- " + titles[i]);
                    }
                }
            }
            catch { }

            _text.text = sb.ToString();
        }
    }
}