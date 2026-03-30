using UnityEngine;
using TMPro;
using System.Collections.Generic;
using System.Collections;
public class BigTurnUIController : MonoBehaviour
{
    [SerializeField] private GameObject _bigTurnUI;

    [SerializeField] private TMP_Text _bigTurnResultText;
    [Header("UI Page")]

    [SerializeField] private GameObject _policyPanel;
    [SerializeField] private GameObject _weatherPanel;
    [SerializeField] private GameObject _eventPanel;
    [SerializeField] private GameObject _expandPanel;

    [Header("Option Text")]
    [SerializeField] private TMP_Text _weatherOptionTextA;
    [SerializeField] private TMP_Text _weatherOptionTextB;
    [SerializeField] private GameObject[] _eventOptionButtons;
    [SerializeField] private TMP_Text[] _eventOptionTexts;

    [SerializeField] private GameObject _expandYesButton;


    private int _openedBigTurnIndex;

    private PolicyType _selectedPolicy;
    private WeatherType _selectedWeather;
    private WorldEventType _selectedEvent;
    private bool _selectedExpand;

    private WeatherType[] _weatherOptions = new WeatherType[2];
    private WorldEventType[] _eventOptions;

    [SerializeField] private EncounterEventController _encounterEventController;
    private bool _isEncounterFlow;

    public void Open(int bigTurnIndex, BigTurnSelectionData prevSelection)
    {
        _openedBigTurnIndex = bigTurnIndex;

        _bigTurnUI.SetActive(true);
        ShowOnly(_policyPanel);

        _selectedPolicy = prevSelection.Policy;
        _selectedExpand = false;

        _weatherOptions = PickRandomWeatherOptions();
        _eventOptions = BuildEventOptions(bigTurnIndex);

        _selectedWeather = _weatherOptions[0];
        _selectedEvent = _eventOptions[0];

        _weatherOptionTextA.text = TextUtil.TranslateKorean(_weatherOptions[0]);
        _weatherOptionTextB.text = TextUtil.TranslateKorean(_weatherOptions[1]);

        for (int i = 0; i < _eventOptionTexts.Length; i++)
        {
            bool active = i < _eventOptions.Length;
            _eventOptionButtons[i].SetActive(active);
            if (active) _eventOptionTexts[i].text = TextUtil.TranslateKorean(_eventOptions[i]);
        }
        RefreshExpandYesButton();

    }

    private void RefreshExpandYesButton()
    {
        bool canExpandThisTurn = IsExpandForcedTurn(_openedBigTurnIndex);
        _expandYesButton.SetActive(canExpandThisTurn);
    }
    public void Close()
    {
        _bigTurnUI.SetActive(false);
    }



    public void Confirm()
    {

        UIManager.Instance.ResultBigChoiceSelect.SetActive(true);
        _bigTurnResultText.text =
        $"{TextUtil.TranslateKorean(_selectedPolicy)}\n" +
        $"{TextUtil.TranslateKorean(_selectedWeather)}";

        BigTurnSelectionData selection = new BigTurnSelectionData
        {
            Policy = _selectedPolicy,
            Weather = _selectedWeather,
            EventType = _selectedEvent,
            ExpandTerritory = _selectedExpand
        };

        TurnManager.Instance.ApplyBigTurnSelection(selection);
    }

    private T WeightedPick<T>(Dictionary<T, int> weights)
    {
        int totalWeight = 0;
        foreach (KeyValuePair<T, int> entry in weights)
        {
            totalWeight += entry.Value;
        }

        int roll = Random.Range(0, totalWeight);
        int cumulative = 0;

        foreach (KeyValuePair<T, int> entry in weights)
        {
            cumulative += entry.Value;
            if (roll < cumulative)
            {
                return entry.Key;
            }
        }

        foreach (KeyValuePair<T, int> entry in weights)
        {
            return entry.Key;
        }

        return default;
    }

    private WeatherType[] PickRandomWeatherOptions()
    {
        Dictionary<WeatherType, int> weatherWeights = new Dictionary<WeatherType, int>
        {
            { WeatherType.Mild, 35 },
            { WeatherType.Hot, 20 },
            { WeatherType.Cold, 20 },
            { WeatherType.Drought, 10 },
            { WeatherType.Heatwave, 4 },
            { WeatherType.ExtremeCold, 1 }
        };

        WeatherType first = WeightedPick(weatherWeights);

        Dictionary<WeatherType, int> secondPool = new Dictionary<WeatherType, int>(weatherWeights);
        secondPool.Remove(first);
        WeatherType second = WeightedPick(secondPool);

        return new WeatherType[] { first, second };
    }

    private WorldEventType[] BuildEventOptions(int bigTurnIndex)
    {
        List<WorldEventType> options = new List<WorldEventType>(4);

        // Visitor는 강제 턴에만
        if (IsVisitorForcedTurn(bigTurnIndex))
            options.Add(WorldEventType.Visitor);

        // 다른 조건 이벤트들 (추가 예정)
        // if (ConditionA(bigTurnIndex)) options.Add(WorldEventType.X);

        if (options.Count == 0)
            options.Add(WorldEventType.None);

        if (options.Count > 4)
            options.RemoveRange(4, options.Count - 4);

        return options.ToArray();
    }


    private void ShowOnly(GameObject panel)
    {
        _policyPanel.SetActive(panel == _policyPanel);
        _weatherPanel.SetActive(panel == _weatherPanel);
        _eventPanel.SetActive(panel == _eventPanel);
        _expandPanel.SetActive(panel == _expandPanel);
    }

    public void SelectPolicy(int value)
    {
        _selectedPolicy = (PolicyType)value;
        ShowOnly(_weatherPanel);
    }

    public void SelectWeather(int optionIndex)
    {
        _selectedWeather = _weatherOptions[optionIndex];
        ShowOnly(_eventPanel);
    }

    public void SelectEvent(int optionIndex)
    {
        _selectedEvent = _eventOptions[optionIndex];

        if (_selectedEvent == WorldEventType.Visitor)
        {
            if (_isEncounterFlow) return;
            StartCoroutine(VisitorFlowThenOpenExpand());
            return;
        }

        ShowOnly(_expandPanel);
    }


    public void SelectExpand(bool value)
    {
        _selectedExpand = value;
        Confirm();
    }

    private IEnumerator VisitorFlowThenOpenExpand()
    {
        _isEncounterFlow = true;

        int prevCameraIndex = CameraManager.Instance.CurrentCameraIndex;
        ShowOnly(null);
        CameraManager.Instance.ActivateCamera(0);

        yield return StartCoroutine(_encounterEventController.RunEncounter());

        CameraManager.Instance.ActivateCamera(prevCameraIndex);

        _isEncounterFlow = false;
        ShowOnly(_expandPanel);
    }



    private bool IsExpandForcedTurn(int bigTurnIndex)
    {
        return bigTurnIndex >= 1 && ((bigTurnIndex - 1) % 2 == 0);
    }

    private bool IsVisitorForcedTurn(int bigTurnIndex)
    {
        return bigTurnIndex >= 1 && ((bigTurnIndex - 1) % 3 == 0);
    }
    

}
