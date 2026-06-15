using UnityEngine;

public class TrainingRoomTester : MonoBehaviour
{
    private PlayerWeapon weapon;
    private PlayerController controller;

    private void Awake()
    {
        weapon = GetComponent<PlayerWeapon>();
        controller = GetComponent<PlayerController>();
    }

    private void Update()
    {
        // [테스트 키 1, 2, 3] 각 속성 몬스터 처치 상황 시뮬레이션 (RGB 경험치 30씩 획득) [cite: 273]
        if (Input.GetKeyDown(KeyCode.Alpha1)) weapon.AddRGBExperience(30f, 0f, 0f); // Red 획득 [cite: 273]
        if (Input.GetKeyDown(KeyCode.Alpha2)) weapon.AddRGBExperience(0f, 30f, 0f); // Green 획득 [cite: 273]
        if (Input.GetKeyDown(KeyCode.Alpha3)) weapon.AddRGBExperience(0f, 0f, 30f); // Blue 획득 [cite: 273]

        // [테스트 키 4] 균등하게 성장시켜 White 시스템 단계 확인용 (80 -> 160 -> 240) [cite: 277, 278]
        if (Input.GetKeyDown(KeyCode.Alpha4)) weapon.AddRGBExperience(40f, 40f, 40f);

        // [테스트 키 5] 염료 아이템 획득 시뮬레이션 (탄약 10발 부여) [cite: 279, 281]
        if (Input.GetKeyDown(KeyCode.Alpha5)) controller.ApplyDye(10);
    }

    private void OnGUI()
    {
        // 화면 침범 최소화 가이드라인 준수, 좌측 상단 디버그 정보 표시 [cite: 302]
        if (weapon == null) return;
        
        GUILayout.BeginArea(new Rect(10, 10, 300, 150));
        GUILayout.Label($"[Training Room Debug UI]");
        GUILayout.Label($"Weapon RGB: {weapon.GetRGB()}");
        GUILayout.Label($"White Stage: {weapon.whiteStage} (Can Enter White Zone: {weapon.bCanEnterWhiteZone})");
        GUILayout.Label($"Controls: 1/2/3=RGB공급, 4=전체RGB공급, 5=염료충전(Z공격)");
        GUILayout.EndArea();
    }
}   