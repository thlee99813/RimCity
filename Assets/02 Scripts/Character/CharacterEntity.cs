using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;


public class CharacterEntity : MonoBehaviour
{


    public CharacterData Data;
    private CharacterBrain _brain = new CharacterBrain();
    public Transform CurrentTile { get; private set; }

    [SerializeField] private float _moveDuration = 0.35f;




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
    public IEnumerator RunSmallTurn(BigTurnSelectionData selection, int bigTurn, int smallTurn, List<Transform> activeTiles, SmallTurnLogController logController)

    {
    SmallTurnActionType action = _brain.DecideSmallTurnAction(Data, selection);

    string logLine = TextUtil.ApplyKoreanParticles($"[{smallTurn} 턴] {Data.Name}은/는 {ToActionText(action)}");
    logController.AddLog(logLine);

    if (activeTiles == null || activeTiles.Count == 0)
        yield break;

    if (CurrentTile == null)
        SetCurrentTile(GetNearestTile(activeTiles, transform.position));

    if (action == SmallTurnActionType.Wander || action == SmallTurnActionType.Gather)
    {
        Transform nextTile = GetRandomNeighborTile(activeTiles, CurrentTile);
        if (nextTile != null)
            yield return MoveToTile(nextTile);
    }
}

    private IEnumerator MoveToTile(Transform targetTile)
    {
        Vector3 target = new Vector3(targetTile.position.x, transform.position.y, targetTile.position.z);
        Tween tween = transform.DOMove(target, _moveDuration).SetEase(Ease.Linear);
        yield return tween.WaitForCompletion();
        CurrentTile = targetTile;
    }
    
    private Transform GetNearestTile(List<Transform> tiles, Vector3 worldPosition)
    {
        Transform best = null;
        float bestSqr = float.MaxValue;

        for (int i = 0; i < tiles.Count; i++)
        {
            float sqr = (tiles[i].position - worldPosition).sqrMagnitude;
            if (sqr < bestSqr)
            {
                bestSqr = sqr;
                best = tiles[i];
            }
        }

        return best;
    }
    private Transform GetRandomNeighborTile(List<Transform> tiles, Transform currentTile)
    {
        if (currentTile == null) return null;

        List<Transform> neighbors = new List<Transform>();

        float minDist = float.MaxValue;
        for (int i = 0; i < tiles.Count; i++)
        {
            if (tiles[i] == currentTile) continue;
            float d = Vector3.Distance(currentTile.position, tiles[i].position);
            if (d > 0.01f && d < minDist) minDist = d;
        }

        if (minDist == float.MaxValue) return currentTile;

        float tolerance = minDist * 0.25f;

        for (int i = 0; i < tiles.Count; i++)
        {
            if (tiles[i] == currentTile) continue;
            float d = Vector3.Distance(currentTile.position, tiles[i].position);
            if (Mathf.Abs(d - minDist) <= tolerance)
                neighbors.Add(tiles[i]);
        }

        if (neighbors.Count == 0) return currentTile;
        return neighbors[Random.Range(0, neighbors.Count)];
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

    private void SetCurrentTile(Transform tile)
    {
        CurrentTile = tile;
        Vector3 pos = transform.position;
        pos.x = tile.position.x;
        pos.z = tile.position.z;
        transform.position = pos;
    }
    


}
