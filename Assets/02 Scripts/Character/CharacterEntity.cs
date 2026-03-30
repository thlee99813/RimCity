using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;


public class CharacterEntity : MonoBehaviour
{
    public CharacterData Data;
    
    [field: SerializeField] public CharacterStatus Status { get; private set; } = new CharacterStatus();
    [field: SerializeField] public CharacterEquipment Equipment { get; private set; } = new CharacterEquipment();

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

    [Header("제작 레시피")]
    [SerializeField] private CraftRecipe[] _craftRecipes;

    private CharacterMover _mover;
    private CharacterNeedsController _needsController;
    private CharacterLifeController _lifeController;

    private const int StatMaxLevel = 11;
    private const int ActionsPerLevel = 3;
    public TileNode CurrentTileNode => _mover.CurrentTileNode;
    private readonly List<string> _moodHintLines = new List<string>();
    public string MoodHintText => _moodHintLines.Count > 0 ? string.Join("\n", _moodHintLines) : "";


    private void Awake()
    {
        _mover = new CharacterMover(transform, _moveDuration);
        _needsController = new CharacterNeedsController(_hungerDeltaPerTurn, _sleepDeltaPerTurn, _funDeltaPerTurn, _healthDeltaWhenStarving);
        _lifeController = new CharacterLifeController();
        _taskController = new CharacterTaskController(_maxMoveTilesPerTurn, _buildRecipes, _craftRecipes);

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
        NormalizeStatDictionaries();
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
        if (activeNodes == null || activeNodes.Count == 0) yield break;

        if (CurrentTileNode == null)
            SetCurrentTileNode(_mover.GetNearestTileNode(activeNodes, transform.position));
        ClearMoodHints();

        CharacterNeedsController.StructureEffectReport report =
            _needsController.Tick(Status, Data, Equipment, selection.Weather, CurrentTileNode, activeNodes);

        if (report.HasAny)
        {
            if (report.HasTorch) AddMoodHint("횃불에 의해 \n기분이 좋아집니다");
            if (report.HasCampfire) AddMoodHint("모닥에 의해 \n기분이 좋아집니다");
            if (report.HasSweatingStone) AddMoodHint("발한석에 의해 \n기분이 좋아집니다");
        }

        if (Status.Hunger <= 0f)
        {
            AddMoodHint($"배고픔으로 체력 감소 ({_healthDeltaWhenStarving})");
        }

        if (_lifeController.TryHandleDeath(this, smallTurn, logController)) yield break;

        SmallTurnActionType action = _taskController.ResolveAction(this, Data, Status, Equipment, selection, activeNodes, _brain);

        if (action == SmallTurnActionType.MoveToShelter)
        {
            yield return _taskController.RunMoveToShelterTurn(this, smallTurn, activeNodes, logController);
            if (_lifeController.TryHandleDeath(this, smallTurn, logController)) yield break;
            yield break;
        }

        if (action == SmallTurnActionType.Rest)
        {
            _taskController.RunRestTurn(this, smallTurn, logController);
            if (_lifeController.TryHandleDeath(this, smallTurn, logController)) yield break;
            yield break;
        }


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

        if (action == SmallTurnActionType.Build)
        {
            yield return _taskController.RunBuildTurn(this, smallTurn, activeNodes, logController);
            if (_lifeController.TryHandleDeath(this, smallTurn, logController)) yield break;
            yield break;
        }

        if (action == SmallTurnActionType.Craft)
        {
            yield return _taskController.RunCraftTurn(this, smallTurn, logController);
            if (_lifeController.TryHandleDeath(this, smallTurn, logController)) yield break;
            yield break;
        }

        if (action == SmallTurnActionType.EquipWoodenSpear)
        {
            _taskController.RunEquipWoodenSpearTurn(this, smallTurn, logController);
            if (_lifeController.TryHandleDeath(this, smallTurn, logController)) yield break;
            yield break;
        }

        if (action == SmallTurnActionType.EquipStoneSpear)
        {
            _taskController.RunEquipStoneSpearTurn(this, smallTurn, logController);
            if (_lifeController.TryHandleDeath(this, smallTurn, logController)) yield break;
            yield break;
        }

        if (action == SmallTurnActionType.EquipFan)
        {
            _taskController.RunEquipFanTurn(this, smallTurn, logController);
            if (_lifeController.TryHandleDeath(this, smallTurn, logController)) yield break;
            yield break;
        }

        if (action == SmallTurnActionType.UseBandage)
        {
            _taskController.RunUseBandageTurn(this, smallTurn, logController);
            if (_lifeController.TryHandleDeath(this, smallTurn, logController)) yield break;
            yield break;
        }

        if (action == SmallTurnActionType.UseMedkit)
        {
            _taskController.RunUseMedkitTurn(this, smallTurn, logController);
            if (_lifeController.TryHandleDeath(this, smallTurn, logController)) yield break;
            yield break;
        }
        if (action == SmallTurnActionType.Social)
        {
            yield return _taskController.RunSocialTurn(this, smallTurn, activeNodes, logController);
            if (_lifeController.TryHandleDeath(this, smallTurn, logController)) yield break;
            yield break;
        }

        logController.AddLog(
            TextUtil.ApplyKoreanParticles(
                $"[{smallTurn} 턴] {Data.Name}은/는 {CharacterActionText.ToActionText(action)}"
            )
        );

        if (action == SmallTurnActionType.Wander)
        {
            TileNode nextNode = _mover.GetRandomNeighborNode(CurrentTileNode, activeNodes);
            if (nextNode != null)
                yield return MoveToTile(nextNode);
        }
    }


    public void EquipWeaponWithSwap(WeaponType newWeapon, PlayerResourceInventory inventory)
    {
        if (newWeapon == WeaponType.None) return;

        WeaponType oldWeapon = Equipment.Weapon;
        if (oldWeapon != WeaponType.None)
        {
            inventory.AddWeapon(oldWeapon, 1); // 기존 무기 반환
            Equipment.UnequipWeapon();
        }

        Equipment.TryEquipWeapon(newWeapon); // 새 무기 장착
    }

    private void NormalizeStatDictionaries()
    {
        if (Data.Stats == null) Data.Stats = new Dictionary<StatType, int>();
        if (Data.StatProgress == null) Data.StatProgress = new Dictionary<StatType, int>();

        foreach (StatType type in Enum.GetValues(typeof(StatType)))
        {
            if (!Data.Stats.ContainsKey(type)) Data.Stats[type] = 1;
            Data.Stats[type] = Mathf.Clamp(Data.Stats[type], 1, StatMaxLevel);

            if (!Data.StatProgress.ContainsKey(type)) Data.StatProgress[type] = 0;
            if (Data.StatProgress[type] < 0) Data.StatProgress[type] = 0;
        }
    }

    public int GetStatLevel(StatType type)
    {
        if (Data == null || Data.Stats == null) return 1;
        if (!Data.Stats.TryGetValue(type, out int level)) return 1;
        return Mathf.Clamp(level, 1, StatMaxLevel);
    }

    public void AddStatActionCount(StatType type, int amount, int smallTurn, SmallTurnLogController log)
    {
        if (amount <= 0) return;

        NormalizeStatDictionaries();

        int level = GetStatLevel(type);
        if (level >= StatMaxLevel) return;

        Data.StatProgress[type] += amount;

        while (Data.StatProgress[type] >= ActionsPerLevel && level < StatMaxLevel)
        {
            Data.StatProgress[type] -= ActionsPerLevel;
            level++;
            Data.Stats[type] = level;

            if (log != null)
                log.AddLog($"[{smallTurn} 턴] {Data.Name}은/는 {StatToKorean(type)} 레벨이 {level}이 되었습니다.");
        }
    }

    private string StatToKorean(StatType type)
    {
        switch (type)
        {
            case StatType.Combat: return "전투";
            case StatType.Craft: return "제작";
            case StatType.Build: return "건축";
            case StatType.Gather: return "수집";
            case StatType.Social: return "매력";
            default: return "능력";
        }
}
private void ClearMoodHints()
{
    _moodHintLines.Clear();
}

private void AddMoodHint(string text)
{
    if (string.IsNullOrEmpty(text)) return;
    if (_moodHintLines.Contains(text)) return;
    _moodHintLines.Add(text);
}

    

    
}
