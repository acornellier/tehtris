using UnityEngine;
using UnityEngine.UI;

public class SpeedSlider : MonoBehaviour
{
    private Slider slider;

    private void Awake()
    {
        slider = GetComponent<Slider>();
    }

    private void Start()
    {
        if (GameManager.Instance.Mode != GameMode.WatchAi)
        {
            gameObject.SetActive(false);
            return;
        }

        slider.minValue = 0.5f;
        slider.maxValue = 1;
        slider.value = 1 - GameManager.Instance.AiTimeBetweenMoves;

        slider.onValueChanged.AddListener(HandleSliderValueChanged);
    }

    private static void HandleSliderValueChanged(float value)
    {
        GameManager.Instance.AiTimeBetweenMoves = 1 - value;
    }
}
