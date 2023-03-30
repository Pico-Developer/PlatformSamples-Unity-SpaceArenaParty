using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SpaceArenaParty.UI
{
    public class UILogPanelManager : MonoBehaviour
    {
        public TMP_Text _text;
        public RectTransform container;
        private readonly Queue<string> _lines = new();
        private bool _autoScroll = true;
        private ScrollRect _scrollRect;

        private void OnEnable()
        {
            _scrollRect = GetComponentInChildren<ScrollRect>();
            _scrollRect.onValueChanged.AddListener(OnScroll);
            Application.logMessageReceived += HandleLog;
            _text.text = "";
        }

        private void OnDisable()
        {
            Application.logMessageReceived -= HandleLog;
        }

        private IEnumerator AutoScroll()
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(container);
            yield return new WaitForEndOfFrame();
            yield return new WaitForEndOfFrame();
            _scrollRect.verticalNormalizedPosition = 0;
        }

        private void OnScroll(Vector2 value)
        {
            _autoScroll = value.y * container.rect.height <= 100;
        }

        private void HandleLog(string logString, string stackTrace, LogType type)
        {
            _lines.Enqueue(logString);
            if (_lines.Count > 100) _lines.Dequeue();
            _text.text = string.Join(Environment.NewLine, _lines);
            if (_autoScroll) StartCoroutine(AutoScroll());
        }
    }
}