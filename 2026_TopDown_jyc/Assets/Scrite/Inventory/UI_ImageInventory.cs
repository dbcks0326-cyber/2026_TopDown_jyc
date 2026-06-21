using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class UI_ImageInventory : MonoBehaviour
{
    [Header("설정 레이아웃")]
    [SerializeField] private Transform slotParent;
    [SerializeField] private GameObject slotPrefab;

    [Header("게임 내 전체 아이템 스크립터블 오브젝트 목록")]
    [SerializeField] public List<itemso> allItemSOList;

    private void OnEnable()
    {
        UpdateInventoryUI();
    }

    private void OnDisable()
    {
        if (UI_Tooltip.Instance != null)
        {
            UI_Tooltip.Instance.HideTooltip();
        }
    }
    public void UpdateInventoryUI()
    {
        if (slotParent == null || slotPrefab == null) return;

        // 1. 기존 슬롯 삭제
        foreach (Transform child in slotParent)
        {
            Destroy(child.gameObject);
        }

        if (GameDataManager.Instance == null || GameDataManager.Instance.playerData.collectedItems == null) return;

        // 2. 슬롯 생성 loop
        foreach (string ownedItemName in GameDataManager.Instance.playerData.collectedItems)
        {
            itemso matchedData = allItemSOList.Find(item => item.itemName == ownedItemName);

            if (matchedData != null && matchedData.itemIcon != null)
            {
                if (matchedData.itemType == ItemType.Currency)
                {
                    continue;
                }

                // 슬롯 생성
                GameObject newSlot = Instantiate(slotPrefab, slotParent);

                // ───────────────────────────────────────────────────────────
                // ★ [변경]: 부모와 자식을 모두 뒤져서 UI_InventorySlot 센서를 확실하게 찾습니다!
                // ───────────────────────────────────────────────────────────
                UI_InventorySlot slotScript = newSlot.GetComponentInChildren<UI_InventorySlot>();
                if (slotScript != null)
                {
                    slotScript.SetupSlot(matchedData); // 센서에 아이템 데이터 완벽 주입!
                }
                else
                {
                    Debug.LogError("🚨 [에러] 생성된 슬롯 프리팹에서 UI_InventorySlot 스크립트를 찾을 수 없습니다!");
                }

                // 자식 오브젝트인 'IconImage'를 찾아 Sprite를 꽂아줍니다.
                Image iconComponent = newSlot.transform.Find("IconImage")?.GetComponent<Image>();
                if (iconComponent == null)
                {
                    // 만약 구조가 다를 걸 대비해 2차로 자식 전체에서 찾아봅니다.
                    iconComponent = newSlot.GetComponentInChildren<Image>();
                }

                if (iconComponent != null)
                {
                    iconComponent.sprite = matchedData.itemIcon;
                    iconComponent.enabled = true;
                }
            }
        }
    }


}