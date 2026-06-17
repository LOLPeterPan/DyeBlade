using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody))] // 3D Rigidbody 필수 지정
public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 8f;
    private float horizontalInput;
    private float currentSpeed;               // 현재 적용 중인 실시간 속도

    [Header("Inertia (관성 세팅)")]
    [Tooltip("수치가 높을수록 마찰력이 낮아져 미끄러지는 관성이 강해집니다. (추천: 0.05 ~ 0.15)")]
    public float accelerationTime = 0.08f;    // 최고 속도까지 도달하는 시간 (가속)
    public float decelerationTime = 0.06f;    // 완전히 멈추는 데 걸리는 시간 (감속)
    private float speedVelocity;              // Mathf.SmoothDamp 연산용 내부 변수

    [Header("Hierarchy References")]
    public Transform playerBody;              
    public Transform cameraContainer;         

    [Header("Body Rotation (Mouse Based)")]
    public float rotationSpeed = 15f;          
    public float maxPitchAngle = 10f;          // 위아래로 쳐다볼 최대 각도 (10도)
    private Quaternion targetBodyRotation;     

    [Header("Jump")]
    public float jumpForce = 10f;       
    public float fallMultiplier = 3.5f;       
    public float jumpCutMultiplier = 5f;      

    [Header("Double Jump")]
    private int jumpCount = 0;          
    public int maxJumpCount = 2;        

    [Header("Jump Time Limit")]
    public float maxJumpHoldTime = 0.3f;      
    private float jumpTimeCounter;
    private bool isJumping;

    [Header("Ground Check")]
    public Transform groundCheck;
    public float checkRadius = 0.2f;
    public LayerMask groundLayer;
    private bool isGrounded;

    [Header("Dynamic Camera Settings")]
    public float lookAheadDistance = 3f;      
    public float lookAheadSpeed = 4f;         
    private float currentLookAheadX;          
    private Vector3 initialContainerPos;      

    private Rigidbody rb;
    private Camera mainCam;

    void Start()
    {
        rb = GetComponent<Rigidbody>() == null ? gameObject.AddComponent<Rigidbody>() : GetComponent<Rigidbody>();
        rb.constraints = RigidbodyConstraints.FreezePositionZ | RigidbodyConstraints.FreezeRotation;
        
        mainCam = Camera.main;

        if (cameraContainer != null)
        {
            initialContainerPos = cameraContainer.localPosition;
        }

        targetBodyRotation = Quaternion.Euler(0, -90, 0);
    }

    void Update()
    {
        // 1. 입력 받기 및 바닥 체크
        horizontalInput = Input.GetAxisRaw("Horizontal");
        isGrounded = Physics.CheckSphere(groundCheck.position, checkRadius, groundLayer);

        if (isGrounded && rb.velocity.y <= 0)
        {
            jumpCount = 0;
        }

        // 2. 점프 시스템
        if (Input.GetButtonDown("Jump"))
        {
            if (isGrounded || jumpCount < maxJumpCount)
            {
                jumpCount++;
                isJumping = true;
                jumpTimeCounter = maxJumpHoldTime;

                float currentJumpForce = jumpForce;
                if (jumpCount == 2)
                {
                    currentJumpForce = jumpForce * (2f / 3f);
                }

                rb.velocity = new Vector3(rb.velocity.x, currentJumpForce, 0f);
            }
        }

        if (Input.GetButton("Jump") && isJumping)
        {
            if (jumpTimeCounter > 0)
            {
                jumpTimeCounter -= Time.deltaTime;
            }
            else
            {
                isJumping = false; 
            }
        }

        if (Input.GetButtonUp("Jump"))
        {
            isJumping = false;
        }

        // 3. 마우스 위치 기반 좌우 각도 및 X축 중심 상하 회전
        if (mainCam != null && playerBody != null)
        {
            Vector3 playerScreenPos = mainCam.WorldToScreenPoint(transform.position);
            Vector3 mouseScreenPos = Input.mousePosition;

            float targetYRotation = 0f;
            float targetXRotation = 0f;

            float angleRad = Mathf.Atan2(mouseScreenPos.y - playerScreenPos.y, mouseScreenPos.x - playerScreenPos.x);
            float angleDeg = angleRad * Mathf.Rad2Deg;

            if (mouseScreenPos.x >= playerScreenPos.x)
            {
                targetYRotation = -90f;
                targetXRotation = Mathf.Clamp(angleDeg, -maxPitchAngle, maxPitchAngle);
            }
            else
            {
                targetYRotation = 90f;

                float leftAngle = angleDeg;
                if (leftAngle > 0) leftAngle = 180f - leftAngle;
                else leftAngle = -180f - leftAngle;

                targetXRotation = Mathf.Clamp(leftAngle, -maxPitchAngle, maxPitchAngle);
            }

            targetBodyRotation = Quaternion.Euler(targetXRotation, targetYRotation, 0f);
            playerBody.localRotation = Quaternion.Slerp(playerBody.localRotation, targetBodyRotation, rotationSpeed * Time.deltaTime);
        }

        // 4. 마우스 위치 기반 다이나믹 카메라 (CamPos) 조작
        if (cameraContainer != null && mainCam != null)
        {
            float mouseNormalizedX = (Input.mousePosition.x - (Screen.width / 2f)) / (Screen.width / 2f);
            float targetLookAheadX = mouseNormalizedX * lookAheadDistance;

            currentLookAheadX = Mathf.Lerp(currentLookAheadX, targetLookAheadX, lookAheadSpeed * Time.deltaTime);

            cameraContainer.localPosition = new Vector3(
                initialContainerPos.x + currentLookAheadX,
                initialContainerPos.y,
                initialContainerPos.z
            );
        }
    }

    void FixedUpdate()
    {
        // 5. [핵심 수정] 관성이 적용된 속도 계산 (Mathf.SmoothDamp 활용)
        // 입력이 있으면 가속 시간(accelerationTime), 입력이 없으면 감속 시간(decelerationTime)을 적용합니다.
        float targetSpeed = horizontalInput * moveSpeed;
        float currentSmoothing = (horizontalInput != 0) ? accelerationTime : decelerationTime;

        currentSpeed = Mathf.SmoothDamp(currentSpeed, targetSpeed, ref speedVelocity, currentSmoothing);

        // 계산된 관성 속도를 Rigidbody에 대입
        rb.velocity = new Vector3(currentSpeed, rb.velocity.y, 0f);

        // 6. 하강 중력 보정
        if (rb.velocity.y < 0)
        {
            rb.velocity += Vector3.up * Physics.gravity.y * (fallMultiplier - 1) * Time.fixedDeltaTime;
        }
        else if (rb.velocity.y > 0 && !isJumping)
        {
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