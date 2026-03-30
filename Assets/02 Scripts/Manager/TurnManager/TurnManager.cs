using System.Collections;
using UnityEngine;
using System.Collections.Generic;


public class TurnManager : Singleton<TurnManager>
{
    [SerializeField] private BigTurnUIController _bTUIController;
    
    [SerializeField] private SmallTurnLogController _smallTurnLogController;

    
    [SerializeField] private int _smallTurnsPerBigTurn = 5;
    [SerializeField] private float _smallTurnInterval = 1f;
    
    [SerializeField] private float _expandTransitionDelay = 1f;

    [SerializeField] private EncounterEventController _encounterEventController;

    [SerializeField] private TurnResultUI _turnResultUI;



    private bool _waitExpandTransitionThisTurn;

    public int CurrentBigTurn { get; private set; }
    public int CurrentSmallTurn { get; private set; }

    private BigTurnSelectionData _currentSelection;
    private bool _isWaitingSelection;

    protected override void Init()
    {
        CurrentBigTurn = 1;
        CurrentSmallTurn = 0;

        _currentSelection = BigTurnSelectionData.Default();
    }

    public void StartTurnLoop()
    {
        StopAllCoroutines();
        StartCoroutine(TurnLoop());
    }

    private IEnumerator TurnLoop()
    {

        while (true)
        {
            UIManager.Instance.SmallTurnEnd();
            CameraManager.Instance.DeactivateSmallTurnCamera();
            CameraManager.Instance.ClearSmallTurnTarget();
            yield return StartCoroutine(OpenBigTurnUIAndWait());
            
            if (_waitExpandTransitionThisTurn)
            {
                _waitExpandTransitionThisTurn = false;
                yield return new WaitForSeconds(_expandTransitionDelay);
            }

            UIManager.Instance.SmallTurnStart();
            EventManager.Instance.PostNotification(MEventType.BigTurnStarted, this, new BigTurnStartedEventArgs(CurrentBigTurn));
            AllyCombatSupportManager.Instance.ClearReports();


            UIManager.Instance.SetSeasonText(GetSeasonText(CurrentBigTurn));

            float weatherHealthDelta = GetWeatherHealthDelta(_currentSelection.Weather);
            if (weatherHealthDelta < 0f)
            {
                _smallTurnLogController.AddLog(
                    $"{TextUtil.TranslateKorean(_currentSelection.Weather)} 효과로 매턴 ({weatherHealthDelta}) 만큼 체력이 줄고 있습니다."
                );
                yield return new WaitForSeconds(_expandTransitionDelay);
            }

            List<CharacterEntity> characters = new List<CharacterEntity>(CharacterManager.Instance.ActiveCharacters);
            List<CharacterEntity> reportCharacters = new List<CharacterEntity>();
            List<List<string>> reportLogs = new List<List<string>>();
            for (int c = 0; c < characters.Count; c++)
            {
                CharacterEntity character = characters[c];
                if (character == null || character.IsDead) continue;

                _smallTurnLogController.ClearLogs();
                CameraManager.Instance.SetSmallTurnTarget(character.transform);
                CameraManager.Instance.ActivateSmallTurnCamera();

                for (int i = 0; i < _smallTurnsPerBigTurn; i++)
                {
                    CurrentSmallTurn = i + 1;
                    yield return StartCoroutine(RunOneSmallTurnForCharacter(character));
                    yield return new WaitForSeconds(_smallTurnInterval);
                }

                reportCharacters.Add(character);
                reportLogs.Add(_smallTurnLogController.GetLogSnapshot());
                _smallTurnLogController.ClearLogs();
            }
            List<TileNode> enemyActiveNodes = CollectActiveTileNodes();
            List<CharacterEntity> enemyTargets = new List<CharacterEntity>(CharacterManager.Instance.ActiveCharacters);
            yield return StartCoroutine(EnemyManager.Instance.RunEnemyPhase(_smallTurnsPerBigTurn, enemyActiveNodes, enemyTargets));

            UIManager.Instance._ingameTextLog.SetActive(false);
            yield return StartCoroutine(_turnResultUI.OpenAndWait(reportCharacters, reportLogs));
            UIManager.Instance._ingameTextLog.SetActive(true);


            _smallTurnLogController.ClearLogs();
            ApplyYearlyAgingIfNeeded();
            CurrentBigTurn++;
            CurrentSmallTurn = 0;
        }
    }
    private IEnumerator OpenBigTurnUIAndWait()
    {
        _isWaitingSelection = true;
        _bTUIController.Open(CurrentBigTurn, _currentSelection);
        yield return new WaitUntil(() => _isWaitingSelection == false);
    }

   
    private List<TileNode> CollectActiveTileNodes()
    {
        List<TileNode> nodes = new List<TileNode>();

        for (int i = 0; i < GameManager.Instance.ActiveStages.Count; i++)
        {
            StageContext stage = GameManager.Instance.ActiveStages[i];
            TileNode[] stageNodes = stage.TileNodes;

            for (int j = 0; j < stageNodes.Length; j++)
                nodes.Add(stageNodes[j]);
        }

        return nodes;
    }
    public void ApplyBigTurnSelection(BigTurnSelectionData selection)
    {
        _currentSelection = selection;

        if (selection.ExpandTerritory)
        {
            bool expanded = StageManager.Instance.ExpandOneStep();
            if (expanded)
                _waitExpandTransitionThisTurn = true;
        }
        if (selection.EventType == WorldEventType.Raid)
        {
            EnemyGenerator target = EnemyGenerator.FindById(selection.RaidTargetGeneratorId);
            RaidDirectiveManager.Instance.SetTarget(target);
        }
        else
        {
            RaidDirectiveManager.Instance.ClearTarget();
        }

        // 나중에 정책/날씨/이벤트/확장 실제 적용
        /*Debug.Log(
            $"[BigTurn {CurrentBigTurn} 선택 확정] " +
            $"Policy:{selection.Policy}, Weather:{selection.Weather}, Event:{selection.EventType}, Expand:{selection.ExpandTerritory}"
        );*/

        _bTUIController.Close();
        _isWaitingSelection = false;

    }
    private float GetWeatherHealthDelta(WeatherType weather)
    {
        switch (weather)
        {
            case WeatherType.Mild: return 0f;
            case WeatherType.Hot: return -2f;
            case WeatherType.Cold: return -2f;
            case WeatherType.Heatwave: return -4f;
            case WeatherType.Drought: return -2f;
            case WeatherType.ExtremeCold: return -4f;
            default: return 0f;
        }
    }
    private IEnumerator RunOneSmallTurnForCharacter(CharacterEntity character)
    {
        List<TileNode> activeNodes = CollectActiveTileNodes();
        if (activeNodes.Count == 0) yield break;
        if (character == null || character.IsDead) yield break;

        yield return StartCoroutine(
            character.RunSmallTurn(_currentSelection, CurrentSmallTurn, activeNodes, _smallTurnLogController)
        );
    }
    private string GetSeasonText(int bigTurn)
    {
        int seasonIndex = (bigTurn - 1) % 4;
        int year = ((bigTurn - 1) / 4);

        string season;
        switch (seasonIndex)
        {
            case 0: season = "봄"; break;
            case 1: season = "여름"; break;
            case 2: season = "가을"; break;
            default: season = "겨울"; break;
        }

        return "서기 " + year + "년 " + season;
    }

    private void ApplyYearlyAgingIfNeeded()
    {
        if (CurrentBigTurn % 4 != 0) return;

        List<CharacterEntity> chars = CharacterManager.Instance.ActiveCharacters;
        for (int i = 0; i < chars.Count; i++)
        {
            CharacterEntity ch = chars[i];
            if (ch == null || ch.IsDead) continue;
            if (ch.Data == null) continue;

            ch.Data.Age += 1;

            float newMax = CharacterStatFormula.CalculateMaxHealthByAge(ch.Data.Age);
            ch.Data.MaxHealth = newMax;

            if (ch.Status.Health > newMax)
                ch.Status.Health = newMax;
        }
    }


}
