using System;
using UnityEngine;

/// <summary>
/// 게임의 모든 이벤트를 관리하고 중계하는 중앙 허브입니다.
/// 다른 스크립트들은 이 매니저를 통해 서로 통신하여 직접적인 참조를 피합니다. (느슨한 결합)
/// </summary>
public class EventManager : MonoBehaviour
{
    #region 싱글턴 패턴
    // 다른 모든 스크립트가 'EventManager.Instance'로 쉽게 접근할 수 있도록 만듭니다.
    public static EventManager Instance { get; private set; }

    private void Awake()
    {
        // 씬에 EventManager가 하나만 존재하도록 보장합니다.
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // 씬이 바뀌어도 파괴되지 않게 설정
        }
        else
        {
            Destroy(gameObject);
        }
    }
    #endregion

    // ===================================================================
    // 여기에 게임에서 사용할 모든 이벤트를 정의합니다.
    // 'event Action'은 '이런 일이 일어났습니다!'라고 방송하는 스피커와 같습니다.
    // ===================================================================

    #region 전투 흐름 이벤트 (Battle Flow Events)
    /// <summary>
    /// 새로운 라운드가 시작될 때 발생하는 이벤트입니다.
    /// CardManager는 이 이벤트를 듣고 새 카드를 드로우합니다.
    /// EnemyController는 이 이벤트를 듣고 다음 행동을 예고합니다.
    /// </summary>
    public event Action OnNewRoundStarted;

    /// <summary>
    /// 플레이어가 '행동 개시' 버튼을 눌러 행동 단계가 시작될 때 발생합니다.
    /// </summary>
    public event Action OnActionPhaseStarted;

    /// <summary>
    /// 모든 행동이 끝나고 라운드가 종료될 때 발생합니다.
    /// </summary>
    public event Action OnRoundEnded;
    #endregion


    #region 플레이어 행동 이벤트 (Player Action Events)
    /// <summary>
    /// 플레이어가 자신의 행동(카드 사용)을 완료했을 때 발생합니다.
    /// GameManager는 이 신호를 듣고 다음 유닛(적이든 플레이어든)의 턴을 진행합니다.
    /// </summary>
    public event Action OnPlayerActionCompleted;

    #endregion


    // ===================================================================
    // 이벤트 발생 함수들 (Event Raiser Methods)
    // 다른 스크립트에서는 이 함수들을 호출하여 이벤트를 발생시킵니다.
    // ===================================================================

    public void RaiseNewRoundStarted()
    {
        Debug.Log("<color=lightblue>EVENT: OnNewRoundStarted 발생!</color>");
        OnNewRoundStarted?.Invoke(); // 구독자가 있을 경우에만 이벤트 호출
    }

    public void RaiseActionPhaseStarted()
    {
        Debug.Log("<color=lightblue>EVENT: OnActionPhaseStarted 발생!</color>");
        OnActionPhaseStarted?.Invoke();
    }

    public void RaiseRoundEnded()
    {
        Debug.Log("<color=lightblue>EVENT: OnRoundEnded 발생!</color>");
        OnRoundEnded?.Invoke();
    }

    public void RaisePlayerActionCompleted()
    {
        Debug.Log("<color=lightblue>EVENT: OnPlayerActionCompleted 발생!</color>");
        OnPlayerActionCompleted?.Invoke();
    }
}
