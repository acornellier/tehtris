using UnityEngine;
using UnityEngine.UI;

public class VolumeControl : MonoBehaviour
{
    [SerializeField] private AudioManager.AudioChannel channel = AudioManager.AudioChannel.Master;
    [SerializeField] private Slider slider;

    private void Awake()
    {
        slider.onValueChanged.AddListener(HandleSliderValueChanged);
    }

    private void Start()
    {
        slider.value = AudioManager.GetChannelValue(channel);
    }

    private void HandleSliderValueChanged(float value)
    {
        AudioManager.Instance.SetVolume(channel, value);
    }
}
