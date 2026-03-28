using UnityEngine;

public class CharacterEntity : MonoBehaviour
{

    //캐릭터의 기본단위이자, Data, Brain, Condition 을 참조하고있음.

    public CharacterData Data;
    private CharacterBrain _brain = new CharacterBrain();


    public void Initialize(CharacterData data)
    {
        Data = data; 
    }

    private void OnEnable()
    {
        CharacterManager.Instance.Register(this);
    }

    private void OnDisable()
    {
        CharacterManager.Instance.Unregister(this);
    }
    public string RunSmallTurn(BigTurnSelectionData selection, int bigTurn, int smallTurn)
    {
        SmallTurnActionType action = _brain.DecideSmallTurnAction(Data, selection);
        return TextUtil.ApplyKoreanParticles($"[{smallTurn} 턴] {Data.Name}은/는 {ToActionText(action)}");

        
    }
    private string ToActionText(SmallTurnActionType action) 
    { 
        switch (action)
        {
            case SmallTurnActionType.Idle: return "잠시 멈춰 서 있습니다.";
            case SmallTurnActionType.Wander: return "주변을 서성거립니다.";
            case SmallTurnActionType.Gather: return "자원을 수집하려고 이동합니다.";
            case SmallTurnActionType.Craft: return "제작 작업을 진행합니다.";
            case SmallTurnActionType.Build: return "건축 작업을 진행합니다.";
            case SmallTurnActionType.Social: return "주변 사람과 대화하려 합니다.";
            case SmallTurnActionType.Rest: return "잠시 휴식을 취합니다.";
            case SmallTurnActionType.Eat: return "음식을 찾아 먹습니다.";
            default: return "무언가를 고민합니다.";
        } 
    }


}
