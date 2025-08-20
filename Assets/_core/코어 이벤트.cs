using System;
using UnityEngine;

/// <summary>
/// ���� ��ü�� �ٽ� �̺�Ʈ�� �����ϴ� �߾� ��۱��Դϴ�.
/// DontDestroyOnLoad�� �����Ǿ� CoreScene���� ��� �����˴ϴ�.
/// </summary>
public class CoreEventManager : MonoBehaviour
{
    public static CoreEventManager instance;

    // === �̺�Ʈ ���� ===
    public event Action<string> OnSceneChangeRequested;
    public event Action OnSaveGameRequested;
    public event Action OnLoadGameRequested;

    // --- ���� �� �κ��� �ٽ� ���� �����Դϴ� ���� ---
    /// <summary>
    /// ���� ���� ������ �ε�ǰ� UI �Ŵ����� �غ��� �ð����� �˸��� �̺�Ʈ�Դϴ�.
    /// </summary>
    public event Action OnBattleSceneReady;

    /// <summary>
    /// �÷��̾��� ���ÿ� ���� ���� ����(Ÿ��, ���� ����) ������ ��û�� �� �߻��ϴ� �̺�Ʈ�Դϴ�.
    /// </summary>
    public event Action OnCombatStartRequested;
    // --- ���� �ٽ� ���� ���� �� ���� ---


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

    // === �̺�Ʈ �߻� �Լ��� ===
    public void RaiseSceneChangeRequested(string sceneName)
    {
        Debug.Log($"<color=orange>CORE EVENT: OnSceneChangeRequested �߻�! -> {sceneName}</color>");
        OnSceneChangeRequested?.Invoke(sceneName);
    }

    public void RaiseSaveGameRequested()
    {
        Debug.Log("<color=orange>CORE EVENT: OnSaveGameRequested �߻�!</color>");
        OnSaveGameRequested?.Invoke();
    }

    public void RaiseLoadGameRequested()
    {
        Debug.Log("<color=orange>CORE EVENT: OnLoadGameRequested �߻�!</color>");
        OnLoadGameRequested?.Invoke();
    }

    // --- ���� �� �κ��� �ٽ� ���� �����Դϴ� ���� ---
    /// <summary>
    /// ���� �� �غ� �Ϸ� �̺�Ʈ�� �߻���ŵ�ϴ�.
    /// </summary>
    public void RaiseBattleSceneReady()
    {
        Debug.Log("<color=orange>CORE EVENT: OnBattleSceneReady �߻�!</color>");
        OnBattleSceneReady?.Invoke();
    }

    /// <summary>
    /// ���� ���� ��û �̺�Ʈ�� �߻���ŵ�ϴ�.
    /// </summary>
    public void RaiseCombatStartRequested()
    {
        Debug.Log("<color=orange>CORE EVENT: OnCombatStartRequested �߻�!</color>");
        OnCombatStartRequested?.Invoke();
    }
    // --- ���� �ٽ� ���� ���� �� ���� ---
}