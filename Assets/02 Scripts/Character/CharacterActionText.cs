public static class CharacterActionText
{
    public static string ToActionText(SmallTurnActionType action)
    {
        switch (action)
        {
            case SmallTurnActionType.Idle: return "잠시 멈춰 있습니다.";
            case SmallTurnActionType.Wander: return "주변을 서성거립니다.";
            case SmallTurnActionType.Gather: return "자원을 수집하러 이동합니다.";
            case SmallTurnActionType.Craft: return "제작 작업을 진행합니다.";
            case SmallTurnActionType.Build: return "건설 작업을 진행합니다.";
            case SmallTurnActionType.Social: return "주민과 대화합니다.";
            case SmallTurnActionType.Rest: return "잠시 휴식을 취합니다.";
            case SmallTurnActionType.Eat: return "음식을 찾아 먹습니다.";
            case SmallTurnActionType.MoveToShelter: return "보호 구조물 쪽으로 이동합니다.";

            default: return "무언가를 고민합니다.";
        }
    }
}
