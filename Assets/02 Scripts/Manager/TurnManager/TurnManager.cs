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

            yield return StartCoroutine(OpenBigTurnUIAndWait());

            if (_waitExpandTransitionThisTurn)
            {
                _waitExpandTransitionThisTurn = false;
                yield return new WaitForSeconds(_expandTransitionDelay);
            }

            UIManager.Instance.SmallTurnStart();

            for (int i = 0; i < _smallTurnsPerBigTurn; i++)
            {

                CurrentSmallTurn = i + 1;
                yield return StartCoroutine(RunOneSmallTurn());
                yield return new WaitForSeconds(_smallTurnInterval);
            }
            _smallTurnLogController.ClearLogs();
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

    private IEnumerator RunOneSmallTurn()
    {
        List<TileNode> activeNodes = CollectActiveTileNodes();
        if (activeNodes.Count == 0) yield break;

        for (int i = 0; i < CharacterManager.Instance.ActiveCharacters.Count; i++)
        {
            CharacterEntity character = CharacterManager.Instance.ActiveCharacters[i];
            yield return StartCoroutine(
                character.RunSmallTurn(_currentSelection, CurrentSmallTurn, activeNodes, _smallTurnLogController)
            );
        }
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

        // 나중에 정책/날씨/이벤트/확장 실제 적용
        /*Debug.Log(
            $"[BigTurn {CurrentBigTurn} 선택 확정] " +
            $"Policy:{selection.Policy}, Weather:{selection.Weather}, Event:{selection.EventType}, Expand:{selection.ExpandTerritory}"
        );*/

        _bTUIController.Close();
        _isWaitingSelection = false;

    }
}
