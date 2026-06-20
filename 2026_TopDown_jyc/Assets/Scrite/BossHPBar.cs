using UnityEngine;
using UnityEngine.UI; // ★ 중요: Image 컴포넌트를 사용하기 위해 필수!

public class BossHPBar : MonoBehaviour
{
    public static BossHPBar Instance { get; private set; }

    [Header("보스 HP UI 전체 부모 오브젝트")]
    [SerializeField] private GameObject hpBarGroup; // 보스 나오기 전까지 통째로 숨겨둘 그룹 (캔버스나 부모 오브젝트)

    [Header("실제 게이지 이미지 연결")]
    [SerializeField] private Image bossHpImage; // ★ 아까 세팅한 BossHP_Fill 이미지를 여기에 드래그앤드롭합니다!

    // ───────────────────────────────────────────────────────────
    // ★ [여기에만 추가]: 맵에 미리 배치해 둔 문 오브젝트 연결용 변수
    // ───────────────────────────────────────────────────────────
    [Header("보스방 문 설정")]
    [SerializeField] private GameObject bossRoomDoor; // 맵에 배치된 문(벽) 게임 오브젝트

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        // 게임 시작할 땐 보스 피통 숨기기
        if (hpBarGroup != null) hpBarGroup.SetActive(false);

        // 🎮 [추가]: 게임 시작할 때는 보스방 문을 열어둡니다(꺼둡니다).
        if (bossRoomDoor != null) bossRoomDoor.SetActive(false);
    }

    // 보스가 스폰될 때 호출
    public void ShowHPBar()
    {
        if (hpBarGroup != null) hpBarGroup.SetActive(true);
        if (bossHpImage != null) bossHpImage.fillAmount = 1f; // 처음엔 풀피(100%)로 채우기

        // ───────────────────────────────────────────────────────────
        // ★ [추가]: HP바 켜질 때 문 오브젝트 활성화 (길 막기)
        // ───────────────────────────────────────────────────────────
        if (bossRoomDoor != null)
        {
            bossRoomDoor.SetActive(true);
            Debug.Log("🔒 보스방 진입: 문이 활성화되어 닫힙니다.");
        }
    }

    // 대미지 입을 때 호출 (CoinUI와 똑같은 나눗셈 공식!)
    public void UpdateHP(float currentHP, float maxHP)
    {
        if (bossHpImage != null && maxHP > 0)
        {
            // ★ 핵심: fillAmount는 0f(0%) ~ 1f(100%) 사이의 소수점을 쓰므로 비율 계산을 해줍니다.
            bossHpImage.fillAmount = currentHP / maxHP;
        }
    }

    // 보스가 죽었을 때 호출
    public void HideHPBar()
    {
        if (hpBarGroup != null) hpBarGroup.SetActive(false);

        // ───────────────────────────────────────────────────────────
        // ★ [추가]: 보스 죽어서 HP바 꺼질 때 문 오브젝트 비활성화 (길 열기)
        // ───────────────────────────────────────────────────────────
        if (bossRoomDoor != null)
        {
            bossRoomDoor.SetActive(false);
            Debug.Log("🔓 보스 처치 성공: 문이 비활성화되어 열립니다.");
        }
    }
}