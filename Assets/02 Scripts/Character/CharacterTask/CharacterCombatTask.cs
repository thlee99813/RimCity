using UnityEngine;

public class CharacterCombatTask
{
    private const float BaseDamagePerCombatLevel = 3f;

    public void RunAttackExchange(CharacterEntity owner, EnemyEntity enemy, int smallTurn, SmallTurnLogController log)
    {
        float combatLevel = owner.GetStatLevel(StatType.Combat);
        float playerDamage = Mathf.Max(1f, combatLevel * BaseDamagePerCombatLevel) * owner.Equipment.GetCombatMultiplier();

        enemy.TakeDamage(playerDamage);
        log.AddLog(TextUtil.ApplyKoreanParticles($"[{smallTurn} 턴] {owner.Data.Name}은/는 적을 공격합니다.(-{Mathf.RoundToInt(playerDamage)})"));

        if (enemy == null || enemy.IsDead)
        {
            log.AddLog($"[{smallTurn} 턴] 적이 쓰러졌습니다.");
            owner.AddStatActionCount(StatType.Combat, 1, smallTurn, log);
            return;
        }

        ApplyEnemyRetaliation(owner, enemy, smallTurn, log);
        owner.AddStatActionCount(StatType.Combat, 1, smallTurn, log);
    }

    public void ApplyEnemyRetaliation(CharacterEntity owner, EnemyEntity enemy, int smallTurn, SmallTurnLogController log)
    {
        if (enemy == null || enemy.IsDead) return;

        float damage = enemy.AttackDamage;
        owner.Status.AddHealth(-damage, owner.Data);
        log.AddLog(TextUtil.ApplyKoreanParticles($"[{smallTurn} 턴] {owner.Data.Name}은/는 적에게 데미지를 입습니다.(-{Mathf.RoundToInt(damage)})"));
    }
}
