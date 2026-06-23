using System.Collections;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class SummonBossController : MonoBehaviour
{
    [Header("플레이어")]
    [SerializeField] private Transform player;

    [Header("플레이어 인식")]
    [SerializeField] private float traceDistance = 8f;
    [Header("패턴 1")]
    [SerializeField] private GameObject skill1Indicator;
    [SerializeField] private float skill1Damage = 20f;
    [SerializeField] private float skill1ChargeTime = 1f;
    [SerializeField] private float skill1Cooldown = 5f;

    [Header("패턴 2")]
    [SerializeField] private GameObject skill2Indicator;
    [SerializeField] private float skill2Damage = 30f;
    [SerializeField] private float skill2ChargeTime = 1.5f;
    [SerializeField] private float skill2Cooldown = 8f;

    [Header("패턴 3")]
    [SerializeField] private GameObject skill3Indicator;
    [SerializeField] private float skill3Damage = 40f;
    [SerializeField] private float skill3ChargeTime = 2f;
    [SerializeField] private float skill3Cooldown = 12f;

    [Header("패턴 4")]
    [SerializeField] private GameObject skill4Indicator;
    [SerializeField] private float skill4Damage = 60f;
    [SerializeField] private float skill4ChargeTime = 3f;
    [SerializeField] private float skill4Cooldown = 15f;

    private float skill4Timer;


    [Header("소환")]
    [SerializeField] private Transform[] spawnPoints;
    [SerializeField] private GameObject summonEffectPrefab;
    [SerializeField] private GameObject monsterPrefab;

    [SerializeField] private float minSummonTime = 2f;
    [SerializeField] private float maxSummonTime = 6f;

    [SerializeField] private GameObject[] skill1Indicators;



    private Animator animator;

    
    [SerializeField] private Light2D[] castLights;

    private float skill1Timer;
    private float skill2Timer;
    private float skill3Timer;

    private bool isUsingSkill;

    private void Start()
    {
        animator = GetComponent<Animator>();

        foreach (Light2D light in castLights)
        {
            if (light != null)
                light.intensity = 0f;
        }

        if (player == null)
        {
            GameObject p = GameObject.FindWithTag("Player");

            if (p != null)
                player = p.transform;
        }

        skill1Timer = skill1Cooldown;
        skill2Timer = skill2Cooldown;
        skill3Timer = skill3Cooldown;
        skill4Timer = skill4Cooldown;

        foreach (GameObject indicator in skill1Indicators)
        {
            if (indicator != null)
                indicator.SetActive(false);
        }

        if (skill2Indicator != null)
            skill2Indicator.SetActive(false);

        if (skill3Indicator != null)
            skill3Indicator.SetActive(false);

        if (skill4Indicator != null)
            skill4Indicator.SetActive(false);

        StartCoroutine(SummonLoop());
    }

    private void Update()
    {
        if (player == null)
            return;

        float distance =
            Vector2.Distance(
                transform.position,
                player.position);

        // 플레이어가 인식 범위 밖이면 아무것도 안함
        if (distance > traceDistance)
            return;

        if (isUsingSkill)
            return;

        skill1Timer -= Time.deltaTime;
        skill2Timer -= Time.deltaTime;
        skill3Timer -= Time.deltaTime;
        skill4Timer -= Time.deltaTime;

        if (skill4Timer <= 0)
        {
            StartCoroutine(
                SkillRoutine(
                    skill4Indicator,
                    skill4Damage,
                    skill4ChargeTime,
                    4));

            return;
        }

        if (skill3Timer <= 0)
        {
            StartCoroutine(SkillRoutine(
                skill3Indicator,
                skill3Damage,
                skill3ChargeTime,
                3));
            return;
        }

        if (skill2Timer <= 0)
        {
            StartCoroutine(SkillRoutine(
                skill2Indicator,
                skill2Damage,
                skill2ChargeTime,
                2));
            return;
        }

        if (skill1Timer <= 0)
        {
            StartCoroutine(MultiSkillRoutine(
                skill1Indicators,
                skill1Damage,
                skill1ChargeTime,
                1));
            return;
        }
    }

    private IEnumerator SkillRoutine(
        GameObject indicator,
        float damage,
        float chargeTime,
        int skillIndex)
    {
        isUsingSkill = true;

        if (skillIndex == 4 && player != null)
        {
            Vector2 offset = new Vector2(
                Random.Range(-0.1f, 0.1f),
                Random.Range(-0.1f, 0.1f));

            indicator.transform.position =
                player.position + (Vector3)offset;
        }
        StartCoroutine(LightEffect());

        yield return StartCoroutine(
            PlayCastAnimation());

        indicator.SetActive(true);

        Vector3 originalScale = indicator.transform.localScale;

        indicator.transform.localScale =
            new Vector3(
                originalScale.x,
                0,
                originalScale.z);

        float timer = 0f;

        while (timer < chargeTime)
        {
            timer += Time.deltaTime;

            float progress =
                timer / chargeTime;

            indicator.transform.localScale =
                new Vector3(
                    originalScale.x,
                    originalScale.y * progress,
                    originalScale.z);

            yield return null;
        }

        AttackPlayerInRange(indicator, damage);

        indicator.SetActive(false);

        switch (skillIndex)
        {
            case 1:
                skill1Timer = skill1Cooldown;
                break;

            case 2:
                skill2Timer = skill2Cooldown;
                break;

            case 3:
                skill3Timer = skill3Cooldown;
                break;

            case 4:
                skill4Timer = skill4Cooldown;
                break;
        }

        yield return new WaitForSeconds(0.3f);

        isUsingSkill = false;
    }

    private IEnumerator MultiSkillRoutine(
    GameObject[] indicators,
    float damage,
    float chargeTime,
    int skillIndex)
    {
        isUsingSkill = true;

        StartCoroutine(LightEffect());

        Vector3[] originalScales =
                    new Vector3[indicators.Length];

        for (int i = 0; i < indicators.Length; i++)
        {
            if (indicators[i] == null)
                continue;

            indicators[i].SetActive(true);

            originalScales[i] =
                indicators[i].transform.localScale;

            indicators[i].transform.localScale =
                new Vector3(
                    originalScales[i].x,
                    0,
                    originalScales[i].z);
        }

        float timer = 0f;

        while (timer < chargeTime)
        {
            timer += Time.deltaTime;

            float progress =
                timer / chargeTime;

            for (int i = 0; i < indicators.Length; i++)
            {
                if (indicators[i] == null)
                    continue;

                indicators[i].transform.localScale =
                    new Vector3(
                        originalScales[i].x,
                        originalScales[i].y * progress,
                        originalScales[i].z);
            }

            yield return null;
        }

        for (int i = 0; i < indicators.Length; i++)
        {
            if (indicators[i] == null)
                continue;

            AttackPlayerInRange(
                indicators[i],
                damage);

            indicators[i].SetActive(false);
        }

        switch (skillIndex)
        {
            case 1:
                skill1Timer = skill1Cooldown;
                break;

            case 2:
                skill2Timer = skill2Cooldown;
                break;

            case 3:
                skill3Timer = skill3Cooldown;
                break;

            case 4:
                skill4Timer = skill4Cooldown;
                break;
        }

        yield return new WaitForSeconds(0.3f);

        isUsingSkill = false;
    }
    private void AttackPlayerInRange(
        GameObject indicator,
        float damage)
    {
        BoxCollider2D box =
            indicator.GetComponent<BoxCollider2D>();

        if (box == null)
        {
            Debug.LogError(indicator.name + "에 BoxCollider2D 없음!");
            return;
        }

        Collider2D[] hits =
            Physics2D.OverlapBoxAll(
                box.bounds.center,
                box.bounds.size,
                0f,
                LayerMask.GetMask("Player"));

        foreach (Collider2D hit in hits)
        {
            Health health = hit.GetComponent<Health>();

            if (health != null)
            {
                health.TakeDamage(damage);

                Debug.Log(
                    $"보스 스킬 적중! {damage}");
            }
        }
    }
    private IEnumerator SummonLoop()
    {
        while (true)
        {
            yield return new WaitForSeconds(
                Random.Range(
                    minSummonTime,
                    maxSummonTime));

            StartCoroutine(SummonRoutine());
        }
    }

    private IEnumerator SummonRoutine()
    {
        if (spawnPoints == null || spawnPoints.Length == 0)
        {
            Debug.LogError("Spawn Points가 등록되지 않았습니다!");
            yield break;
        }

        

        StartCoroutine(LightEffect());

        yield return StartCoroutine(
            PlayCastAnimation());

        Transform spawnPoint =
                    spawnPoints[Random.Range(0, spawnPoints.Length)];

        if (spawnPoint == null)
        {
            Debug.LogError("Spawn Point 슬롯이 비어있습니다!");
            
            yield break;
        }

        GameObject effect =
            Instantiate(
                summonEffectPrefab,
                spawnPoint.position,
                Quaternion.identity);


        Animator anim =
     effect.GetComponent<Animator>();

        float animLength = 1f;

        if (anim != null)
        {
            animLength =
                anim.runtimeAnimatorController
                .animationClips[0].length;

            yield return new WaitForSeconds(animLength);
        }
        else
        {
            yield return new WaitForSeconds(1f);
        }

        Instantiate(
            monsterPrefab,
            spawnPoint.position,
            Quaternion.identity);

        Destroy(effect);
    }
    private void OnDrawGizmosSelected()
    {
        // 플레이어 인식 범위
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(
            transform.position,
            traceDistance);

        // 소환 포인트
        Gizmos.color = Color.red;

        if (spawnPoints == null)
            return;

        foreach (Transform point in spawnPoints)
        {
            if (point == null)
                continue;

            Gizmos.DrawWireSphere(
                point.position,
                0.3f);
        }
    }

    private void OnDrawGizmos()
    {
        DrawSkillBox(skill1Indicator, Color.red);
        DrawSkillBox(skill2Indicator, Color.blue);
        DrawSkillBox(skill3Indicator, Color.green);
        DrawSkillBox(skill4Indicator, Color.magenta);
    }

    private void DrawSkillBox(
        GameObject indicator,
        Color color)
    {
        if (indicator == null)
            return;

        Gizmos.color = color;

        Gizmos.matrix =
            indicator.transform.localToWorldMatrix;

        Gizmos.DrawWireCube(
            Vector3.zero,
            Vector3.one);
    }

    private IEnumerator PlayCastAnimation()
    {
        if (animator != null)
        {
            animator.SetTrigger("Cast");

            yield return new WaitForSeconds(0.5f);
        }
    }

    private IEnumerator LightEffect()
    {
        float duration = 0.5f;

        float timer = 0f;

        while (timer < duration)
        {
            timer += Time.deltaTime;

            foreach (Light2D light in castLights)
            {
                if (light != null)
                {
                    light.intensity =
                        Mathf.Lerp(
                            0f,
                            3f,
                            timer / duration);
                }
            }

            yield return null;
        }

        timer = 0f;

        while (timer < duration)
        {
            timer += Time.deltaTime;

            foreach (Light2D light in castLights)
            {
                if (light != null)
                {
                    light.intensity =
                        Mathf.Lerp(
                            3f,
                            0f,
                            timer / duration);
                }
            }

            yield return null;
        }
    }
}