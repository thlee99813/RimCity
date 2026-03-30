using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class UIManager : Singleton<UIManager>
{
    protected override void Init()
    {
        _currentPage.SetActive(true);
        _nextPage.SetActive(false);
        _smallTurnPage.SetActive(false);
        _bigTurnPage.SetActive(false);
        ResultBigChoiceSelect.SetActive(false);
        _characterReport.SetActive(false);

        

    }    
    
    [SerializeField] private TMP_InputField _nameInput;
    [SerializeField] private GameObject _currentPage;
    [SerializeField] private GameObject _nextPage;
    [SerializeField] private GameObject _smallTurnPage;
    [SerializeField] private GameObject _bigTurnPage;
    [SerializeField] private GameObject _ingameBackGround;
    [SerializeField] private GameObject _characterReport;
    [SerializeField] public GameObject _ingameTextLog;


    [SerializeField] public GameObject ResultBigChoiceSelect;



    [Header("Header Slot Spawn")]

    [SerializeField] private CharacterHeaderSlotUI _slotPrefab;
    [SerializeField] private Transform _slotParent;
    [SerializeField] private int _maxSlots = 20;

    [SerializeField] private TMP_Text _seasonText;

    [SerializeField] private GameObject _endingPanel;
    private readonly List<CharacterHeaderSlotUI> _spawnedSlots = new List<CharacterHeaderSlotUI>();



    public void SmallTurnStart()
    {
        _smallTurnPage.SetActive(true);

    }
    public void SmallTurnEnd()
    {
        _smallTurnPage.SetActive(false);

    }
    public void AddCharacterSlot(string characterId, string characterName)
    {
        if (_spawnedSlots.Count >= _maxSlots) return;

        GameObject slotObject = Instantiate(_slotPrefab.gameObject, _slotParent);
        CharacterHeaderSlotUI slotUI = slotObject.GetComponent<CharacterHeaderSlotUI>();
        slotUI.SetData(characterId, characterName);
        _spawnedSlots.Add(slotUI);
    }

    public void RemoveCharacterSlot(string characterId)
    {
        for (int i = 0; i < _spawnedSlots.Count; i++)
        {
            CharacterHeaderSlotUI slotUI = _spawnedSlots[i];
            if (slotUI == null) continue;
            if (slotUI.CharacterId != characterId) continue;

            Destroy(slotUI.gameObject);
            _spawnedSlots.RemoveAt(i);
            return;
        }
    }
    public void SetSeasonText(string text)
    {
        _seasonText.text = text;
    }
    public void ShowKomaEnding()
    {
        _endingPanel.SetActive(true);
        Time.timeScale = 0f;
    }

    
}
