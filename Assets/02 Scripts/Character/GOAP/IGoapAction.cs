using System.Collections;

public enum GoapActionStatus
{
    Ready,
    Running,
    Success,
    Failure
}

public interface IGoapAction
{
    string Name { get; }
    GoapActionStatus Status { get; }
    bool ArePreconditionsMet(GoapWorldState state, GoapContext context);
    GoapWorldState ApplyEffects(GoapWorldState state, GoapContext context);
    int GetCost(GoapWorldState state, GoapContext context);
    bool CanStart(GoapContext context);
    bool CanContinue(GoapContext context);
    IEnumerator ExecuteTurn(GoapContext context);
    void ResetRuntimeState();
}

public abstract class GoapAction : IGoapAction
{
    public abstract string Name { get; }
    public GoapActionStatus Status { get; protected set; } = GoapActionStatus.Ready;

    public abstract bool ArePreconditionsMet(GoapWorldState state, GoapContext context);
    public abstract GoapWorldState ApplyEffects(GoapWorldState state, GoapContext context);
    public abstract int GetCost(GoapWorldState state, GoapContext context);
    public abstract bool CanStart(GoapContext context);
    public abstract bool CanContinue(GoapContext context);
    public abstract IEnumerator ExecuteTurn(GoapContext context);

    public virtual void ResetRuntimeState()
    {
        Status = GoapActionStatus.Ready;
    }
}
