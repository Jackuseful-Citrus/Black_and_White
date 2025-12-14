using UnityEngine;
using UnityEngine.UI;

public class GlobalVolumeSimple : MonoBehaviour
{
    [SerializeField] private Slider slider;
    [SerializeField] private float defaultVolume = 0.8f;

    private const string PREF_KEY = "GLOBAL_VOLUME";

    void Awake()
    {
        if (!slider) slider = GetComponent<Slider>();

        float v = PlayerPrefs.GetFloat(PREF_KEY, defaultVolume);
        v = Mathf.Clamp01(v);

        slider.SetValueWithoutNotify(v);
        AudioListener.volume = v;

        slider.onValueChanged.AddListener(OnSliderChanged);
    }

    private void OnSliderChanged(float v)
    {
        v = Mathf.Clamp01(v);
        AudioListener.volume = v;
        PlayerPrefs.SetFloat(PREF_KEY, v);
        PlayerPrefs.Save();
    }
}
