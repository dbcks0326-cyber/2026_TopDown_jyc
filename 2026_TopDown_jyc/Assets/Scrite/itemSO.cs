using UnityEngine;

// 💡 아이템의 종류를 정의하는 열거형 (인벤토리에서 코인을 걸러내기 위해 사용!)
public enum ItemType
{
    Equipment,   // 장착 아이템 (무기, 방어구 등)
    Quest,       // 퀘스트/키 아이템 (보스방 열쇠 등)
    Consumable,  // 소비 아이템 (물약 등)
    Currency     // 코인/재화 (★ 인벤토리 이미지 생성에서 제외될 태그)
}

[CreateAssetMenu(fileName = "itemso", menuName = "Game/Create item")]
public class itemso : ScriptableObject
{
    [Header("Score Value")]
    public int point = 10;
    public string itemName = string.Empty;
    public int price;

    [Header("Item Visual")]
    public Sprite itemIcon;

    // ───────────────────────────────────────────────────────────
    // ★ 추가: 인벤토리 기능 확장을 위한 종류 설정 및 설명란
    // ───────────────────────────────────────────────────────────
    [Header("Item Settings")]
    public ItemType itemType;

    [TextArea]
    public string itemDescription; // 툴팁 UI에 띄워줄 상세 설명


    [Header("장비 스탯 설정")]
    public float speedBonus;    // 장착 시 이동 속도 증가량 (예: 2.0)
    public int attackBonus;     // 장착 시 공격력 증가량 (예: 5)
}