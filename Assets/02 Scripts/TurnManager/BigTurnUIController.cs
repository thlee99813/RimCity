using UnityEngine;
using TMPro;
using System.Collections.Generic;
public class BigTurnUIController : MonoBehaviour
{
    [SerializeField] private GameObject _root;

    [Header("Option Text")]
    [SerializeField] private TMP_Text _weatherOptionTextA;
    [SerializeField] private TMP_Text _weatherOptionTextB;
    [SerializeField] private TMP_Text _eventOptionTextA;
    [SerializeField] private TMP_Text _eventOptionTextB;

    private PolicyType _selectedPolicy;
    private WeatherType _selectedWeather;
    private WorldEventType _selectedEvent;
    private bool _selectedExpand;

    private WeatherType[] _weatherOptions = new WeatherType[2];
    private WorldEventType[] _eventOptions = new WorldEventType[2];

    public void Open(int bigTurnIndex, BigTurnSelectionData prevSelection)
    {
        _root.SetActive(true);

        _selectedPolicy = prevSelection.Policy;
        _selectedExpand = prevSelection.ExpandTerritory;

        _weatherOptions = PickRandomWeatherOptions();
        _eventOptions = PickRandomEventOptions();

        _selectedWeather = _weatherOptions[0];
        _selectedEvent = _eventOptions[0];

        _weatherOptionTextA.text = _weatherOptions[0].ToString();
        _weatherOptionTextB.text = _weatherOptions[1].ToString();
        _eventOptionTextA.text = _eventOptions[0].ToString();
        _eventOptionTextB.text = _eventOptions[1].ToString();
    }

    public void Close()
    {
        _root.SetActive(false);
    }

    public void SelectPolicy(int value) { _selectedPolicy = (PolicyType)value; }
    public void SelectWeather(int optionIndex) { _selectedWeather = _weatherOptions[optionIndex]; }
    public void SelectEvent(int optionIndex) { _selectedEvent = _eventOptions[optionIndex]; }
    public void SelectExpand(bool value) { _selectedExpand = value; }

    // 확인 버튼
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

    private WorldEventType[] PickRandomEventOptions()
    {
        Dictionary<WorldEventType, int> eventWeights = new Dictionary<WorldEventType, int>
        {
            { WorldEventType.None, 50 },
            { WorldEventType.Visitor, 35 },
            { WorldEventType.Raid, 15 }
        };

        WorldEventType first = WeightedPick(eventWeights);

        Dictionary<WorldEventType, int> secondPool = new Dictionary<WorldEventType, int>(eventWeights);
        secondPool.Remove(first);
        WorldEventType second = WeightedPick(secondPool);

        return new WorldEventType[] { first, second };
    }
}
