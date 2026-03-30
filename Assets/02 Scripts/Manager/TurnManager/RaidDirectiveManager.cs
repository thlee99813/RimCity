using UnityEngine;

public class RaidDirectiveManager : Singleton<RaidDirectiveManager>
{
    public EnemyGenerator CurrentTarget { get; private set; }

    protected override void Init() { }

    public void SetTarget(EnemyGenerator target)
    {
        if (target == null || target.IsDestroyed)
        {
            CurrentTarget = null;
            return;
        }

        CurrentTarget = target;
    }

    public void ClearTarget()
    {
        CurrentTarget = null;
    }

    public bool TryGetTarget(out EnemyGenerator target)
    {
        if (CurrentTarget == null || CurrentTarget.IsDestroyed)
        {
            CurrentTarget = null;
            target = null;
            return false;
        }

        target = CurrentTarget;
        return true;
    }
}
