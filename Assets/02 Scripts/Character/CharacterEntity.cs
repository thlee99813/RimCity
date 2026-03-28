using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;


public class CharacterEntity : MonoBehaviour
{


    public CharacterData Data;
    private CharacterBrain _brain = new CharacterBrain();
    public TileNode CurrentTileNode { get; private set; }
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
    public IEnumerator RunSmallTurn(
    BigTurnSelectionData selection,
    int bigTurn,
    int smallTurn,
    List<TileNode> activeNodes,
    SmallTurnLogController logController)
    {
        SmallTurnActionType action = _brain.DecideSmallTurnAction(Data, selection);

        string logLine = TextUtil.ApplyKoreanParticles($"[{smallTurn} 턴] {Data.Name}은/는 {ToActionText(action)}");
        logController.AddLog(logLine);

        if (activeNodes == null || activeNodes.Count == 0) yield break;

        if (CurrentTileNode == null)
            SetCurrentTileNode(GetNearestTileNode(activeNodes, transform.position));

        if (action == SmallTurnActionType.Wander || action == SmallTurnActionType.Gather)
        {
            TileNode nextNode = GetRandomNeighborNode(CurrentTileNode, activeNodes);
            if (nextNode != null)
                yield return MoveToTile(nextNode); 
        }
    }
    private void SetCurrentTileNode(TileNode node)
    {
        CurrentTileNode = node;
        Vector3 p = transform.position;
        p.x = node.WorldPosition.x;
        p.z = node.WorldPosition.z;
        transform.position = p;
    }


    private IEnumerator MoveToTile(TileNode targetNode)
    {
        Vector3 target = new Vector3(targetNode.WorldPosition.x, transform.position.y, targetNode.WorldPosition.z);
        Tween tween = transform.DOMove(target, _moveDuration).SetEase(Ease.Linear);
        yield return tween.WaitForCompletion();
        CurrentTileNode = targetNode;
    }
    
   
    private TileNode GetNearestTileNode(List<TileNode> nodes, Vector3 worldPos)
    {
        TileNode best = null;
        float bestSqr = float.MaxValue;

        for (int i = 0; i < nodes.Count; i++)
        {
            float sqr = (nodes[i].WorldPosition - worldPos).sqrMagnitude;
            if (sqr < bestSqr)
            {
                bestSqr = sqr;
                best = nodes[i];
            }
        }

        return best;
    }

  

    private TileNode GetRandomNeighborNode(TileNode current, List<TileNode> activeNodes)
    {
        if (current == null) return null;

        List<TileNode> valid = new List<TileNode>();
        for (int i = 0; i < current.Neighbors.Count; i++)
        {
            TileNode n = current.Neighbors[i];
            if (activeNodes.Contains(n)) valid.Add(n);
        }

        if (valid.Count == 0) return current;
        return valid[UnityEngine.Random.Range(0, valid.Count)];
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
