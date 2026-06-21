using UnityEngine;
using UnityEngine.EventSystems; // 마우스 이벤트를 받기 위해 필수!
using UnityEngine.UI;

public class UI_InventorySlot : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    private itemso slotItemData;

    [Header("장착 표시 UI")]
    // ★ [추가]: 유니티 인스펙터에서 슬롯 프리팹 자식으로 만든 '장착 체크 이미지(또는 테두리)'를 연결하세요!
    [SerializeField] private GameObject equipMark;

    // 이 슬롯이 어떤 아이템 데이터를 품고 있는지 설정하는 함수
    public void SetupSlot(itemso data)
    {
        slotItemData = data;

        // ★ [추가]: 슬롯이 새로 그려질 때, 내가 현재 장착한 아이템이면 체크 표시를 켜고 아니면 끕니다.
        UpdateEquipStatusUI();
    }

    // 🎯 3. 아이템 설명 툴팁 (마우스 올렸을 때)
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (slotItemData == null) return;

        if (UI_Tooltip.Instance != null)
        {
            UI_Tooltip.Instance.ShowTooltip(slotItemData.itemName, slotItemData.itemDescription);
        }
    }

    // 마우스가 슬롯을 벗어났을 때 (툴팁 끄기)
    public void OnPointerExit(PointerEventData eventData)
    {
        if (UI_Tooltip.Instance != null)
        {
            UI_Tooltip.Instance.HideTooltip();
        }
    }

    // 클릭 이벤트 (장착 및 버리기)
    public void OnPointerClick(PointerEventData eventData)
    {
        if (slotItemData == null) return;

        // 🖱️ 마우스 좌클릭 -> 1. 아이템 장착 (Equipment일 때만)
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            if (slotItemData.itemType == ItemType.Equipment)
            {
                ExecuteEquipment();
            }
        }
        // 🖱️ 마우스 우클릭 -> 4. 아이템 파괴/버리기
        else if (eventData.button == PointerEventData.InputButton.Right)
        {
            ExecuteDestroy();
        }
    }

    // ⚔️ 1. 장착 및 해제 기능 토글 완벽 연동
    private void ExecuteEquipment()
    {
        PlayerController player = FindFirstObjectByType<PlayerController>();

        if (player != null)
        {
            // ───────────────────────────────────────────────────────────
            // ★ [수정]: 플레이어가 현재 장착 중인 아이템이 "이 슬롯의 아이템"인지 검사
            // ───────────────────────────────────────────────────────────
            if (player.GetEquippedItem() == slotItemData)
            {
                // 이미 장착된 상태라면 해제 신호 송신
                player.UnequipItem();
            }
            else
            {
                // 장착되지 않은 상태라면 장착 신호 송신
                player.EquipItem(slotItemData);
            }

            // 장착 상태가 변했으므로 씬에 있는 모든 슬롯들의 장착 UI 불빛을 새로고침합니다.
            UI_InventorySlot[] allSlots = FindObjectsByType<UI_InventorySlot>(FindObjectsSortMode.None);
            foreach (var slot in allSlots)
            {
                slot.UpdateEquipStatusUI();
            }
        }
        else
        {
            Debug.LogError("🚨 [에러] 씬에 PlayerController를 가진 오브젝트가 없습니다!");
        }
    }

    // 🗑️ 4. 버리기/파괴 기능 구현
    private void ExecuteDestroy()
    {
        // ★ [추가]: 장착 중인 아이템을 파괴할 수도 있으므로 안전하게 플레이어에게 해제 신호를 먼저 보냅니다.
        PlayerController player = FindFirstObjectByType<PlayerController>();
        if (player != null && player.GetEquippedItem() == slotItemData)
        {
            player.UnequipItem();
        }

        Debug.Log($"🗑️ {slotItemData.itemName}을(를) 파괴했습니다.");

        if (UI_Tooltip.Instance != null)
        {
            UI_Tooltip.Instance.HideTooltip();
        }

        // 데이터 매니저 리스트에서 삭제
        GameDataManager.Instance.playerData.collectedItems.Remove(slotItemData.itemName);
        GameDataManager.Instance.SaveData(GameDataManager.Instance.playerData);

        // 인벤토리 UI 전체 새로고침
        FindFirstObjectByType<UI_ImageInventory>()?.UpdateInventoryUI();
    }

    // ───────────────────────────────────────────────────────────
    // ★ [추가]: 현재 플레이어 장착 상태와 비교해서 내 슬롯의 불빛(체크)을 켜고 끄는 함수
    // ───────────────────────────────────────────────────────────
    public void UpdateEquipStatusUI()
    {
        if (equipMark == null) return;

        PlayerController player = FindFirstObjectByType<PlayerController>();
        if (player != null && slotItemData != null)
        {
            // 플레이어가 장착한 아이템 정보와 내 슬롯의 정보가 같다면 체크 활성화!
            bool isCurrentEquipped = (player.GetEquippedItem() == slotItemData);
            equipMark.SetActive(isCurrentEquipped);
        }
        else
        {
            equipMark.SetActive(false);
        }
    }
}