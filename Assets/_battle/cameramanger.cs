using UnityEngine;
using DG.Tweening;
using System.Collections;

public class BattleCameraController : MonoBehaviour
{
    public static BattleCameraController instance;

    private Camera mainCamera;
    private Vector3 originalPosition;
    private float originalOrthoSize;

    [Header("카메라 스크롤 설정")]
    public float panSpeed = 20f;
    public Vector2 minBounds;
    public Vector2 maxBounds;
    private Vector3 dragOrigin;

    // ▼▼▼ 재사용 가능한 트윈 객체 ▼▼▼
    private Tweener zoomTween;
    private Tweener moveTween;

    void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);

        mainCamera = GetComponent<Camera>();
        originalPosition = transform.position;

        if (mainCamera != null)
            originalOrthoSize = mainCamera.orthographicSize;
    }

    void OnDestroy()
    {
        zoomTween?.Kill();
        moveTween?.Kill();
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(2))
        {
            dragOrigin = Input.mousePosition;
            return;
        }

        if (!Input.GetMouseButton(2)) return;

        Vector3 difference = mainCamera.ScreenToViewportPoint(Input.mousePosition - dragOrigin);
        Vector3 move = new Vector3(difference.x * panSpeed, difference.y * panSpeed, 0);
        transform.Translate(-move, Space.World);
    }

    void LateUpdate()
    {
        Vector3 clampedPos = transform.position;
        clampedPos.x = Mathf.Clamp(clampedPos.x, minBounds.x, maxBounds.x);
        clampedPos.y = Mathf.Clamp(clampedPos.y, minBounds.y, maxBounds.y);
        transform.position = clampedPos;
    }

    public void MoveToTarget(Transform target, float duration = 0.5f)
    {
        Vector3 targetPos = new Vector3(target.position.x, target.position.y, originalPosition.z);
        transform.DOMove(targetPos, duration).SetEase(Ease.OutQuad);
    }

    public void ShakeCamera(float duration = 0.2f, float strength = 0.5f)
    {
        mainCamera.DOShakePosition(duration, strength);
    }

    /// <summary>
    /// '캔슬' 시, 플레이어를 향해 부드럽게 줌인합니다.
    /// 연속 호출 시에도 부드럽게 덮어쓰며 이어집니다.
    /// </summary>
    public void ZoomInForCancel(Transform playerTransform, float zoomInSize = 4.0f, float duration = 0.1f)
    {
        if (mainCamera == null || playerTransform == null) return;

        Vector3 targetPos = new Vector3(playerTransform.position.x, playerTransform.position.y, originalPosition.z);

        // 줌 트윈 재사용 또는 생성
        if (zoomTween == null)
        {
            zoomTween = mainCamera.DOOrthoSize(zoomInSize, duration)
                .SetEase(Ease.OutQuad)
                .SetAutoKill(false)
                .Pause()
                .SetUpdate(true);
        }
        else
        {
            zoomTween.ChangeEndValue(zoomInSize, true);
        }

        // 이동 트윈 재사용 또는 생성
        if (moveTween == null)
        {
            moveTween = transform.DOMove(targetPos, duration)
                .SetEase(Ease.OutQuad)
                .SetAutoKill(false)
                .Pause()
                .SetUpdate(true);
        }
        else
        {
            moveTween.ChangeEndValue(targetPos, true);
        }

        // 트윈 재시작
        zoomTween.Restart();
        moveTween.Restart();
    }

    /// <summary>
    /// '캔슬' 연출이 끝난 후, 카메라를 원래 상태로 되돌립니다.
    /// </summary>
    public void ResetZoom(float duration = 0.1f)
    {
        if (mainCamera == null) return;

        // 트윈 존재 시 역재생
        if (zoomTween != null)
        {
            zoomTween.PlayBackwards();
        }

        if (moveTween != null)
        {
            moveTween.PlayBackwards();
        }
    }
}
