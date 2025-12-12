using UnityEngine;

public class RoomMusicTrigger : MonoBehaviour
{
    public AudioSource roomAudioSource;
    public string playerTag = "Player";

    private void OnTriggerEnter2D(Collider2D other)
    {

        if (other.CompareTag(playerTag))
        {
            if (roomAudioSource != null && !roomAudioSource.isPlaying)
            {
                roomAudioSource.Play();
            }
        }
    }
    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag(playerTag))
        {
            if (roomAudioSource != null && roomAudioSource.isPlaying)
            {
                roomAudioSource.Stop();
            }
        }
    }
}