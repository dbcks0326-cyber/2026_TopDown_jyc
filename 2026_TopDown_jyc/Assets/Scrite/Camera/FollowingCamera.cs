using UnityEngine;
using System.Collections;

public class FollowingCamera : MonoBehaviour
{
    private Transform player;
    private Vector3 offset;

    Camera cam;

    float defaultSize;
    bool isZooming = false;

    [Header("카메라 확대 값")]
    public float zoomSize = 3f;

    [Header("확대 속도")]
    public float zoomSpeed = 5f;

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

        transform.position =
            new Vector3(player.position.x,
                        player.position.y,
                        -10f) + offset;

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

    public void ZoomIn()
    {
        isZooming = true;
    }

    public void ZoomOut()
    {
        isZooming = false;
    }
}