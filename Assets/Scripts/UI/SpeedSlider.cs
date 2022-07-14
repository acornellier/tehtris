using UnityEngine;
using UnityEngine.UI;

public class SpeedSlider : MonoBehaviour
{
    private Slider slider;

    private void Awake()
    {
        slider = GetComponent<Slider>();
        slider.onValueChanged.AddListener(HandleSliderValueChanged);
    }

    private void Start()
    {
        slider.minValue = 0;
        slider.maxValue = 1;
        slider.value = GameManager.Instance.AiTimeBetweenMoves;
    }

    private static void HandleSliderValueChanged(float value)
    {
        GameManager.Instance.AiTimeBetweenMoves = 1 - value;
    }
}
