using System;
using UnityEngine;

/// <summary>
/// 게임 전체의 핵심 이벤트를 관리하는 중앙 방송국입니다.
/// DontDestroyOnLoad로 설정되어 CoreScene에서 계속 유지됩니다.
/// </summary>
public class CoreEventManager : MonoBehaviour
{
    public static CoreEventManager instance;

    // === 이벤트 선언 ===
    public event Action<string> OnSceneChangeRequested;
    public event Action OnSaveGameRequested;
    public event Action OnLoadGameRequested;

    // --- ▼▼▼ 이 부분이 핵심 수정 내용입니다 ▼▼▼ ---
    /// <summary>
    /// 전투 씬이 완전히 로드되고 UI 매니저가 준비할 시간임을 알리는 이벤트입니다.
    /// </summary>
    public event Action OnBattleSceneReady;

    /// <summary>
    /// 플레이어의 선택에 따라 실제 전투(타일, 유닛 생성) 시작을 요청할 때 발생하는 이벤트입니다.
    /// </summary>
    public event Action OnCombatStartRequested;
    // --- ▲▲▲ 핵심 수정 내용 끝 ▲▲▲ ---


    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // === 이벤트 발생 함수들 ===
    public void RaiseSceneChangeRequested(string sceneName)
    {
        Debug.Log($"<color=orange>CORE EVENT: OnSceneChangeRequested 발생! -> {sceneName}</color>");
        OnSceneChangeRequested?.Invoke(sceneName);
    }

    public void RaiseSaveGameRequested()
    {
        Debug.Log("<color=orange>CORE EVENT: OnSaveGameRequested 발생!</color>");
        OnSaveGameRequested?.Invoke();
    }

    public void RaiseLoadGameRequested()
    {
        Debug.Log("<color=orange>CORE EVENT: OnLoadGameRequested 발생!</color>");
        OnLoadGameRequested?.Invoke();
    }

    // --- ▼▼▼ 이 부분이 핵심 수정 내용입니다 ▼▼▼ ---
    /// <summary>
    /// 전투 씬 준비 완료 이벤트를 발생시킵니다.
    /// </summary>
    public void RaiseBattleSceneReady()
    {
        Debug.Log("<color=orange>CORE EVENT: OnBattleSceneReady 발생!</color>");
        OnBattleSceneReady?.Invoke();
    }

    /// <summary>
    /// 전투 시작 요청 이벤트를 발생시킵니다.
    /// </summary>
    public void RaiseCombatStartRequested()
    {
        Debug.Log("<color=orange>CORE EVENT: OnCombatStartRequested 발생!</color>");
        OnCombatStartRequested?.Invoke();
    }
    // --- ▲▲▲ 핵심 수정 내용 끝 ▲▲▲ ---
}