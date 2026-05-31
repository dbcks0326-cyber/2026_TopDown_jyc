using TMPro;
using UnityEngine;

public class CoinUI : MonoBehaviour
{
    // 코인 숫자를 표시할 TextMeshPro 텍스트 컴포넌트
    private TextMeshProUGUI coinText;

    void Awake()
    {
        // 이 스크립트가 붙은 오브젝트의 TextMeshPro 컴포넌트를 가져옴
        coinText = GetComponent<TextMeshProUGUI>();
    }

    void Update()
    {
        if (GameDataManager.Instance != null && GameDataManager.Instance.playerData != null)
        {
            // 매 프레임마다 세이브 데이터의 코인 값을 가져와서 텍스트로 표시
            coinText.text = GameDataManager.Instance.playerData.coin.ToString();
        }
    }
}