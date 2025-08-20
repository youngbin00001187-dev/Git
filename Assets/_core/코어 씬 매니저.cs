using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

/// <summary>
/// 게임의 시작점인 CoreScene을 관리하고, 게임 모드를 선택하여 다음 씬으로 전환합니다.
/// </summary>
public class CoreSceneManager : MonoBehaviour
{
    [Header("UI Elements")]
    [Tooltip("RPG 모드(객잔 경영)를 시작하는 버튼입니다.")]
    public Button startTycoonModeButton;
    [Tooltip("로그라이크 배틀 모드를 시작하는 버튼입니다.")]
    public Button startBattleModeButton;

    [Header("로그라이크 모드 기본 설정")]
    [Tooltip("배틀 모드 시작 시 플레이어가 기본으로 가질 심법 목록입니다.")]
    public List<SimbeopDataSO> battleMode_InitialSimbeops;
    [Tooltip("배틀 모드 시작 시 플레이어가 기본으로 가질 카드 목록입니다.")]
    public List<CardDataSO> battleMode_InitialCards;

    void Start()
    {
        if (startTycoonModeButton != null)
        {
            startTycoonModeButton.onClick.AddListener(StartTycoonMode);
        }
        if (startBattleModeButton != null)
        {
            startBattleModeButton.onClick.AddListener(StartBattleMode);
        }
    }

    private void StartTycoonMode()
    {
        if (GlobalManager.instance != null)
        {
            GlobalManager.instance.currentGameMode = GameMode.Tycoon;
        }

        Debug.Log("RPG 모드를 시작합니다. tycoon 씬으로 이동을 요청합니다.");
        if (CoreEventManager.instance != null)
        {
            CoreEventManager.instance.RaiseSceneChangeRequested("tycoon");
        }
        else
        {
            SceneManager.LoadScene("tycoon");
        }
    }

    private void StartBattleMode()
    {
        GlobalManager gm = GlobalManager.instance;
        if (gm != null)
        {
            gm.currentGameMode = GameMode.RoguelikeBattle;

            gm.ownedSimbeops = new List<SimbeopDataSO>(battleMode_InitialSimbeops);
            gm.playerCardCollection = new List<CardDataSO>(battleMode_InitialCards);

            if (gm.ownedSimbeops.Count > 0)
            {
                gm.equippedSimbeop = gm.ownedSimbeops[0];
            }
            gm.playerBattleDeck = new List<CardDataSO>(gm.playerCardCollection);

            Debug.Log($"배틀 모드 설정 완료: 심법 {gm.ownedSimbeops.Count}개, 카드 {gm.playerCardCollection.Count}개");
        }
        else
        {
            Debug.LogError("GlobalManager를 찾을 수 없어 배틀 모드를 시작할 수 없습니다!");
            return;
        }

        if (CoreEventManager.instance != null)
        {
            CoreEventManager.instance.RaiseSceneChangeRequested("BattleScene");
        }
        else
        {
            SceneManager.LoadScene("BattleScene");
        }
    }
}