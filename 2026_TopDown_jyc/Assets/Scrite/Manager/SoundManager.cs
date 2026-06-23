using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Rendering;
using static UnityEngine.Rendering.DebugUI;

public class Soundmanager : MonoBehaviour
{
    public static Soundmanager Instance;

    public AudioClip clipBGM;

    public AudioClip click;

    public AudioClip Text;
    AudioSource audioSourceFootstep;

    public AudioClip dashSound;
    AudioSource audioSourceBGM;
    AudioSource audioSource;
    AudioSource audioSourceText;
    [SerializeField] private AudioClip[] footstepClips;
    [SerializeField] private AudioClip bossImpactClip;

    private void Start()
    {
        PlayerData data = GameDataManager.Instance.playerData;

        audioSourceBGM.volume = data.volume;

        if (data.BGM == false)
        {
            audioSourceBGM.volume = 0;
        }

        audioSource.volume = data.volume;
        audioSourceText.volume = data.volume;
        audioSourceFootstep.volume = data.volume;

        PlayBGM();
    }


    void Awake()
    {
        Instance = this;

        
        audioSourceBGM = gameObject.AddComponent<AudioSource>();
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSourceText = gameObject.AddComponent<AudioSource>();
        audioSourceFootstep = gameObject.AddComponent<AudioSource>();

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

   
    public void ChangeTextVolume(float volume)
    {
        audioSource.volume = volume;
        audioSourceText.volume = volume;
        audioSourceFootstep.volume = volume;

        GameDataManager.Instance.playerData.volume = volume;

        GameDataManager.Instance.SaveData(
        GameDataManager.Instance.playerData
        );

    }
    public void PlaySound()
    {
        audioSource.PlayOneShot(click);
    }


    public void TextSound()
    {
        audioSource.PlayOneShot(Text);
    }

    public void PlayFootstep()
    {
        if (footstepClips == null || footstepClips.Length == 0)
            return;

        int randomIndex =
            Random.Range(0, footstepClips.Length);

        audioSourceFootstep.PlayOneShot(
            footstepClips[randomIndex]);
    }

    public void PlayBossImpact()
    {
        if (bossImpactClip == null)
            return;

        audioSource.PlayOneShot(bossImpactClip);
    }

    public void PlayDashSound()
    {
        if (dashSound != null)
        {
            audioSource.PlayOneShot(dashSound);
        }
    }
}
