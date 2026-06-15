using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody))] // 3D Rigidbody 필수 지정
public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 8f;
    [SerializeField] private float jumpForce = 12f;
    private float horizontalInput;
    private bool isGrounded;

    [Header("Dash (White System Synergy)")]
    [SerializeField] private float dashSpeed = 16f;
    [SerializeField] private float dashDuration = 0.2f;
    private bool isDashing = false;
    private bool isInvincible = false; // White 2단계 무적 판정용

    [Header("Dye System (Weapon Override)")]
    [SerializeField] private int currentAmmo = 0; 
    private bool isDyeActive = false;

    // 3D 물리 컴포넌트 참조로 수정
    private Rigidbody rb;
    private PlayerWeapon playerWeapon;

    [Header("Ground Check (3D Physics)")]
    [SerializeField] private Transform groundCheckPoint;
    [SerializeField] private float groundCheckRadius = 0.2f;
    [SerializeField] private LayerMask groundLayer; // 바닥 세팅된 3D Layer

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        playerWeapon = GetComponent<PlayerWeapon>();

        // 코딩으로 안전하게 Z축 및 회전 고정 (인스펙터 설정을 깜빡했을 때를 대비)
        rb.constraints = RigidbodyConstraints.FreezePositionZ | RigidbodyConstraints.FreezeRotation;
    }

    private void Update()
    {
        if (isDashing) return;

        horizontalInput = Input.GetAxisRaw("Horizontal");

        // 3D 점프 입력 처리
        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            rb.velocity = new Vector3(rb.velocity.x, jumpForce, 0f);
        }

        if (Input.GetKeyDown(KeyCode.LeftShift) && !isDashing)
        {
            StartCoroutine(DashRoutine());
        }

        if (Input.GetKeyDown(KeyCode.Z)) 
        {
            Attack();
        }
    }

    private void FixedUpdate()
    {
        if (isDashing) return;

        // X축(좌우) 속도는 유지하되, Z축(앞뒤) 속도는 0으로 강제 고정
        rb.velocity = new Vector3(horizontalInput * moveSpeed, rb.velocity.y, 0f);
        
        // 3D 물리 기반의 지면 체크 (OverlapSphere 사용)
        isGrounded = Physics.OverlapSphere(groundCheckPoint.position, groundCheckRadius, groundLayer).Length > 0;
    }

    public void ApplyDye(int ammoAmount)
    {
        currentAmmo = ammoAmount;
        isDyeActive = true;
        Debug.Log($"염료 획득! 탄약: {currentAmmo}발 패턴 오버라이드 활성화");
    }

    private void Attack()
    {
        if (isDyeActive && currentAmmo > 0)
        {
            ExecuteDyeAttack();
        }
        else
        {
            ExecuteBaseAttack();
        }
    }

    private void ExecuteBaseAttack()
    {
        Debug.Log("기본 공격 실행 (Swing 패턴)");
    }

    private void ExecuteDyeAttack()
    {
        Debug.Log("염료 오버라이드 공격 실행 (Projectile 패턴)");

        currentAmmo--;
        if (currentAmmo <= 0)
        {
            isDyeActive = false;
            Debug.Log("염료 소진, 기본 무기로 롤백");
        }
    }

    private IEnumerator DashRoutine()
    {
        isDashing = true;
        
        // 3D 중력 제어
        bool originalUseGravity = rb.useGravity;
        rb.useGravity = false;
        
        float dashDirection = horizontalInput != 0 ? Mathf.Sign(horizontalInput) : 1f;
        rb.velocity = new Vector3(dashDirection * dashSpeed, 0f, 0f);

        if (playerWeapon != null && playerWeapon.whiteStage >= 2)
        {
            isInvincible = true;
            TriggerWhiteShockwave();
        }

        yield return new WaitForSeconds(dashDuration);

        isInvincible = false;
        rb.useGravity = originalUseGravity;
        rb.velocity = new Vector3(0f, rb.velocity.y, 0f);
        isDashing = false;
    }

    private void TriggerWhiteShockwave()
    {
        Debug.Log("White 2단계: 대시 무속성 충격파 발동!");
    }

    public void TakeDamage(int damage)
    {
        if (isInvincible) return;
    }

    // 에디터 뷰에서 기즈모로 바닥 체크 범위 시각화
    private void OnDrawGizmosSelected()
    {
        if (groundCheckPoint == null) return;
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(groundCheckPoint.position, groundCheckRadius);
    }
}