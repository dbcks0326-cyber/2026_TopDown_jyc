using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
// ───────────────────────────────────────────────────────────
// ★ [중요]: New Input System 패키지를 사용하기 위해 필수 추가!
// ───────────────────────────────────────────────────────────
using UnityEngine.InputSystem;

public class TitleManager : MonoBehaviour
{
    public GameObject HelpPanel;
    public Toggle bgmToggle;
    public Slider bgmSlider;
    public Slider TextSlider;

    private void Awake()
    {
        bgmToggle.onValueChanged.AddListener(OnBGMToggleChange);
        bgmSlider.onValueChanged.AddListener(OnBGMSliderChange);
        TextSlider.onValueChanged.AddListener(OnTextSliderChange);
    }

    // ───────────────────────────────────────────────────────────
    // ⌨️ New Input System 방식의 ESC 키 입력 감지
    // ───────────────────────────────────────────────────────────
    private void Update()
    {
        // Keyboard.current가 존재하고, Escape 키가 이번 프레임에 눌렸는지 검사
        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            // HelpPanel이 켜져 있다면 끄고, 꺼져 있다면 켭니다 (토글 기능)
            if (HelpPanel.activeSelf)
            {
                ClosePanel();
            }
            else
            {
                OpenPanel();
            }
        }
    }

    public void GameStart()
    {
        SceneManager.LoadScene("Stage_0");
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