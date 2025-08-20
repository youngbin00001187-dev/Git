using UnityEngine;

네, 스크립트를 요청하신 것을 확인했습니다. 이전 답변에서 실수로 이미지를 생성했습니다. 죄송합니다.

요청하신 대로 좌우 이동 속도에 비례하여 좌우로 기울어지는 틸트 기능을 추가한 유니티 스크립트입니다.

📜 유니티 플레이어 이동 스크립트 (틸트 기능 추가)
C#

using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    // ✨ 에디터에서 조절할 수 있는 변수들
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 5.0f; // 기본 이동 속도
    [SerializeField] private float slowModeSpeedMultiplier = 0.5f; // 저속 모드 속도 배율 (0.5는 절반 속도)

    [Header("Tilt Settings")]
    [SerializeField] private float maxTiltAngle = 15.0f; // 최대 기울기 각도
    [SerializeField] private float tiltSpeed = 5.0f; // 기울기가 부드러워지는 속도

    private float currentMoveSpeed;

    void Update()
    {
        // 🕹️ 키보드 입력 처리
        float h = Input.GetAxis("Horizontal"); // 좌우 방향키 (A/D, Left/Right)
        float v = Input.GetAxis("Vertical");   // 상하 방향키 (W/S, Up/Down)

        Vector3 movement = new Vector3(h, 0f, v).normalized; // 대각선 이동 시 속도 일정하게 유지

        // 🚀 이동 속도 계산
        // Shift 키가 눌렸는지 확인하여 현재 속도 설정
        if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
        {
            currentMoveSpeed = moveSpeed * slowModeSpeedMultiplier;
        }
        else
        {
            currentMoveSpeed = moveSpeed;
        }

        // 🏃‍♂️ 캐릭터 이동
        transform.Translate(movement * currentMoveSpeed * Time.deltaTime, Space.World);

        // 🤸‍♂️ 좌우 틸트 (기울기) 계산 및 적용
        // 목표 기울기 각도 계산: 수평 입력값(h)에 최대 기울기 각도를 곱합니다.
        float targetTilt = h * -maxTiltAngle;

        // 현재 기울기 각도: 현재 회전값의 z축 각도를 가져옵니다.
        float currentTilt = transform.localRotation.eulerAngles.z;
        if (currentTilt > 180) currentTilt -= 360; // 0~360 범위를 -180~180으로 변환

        // 목표 각도로 부드럽게 회전
        float newTilt = Mathf.Lerp(currentTilt, targetTilt, Time.deltaTime * tiltSpeed);
        transform.localRotation = Quaternion.Euler(0, 0, newTilt);
    }
}