using UnityEngine;

public class GlobalAudioSettings : MonoBehaviour
{
    public static GlobalAudioSettings Instance { get; private set; }

    private const string PREF_KEY = "GLOBAL_VOLUME";
    [Range(0f, 1f)] public float defaultVolume = 0.8f;

    public float Volume { get; private set; }

    void Awake()
    {
        // 防止多场景重复生成
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        // 读取并应用
        Volume = Mathf.Clamp01(PlayerPrefs.GetFloat(PREF_KEY, defaultVolume));
        AudioListener.volume = Volume;
    }

    public void SetVolume(float v)
    {
        Volume = Mathf.Clamp01(v);
        AudioListener.volume = Volume;
        PlayerPrefs.SetFloat(PREF_KEY, Volume);
        PlayerPrefs.Save();
    }
}

