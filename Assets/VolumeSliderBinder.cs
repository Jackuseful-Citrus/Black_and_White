using UnityEngine;
using UnityEngine.UI;

public class VolumeSliderBinder : MonoBehaviour
{
    [SerializeField] private Slider slider;

    void Awake()
    {
        if (!slider) slider = GetComponent<Slider>();

        // 确保场景里有全局管理器（如果你忘了放，也能自动补一个）
        if (GlobalAudioSettings.Instance == null)
        {
            var go = new GameObject("GlobalAudio");
            go.AddComponent<GlobalAudioSettings>();
        }

        // 初始化 slider 显示为当前音量
        slider.SetValueWithoutNotify(GlobalAudioSettings.Instance.Volume);

        // 监听 slider 改动
        slider.onValueChanged.AddListener(v =>
        {
            GlobalAudioSettings.Instance.SetVolume(v);
        });
    }
}
