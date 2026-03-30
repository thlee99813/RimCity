using System;
using TMPro;
using UnityEngine;

public class EncounterUIController : MonoBehaviour
{
    [Header("Input Flow")]
    [SerializeField] private GameObject _root;
    [SerializeField] private GameObject _statPage;
    [SerializeField] private GameObject _namePage;
    [SerializeField] private TMP_InputField _nameInput;

    [Header("Stat UI")]
    [SerializeField] private TMP_Text _nameText;
    [SerializeField] private TMP_Text _genderAgeText;
    [SerializeField] private TMP_Text _traitsText;
    [SerializeField] private GameObject[] _combatBoxes;
    [SerializeField] private GameObject[] _craftBoxes;
    [SerializeField] private GameObject[] _buildBoxes;
    [SerializeField] private GameObject[] _gatherBoxes;
    [SerializeField] private GameObject[] _socialBoxes;

    private CharacterData _candidate;
    private Action<string> _onAcceptWithName;
    private Action _onReject;

    private void OnEnable()
    {
        _nameInput.onSubmit.AddListener(OnSubmitName);
    }

    private void OnDisable()
    {
        _nameInput.onSubmit.RemoveListener(OnSubmitName);
    }

    public void Open(CharacterData candidate, Action<string> onAcceptWithName, Action onReject)
    {
        _candidate = candidate;
        _onAcceptWithName = onAcceptWithName;
        _onReject = onReject;

        _root.SetActive(true);
        _statPage.SetActive(true);
        _namePage.SetActive(false);

        ApplyCharacterToUI(_candidate);
    }

    public void Close()
    {
        _root.SetActive(false);
        _candidate = null;
        _onAcceptWithName = null;
        _onReject = null;
    }

    public void OnClickAccept()
    {
        _statPage.SetActive(false);
        _namePage.SetActive(true);

        _nameInput.text = _candidate.Name;
        _nameInput.ActivateInputField();
        _nameInput.Select();
    }

    public void OnClickReject()
    {
        _onReject?.Invoke();
        Close();
    }

    private void OnSubmitName(string _)
    {
        string enteredName = _nameInput.text.Trim();
        if (string.IsNullOrWhiteSpace(enteredName))
            enteredName = _candidate.Name;

        _onAcceptWithName?.Invoke(enteredName);
        Close();
    }

    private void ApplyCharacterToUI(CharacterData data)
    {
        _nameText.text = data.Name;
        _genderAgeText.text = $"{TextUtil.TranslateKorean(data.Gender)} / {data.Age}";

        SetGauge(_combatBoxes, data.Stats[StatType.Combat]);
        SetGauge(_craftBoxes,  data.Stats[StatType.Craft]);
        SetGauge(_buildBoxes,  data.Stats[StatType.Build]);
        SetGauge(_gatherBoxes, data.Stats[StatType.Gather]);
        SetGauge(_socialBoxes, data.Stats[StatType.Social]);

        _traitsText.text = data.Traits.Count > 0
            ? string.Join("\n", data.Traits.ConvertAll(TextUtil.TranslateKorean))
            : "특성 없음";
    }

    private void SetGauge(GameObject[] boxes, int value)
    {
        for (int i = 0; i < boxes.Length; i++)
            boxes[i].SetActive(i < value);
    }
}
