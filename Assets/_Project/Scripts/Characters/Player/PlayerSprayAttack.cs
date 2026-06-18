using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSprayAttack : MonoBehaviour
{
    [Header("Attack Settings")]
    [Tooltip("공격 연사 속도 (초 단위 쿨다운)")]
    public float attackCooldown = 0.15f;
    [Tooltip("분무기 공격 사거리 (반지름)")]
    public float attackRange = 5f;
    [Range(0f, 360f)]
    [Tooltip("분무기 공격 발사 각도 (부채꼴의 총 각도)")]
    public float attackAngle = 60f;
    [Tooltip("타격당 입히는 대미지")]
    public float damage = 5f;
    [Tooltip("타격 판정을 적용할 에너미 레이어")]
    public LayerMask enemyLayer;

    [Header("VFX Reference")]
    [Tooltip("분무기 가루 파티클 시스템")]
    public ParticleSystem mistParticle;

    private Camera mainCamera;
    private float nextAttackTime = 0f;

    private void Start()
    {
        mainCamera = Camera.main;
        
        if (mistParticle != null) 
        {
            mistParticle.Stop();
        }
    }

    private void Update()
    {
        // 1. 마우스 조향 (캐릭터가 마우스를 바라보도록 회전)
        RotateTowardsMouse();

        // 2. 공격 처리 (꾹 누르기 + 광클 대응 타이머)
        if (Input.GetMouseButton(0))
        {
            if (Time.time >= nextAttackTime)
            {
                nextAttackTime = Time.time + attackCooldown;
                ExecuteAttack();
            }
        }

        // 3. 마우스를 떼면 즉시 파티클 중지
        if (Input.GetMouseButtonUp(0))
        {
            StopAttack();
        }
    }

    /// <summary>
    /// 정해진 쿨다운 주기마다 실행되는 공격 및 피격 판정 로직
    /// </summary>
    private void ExecuteAttack()
    {
        // 파티클 중복 재생 방지 및 실행
        if (mistParticle != null && !mistParticle.isPlaying)
        {
            mistParticle.Play();
        }

        // -------------------------------------------------------------
        // 🎯 [부채꼴 범위 피격 판정 및 적 상호작용 영역]
        // -------------------------------------------------------------
        // 1. 사거리(반지름) 내에 있는 모든 적의 Collider를 먼저 둥글게 검출합니다.
        Collider[] targetsInMinusRadius = Physics.OverlapSphere(transform.position, attackRange, enemyLayer);

        foreach (Collider enemyCollider in targetsInMinusRadius)
        {
            // 2. 적이 위치한 방향 벡터를 구합니다.
            Vector3 directionToTarget = (enemyCollider.transform.position - transform.position).normalized;

            // 3. 플레이어가 바라보는 정면(transform.forward)과 적의 방향 사이의 각도를 측정합니다.
            float angleToTarget = Vector3.Angle(transform.forward, directionToTarget);

            // 4. 측정된 각도가 설정한 '공격 각도 / 2' 이내라면 부채꼴 범위 안에 있는 것입니다.
            if (angleToTarget <= attackAngle / 2f)
            {
                // 5. 적 스크립트(Enemy)와 상호작용하여 대미지를 전달합니다.
                // (※ 사용하시는 적 스크립트의 컴포넌트 이름과 대미지 메서드 명에 맞게 수정해 주세요)
                Enemy enemy = enemyCollider.GetComponent<Enemy>();
                if (enemy != null)
                {
                    enemy.TakeDamage(damage); 
                    Debug.Log($"{enemyCollider.name}에게 {damage}의 대미지를 입혔습니다!");
                }
            }
        }
    }

    private void StopAttack()
    {
        if (mistParticle != null && mistParticle.isPlaying)
        {
            mistParticle.Stop();
        }
    }

    private void RotateTowardsMouse()
    {
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        Plane playerPlane = new Plane(Vector3.forward, new Vector3(0, 0, transform.position.z));

        if (playerPlane.Raycast(ray, out float enterDistance))
        {
            Vector3 mouseWorldPos = ray.GetPoint(enterDistance);
            Vector3 lookDirection = (mouseWorldPos - transform.position).normalized;
            
            if (lookDirection != Vector3.zero)
            {
                transform.forward = lookDirection;
            }
        }
    }

    // 유니티 씬(Scene) 뷰에서 공격 범위(사거리 및 각도)를 시각적으로 보여주는 기즈모
    private void OnDrawGizmosSelected()
    {
        // 사거리 원 그리기
        Gizmos.color = new Color(1f, 0f, 0f, 0.2f);
        Gizmos.DrawWireSphere(transform.position, attackRange);

        // 부채꼴의 좌우 경계선 그리기
        Vector3 leftBoundary = Quaternion.Euler(0, -attackAngle / 2f, 0) * transform.forward;
        Vector3 rightBoundary = Quaternion.Euler(0, attackAngle / 2f, 0) * transform.forward;

        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position, transform.position + leftBoundary * attackRange);
        Gizmos.DrawLine(transform.position, transform.position + rightBoundary * attackRange);
    }
}