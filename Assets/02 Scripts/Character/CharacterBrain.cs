using UnityEngine;

public class CharacterBrain

//캐릭터가 어떤 액션을 할지 결정해줌
{
    public CharacterAction DecideAction(CharacterData data, TurnContext ctx)
    {
        return new IdleAction();
    }
}

