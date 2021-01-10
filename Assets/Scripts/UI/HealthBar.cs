using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    private Slider _slider;

    // Získá odkaz na Slider
    void Awake()
    {
        _slider = GetComponentInChildren<Slider>();
    }

    // Změní hodnotu Slideru
    public void SetValue(float value)
    {
        value = Mathf.Clamp01(value);
        _slider.value = value;
    }

    // Mění viditelnost Slideru
    public void SetVisibility(bool visible)
    {
        foreach(Image image in transform.GetComponentsInChildren<Image>())
        {
            image.enabled = visible;
        }
    }
}
