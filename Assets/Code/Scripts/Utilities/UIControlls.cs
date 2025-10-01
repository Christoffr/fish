using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIControlls : MonoBehaviour
{
    [SerializeField] private BoidsSettingsOS _settings;
    [SerializeField] private Slider _instanceSlider;
    [SerializeField] private TMP_Text _instnaceSliderText;

    private void Start()
    {
        _instnaceSliderText.text = $"Fish:{_settings.InstanceCount.ToString()}";
        _instanceSlider.value = _settings.InstanceCount;
    }

    private void Update()
    {
        _instnaceSliderText.text = $"Fish:{_settings.InstanceCount.ToString()}";
        _settings.InstanceCount = (int)_instanceSlider.value;
    }

}
