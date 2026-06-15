using UnityEngine;

/// <summary>
/// 수식어(Modifier) 간의 상호작용 예외 처리를 최소화하기 위한 Event-driven 공용 인터페이스입니다.
/// 모든 수식어 아이템(인챈트)은 이 인터페이스를 구현하여 플레이어의 이벤트 파이프라인에 참여합니다.
/// </summary>
public interface IModifier
{
    string ModifierName { get; }  // 수식어의 이름 (예: 프리즘, 폭발)
    int Level { get; }            // 수식어 레벨 (합성을 통해 최대 3레벨까지 성장)

    /// <summary>
    /// 플레이어에게 수식어가 장착될 때 호출되어 이벤트를 구독합니다.
    /// </summary>
    void OnEquip(GameObject player);

    /// <summary>
    /// 수식어가 해제되거나 사망 시 호출되어 구독했던 이벤트를 안전하게 해제합니다.
    /// </summary>
    void OnUnequip(GameObject player);
}

/// <summary>
/// 인터페이스를 활용한 '조건 발동형' 수식어 구현의 간단한 예시 구조입니다.
/// </summary>
public class ExampleOnHitModifier : IModifier
{
    public string ModifierName => "색상 증폭형 프리즘";
    public int Level { get; private set; }

    public ExampleOnHitModifier(int level)
    {
        Level = level;
    }

    public void OnEquip(GameObject player)
    {
        // 예시: 플레이어의 공격 델리게이트를 찾아 OnEnemyHit 이벤트를 구독시킵니다.
        // player.GetComponent<PlayerAttack>().OnEnemyHit += AttackSynergyLogic;
    }

    public void OnUnequip(GameObject player)
    {
        // 메모리 누수 및 예외 처리 방지를 위한 구독 해제
        // player.GetComponent<PlayerAttack>().OnEnemyHit -= AttackSynergyLogic;
    }

    private void AttackSynergyLogic(GameObject target)
    {
        // 12종 수식어 간 충돌 없이 깔끔하게 독립 동작하는 핵심 로직 기술
        Debug.Log($"{ModifierName} 레벨 {Level} 효과 발동!");
    }
}