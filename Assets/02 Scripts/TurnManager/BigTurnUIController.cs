using UnityEngine;
using TMPro;
using System.Collections.Generic;
public class BigTurnUIController : MonoBehaviour
{
    [SerializeField] private GameObject _bigTurnUI;
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


    private PolicyType _selectedPolicy;
    private WeatherType _selectedWeather;
    private WorldEventType _selectedEvent;
    private bool _selectedExpand;

    private WeatherType[] _weatherOptions = new WeatherType[2];
    private WorldEventType[] _eventOptions;

    public void Open(int bigTurnIndex, BigTurnSelectionData prevSelection)
    {
        _bigTurnUI.SetActive(true);
        ShowOnly(_policyPanel);



        _selectedPolicy = prevSelection.Policy;
        _selectedExpand = prevSelection.ExpandTerritory;

        _weatherOptions = PickRandomWeatherOptions();

        int eventOptionCount = UnityEngine.Random.Range(2, 5);
        _eventOptions = PickRandomEventOptions(eventOptionCount);

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
    }

    public void Close()
    {
        _bigTurnUI.SetActive(false);
    }



    public void Confirm()
    {
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
            { WeatherType.Flood, 7 },
            { WeatherType.Heatwave, 4 },
            { WeatherType.Snowstorm, 3 },
            { WeatherType.ExtremeCold, 1 }
        };

        WeatherType first = WeightedPick(weatherWeights);

        Dictionary<WeatherType, int> secondPool = new Dictionary<WeatherType, int>(weatherWeights);
        secondPool.Remove(first);
        WeatherType second = WeightedPick(secondPool);

        return new WeatherType[] { first, second };
    }

    private WorldEventType[] PickRandomEventOptions(int count)
    {
        Dictionary<WorldEventType, int> weights  = new Dictionary<WorldEventType, int>
        {
            { WorldEventType.None, 50 },
            { WorldEventType.Visitor, 35 },
            { WorldEventType.Raid, 15 }
        };

        List<WorldEventType> selected = new List<WorldEventType>();
        Dictionary<WorldEventType, int> pool = new Dictionary<WorldEventType, int>(weights);

        int safeCount = Mathf.Min(count, pool.Count);

        for (int i = 0; i < safeCount; i++)
        {
            WorldEventType picked = WeightedPick(pool);
            selected.Add(picked);
            pool.Remove(picked);
        }

        return selected.ToArray();
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
        ShowOnly(_expandPanel);
    }
    public void SelectExpand(bool value)
    {
        _selectedExpand = value;
        Confirm();
    }
}
