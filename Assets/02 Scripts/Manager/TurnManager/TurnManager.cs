// TurnManager.cs
using System.Collections;
using UnityEngine;

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
                RunOneSmallTurn();
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

    private void RunOneSmallTurn()

    {
        for (int i = 0; i < CharacterManager.Instance.ActiveCharacters.Count; i++)
        {
                CharacterEntity character = CharacterManager.Instance.ActiveCharacters[i];
                string logLine = character.RunSmallTurn(_currentSelection, CurrentBigTurn, CurrentSmallTurn);
                _smallTurnLogController.AddLog(logLine);
        }
        
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
