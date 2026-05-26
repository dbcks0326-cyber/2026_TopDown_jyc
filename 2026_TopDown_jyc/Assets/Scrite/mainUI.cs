using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class TitleManager : MonoBehaviour
{

    public GameObject HelpPanel;

    public Toggle bgmToggle;

    public Slider bgmSlider;


    public void GameStart()
    {
        SceneManager.LoadScene("Stage_1");
    }
    private void Awake()
    {
        bgmToggle.onValueChanged.AddListener(OnBGMToggleChange);
        bgmSlider.onValueChanged.AddListener(OnBGMSliderChange);
       
    }
    public void OpenPanel()
    {
        HelpPanel.SetActive(true);

    }
    public void ClosePanel()
    {
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
        Soundmanager.Instance.OnOffBGM(isOn);

    }
    private void OnBGMSliderChange(float volume)
    {
        Soundmanager.Instance.ChangeBGMVolume(volume);
    }
}