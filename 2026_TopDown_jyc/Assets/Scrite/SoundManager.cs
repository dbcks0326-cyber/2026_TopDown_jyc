using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Rendering;

public class Soundmanager : MonoBehaviour
{
    public static Soundmanager Instance;

    public AudioClip clipBGM;

    public AudioClip click;


    AudioSource audioSourceBGM;
    AudioSource audioSource;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        Instance = this;

        
        audioSourceBGM = gameObject.AddComponent<AudioSource>();
        audioSource = gameObject.AddComponent<AudioSource>();
        PlayBGM();
    }

    // Update is called once per frame
    void Update()
    {
            
    }

   

    public void PlayBGM()
    {
        Debug.Log("BGM 재생");
        audioSourceBGM.clip = clipBGM;
        audioSourceBGM.loop = true;
        audioSourceBGM.Play();
    }

    public void OnOffBGM(bool isOn)
    {
        if (isOn)
        {
            audioSourceBGM.volume = 1;
        }
        else
        {
            audioSourceBGM.volume = 0;
        }
    }

  
    public void ChangeBGMVolume(float volume)
    {
        audioSourceBGM.volume = volume;
    }
    public void PlaySound()
    {
        audioSource.PlayOneShot(click);
    }

}
