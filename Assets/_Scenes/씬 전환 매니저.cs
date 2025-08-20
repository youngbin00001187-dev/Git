using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

/// <summary>
/// CoreEventManager�� �� ��ȯ ��û�� ���, ������ ���� �ε��ϸ� ���� �̺�Ʈ�� �߻���Ű�� ������ ����մϴ�.
/// </summary>
public class SceneTransitionManager : MonoBehaviour
{
    public static SceneTransitionManager instance;

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

    private void OnEnable()
    {
        // CoreEventManager�� ������ ���� �̺�Ʈ�� �����մϴ�.
        if (CoreEventManager.instance != null)
        {
            CoreEventManager.instance.OnSceneChangeRequested += HandleSceneChangeRequest;
        }
        // Unity�� �⺻ �� �ε� �Ϸ� �̺�Ʈ�� �����մϴ�.
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        if (CoreEventManager.instance != null)
        {
            CoreEventManager.instance.OnSceneChangeRequested -= HandleSceneChangeRequest;
        }
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    /// <summary>
    /// OnSceneChangeRequested �̺�Ʈ�� �߻����� �� ȣ��� �Լ��Դϴ�.
    /// </summary>
    private void HandleSceneChangeRequest(string sceneName)
    {
        // TODO: ���߿� ���⿡ ���̵� �ƿ� �ڷ�ƾ�� �����ϴ� ������ �߰��� �� �ֽ��ϴ�.
        Debug.Log($"[SceneTransitionManager] �� ��ȯ ��û ����: {sceneName}. ���� �ε��մϴ�.");
        SceneManager.LoadScene(sceneName);
    }

    /// <summary>
    /// Unity�� ���� �� �ε尡 �Ϸ�Ǹ� �ڵ����� ȣ��� �Լ��Դϴ�.
    /// </summary>
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // �ε�� ���� "BattleScene"�̶��, CoreEventManager���� ����� �϶�� �˸��ϴ�.
        if (scene.name == "BattleScene") // ���� ���� �� �̸��� ��ġ�ؾ� �մϴ�.
        {
            if (CoreEventManager.instance != null)
            {
                CoreEventManager.instance.RaiseBattleSceneReady();
            }
        }
        // TODO: ���߿� Ÿ���� ���� �غ�Ǿ��� �� �˷��ִ� �̺�Ʈ�� ���⿡ �߰��� �� �ֽ��ϴ�.
        // else if (scene.name == "tycoon") { CoreEventManager.instance.RaiseTycoonSceneReady(); }
    }
}