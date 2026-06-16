using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody))] // 3D Rigidbody 필수 지정
public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 8f;
    private float horizontalInput;

    [Header("Rotation")]
    public float rotationSpeed = 10f;          // 회전 속도 변수입니다. 값이 클수록 더 빠르게 돕니다.
    private Quaternion targetRotation;         // 목표 회전 값을 저장할 변수

    [Header("Jump")]
    public float jumpForce = 10f;       
    public float fallMultiplier = 3.5f;       // [수정] 떨어질 때의 중력 배율을 높여 뚝 떨어지게 만듭니다 (기존 2.5f -> 3.5f)
    public float jumpCutMultiplier = 5f;      // [추가] 점프 도중 키를 뗐을 때 상승을 강하게 끊어주는 배율입니다.

    [Header("Double Jump")]
    private int jumpCount = 0;          
    public int maxJumpCount = 2;        

    [Header("Jump Time Limit")]
    public float maxJumpHoldTime = 0.3f;      // [추가] 스페이스바를 꾹 누를 수 있는 최대 시간 (예: 0.3초 지나면 자동으로 상승 종료)
    private float jumpTimeCounter;
    private bool isJumping;

    [Header("Ground Check")]
    public Transform groundCheck;
    public float checkRadius = 0.2f;
    public LayerMask groundLayer;
    private bool isGrounded;

    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>() == null ? gameObject.AddComponent<Rigidbody>() : GetComponent<Rigidbody>();
        rb.constraints = RigidbodyConstraints.FreezePositionZ | RigidbodyConstraints.FreezeRotation;
    }

    void Update()
    {
        // 1. 입력 받기 및 바닥 체크
        horizontalInput = Input.GetAxisRaw("Horizontal");
        isGrounded = Physics.CheckSphere(groundCheck.position, checkRadius, groundLayer);

        // 바닥에 안정적으로 착지하면 점프 횟수 초기화
        if (isGrounded && rb.velocity.y <= 0)
        {
            jumpCount = 0;
        }

        // 2. 점프 시작 (스페이스바를 최초로 누른 순간)
        if (Input.GetButtonDown("Jump"))
        {
            if (isGrounded || jumpCount < maxJumpCount)
            {
                jumpCount++;
                isJumping = true;
                jumpTimeCounter = maxJumpHoldTime; // 최대 상승 제한 타이머 리셋

                // 첫 점프와 2단 점프의 힘 차등 적용 (2번째는 2/3 강도)
                float currentJumpForce = jumpForce;
                if (jumpCount == 2)
                {
                    currentJumpForce = jumpForce * (2f / 3f);
                }

                rb.velocity = new Vector3(rb.velocity.x, currentJumpForce, 0f);
            }
        }

        // 3. 점프 키를 유지하고 있을 때 (최대 제한 시간 동안만 상승 보장)
        if (Input.GetButton("Jump") && isJumping)
        {
            if (jumpTimeCounter > 0)
            {
                // 시간에 따라 타이머를 깎아 나갑니다.
                jumpTimeCounter -= Time.deltaTime;
            }
            else
            {
                // 설정한 시간(maxJumpHoldTime)을 초과하면 강제로 상승 상태를 해제합니다.
                isJumping = false; 
            }
        }

        // 4. 점프 키에서 손을 떼거나 강제 종료되었을 때 상승 속도를 급격히 제어
        if (Input.GetButtonUp("Jump"))
        {
            isJumping = false;
        }

        // 5. [핵심 수정] 엉덩이가 보이지 않도록 Y축 회전 각도 수정
        // 오른쪽 이동(AD 중 D) : 0도 (카메라를 정면으로 바라보는 기준에서 오른쪽 배치)
        // 왼쪽 이동(AD 중 A) : 180도 (정면을 유지한 채 180도 회전하므로 뒷면이 안 보임)
        if (horizontalInput > 0) 
        {
            targetRotation = Quaternion.Euler(0, -90, 0);
        }
        else if (horizontalInput < 0) 
        {
            targetRotation = Quaternion.Euler(0, 90, 0);
        }

        // 부드러운 구면 선형 보간 회전 실행
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
    }

    void FixedUpdate()
    {
        // 6. 좌우 이동 적용
        rb.velocity = new Vector3(horizontalInput * moveSpeed, rb.velocity.y, 0f);

        // 7. 속도감 있는 낙하를 위한 물리 법칙 세부 제어 (체공시간 단축 핵심)
        if (rb.velocity.y < 0)
        {
            // 하강 중일 때는 묵직하고 빠르게 툭 떨어지도록 높은 중력을 적용합니다.
            rb.velocity += Vector3.up * Physics.gravity.y * (fallMultiplier - 1) * Time.fixedDeltaTime;
        }
        else if (rb.velocity.y > 0 && !isJumping)
        {
            // 스페이스바를 뗐거나 최대 시간에 도달해 상승이 끊겼을 때, 위로 향하던 관성을 강제로 억제해 즉시 하강하도록 만듭니다.
            rb.velocity += Vector3.up * Physics.gravity.y * (jumpCutMultiplier - 1) * Time.fixedDeltaTime;
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(groundCheck.position, checkRadius);
        }
    }
}