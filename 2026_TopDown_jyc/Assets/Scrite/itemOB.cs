using UnityEngine;

public class itemOB : MonoBehaviour
{
    [SerializeField] itemso data;

    [Header("둥둥 효과 설정")]
    [SerializeField] private float floatSpeed = 3f;
    [SerializeField] private float floatAmplitude = 0.01f;

    private Vector3 startPosition;

    void Start()
    {
        // -------------------------------------------------------------
        // ★ [핵심 추가]: 생성되자마자 유니티 타이머에 20초 뒤 삭제를 예약합니다.
        // gameObject는 이 스크립트가 붙어있는 코인 자신을 의미합니다.
        // -------------------------------------------------------------
        Destroy(gameObject, 20f);

        // 둥둥 떠다니기 위한 시작 위치를 기억합니다.
        startPosition = transform.localPosition;
    }

    void Update()
    {
        // Y축 둥둥 효과 연산
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
}