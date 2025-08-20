using System;
using UnityEngine;

/// <summary>
/// ������ ��� �̺�Ʈ�� �����ϰ� �߰��ϴ� �߾� ����Դϴ�.
/// �ٸ� ��ũ��Ʈ���� �� �Ŵ����� ���� ���� ����Ͽ� �������� ������ ���մϴ�. (������ ����)
/// </summary>
public class EventManager : MonoBehaviour
{
    #region �̱��� ����
    // �ٸ� ��� ��ũ��Ʈ�� 'EventManager.Instance'�� ���� ������ �� �ֵ��� ����ϴ�.
    public static EventManager Instance { get; private set; }

    private void Awake()
    {
        // ���� EventManager�� �ϳ��� �����ϵ��� �����մϴ�.
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // ���� �ٲ� �ı����� �ʰ� ����
        }
        else
        {
            Destroy(gameObject);
        }
    }
    #endregion

    // ===================================================================
    // ���⿡ ���ӿ��� ����� ��� �̺�Ʈ�� �����մϴ�.
    // 'event Action'�� '�̷� ���� �Ͼ���ϴ�!'��� ����ϴ� ����Ŀ�� �����ϴ�.
    // ===================================================================

    #region ���� �帧 �̺�Ʈ (Battle Flow Events)
    /// <summary>
    /// ���ο� ���尡 ���۵� �� �߻��ϴ� �̺�Ʈ�Դϴ�.
    /// CardManager�� �� �̺�Ʈ�� ��� �� ī�带 ��ο��մϴ�.
    /// EnemyController�� �� �̺�Ʈ�� ��� ���� �ൿ�� �����մϴ�.
    /// </summary>
    public event Action OnNewRoundStarted;

    /// <summary>
    /// �÷��̾ '�ൿ ����' ��ư�� ���� �ൿ �ܰ谡 ���۵� �� �߻��մϴ�.
    /// </summary>
    public event Action OnActionPhaseStarted;

    /// <summary>
    /// ��� �ൿ�� ������ ���尡 ����� �� �߻��մϴ�.
    /// </summary>
    public event Action OnRoundEnded;
    #endregion


    #region �÷��̾� �ൿ �̺�Ʈ (Player Action Events)
    /// <summary>
    /// �÷��̾ �ڽ��� �ൿ(ī�� ���)�� �Ϸ����� �� �߻��մϴ�.
    /// GameManager�� �� ��ȣ�� ��� ���� ����(���̵� �÷��̾��)�� ���� �����մϴ�.
    /// </summary>
    public event Action OnPlayerActionCompleted;

    #endregion


    // ===================================================================
    // �̺�Ʈ �߻� �Լ��� (Event Raiser Methods)
    // �ٸ� ��ũ��Ʈ������ �� �Լ����� ȣ���Ͽ� �̺�Ʈ�� �߻���ŵ�ϴ�.
    // ===================================================================

    public void RaiseNewRoundStarted()
    {
        Debug.Log("<color=lightblue>EVENT: OnNewRoundStarted �߻�!</color>");
        OnNewRoundStarted?.Invoke(); // �����ڰ� ���� ��쿡�� �̺�Ʈ ȣ��
    }

    public void RaiseActionPhaseStarted()
    {
        Debug.Log("<color=lightblue>EVENT: OnActionPhaseStarted �߻�!</color>");
        OnActionPhaseStarted?.Invoke();
    }

    public void RaiseRoundEnded()
    {
        Debug.Log("<color=lightblue>EVENT: OnRoundEnded �߻�!</color>");
        OnRoundEnded?.Invoke();
    }

    public void RaisePlayerActionCompleted()
    {
        Debug.Log("<color=lightblue>EVENT: OnPlayerActionCompleted �߻�!</color>");
        OnPlayerActionCompleted?.Invoke();
    }
}
