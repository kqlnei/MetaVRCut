using UnityEngine;

public class PlayBGM : MonoBehaviour
{
    private AudioSource audioSource;

    // Start is called before the first frame update
    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        audioSource.Play();  // BGM‚ğÄ¶
    }
}
