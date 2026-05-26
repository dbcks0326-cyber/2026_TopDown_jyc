using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Rendering;

public class Soundmanager : MonoBehaviour
{
    public static Soundmanager Instance;

    public AudioClip clipBGM;
   

   
    AudioSource audioSourceBGM;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Instance = this;

      
        audioSourceBGM = gameObject.AddComponent<AudioSource>();
      
    }

    // Update is called once per frame
    void Update()
    {
        
    }

   

    public void PlayBGM()
    {
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

    
}
