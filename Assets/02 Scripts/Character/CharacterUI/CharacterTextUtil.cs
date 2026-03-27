public static class CharacterTextUtil
{
    public static string TranslateKorean(Gender g)
    {
        return g switch
        {
            Gender.Male => "남성",
            Gender.Female => "여성",
            _ => "알수없음"
        };
    }

    public static string TranslateKorean(TraitType t)
    {
        return t switch
        {
            TraitType.Hikikomori => "히키코모리",
            TraitType.Psychopath => "사이코패스",
            TraitType.MisogynyMisandry => "성별 혐오",
            TraitType.Depression => "우울증",
            TraitType.Kind => "다정다감",
            TraitType.Tough => "강인함",
            TraitType.Diligent => "근면함",
            TraitType.Optimistic => "낙천적",
            _ => t.ToString()
        };
    }

    public static string TranslateKorean(StatType s)
    {
        return s switch
        {
            StatType.Combat => "전투",
            StatType.Craft => "제작",
            StatType.Build => "건축",
            StatType.Gather => "수집",
            StatType.Social => "매력",
            _ => s.ToString()
        };
    }
}
