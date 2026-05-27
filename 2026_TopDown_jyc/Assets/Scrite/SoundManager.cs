using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Rendering;
using static UnityEngine.Rendering.DebugUI;

public class Soundmanager : MonoBehaviour
{
    public static Soundmanager Instance;

    public AudioClip clipBGM;

    public AudioClip click;


    AudioSource audioSourceBGM;
    AudioSource audioSource;

    private void Start()
    {
        PlayerData data = GameDataManager.Instance.playerData;

        audioSourceBGM.volume = data.volume;

        if (data.BGM == false)
        {
            audioSourceBGM.volume = 0;
        }

        PlayBGM();
    }
    void Awake()
    {
        Instance = this;

        
        audioSourceBGM = gameObject.AddComponent<AudioSource>();
        audioSource = gameObject.AddComponent<AudioSource>();
        
       
    }

    // Update is called once per frame
    void Update()
    {
            
    }

   

    public void PlayBGM()
    {
        
        audioSourceBGM.clip = clipBGM;
        audioSourceBGM.loop = true;
        audioSourceBGM.playOnAwake = false;

        audioSourceBGM.Play();
    }

    public void OnOffBGM(bool isOn)
    {
        GameDataManager.Instance.playerData.BGM = isOn;

        if (isOn)
        {
            audioSourceBGM.volume = 
            GameDataManager.Instance.playerData.volume;
        }
        else
        {
            audioSourceBGM.volume = 0;
        }

        GameDataManager.Instance.SaveData(
        GameDataManager.Instance.playerData
        );
    }

  
    public void ChangeBGMVolume(float volume)
    {
        audioSourceBGM.volume = volume;

        GameDataManager.Instance.playerData.volume = volume;

        GameDataManager.Instance.SaveData(
        GameDataManager.Instance.playerData
        );

    }
    public void PlaySound()
    {
        audioSource.PlayOneShot(click);
    }

}
