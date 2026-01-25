using UnityEngine;

public class BackgroundMusic : MonoBehaviour
{
    public static BackgroundMusic Instance;

    [Header("Music Clips")]
    public AudioClip normalMusic;
    public AudioClip bossMusic;

    AudioSource audioSource;

    void Awake()
    {
        // Singleton
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        audioSource = GetComponent<AudioSource>();

        PlayNormalMusic();
    }

    public void PlayNormalMusic()
    {
        if (audioSource.clip == normalMusic) return;

        audioSource.clip = normalMusic;
        audioSource.loop = true;
        audioSource.Play();
    }

    public void PlayBossMusic()
    {
        if (audioSource.clip == bossMusic) return;

        audioSource.clip = bossMusic;
        audioSource.loop = true;
        audioSource.Play();
    }
}
