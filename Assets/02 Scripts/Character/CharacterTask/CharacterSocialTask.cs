using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum SocialInteractionType
{
    Talk,
    Flex,
    DeepTalk,
    Love
}

public class CharacterSocialTask
{
    public bool IsForced { get; private set; }

    private CharacterEntity _target;
    private SocialInteractionType _interaction;

    public IEnumerator RunTurn(CharacterEntity owner, int smallTurn, List<TileNode> activeNodes, SmallTurnLogController log, int maxMoveTilesPerTurn, int socialLevel)
    {
        if (!IsForced)
        {
            if (!TryFindTarget(owner, activeNodes, maxMoveTilesPerTurn * 2, out _target))
            {
                log.AddLog(TextUtil.ApplyKoreanParticles($"[{smallTurn} 턴] {owner.Data.Name}은/는 허공에 혼잣말을 뱉습니다."));
                yield break;
            }

            int triggerChance = Mathf.Clamp(30 + socialLevel * 4, 30, 70);
            if (Random.Range(0, 100) >= triggerChance)
            {
                log.AddLog(TextUtil.ApplyKoreanParticles($"[{smallTurn} 턴] {owner.Data.Name}은/는 허공에 혼잣말을 뱉습니다."));
                yield break;
            }

            _interaction = PickInteraction(socialLevel);
            IsForced = true;
        }

        if (_target == null || _target.IsDead || _target.CurrentTileNode == null)
        {
            Clear();
            log.AddLog(TextUtil.ApplyKoreanParticles($"[{smallTurn} 턴] {owner.Data.Name}은/는 허공에 혼잣말을 뱉습니다."));
            yield break;
        }

        if (owner.CurrentTileNode != _target.CurrentTileNode)
        {
            List<TileNode> path = CharacterTaskCommon.FindPath(owner.CurrentTileNode, _target.CurrentTileNode, activeNodes);
            if (path == null || path.Count == 0)
            {
                Clear();
                log.AddLog(TextUtil.ApplyKoreanParticles($"[{smallTurn} 턴] {owner.Data.Name}은/는 허공에 혼잣말을 뱉습니다."));
                yield break;
            }

            int moveCount = Mathf.Min(maxMoveTilesPerTurn, path.Count);
            log.AddLog(TextUtil.ApplyKoreanParticles($"[{smallTurn} 턴] {owner.Data.Name}은/는 {_target.Data.Name}에게 다가갑니다. ({moveCount}칸)"));

            for (int i = 0; i < moveCount; i++)
                yield return owner.MoveToTile(path[i]);

            yield break;
        }

        ApplyInteraction(owner, _target, socialLevel, smallTurn, log);
        owner.AddStatActionCount(StatType.Social, 1, smallTurn, log);
        Clear();
    }

    private void ApplyInteraction(CharacterEntity owner, CharacterEntity target, int socialLevel, int smallTurn, SmallTurnLogController log)
    {
        int vTalk = Mathf.CeilToInt(socialLevel * 1.5f);
        int vFlex = Mathf.CeilToInt(socialLevel * 1.2f);
        int vDeep = Mathf.CeilToInt(socialLevel * 1.3f);
        int vLove = Mathf.CeilToInt(socialLevel * 2.0f);

        switch (_interaction)
        {
            case SocialInteractionType.Talk:
                owner.Status.AddMood(vTalk, owner.Data);
                target.Status.AddMood(vTalk, target.Data);
                log.AddLog(TextUtil.ApplyKoreanParticles($"[{smallTurn} 턴] {owner.Data.Name}은/는 {target.Data.Name}와 대화를 나눕니다. (기분 +{vTalk})"));
                owner.AddStatActionCount(StatType.Social, 1, smallTurn, log);
                break;

            case SocialInteractionType.Flex:
                owner.Status.AddHealth(vFlex, owner.Data);
                target.Status.AddHealth(vFlex, target.Data);
                

                log.AddLog(TextUtil.ApplyKoreanParticles($"[{smallTurn} 턴] {owner.Data.Name}은/는 {target.Data.Name}에게 근육을 자랑합니다. (체력 +{vFlex})"));
                owner.AddStatActionCount(StatType.Social, 1, smallTurn, log);

                break;

            case SocialInteractionType.DeepTalk:
                owner.Status.AddHealth(vDeep, owner.Data);
                owner.Status.AddMood(vDeep, owner.Data);
                target.Status.AddHealth(vDeep, target.Data);
                target.Status.AddMood(vDeep, target.Data);
                log.AddLog(TextUtil.ApplyKoreanParticles($"[{smallTurn} 턴] {owner.Data.Name}은/는 {target.Data.Name}와 깊은 대화를 나눕니다. (체력 +{vDeep}, 기분 +{vDeep})"));
                owner.AddStatActionCount(StatType.Social, 1, smallTurn, log);

                break;

            case SocialInteractionType.Love:
                owner.Status.AddHealth(vLove, owner.Data);
                owner.Status.AddMood(vLove, owner.Data);
                target.Status.AddHealth(vLove, target.Data);
                target.Status.AddMood(vLove, target.Data);
                log.AddLog(TextUtil.ApplyKoreanParticles($"[{smallTurn} 턴] {owner.Data.Name}은/는 {target.Data.Name}와 사랑을 나눕니다. (체력 +{vLove}, 기분 +{vLove})"));
                owner.AddStatActionCount(StatType.Social, 1, smallTurn, log);
                break;
        }
    }

    private SocialInteractionType PickInteraction(int socialLevel)
    {
        List<SocialInteractionType> pool = new List<SocialInteractionType> { SocialInteractionType.Talk };
        if (socialLevel >= 3) pool.Add(SocialInteractionType.Flex);
        if (socialLevel >= 5) pool.Add(SocialInteractionType.DeepTalk);
        if (socialLevel >= 8) pool.Add(SocialInteractionType.Love);

        int roll = Random.Range(0, 100);
        if (pool.Contains(SocialInteractionType.Love) && roll < 7) return SocialInteractionType.Love;
        if (pool.Contains(SocialInteractionType.DeepTalk) && roll < 25) return SocialInteractionType.DeepTalk;
        if (pool.Contains(SocialInteractionType.Flex) && roll < 50) return SocialInteractionType.Flex;
        return SocialInteractionType.Talk;
    }

    private bool TryFindTarget(CharacterEntity owner, List<TileNode> activeNodes, int maxSteps, out CharacterEntity target)
    {
        target = null;
        int best = int.MaxValue;
        List<CharacterEntity> chars = CharacterManager.Instance.ActiveCharacters;

        for (int i = 0; i < chars.Count; i++)
        {
            CharacterEntity other = chars[i];
            if (other == null || other == owner || other.IsDead) continue;
            if (other.CurrentTileNode == null || owner.CurrentTileNode == null) continue;

            List<TileNode> path = CharacterTaskCommon.FindPath(owner.CurrentTileNode, other.CurrentTileNode, activeNodes);
            if (path == null) continue;
            if (path.Count > maxSteps) continue;

            if (path.Count < best)
            {
                best = path.Count;
                target = other;
            }
        }

        return target != null;
    }

    private void Clear()
    {
        IsForced = false;
        _target = null;
    }
}
