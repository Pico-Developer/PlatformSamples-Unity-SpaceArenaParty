using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SpaceArenaParty.UI.Base
{
    public class UIRadioManager : MonoBehaviour
    {
        public Toggle radioPrefab;
        public string currentValue;
        private readonly List<Toggle> _radios = new();
        private readonly List<string> _values = new();
        public Action<string> OnValueChanged;

        public void Init(string[] values)
        {
            currentValue = values[0];

            for (var i = 0; i < values.Length; i++)
            {
                _values.Add(values[i]);
                var index = i;
                var radio = Instantiate(radioPrefab, transform);
                var label = radio.gameObject.GetComponentInChildren<TMP_Text>();
                label.SetText(values[i]);
                if (i == 0) radio.isOn = true;

                radio.onValueChanged.AddListener(value =>
                {
                    if (value) OnRadioSelect(index);
                });

                _radios.Add(radio);
            }
        }

        private void OnRadioSelect(int index)
        {
            if (currentValue != _values[index])
            {
                currentValue = _values[index];
                OnValueChanged?.Invoke(currentValue);
            }

            for (var i = 0; i < _radios.Count; i++)
                if (i == index)
                {
                    _radios[i].interactable = false;
                    _radios[i].isOn = true;
                }
                else
                {
                    _radios[i].interactable = true;
                    _radios[i].isOn = false;
                }

            ;
        }
    }
}