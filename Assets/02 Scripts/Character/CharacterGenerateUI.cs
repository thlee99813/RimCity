using UnityEngine;
using TMPro;
public class CharacterGenerateUI : MonoBehaviour
{

    [Header("Input Flow")]

    [SerializeField] private TMP_InputField _nameInput;
    [SerializeField] private GameObject _nameInputPage;
    [SerializeField] private GameObject _characterStatPage;

    [Header("Stat Text")]
    [SerializeField] private TMP_Text _nameText;
    [SerializeField] private TMP_Text _genderAgeText;
    [SerializeField] private TMP_Text _traitsText;
    [SerializeField] private GameObject[] _combatBoxes;
    [SerializeField] private GameObject[] _craftBoxes;
    [SerializeField] private GameObject[] _buildBoxes;
    [SerializeField] private GameObject[] _gatherBoxes;
    [SerializeField] private GameObject[] _socialBoxes;

    private CharacterEntity _previewCharacter;
    private string _characterName;


    private void OnEnable()
    {
        _nameInput.onSubmit.AddListener(OnSubmit);
    }

    private void OnDisable()
    {
        _nameInput.onSubmit.RemoveListener(OnSubmit);
    }

    private void OnSubmit(string _)
    {
        CameraManager.Instance.ActivateCamera(5);
        _characterName = _nameInput.text.Trim();

        _previewCharacter = GameManager.Instance.CharacterGenerator.SpawnCharacter(_characterName);
        
        ApplyCharacterToUI(_previewCharacter.Data);
        UIManager.Instance.AddCharacterSlot(_previewCharacter.Data.Name);


        _nameInputPage.SetActive(false);
        _characterStatPage.SetActive(true);
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

        _traitsText.text = data.Traits.Count > 0? string.Join("\n", data.Traits.ConvertAll(TextUtil.TranslateKorean)) : "특성 없음";
    }
    private void SetGauge(GameObject[] boxes, int value)
    {
        for (int i = 0; i < boxes.Length; i++)
        {
            boxes[i].SetActive(i < value);
        }
    }

    public void RerollCharacter()
    {
        CharacterData data = GameManager.Instance.CharacterGenerator
            .RerollCharacterData(_previewCharacter, _characterName);

        ApplyCharacterToUI(data);
    }
    public void GenerateCharacter()
    {
        CameraManager.Instance.ActivateCamera(0);

        _nameInputPage.SetActive(false);
        _characterStatPage.SetActive(false);

        GameManager.Instance.StartFirstTurn();
    }

    







}
