using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    private Slider _slider;

    void Awake()
    {
        _slider = GetComponentInChildren<Slider>();
    }

    public void SetValue(float value)
    {
        value = Mathf.Clamp01(value);
        _slider.value = value;
    }

    public void SetVisibility(bool visible)
    {
        foreach(Image image in transform.GetComponentsInChildren<Image>())
        {
            image.enabled = visible;
        }
    }
}
