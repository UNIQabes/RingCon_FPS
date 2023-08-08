using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Events;

public class SliderValueDisp : MonoBehaviour
{
    TextMeshProUGUI _tmp;
    [SerializeField] Slider _slider;

    void Start()
    {
        _tmp = GetComponent<TextMeshProUGUI>();
        _slider.onValueChanged.AddListener(OnSliderValueChanged);
        OnSliderValueChanged(_slider.value);
    }

    public void OnSliderValueChanged(float value)
    {
        _tmp.text = ((int)(value)).ToString();
    }

    
    
}
