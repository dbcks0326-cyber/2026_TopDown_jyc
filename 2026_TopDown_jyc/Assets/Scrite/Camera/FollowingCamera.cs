    using UnityEngine;
    using System.Collections;
    using DG.Tweening; // ★ DoTween 네임스페이스 필수 추가!

    public class FollowingCamera : MonoBehaviour
    {
        // ★ 어디서나 FollowingCamera.Instance.Shake()로 호출할 수 있도록 싱글톤 세팅
        public static FollowingCamera Instance { get; private set; }

        private Transform player;
        private Vector3 offset;

        Camera cam;

        float defaultSize;
        bool isZooming = false;

        [Header("카메라 확대 값")]
        public float zoomSize = 3f;

        [Header("확대 속도")]
        public float zoomSpeed = 5f;

        // 쉐이크 오프셋을 저장할 변수
        private Vector3 shakeOffset = Vector3.zero;

        void Awake()
        {
            // 싱글톤 초기화
            if (Instance == null) Instance = this;
            else Destroy(gameObject);
        }

        void Start()
        {
            player = GameObject.FindGameObjectWithTag("Player").transform;
            cam = GetComponent<Camera>();
            defaultSize = cam.orthographicSize;
        }

        void LateUpdate()
        {
            if (player == null)
                return;

            // ★ [핵심]: 플레이어 추적 위치에 DoTween이 계산한 shakeOffset을 더해줍니다.
            // 이렇게 하면 팔로우 스크립트와 DoTween이 서로 위치를 빼앗으려고 싸우지 않습니다!
            transform.position =
                new Vector3(player.position.x, player.position.y, -10f) + offset + shakeOffset;

            float targetSize;

            if (isZooming)
                targetSize = zoomSize;
            else
                targetSize = defaultSize;

            cam.orthographicSize =
                Mathf.Lerp(
                    cam.orthographicSize,
                    targetSize,
                    Time.deltaTime * zoomSpeed
                );
        }

    // ★ [신규 추가]: 슬라임 착지 등에서 호출할 흔들기 메서드
    // ★ [수정완료]: 완벽하게 화면을 흔들어주는 2D 전용 쉐이크 메서드
    public void Shake(float duration = 0.25f, float strength = 0.4f, int vibrato = 12)
    {
        // 1. 디버그 확인용 로그
       // Debug.Log($"🎥 카메라 쉐이크 가동! (시간: {duration}초, 강도: {strength})");

        // 2. 작동 중인 이전 쉐이크 트윈이 있다면 깔끔하게 지우고 오프셋 초기화
        DOTween.Kill("CameraShakeTween");
        shakeOffset = Vector3.zero;

        // 3. [핵심]: DoTween.To를 이용해 지정된 시간(duration) 동안 shakeOffset을 사정없이 흔듭니다.
        DOTween.To(() => strength, x => {
            // 남은 강도(x)를 기준으로 랜덤한 2D 방향 벡터를 생성해 오프셋에 대입합니다.
            shakeOffset = Random.insideUnitSphere * x;
            shakeOffset.z = 0f; // Z축은 앞뒤이므로 2D 게임에선 흔들리지 않게 고정!
        }, 0f, duration)
        .SetEase(Ease.OutQuad) // 갈수록 흔들림이 자연스럽게 줄어들도록 감쇠 효과 적용
        .SetId("CameraShakeTween")
        .OnComplete(() => {
            // 4. 쉐이크가 완전히 끝나면 오프셋을 깔끔하게 제자리(0)로 리셋
            shakeOffset = Vector3.zero;
        });
    }
    public void ZoomIn()
        {
            isZooming = true;
        }

        public void ZoomOut()
        {
            isZooming = false;
        }
    }