using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponStatus : MonoBehaviour
{
    [Header("RGB Stats (0 ~ 255)")]
    public Vector3 currentRGB;

    [Header("Dimming Return Settings")]
    [Tooltip("이 수치 이상부터 성장이 감쇄됩니다.")]
    public float dimmingThreshold = 180f;
    [Tooltip("감쇄 강도 (로그 연산 조절용)")]
    public float dimmingModifier = 0.5f;

    [Header("Visual Feedback")]
    public Renderer weaponRenderer; // 무기 큐브의 렌더러 연결
    [ColorUsage(true, true)] // HDR 설정으로 란_EmissionColor 대응
    private Color baseColor;

    private void Start()
    {
        if (weaponRenderer == null)
            weaponRenderer = GetComponentInChildren<Renderer>();
        
        UpdateWeaponVisual();
    }

    // 몬스터 처치 시 호출하여 RGB를 누적시키는 함수
    public void AddRGB(Vector3 amount)
    {
        // R, G, B 각각 디밍 리턴 공식 적용
        currentRGB.x = CalculateDimmingReturn(currentRGB.x, amount.x);
        currentRGB.y = CalculateDimmingReturn(currentRGB.y, amount.y);
        currentRGB.z = CalculateDimmingReturn(currentRGB.z, amount.z);

        UpdateWeaponVisual();
        CheckWhiteSystem();
    }

    // 디밍 리턴 수식 (Threshold 이상부터는 로그 스케일 감쇄)
    private float CalculateDimmingReturn(float current, float gain)
    {
        if (current >= dimmingThreshold)
        {
            // 한계점 돌파 후에는 자연로그(Mathf.Log)를 활용해 완만하게 증가
            return current + (gain * (1f / (1f + (current - dimmingThreshold) * dimmingModifier)));
        }
        else
        {
            float next = current + gain;
            if (next > dimmingThreshold)
            {
                float over = next - dimmingThreshold;
                return dimmingThreshold + (over * dimmingModifier);
            }
            return Mathf.Clamp(next, 0f, 255f);
        }
    }

    // 무기 머티리얼 색상 및 에미션 실시간 갱신
    private void UpdateWeaponVisual()
    {
        if (weaponRenderer == null) return;

        // 0~255 값을 0~1 값으로 치환
        Color colorTarget = new Color(currentRGB.x / 255f, currentRGB.y / 255f, currentRGB.z / 255f);
        
        // 일반 컬러와 인스펙터 창의 에미션 컬러 동시 적용
        weaponRenderer.material.color = colorTarget;
        weaponRenderer.material.SetColor("_EmissionColor", colorTarget * 2f); // 곱하는 수치로 광량 조절
    }

    // White 시스템 단계 체크 (추후 2번 기능 확장용)
    private void CheckWhiteSystem()
    {
        float minVal = Mathf.Min(currentRGB.x, currentRGB.y, currentRGB.z);
        
        if (minVal >= 240f) { /* 3단계 절대피해 */ }
        else if (minVal >= 160f) { /* 2단계 대시무적 */ }
        else if (minVal >= 80f) { /* 1단계 관통 */ }
    }
}