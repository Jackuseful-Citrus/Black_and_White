using UnityEngine;

public class AudioControl1 : MonoBehaviour
{
    public AudioClip backgroundClip;
    private AudioSource audioSource;

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
    }

    void Start()
    {
        if (backgroundClip != null)
        {
            audioSource.clip = backgroundClip;
            audioSource.loop = true;
            audioSource.Play();
        }
        else
        {
            Debug.LogError("Error: 请在 Inspector 面板中设置 backgroundClip！");
        }
    }
    public void StopMusic()
    {
        if (audioSource.isPlaying)
        {
            audioSource.Stop();
        }
    }
}