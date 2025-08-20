using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using TMPro;

public class PlayerController : UnitController
{
    [Header("플레이어 전용 능력치")]
    public float moveSpeed = 8f;

    [Header("플레이어 전용 UI")]
    public TextMeshProUGUI healthText;

    protected override void Awake()
    {
        Debug.Log("[PlayerController] Awake 시작");

        base.Awake(); // UnitController의 Awake 호출

        Debug.Log($"[PlayerController] base.Awake 완료 - maxHealth: {maxHealth}, currentHealth: {currentHealth}");

        if (GlobalManager.instance != null)
        {
            maxHealth = GlobalManager.instance.playerBaseMaxHealth;
            currentHealth = GlobalManager.instance.playerCurrentHealth;
            Debug.Log($"[PlayerController] GlobalManager에서 값 로드 - maxHealth: {maxHealth}, currentHealth: {currentHealth}");
        }
        else
        {
            Debug.LogError("[PlayerController] GlobalManager 인스턴스를 찾을 수 없습니다! 기본값으로 초기화됩니다.");
            maxHealth = 100;
            currentHealth = 100;
        }

        Debug.Log($"[PlayerController] UpdateHealthUI 호출 전 - healthBar null?: {healthBar == null}");
        UpdateHealthUIWithDebug(); // 디버그용 메서드 호출
        Debug.Log("[PlayerController] Awake 완료");
    }

    // UpdateHealthUI 직접 호출용 메서드
    private void UpdateHealthUIWithDebug()
    {
        Debug.Log($"[PlayerController] UpdateHealthUI 호출됨 - currentHealth: {currentHealth}, maxHealth: {maxHealth}");

        if (healthBar != null)
        {
            Debug.Log("[PlayerController] healthBar 존재, UpdateHealth 호출");
            healthBar.UpdateHealth(currentHealth, maxHealth);
        }
        else
        {
            Debug.LogError("[PlayerController] healthBar가 null! Inspector에서 할당하세요!");
        }
    }

    public override void TakeDamage(int damage)
    {
        Debug.Log($"[PlayerController] TakeDamage 호출됨 - damage: {damage}, 현재 체력: {currentHealth}");

        base.TakeDamage(damage);

        Debug.Log($"[PlayerController] base.TakeDamage 완료 - 새로운 체력: {currentHealth}");

        // 디버그용 UI 업데이트 호출
        UpdateHealthUIWithDebug();

        // GlobalManager의 현재 체력도 업데이트
        if (GlobalManager.instance != null)
        {
            GlobalManager.instance.playerCurrentHealth = currentHealth;
            Debug.Log($"[PlayerController] GlobalManager 체력 업데이트: {currentHealth}");
        }

        if (healthText != null)
        {
            healthText.text = $"HP: {currentHealth}";
            Debug.Log($"[PlayerController] healthText 업데이트: {healthText.text}");
        }
    }

    public bool IsActing()
    {
        return isActing;
    }
}