using System.Collections;
using UnityEngine;
using System.Collections.Generic;


public class TurnManager : Singleton<TurnManager>
{
    [SerializeField] private BigTurnUIController _bTUIController;
    
    [SerializeField] private SmallTurnLogController _smallTurnLogController;

    
    [SerializeField] private int _smallTurnsPerBigTurn = 5;
    [SerializeField] private float _smallTurnInterval = 1f;
    


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
        List<Transform> activeTiles = CollectActiveTiles();
        if (activeTiles.Count == 0) yield break;

        for (int i = 0; i < CharacterManager.Instance.ActiveCharacters.Count; i++)
        {
            CharacterEntity character = CharacterManager.Instance.ActiveCharacters[i];
            yield return StartCoroutine(
                character.RunSmallTurn(_currentSelection, CurrentBigTurn, CurrentSmallTurn, activeTiles, _smallTurnLogController)
            );
        }

    }
    private List<Transform> CollectActiveTiles()
    {
        List<Transform> tiles = new List<Transform>();

        for (int i = 0; i < GameManager.Instance.ActiveStages.Count; i++)
        {
            StageContext stage = GameManager.Instance.ActiveStages[i];
            Transform[] stageTiles = stage.Tiles;

            for (int j = 0; j < stageTiles.Length; j++)
            {
                tiles.Add(stageTiles[j]);
            }
        }

        return tiles;
    }
    public void ApplyBigTurnSelection(BigTurnSelectionData selection)
    {
        _currentSelection = selection;

        // 나중에 정책/날씨/이벤트/확장 실제 적용
        Debug.Log(
            $"[BigTurn {CurrentBigTurn} 선택 확정] " +
            $"Policy:{selection.Policy}, Weather:{selection.Weather}, Event:{selection.EventType}, Expand:{selection.ExpandTerritory}"
        );

        _bTUIController.Close();
        _isWaitingSelection = false;

    }
}
