using UnityEngine;
using System;

public class PlayerWeapon : MonoBehaviour
{
    [Header("RGB Data Structure")]
    [SerializeField] private Vector3 currentRGB = Vector3.zero; // X:R, Y:G, Z:B
    private const float DIMMING_THRESHOLD = 180f; // 감쇄 시작 임계치

    [Header("White System State")]
    public int whiteStage = 0; // 0~3 단계
    public bool bCanEnterWhiteZone = false;

    [Header("Visual Feedback")]
    [SerializeField] private Material weaponMaterial; // 프리미티브 큐브의 셰이더 제어용
    private static readonly int EmissionColorHash = Shader.PropertyToID("_EmissionColor");
    private static readonly int ColorHash = Shader.PropertyToID("_Color");

    // UI 및 이펙트 연동용 이벤트
    public event Action<Vector3> OnRGBChanged;
    public event Action<int> OnWhiteStageChanged;

    private void Start()
    {
        UpdateVisualFeedback();
    }

    /// <summary>
    /// 몬스터 처치 시 RGB 경험치 누적 (디밍 리턴 적용)
    /// </summary>
    public void AddRGBExperience(float r, float g, float b)
    {
        currentRGB.x = CalculateDimmingReturn(currentRGB.x, r);
        currentRGB.y = CalculateDimmingReturn(currentRGB.y, g);
        currentRGB.z = CalculateDimmingReturn(currentRGB.z, b);

        OnRGBChanged?.Invoke(currentRGB);
        CheckWhiteSystem();
        UpdateVisualFeedback();
    }

    /// <summary>
    /// 디밍 리턴 감쇄 곡선 수식 (Log 적용)
    /// </summary>
    private float CalculateDimmingReturn(float current, float amount)
    {
        if (current >= DIMMING_THRESHOLD)
        {
            // 임계치 이상일 경우 완만하게 증가 (Log 수식 적용)
            return Mathf.Clamp(current + (amount * (1f / (1f + Mathf.Log(current - DIMMING_THRESHOLD + 2f)))), 0f, 255f);
        }
        
        float next = current + amount;
        if (next > DIMMING_THRESHOLD)
        {
            float over = next - DIMMING_THRESHOLD;
            return DIMMING_THRESHOLD + (over * 0.5f); // 경계면 부드러운 전이용 기본 감쇄
        }

        return Mathf.Clamp(next, 0f, 255f);
    }

    /// <summary>
    /// White 시스템 3단계 개화 메커니즘 체크
    /// </summary>
    private void CheckWhiteSystem()
    {
        float minRGB = Mathf.Min(currentRGB.x, Mathf.Min(currentRGB.y, currentRGB.z));
        int previousStage = whiteStage;

        if (minRGB >= 240f)
        {
            whiteStage = 3;
            bCanEnterWhiteZone = true; // 최종 스테이지 진입 플래그 활성화
        }
        else if (minRGB >= 160f)
        {
            whiteStage = 2;
        }
        else if (minRGB >= 80f)
        {
            whiteStage = 1;
        }
        else
        {
            whiteStage = 0;
        }

        if (previousStage != whiteStage)
        {
            OnWhiteStageChanged?.Invoke(whiteStage);
            ApplyWhiteStageEffects();
        }
    }

    private void ApplyWhiteStageEffects()
    {
        switch (whiteStage)
        {
            case 1:
                Debug.Log("White 1단계 발동: 속성 저항 10% 무시");
                break;
            case 2:
                Debug.Log("White 2단계 발동: 대시 시 무적 및 무속성 충격파");
                break;
            case 3:
                Debug.Log("White 3단계 발동: 무기 완전 White 고정, 절대 피해 전환");
                break;
        }
    }

    /// <summary>
    /// 무기 Material 실시간 갱신
    /// </summary>
    private void UpdateVisualFeedback()
    {
        if (weaponMaterial == null) return;

        if (whiteStage == 3)
        {
            Color whiteColor = Color.white * 3f; // Emission 증폭
            weaponMaterial.SetColor(ColorHash, Color.white);
            weaponMaterial.SetColor(EmissionColorHash, whiteColor);
            return;
        }

        Color renderColor = new Color(currentRGB.x / 255f, currentRGB.y / 255f, currentRGB.z / 255f);
        weaponMaterial.SetColor(ColorHash, renderColor);
        weaponMaterial.SetColor(EmissionColorHash, renderColor * 1.5f);
    }

    public Vector3 GetRGB() => currentRGB;
}