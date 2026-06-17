using UnityEngine;

public class itemOB : MonoBehaviour
{
    [SerializeField] itemso data;

    [Header("둥둥 효과 설정")]
    [SerializeField] private float floatSpeed = 3f;
    [SerializeField] private float floatAmplitude = 0.01f;

    [Header("자석 효과 설정")]
    [SerializeField] private float magnetRadius = 3.5f;   // 플레이어를 감지할 자석 범위
    [SerializeField] private float magnetSpeed = 2f;      // 처음 끌려가기 시작하는 속도
    [SerializeField] private float acceleration = 8f;     // 끌려가면서 점점 빨라지는 가속도

    private Vector3 startPosition;
    private Transform playerTransform;
    private Rigidbody2D rb;
    private bool isFlying = false;                        // 플레이어에게 끌려가는 중인가?

    void Start()
    {
        Destroy(gameObject, 30f);
        startPosition = transform.localPosition;

        // 리지드바디가 있다면 가져오고, 맵에서 플레이어를 미리 찾아둡니다.
        rb = GetComponent<Rigidbody2D>();
        GameObject playerGO = GameObject.FindWithTag("Player");
        if (playerGO != null)
        {
            playerTransform = playerGO.transform;
        }
    }

    void Update()
    {
        if (playerTransform == null)
        {
            HandleFloating();
            return;
        }

        // 플레이어와 아이템 사이의 거리 측정
        float distance = Vector2.Distance(transform.position, playerTransform.position);

        // 자석 범위 안에 들어오면 날아가는 상태 활성화
        if (distance <= magnetRadius)
        {
            isFlying = true;
        }

        if (isFlying)
        {
            // 1. 플레이어 방향 벡터 구하기
            Vector2 direction = ((Vector2)playerTransform.position - (Vector2)transform.position).normalized;

            // 2. 가속도를 더해 점점 빨라지게 만듦
            magnetSpeed += acceleration * Time.deltaTime;

            // 3. 물리(Rigidbody2D) 유무에 따라 이동 처리
            if (rb != null)
            {
                rb.linearVelocity = direction * magnetSpeed;
            }
            else
            {
                transform.Translate(direction * magnetSpeed * Time.deltaTime, Space.World);
            }
        }
        else
        {
            // 자석이 발동하기 전까지만 기존의 둥둥 효과를 줍니다.
            HandleFloating();
        }
    }

    // 기존의 둥둥 떠 있는 로직 (코드가 복잡해지지 않게 메서드로 분리)
    private void HandleFloating()
    {
        float newY = startPosition.y + Mathf.Sin(Time.time * floatSpeed) * floatAmplitude;
        transform.localPosition = new Vector3(startPosition.x, newY, startPosition.z);
    }

    public int GetCoin()
    {
        return data.point;
    }

    public string GetItemName()
    {
        return data.name;
    }

    // 에디터에서 자석 범위를 하늘색 원으로 볼 수 있게 해주는 기능
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, magnetRadius);
    }
}