using System.Globalization;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Examples.Scripts.UI
{
    public class ControlButtons : MonoBehaviour
    {
        [SerializeField] public ButtonHoldAction upButton;
        [SerializeField] public ButtonHoldAction downButton;
        [SerializeField] public ButtonHoldAction leftButton;
        [SerializeField] public ButtonHoldAction rightButton;
        [SerializeField] public Slider slider;
        [SerializeField] public TextMeshProUGUI speedText;

        private void Awake()
        {
            speedText.text = slider.value.ToString(CultureInfo.InvariantCulture);
            slider.onValueChanged.AddListener(speed =>
            {
                speedText.text = speed.ToString(CultureInfo.InvariantCulture);
            });
        }
    }
}