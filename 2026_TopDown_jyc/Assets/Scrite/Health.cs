using System.Collections; // ★ 추가: 코루틴(IEnumerator)을 쓰기 위해 필수!
using UnityEngine;
using UnityEngine.SceneManagement; // 씬 전환용

public class Health : MonoBehaviour
{
    [SerializeField] protected float maxHP = 100f;
    protected float currentHealth;
    [SerializeField] private bool isPlayer = false;

    // -------------------------------------------------------------
    // ★ 추가: 몬스터(Enemy) 피격 연출용 변수
    // -------------------------------------------------------------
    private SpriteRenderer sr;
    private bool isHurt = false; // 중복 깜빡임 방지용 스위치

    protected virtual void Start()
    {
        // 내 몸에 붙어있는 그림 컴포넌트를 미리 찾아둡니다.
        sr = GetComponent<SpriteRenderer>();

        // 안전장치: 플레이어이고 데이터가 정상적일 때만 세이브 데이터 연동
        if (isPlayer && GameDataManager.Instance != null && GameDataManager.Instance.playerData != null)
        {
            maxHP = GameDataManager.Instance.playerData.maxHp;
            currentHealth = GameDataManager.Instance.playerData.currentHp;
        }
        else
        {
            currentHealth = maxHP;
        }
    }

    public virtual void TakeDamage(float damage)
    {
        currentHealth -= damage;

        // ★ 오직 '진짜 플레이어'일 때만 세이브 데이터를 갱신하고 PlayerController의 OnHurt를 부릅니다.
        if (isPlayer)
        {
            if (GameDataManager.Instance != null && GameDataManager.Instance.playerData != null)
            {
                GameDataManager.Instance.playerData.currentHp = (int)currentHealth;
            }

            // 내 몸에 붙어있는 PlayerController를 찾아서 OnHurt 호출 (거기서 플레이어 전용 연한 빨강이 돎)
            PlayerController player = GetComponent<PlayerController>();
            if (player != null)
            {
                player.OnHurt();
            }
        }
        else
        {
            // ★ 플레이어가 아닐 때(=몬스터일 때)는 여기서 직접 연한 빨간색 코루틴을 실행합니다!
            if (!isHurt)
            {
                StartCoroutine(EnemyHurtRoutine());
            }
        }

        // 대미지 텍스트 팝업 (없으면 안전하게 리턴됨)
        CreateDamageText(damage);

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    // -------------------------------------------------------------
    // ★ 추가: 몬스터 전용 부드러운 연한 빨간색 깜빡임 코루틴
    // -------------------------------------------------------------
    private IEnumerator EnemyHurtRoutine()
    {
        isHurt = true;

        if (sr != null)
        {
            // 부드러운 연빨강으로 변경
            sr.color = new Color(1f, 0.4f, 0.4f, 1f);
        }

        // 0.15초 동안 유지
        yield return new WaitForSeconds(0.15f);

        if (sr != null)
        {
            // 원래 색상으로 복구
            sr.color = Color.white;
        }

        isHurt = false;
    }

    public void CreateDamageText(float damage)
    {
        GameObject dmgPrefab = Resources.Load<GameObject>("Damage_Text");
        if (dmgPrefab == null) return;

        GameObject DmgGO = Instantiate(dmgPrefab);
        DmgGO.transform.SetParent(transform);
        DmgGO.transform.position = new Vector2(
            Random.Range(transform.position.x - 0.5f, transform.position.x + 0.5f),
            Random.Range(transform.position.y - 0.5f, transform.position.y + 0.5f)
        );
    }

    public virtual void Die()
    {
        if (isPlayer)
        {
            currentHealth = 0;
            if (GameDataManager.Instance != null && GameDataManager.Instance.playerData != null)
            {
                GameDataManager.Instance.playerData.currentHp = (int)maxHP; // 리스폰 시 풀피로 초기화
                GameDataManager.Instance.SaveData(GameDataManager.Instance.playerData);
            }

            Debug.Log("플레이어 사망 -> Stage_1 재시작");
            SceneManager.LoadScene("Stage_1");
        }
        else
        {
            Destroy(gameObject);
        }
    }
}