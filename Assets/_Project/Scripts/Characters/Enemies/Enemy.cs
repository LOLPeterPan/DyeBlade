using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    [Header("Enemy Info")]
    public string enemyName = "Dye Monster";
    public float maxHealth = 30f;
    private float currentHealth;

    [Header("Hit Delay Settings")]
    [Tooltip("피격 후 다시 맞을 때까지 걸리는 무적 시간 (초)")]
    public float invincibilityDuration = 0.5f;
    private float nextTakeDamageTime = 0f;

    [Header("RGB Reward Data")]
    [Tooltip("이 몬스터를 처치할 때 플레이어에게 추출될 RGB 성분량")]
    public Vector3 rgbReward = new Vector3(50f, 0f, 0f); // 예: 빨간색 몬스터

    // [선택 사항] 피격 시 시각적 피드백을 위한 렌더러 참조
    private Renderer enemyRenderer;
    private Color originalColor;

    private void Start()
    {
        currentHealth = maxHealth;
        
        // 큐브의 색상 제어를 위해 Renderer 컴포넌트 캐싱
        enemyRenderer = GetComponent<Renderer>();
        if (enemyRenderer != null)
        {
            originalColor = enemyRenderer.material.color;
        }
    }

    // 플레이어의 분무기 공격을 받았을 때 호출되는 함수
    public Vector3 TakeDamage(float damage)
    {
        // 1. 아직 무적 시간 상태라면 대미지 처리를 완전히 무시
        if (Time.time < nextTakeDamageTime)
        {
            return Vector3.zero; // 아무런 RGB 보상도 주지 않고 리턴
        }

        // 2. 무적 시간이 지났다면 무적 타이머를 즉시 갱신 (현재 시간 + 1초)
        nextTakeDamageTime = Time.time + invincibilityDuration;

        // 3. 체력 차감
        currentHealth -= damage;
        Debug.Log($"{enemyName}이(가) {damage}의 피해를 입음! (남은 체력: {currentHealth}/{maxHealth})");

        // 4. 피격 시 시각적 연출 (빨간색으로 깜빡임 트리거)
        TriggerHitFlash();

        // 5. 사망 판정
        if (currentHealth <= 0)
        {
            return Die();
        }

        // 아직 살아있다면 추출 데이터는 0
        return Vector3.zero; 
    }

    // 깜빡임 연출 (코루틴을 쓰지 않고 Invoke를 활용한 깔끔한 방식)
    private void TriggerHitFlash()
    {
        if (enemyRenderer == null) return;

        // 피격 시 잠시 하얗거나 붉은색으로 변경
        enemyRenderer.material.color = Color.red;
        
        // 0.1초 뒤에 원래 원래 자기 색상으로 복구
        Invoke(nameof(ResetColor), 0.1f);
    }

    private void ResetColor()
    {
        if (enemyRenderer != null)
        {
            enemyRenderer.material.color = originalColor;
        }
    }

    private Vector3 Die()
    {
        Debug.Log($"{enemyName} 처치 완료! RGB 추출 완료: {rgbReward}");
        
        // 몬스터 오브젝트 파괴
        Destroy(gameObject);
        
        // 누적용 RGB 보상 데이터 반환
        return rgbReward;
    }
}