using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

/// <summary>
/// CoreEventManager의 씬 전환 요청을 듣고, 실제로 씬을 로드하며 관련 이벤트를 발생시키는 역할을 담당합니다.
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
        // CoreEventManager가 존재할 때만 이벤트를 구독합니다.
        if (CoreEventManager.instance != null)
        {
            CoreEventManager.instance.OnSceneChangeRequested += HandleSceneChangeRequest;
        }
        // Unity의 기본 씬 로드 완료 이벤트를 구독합니다.
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
    /// OnSceneChangeRequested 이벤트가 발생했을 때 호출될 함수입니다.
    /// </summary>
    private void HandleSceneChangeRequest(string sceneName)
    {
        // TODO: 나중에 여기에 페이드 아웃 코루틴을 시작하는 로직을 추가할 수 있습니다.
        Debug.Log($"[SceneTransitionManager] 씬 전환 요청 수신: {sceneName}. 씬을 로드합니다.");
        SceneManager.LoadScene(sceneName);
    }

    /// <summary>
    /// Unity에 의해 씬 로드가 완료되면 자동으로 호출될 함수입니다.
    /// </summary>
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // 로드된 씬이 "BattleScene"이라면, CoreEventManager에게 방송을 하라고 알립니다.
        if (scene.name == "BattleScene") // 실제 전투 씬 이름과 일치해야 합니다.
        {
            if (CoreEventManager.instance != null)
            {
                CoreEventManager.instance.RaiseBattleSceneReady();
            }
        }
        // TODO: 나중에 타이쿤 씬이 준비되었을 때 알려주는 이벤트도 여기에 추가할 수 있습니다.
        // else if (scene.name == "tycoon") { CoreEventManager.instance.RaiseTycoonSceneReady(); }
    }
}