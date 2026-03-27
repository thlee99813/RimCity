using UnityEngine;

public class CharacterEntity : MonoBehaviour
{

    //캐릭터의 기본단위이자, Data, Brain, Condition 을 참조하고있음.

    public CharacterData Data;

    public CharacterBrain Brain;
    public CharacterConditionController Condition;

    public void Initialize(CharacterData data)
    {
        Data = data; 
        Brain = new CharacterBrain();
        Condition = new CharacterConditionController();
    }

    public string RunSmallTurn(TurnContext ctx)
    {
        Condition.Tick(Data, ctx); 
        CharacterAction action = Brain.DecideAction(Data, ctx);
        ActionResult result = action.Execute(this, ctx);
        Condition.ApplyActionResult(Data, result, ctx);
        return result.LogText;
    }
    private void OnEnable()
    {
        CharacterManager.Instance.Register(this);
    }

    private void OnDisable()
    {
        CharacterManager.Instance.Unregister(this);
    }

}
