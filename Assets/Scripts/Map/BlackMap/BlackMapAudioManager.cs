using UnityEngine;

/// <summary>
/// BlackMap 音频管理：三阶段播放不同 BGM。
/// - 场景开始播放 Stage1Clip。
/// - Stage2StartTrigger 调用 PlayStage2。
/// - 拾取碎片后调用 PlayEnding。
/// </summary>
[RequireComponent(typeof(AudioSource))]
public class BlackMapAudioManager : MonoBehaviour
{
    public static BlackMapAudioManager Instance { get; private set; }

    [Header("Clips")]
    [SerializeField] private AudioClip stage1Clip;
    [SerializeField] private AudioClip stage2Clip;
    [SerializeField] private AudioClip endingClip;

    private AudioSource source;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        source = GetComponent<AudioSource>();
        source.loop = true;
    }

    private void Start()
    {
        PlayStage1();
    }

    public void PlayStage1() => PlayClip(stage1Clip);
    public void PlayStage2() => PlayClip(stage2Clip);
    public void PlayEnding() => PlayClip(endingClip);
    public void ResetBgm() => PlayStage1();

    private void PlayClip(AudioClip clip)
    {
        if (clip == null || source == null) return;
        if (source.clip == clip && source.isPlaying) return;
        source.Stop();
        source.clip = clip;
        source.Play();
    }
}
