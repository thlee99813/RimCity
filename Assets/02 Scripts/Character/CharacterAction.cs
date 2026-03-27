using UnityEngine;
using System.Collections.Generic;

public abstract class CharacterAction
{
    public abstract string Id { get; }
    public virtual bool CanExecute(CharacterEntity self, TurnContext ctx) => true;
    public abstract float GetWeight(CharacterData data, TurnContext ctx);
    public abstract ActionResult Execute(CharacterEntity self, TurnContext ctx);
}
