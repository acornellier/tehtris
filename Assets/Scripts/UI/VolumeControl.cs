﻿using UnityEngine;
using UnityEngine.UI;

public class VolumeControl : MonoBehaviour
{
    private Slider slider;
    [SerializeField] private AudioManager.AudioChannel channel = AudioManager.AudioChannel.Master;

    private void Awake()
    {
        slider = GetComponent<Slider>();
        slider.onValueChanged.AddListener(HandleSliderValueChanged);
    }

    private void Start()
    {
        slider.minValue = 0;
        slider.maxValue = 1;
        slider.value = AudioManager.GetChannelValue(channel);
    }

    private void HandleSliderValueChanged(float value)
    {
        AudioManager.Instance.SetVolume(channel, value);
    }
}
