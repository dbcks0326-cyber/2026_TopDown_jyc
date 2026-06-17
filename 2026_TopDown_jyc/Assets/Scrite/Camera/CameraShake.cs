using UnityEngine;
using DG.Tweening; // ★ DoTween 네임스페이스 필수!

public class CameraShake : MonoBehaviour
{
    // 어디서나 쉽게 카메라를 흔들 수 있도록 싱글톤(Singleton)으로 설정합니다.
    public static CameraShake Instance { get; private set; }

    private Vector3 originalPosition;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        originalPosition = transform.localPosition;
    }

    // ★ 슬라임이나 다른 오브젝트가 호출할 흔들기 메서드
    public void Shake(float duration = 0.1f, float strength = 0.06f, int vibrato = 3)
    {
        // 디버그 로그를 심어서 이 메서드 자체가 실행은 되는지 콘솔창에서 꼭 확인하세요!
        //Debug.Log("🎥 CameraShake: Shake 메서드가 정상적으로 호출되었습니다!");

        // 해당 트랜스폼에 걸린 트윈만 안전하게 저격해서 삭제
        DOTween.Kill(transform);
        transform.localPosition = originalPosition;

        // .SetRelative(true)를 붙여주면 현재 위치 기준으로 안정적으로 흔들립니다.
        transform.DOShakePosition(duration, strength, vibrato)
                 .SetRelative(true)
                 .OnComplete(() => transform.localPosition = originalPosition);
    }
}