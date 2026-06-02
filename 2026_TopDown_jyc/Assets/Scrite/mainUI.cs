using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class TitleManager : MonoBehaviour
{

    public GameObject HelpPanel;

    public Toggle bgmToggle;

    public Slider bgmSlider;

    public Slider TextSlider;


    public void GameStart()
    {
        
 
        SceneManager.LoadScene("Stage_0");
        
    }
    private void Awake()
    {
        bgmToggle.onValueChanged.AddListener(OnBGMToggleChange);
        bgmSlider.onValueChanged.AddListener(OnBGMSliderChange);
        TextSlider.onValueChanged.AddListener(OnTextSliderChange);

    }
    public void OpenPanel()
    {
        Soundmanager.Instance.PlaySound();
        HelpPanel.SetActive(true);

    }
    public void ClosePanel()
    {
        Soundmanager.Instance.PlaySound();
        HelpPanel.SetActive(false);
    }
    public void QuitGame()
    {
        Application.Quit();
    }
    public void mainScene()
    {
        SceneManager.LoadScene("Main");
    }
    private void OnBGMToggleChange(bool isOn)
    {
        Soundmanager.Instance.PlaySound();
        Soundmanager.Instance.OnOffBGM(isOn);

    }
    private void OnBGMSliderChange(float volume)
    {
        Soundmanager.Instance.ChangeBGMVolume(volume);
    }
    private void OnTextSliderChange(float volume)
    {
        Soundmanager.Instance.ChangeTextVolume(volume);
    }
}