using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class CharacterMover
{
    private readonly Transform _ownerTransform;
    private readonly float _moveDuration;

    public TileNode CurrentTileNode { get; private set; }

    public CharacterMover(Transform ownerTransform, float moveDuration)
    {
        _ownerTransform = ownerTransform;
        _moveDuration = moveDuration;
    }

    public void SetCurrentTileNode(TileNode node)
    {
        CurrentTileNode = node;
        Vector3 p = _ownerTransform.position;
        p.x = node.WorldPosition.x;
        p.z = node.WorldPosition.z;
        _ownerTransform.position = p;
    }

    public IEnumerator MoveToTile(TileNode targetNode)
    {
        Vector3 target = new Vector3(targetNode.WorldPosition.x, _ownerTransform.position.y, targetNode.WorldPosition.z);
        Tween tween = _ownerTransform.DOMove(target, _moveDuration).SetEase(Ease.Linear);
        yield return tween.WaitForCompletion();
        CurrentTileNode = targetNode;
    }

    public TileNode GetNearestTileNode(List<TileNode> nodes, Vector3 worldPos)
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

    public TileNode GetRandomNeighborNode(TileNode current, List<TileNode> activeNodes)
    {
        if (current == null) return null;

        List<TileNode> valid = new List<TileNode>();
        for (int i = 0; i < current.Neighbors.Count; i++)
        {
            TileNode n = current.Neighbors[i];
            if (activeNodes.Contains(n)) valid.Add(n);
        }

        if (valid.Count == 0) return current;
        return valid[Random.Range(0, valid.Count)];
    }
}
