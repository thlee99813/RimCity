using UnityEngine;

public class CharacterLifeController
{
    public bool TryHandleDeath(CharacterEntity owner, int smallTurn, SmallTurnLogController logController)
    {
        if (owner.IsDead) return true;
        if (owner.Status.Health > 0f) return false;

        owner.MarkDead();
        logController.AddLog(TextUtil.ApplyKoreanParticles($"[{smallTurn} 턴] {owner.Data.Name}은/는 죽었습니다."));
        UIManager.Instance.RemoveCharacterSlot(owner.Data.Id);
        Object.Destroy(owner.gameObject);
        return true;
    }
}
