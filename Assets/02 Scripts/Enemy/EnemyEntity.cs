using UnityEngine;
using System.Collections.Generic;

public class EnemyEntity : MonoBehaviour
{
    [Header("이동")]
    [SerializeField] private int _maxMoveTilesPerTurn = 4;
    [SerializeField] private int _chaseRangeTiles = 8; // 2턴(4칸 x 2턴)

    public TileNode CurrentTileNode { get; private set; }

    private void OnEnable()
    {
        EnemyManager.Instance.Register(this);
    }

    private void OnDisable()
    {
        if (EnemyManager.Instance == null) return;
        EnemyManager.Instance.Unregister(this);
    }

    public void SetCurrentTileNode(TileNode node)
    {
        CurrentTileNode = node;
        if (node == null) return;

        Vector3 p = transform.position;
        p.x = node.WorldPosition.x;
        p.z = node.WorldPosition.z;
        transform.position = p;
    }

    public void RunEnemySmallTurn(List<TileNode> activeNodes, List<CharacterEntity> allyCharacters)
    {
        if (activeNodes == null || activeNodes.Count == 0) return;

        if (CurrentTileNode == null)
            SetCurrentTileNode(GetNearestTile(activeNodes, transform.position));

        CharacterEntity chaseTarget = FindNearestChaseTarget(activeNodes, allyCharacters);

        if (chaseTarget != null && chaseTarget.CurrentTileNode != null)
        {
            List<TileNode> path = CharacterTaskCommon.FindPath(CurrentTileNode, chaseTarget.CurrentTileNode, activeNodes);
            MoveAlongPath(path, _maxMoveTilesPerTurn);
            return;
        }

        RunRandomMove(activeNodes);
    }

    private void MoveAlongPath(List<TileNode> path, int maxStep)
    {
        if (path == null || path.Count == 0) return;

        int step = Mathf.Min(maxStep, path.Count);
        for (int i = 0; i < step; i++)
            SetCurrentTileNode(path[i]);
    }

    private void RunRandomMove(List<TileNode> activeNodes)
    {
        int steps = Random.Range(1, _maxMoveTilesPerTurn + 1);

        for (int i = 0; i < steps; i++)
        {
            TileNode next = GetRandomNeighbor(CurrentTileNode, activeNodes);
            if (next == null || next == CurrentTileNode) break;
            SetCurrentTileNode(next);
        }
    }

    private CharacterEntity FindNearestChaseTarget(List<TileNode> activeNodes, List<CharacterEntity> allyCharacters)
    {
        if (allyCharacters == null || allyCharacters.Count == 0) return null;

        CharacterEntity best = null;
        int bestSteps = int.MaxValue;

        for (int i = 0; i < allyCharacters.Count; i++)
        {
            CharacterEntity ch = allyCharacters[i];
            if (ch == null || ch.IsDead) continue;
            if (ch.CurrentTileNode == null) continue;

            List<TileNode> path = CharacterTaskCommon.FindPath(CurrentTileNode, ch.CurrentTileNode, activeNodes);
            if (path == null) continue;

            int steps = path.Count;
            if (steps > _chaseRangeTiles) continue;

            if (steps < bestSteps)
            {
                bestSteps = steps;
                best = ch;
            }
        }

        return best;
    }

    private TileNode GetNearestTile(List<TileNode> nodes, Vector3 worldPos)
    {
        TileNode best = null;
        float bestSqr = float.MaxValue;

        for (int i = 0; i < nodes.Count; i++)
        {
            TileNode n = nodes[i];
            if (n == null) continue;

            float sqr = (n.WorldPosition - worldPos).sqrMagnitude;
            if (sqr < bestSqr)
            {
                bestSqr = sqr;
                best = n;
            }
        }

        return best;
    }

    private TileNode GetRandomNeighbor(TileNode current, List<TileNode> activeNodes)
    {
        if (current == null) return null;

        List<TileNode> candidates = new List<TileNode>();
        for (int i = 0; i < current.Neighbors.Count; i++)
        {
            TileNode n = current.Neighbors[i];
            if (n == null) continue;
            if (!activeNodes.Contains(n)) continue;
            candidates.Add(n);
        }

        if (candidates.Count == 0) return current;
        return candidates[Random.Range(0, candidates.Count)];
    }
}
