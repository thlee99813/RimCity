using UnityEngine;
using TMPro;
using System.Collections.Generic;
using System.Collections;

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
        _endingPanel.SetActive(false);
        _gameOverPanel.SetActive(false);


        

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
    [SerializeField] private TMP_Text _endingResultText;
    [SerializeField] private GameObject _gameOverPanel;
    [SerializeField] private TMP_Text _gameOverText;
    [SerializeField] private float _endingTypeInterval = 0.03f;

    private Coroutine _endingTypeRoutine;
    private bool _endingShown;
    private Coroutine _gameOverTypeRoutine;
    private bool _gameOverShown;

    private readonly List<CharacterHeaderSlotUI> _spawnedSlots = new List<CharacterHeaderSlotUI>();



    public void SmallTurnStart()
    {
        _smallTurnPage.SetActive(true);

    }
    public void SmallTurnEnd()
    {
        _smallTurnPage.SetActive(false);

    }
    public void AddCharacterSlot(CharacterEntity character)
    {
        if (_spawnedSlots.Count >= _maxSlots) return;
        if (character == null || character.Data == null) return;

        GameObject slotObject = Instantiate(_slotPrefab.gameObject, _slotParent);
        CharacterHeaderSlotUI slotUI = slotObject.GetComponent<CharacterHeaderSlotUI>();
        slotUI.Bind(character);
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
        if (_endingShown) return;
        _endingShown = true;

        _endingPanel.SetActive(true);

        string endingText = BuildKomaEndingText();

        if (_endingTypeRoutine != null) StopCoroutine(_endingTypeRoutine);
        _endingTypeRoutine = StartCoroutine(TypeTextRoutine(_endingResultText, endingText, _endingTypeInterval));

    }
    private string BuildKomaEndingText()
    {
        string survivorNames = BuildSurvivorNames();
        int clearTurn = (TurnManager.Instance != null) ? TurnManager.Instance.CurrentBigTurn : 0;

        return
            "멸망을 막은 부족민 : " + survivorNames + "\n" +
            "클리어하는데 걸린 턴 : " + clearTurn;
    }
    private string BuildSurvivorNames()
    {
        List<string> names = new List<string>();

        if (CharacterManager.Instance != null)
        {
            List<CharacterEntity> chars = CharacterManager.Instance.ActiveCharacters;
            for (int i = 0; i < chars.Count; i++)
            {
                CharacterEntity ch = chars[i];
                if (ch == null || ch.IsDead) continue;
                if (ch.Data == null || string.IsNullOrWhiteSpace(ch.Data.Name)) continue;
                names.Add(ch.Data.Name);
            }
        }

        return names.Count > 0 ? string.Join(", ", names) : "없음";
    }

    private IEnumerator TypeTextRoutine(TMP_Text target, string fullText, float interval)
    {
        target.text = "";

        for (int i = 0; i < fullText.Length; i++)
        {
            target.text += fullText[i];
            yield return new WaitForSecondsRealtime(interval);
        }
    }
    public void ShowGameOver()
    {
        if (_gameOverShown) return;
        _gameOverShown = true;

        _gameOverPanel.SetActive(true);

        int clearTurn = (TurnManager.Instance != null) ? TurnManager.Instance.CurrentBigTurn : 0;
        string text = "부족이 전멸했습니다.\n" + "버틴 턴 : " + clearTurn;

        if (_gameOverTypeRoutine != null) StopCoroutine(_gameOverTypeRoutine);
        _gameOverTypeRoutine = StartCoroutine(TypeTextRealtime(_gameOverText, text, _endingTypeInterval));

        Time.timeScale = 0f;
    }

    private IEnumerator TypeTextRealtime(TMP_Text target, string fullText, float interval)
    {
        target.text = "";
        for (int i = 0; i < fullText.Length; i++)
        {
            target.text += fullText[i];
            yield return new WaitForSecondsRealtime(interval);
        }
    }
    private void LateUpdate()
    {
        for (int i = 0; i < _spawnedSlots.Count; i++)
        {
            CharacterHeaderSlotUI slotUI = _spawnedSlots[i];
            if (slotUI == null) continue;
            slotUI.Refresh();
        }
    }



    
}
