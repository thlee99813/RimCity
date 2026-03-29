using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CharacterEntity : MonoBehaviour
{
    public CharacterData Data;
    
    [field: SerializeField] public CharacterStatus Status { get; private set; } = new CharacterStatus();
    public bool IsDead { get; private set; }

    private CharacterBrain _brain = new CharacterBrain();
    private CharacterTaskController _taskController;

    [Header("매턴 움직임")]
    [SerializeField] private float _moveDuration = 0.35f;
    [SerializeField] private int _maxMoveTilesPerTurn = 4;

    [Header("매턴 배고픔, 수면, 재미, 배고픔에 따른 체력 변화량")]
    [SerializeField] private float _hungerDeltaPerTurn = -3f;
    [SerializeField] private float _sleepDeltaPerTurn = -2f;
    [SerializeField] private float _funDeltaPerTurn = -4f;
    [SerializeField] private float _healthDeltaWhenStarving = -5f;



    [Header("먹기")]
    [SerializeField] private float _berryHungerRecoverAmount = 20f;

    [Header("건축 레시피")]
    [SerializeField] private BuildRecipe[] _buildRecipes;


    private CharacterMover _mover;
    private CharacterNeedsController _needsController;
    private CharacterLifeController _lifeController;

    public TileNode CurrentTileNode => _mover.CurrentTileNode;

    private void Awake()
    {
        _mover = new CharacterMover(transform, _moveDuration);
        _needsController = new CharacterNeedsController(_hungerDeltaPerTurn, _sleepDeltaPerTurn, _funDeltaPerTurn, _healthDeltaWhenStarving);
        _lifeController = new CharacterLifeController();
        _taskController = new CharacterTaskController(_maxMoveTilesPerTurn, _buildRecipes);

    }
    private void OnEnable()
    {
        CharacterManager.Instance.Register(this);
    }

    private void OnDisable()
    {
        if (CharacterManager.Instance == null) return;
        CharacterManager.Instance.Unregister(this);
    }
    public void Initialize(CharacterData data)
    {
        Data = data;
        Status.InitializeFromData(data);
    }

    public void MarkDead()
    {
        IsDead = true;
    }

    public void SetCurrentTileNode(TileNode node)
    {
        _mover.SetCurrentTileNode(node);
    }

    public IEnumerator MoveToTile(TileNode node)
    {
        yield return _mover.MoveToTile(node);
    }

    public IEnumerator RunSmallTurn(
        BigTurnSelectionData selection,
        int smallTurn,
        List<TileNode> activeNodes,
        SmallTurnLogController logController)
    {

        _needsController.Tick(Status, Data);
        if (_lifeController.TryHandleDeath(this, smallTurn, logController)) yield break;

        if (activeNodes == null || activeNodes.Count == 0) yield break;

        if (CurrentTileNode == null)
            SetCurrentTileNode(_mover.GetNearestTileNode(activeNodes, transform.position));

        SmallTurnActionType action = _taskController.ResolveAction(Data, selection, _brain);

        if (action == SmallTurnActionType.Gather)
        {
            yield return _taskController.RunGatherTurn(this, smallTurn, activeNodes, logController);
            if (_lifeController.TryHandleDeath(this, smallTurn, logController)) yield break;
            yield break;
        }

        if (action == SmallTurnActionType.Eat)
        {
            _taskController.RunEatAction(this, smallTurn, logController, _berryHungerRecoverAmount);
            if (_lifeController.TryHandleDeath(this, smallTurn, logController)) yield break;
            yield break;
        }

        logController.AddLog(TextUtil.ApplyKoreanParticles($"[{smallTurn} 턴] {Data.Name}은/는 {CharacterActionText.ToActionText(action)}"));

        if (action == SmallTurnActionType.Wander)
        {
            TileNode nextNode = _mover.GetRandomNeighborNode(CurrentTileNode, activeNodes);
            if (nextNode != null)
                yield return MoveToTile(nextNode);
        }

        if (action == SmallTurnActionType.Build)
        {
            yield return _taskController.RunBuildTurn(this, smallTurn, activeNodes, logController);
            if (_lifeController.TryHandleDeath(this, smallTurn, logController)) yield break;
            yield break;
        }
    }

    

    
}
