using UnityEngine;

public class RoomMusicTrigger : MonoBehaviour
{
    [Header("Audio Source (单个)")]
    [SerializeField] private AudioSource audioSource;   // 场景里放一个 AudioSource

    [Header("Clips")]
    [SerializeField] private AudioClip defaultClip;     // 开场默认音乐
    [SerializeField] private AudioClip triggeredClip;   // 触发后音乐

    [Header("Trigger")]
    [SerializeField] private string playerTag = "Player";

    private void Start()
    {
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
        }

        // 开场播放默认音乐
        PlayClip(defaultClip);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag(playerTag)) return;

        PlayClip(triggeredClip);
    }

    private void PlayClip(AudioClip clip)
    {
        if (audioSource == null || clip == null) return;
        audioSource.loop = true;
        audioSource.Stop();
        audioSource.clip = clip;
        audioSource.Play();
    }
}
