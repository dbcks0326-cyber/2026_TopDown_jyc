using UnityEngine;
using TMPro;

public class UI_Tooltip : MonoBehaviour
{
    // 싱글톤으로 만들어서 어디서든 쉽게 접근할 수 있게 합니다.
    public static UI_Tooltip Instance;

    [SerializeField] private GameObject tooltipWindow; // 설명창 패널 오브젝트
    [SerializeField] private TextMeshProUGUI nameText;  // 아이템 이름 텍스트
    [SerializeField] private TextMeshProUGUI descText;  // 아이템 설명 텍스트

    private void Awake()
    {
        if (Instance == null) Instance = this;
        HideTooltip(); // 시작할 때는 설명창을 숨깁니다.
    }

    private void Update()
    {
        // 설명창이 켜져 있다면 마우스 커서 위치를 실시간으로 따라다니게 만듭니다.
        /*  if (tooltipWindow.activeSelf)
           {
                ❌ 기존 구형 코드: transform.position = Input.mousePosition;

                 [변경]: 새로운 인풋 시스템 전용 마우스 위치 문법으로 교체!
             if (UnityEngine.InputSystem.Mouse.current != null)
             {
                  Vector2 mousePos = UnityEngine.InputSystem.Mouse.current.position.ReadValue();
                  transform.position = mousePos;
              }
          }
       }*/
    }

    // 툴팁 보여주기 함수
    public void ShowTooltip(string itemName, string itemDescription)
    {
        tooltipWindow.SetActive(true);
        nameText.text = itemName;
        descText.text = itemDescription;
    }

    // 툴팁 숨기기 함수
    public void HideTooltip()
    {
        tooltipWindow.SetActive(false);
    }
}