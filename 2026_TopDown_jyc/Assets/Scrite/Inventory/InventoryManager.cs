using UnityEngine;
using UnityEngine.InputSystem; // 신규 Input System 사용

public class InventoryManager : MonoBehaviour
{
    [Header("인벤토리 UI 창 오브젝트")]
    [SerializeField] private GameObject inventoryWindow;

    private UI_ImageInventory imageInventory;

    private void Awake()
    {
        // 인벤토리 창 안에 붙어있는 이미지 갱신 스크립트 가져오기
        if (inventoryWindow != null)
        {
            imageInventory = inventoryWindow.GetComponent<UI_ImageInventory>();
        }
    }

    private void Start()
    {
        // 💡 게임 시작할 때는 인벤토리 창을 꺼둡니다.
        if (inventoryWindow != null)
        {
            inventoryWindow.SetActive(false);
        }
    }

    private void Update()
    {
        // ⌨️ 키보드 'I' 키가 이번 프레임에 눌렸는지 체크
        if (Keyboard.current != null && Keyboard.current.iKey.wasPressedThisFrame)
        {
            ToggleInventory();
        }
    }

    // 🔄 인벤토리를 켜고 끄는 함수
    public void ToggleInventory()
    {
        if (inventoryWindow == null) return;

        // 현재 상태의 반대로 뒤집기 ( 켜져있으면 끄고, 꺼져있으면 켜기 )
        bool isActive = !inventoryWindow.activeSelf;
        inventoryWindow.SetActive(isActive);

        // 창이 켜질 때 최신 아이템 상태로 UI를 강제 새로고침!
        if (isActive && imageInventory != null)
        {
            imageInventory.UpdateInventoryUI();
        }

        // 💡 [선택 사항]: 인벤토리가 켜져 있을 때는 플레이어가 못 움직이게 막고 싶다면?
        PlayerController player = FindFirstObjectByType<PlayerController>();
        if (player != null)
        {
            // 인벤토리가 켜지면 canMove = false, 꺼지면 true
            player.canMove = !isActive;

            // 멈췄을 때 움직이던 가속도도 0으로 초기화
            Rigidbody2D rb = player.GetComponent<Rigidbody2D>();
            if (rb != null) rb.linearVelocity = Vector2.zero;
        }
    }
}