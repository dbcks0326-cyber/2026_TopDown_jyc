using TMPro;
using UnityEngine;
using UnityEngine.UI; // ★ 중요: Image 컴포넌트를 제어하기 위해 필수!

public class CoinUI : MonoBehaviour
{
    // 코인 숫자를 표시할 TextMeshPro 컴포넌트
    private TextMeshProUGUI coinText;

    // ★ 유니티 인스펙터에서 하트 체력바 Image 오브젝트를 드래그 앤 드롭할 칸
    [Header("하트 체력바 이미지 연결")]
    [SerializeField] private Image hpBarImage;

    void Awake()
    {
        // 이 스크립트가 붙은 오브젝트에서 코인 텍스트를 가져옴
        coinText = GetComponent<TextMeshProUGUI>();
    }

    void Update()
    {
        // 싱글톤 매니저와 데이터가 존재하는지 안전하게 체크
        if (GameDataManager.Instance != null && GameDataManager.Instance.playerData != null)
        {
            // 1. 코인 텍스트 실시간 갱신
            if (coinText != null)
            {
                coinText.text = GameDataManager.Instance.playerData.coin.ToString();
            }

            // 2. 하트 체력바 이미지 Fill Amount 조절 (100에서 90, 80 되듯이 툭툭 깎임)
            if (hpBarImage != null)
            {
                float targetHp = GameDataManager.Instance.playerData.currentHp;
                float maxHp = GameDataManager.Instance.playerData.maxHp;

                // ★ 핵심: Fill Amount는 0(비어있음) ~ 1(가득참) 사이의 비율을 원하므로 나눗셈을 해줍니다.
                // 체력이 10씩 깎이면 fillAmount가 0.1씩 탁, 탁 줄어들면서 게이지가 깎입니다!
                hpBarImage.fillAmount = targetHp / maxHp;
            }
        }
    }
}