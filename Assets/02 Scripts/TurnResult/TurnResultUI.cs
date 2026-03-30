using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class TurnResultUI : MonoBehaviour
{
    [SerializeField] private GameObject _root;
    [SerializeField] private TMP_Text _nameText;
    [SerializeField] private TMP_Text _genderAgeText;

    [SerializeField] private GameObject[] _combatBoxes;
    [SerializeField] private GameObject[] _craftBoxes;
    [SerializeField] private GameObject[] _buildBoxes;
    [SerializeField] private GameObject[] _gatherBoxes;
    [SerializeField] private GameObject[] _socialBoxes;

    [SerializeField] private TMP_Text _statusText;    
    [SerializeField] private TMP_Text _traitsText;   
    [SerializeField] private TMP_Text _equipmentText; 
    [SerializeField] private TMP_Text _rightLogText;

    private List<CharacterEntity> _characters;
    private List<List<string>> _logsPerCharacter;
    private int _index;
    private bool _waiting;

    public IEnumerator OpenAndWait(List<CharacterEntity> characters, List<List<string>> logsPerCharacter)
    {
        _characters = characters;
        _logsPerCharacter = logsPerCharacter;
        _index = 0;
        _waiting = true;

        _root.SetActive(true);
        Refresh();

        yield return new WaitUntil(() => _waiting == false);
    }

    public void OnClickNextCharacter()
    {
        if (_characters == null || _characters.Count == 0) return;

        _index++;
        if (_index >= _characters.Count)
        {
            _root.SetActive(false);
            _waiting = false;
            return;
        }

        Refresh();
    }

    private void Refresh()
    {
        CharacterEntity ch = _characters[_index];
        CharacterData d = ch.Data;
        CharacterStatus s = ch.Status;

        _rightLogText.text = (_logsPerCharacter[_index].Count > 0) ? string.Join("\n", _logsPerCharacter[_index]) : "로그 없음";

        string traits = d.Traits.Count > 0 ? string.Join(", ", d.Traits.ConvertAll(TextUtil.TranslateKorean)) : "없음";
        _nameText.text = d.Name;
        _genderAgeText.text = $"{TextUtil.TranslateKorean(d.Gender)} / {d.Age}";

        SetGauge(_combatBoxes, d.Stats[StatType.Combat]);
        SetGauge(_craftBoxes,  d.Stats[StatType.Craft]);
        SetGauge(_buildBoxes,  d.Stats[StatType.Build]);
        SetGauge(_gatherBoxes, d.Stats[StatType.Gather]);
        SetGauge(_socialBoxes, d.Stats[StatType.Social]);

        _statusText.text =
            $"체력: {s.Health:0}/{d.MaxHealth:0}\n" +
            $"기분: {s.Mood:0}/{d.MaxMood:0}\n" +
            $"허기: {s.Hunger:0}/{d.MaxHunger:0}\n" +
            $"수면: {s.Sleep:0}/{d.MaxSleep:0}";

        _traitsText.text = d.Traits.Count > 0? string.Join("\n", d.Traits.ConvertAll(TextUtil.TranslateKorean)): "없음";
        _equipmentText.text = BuildEquipmentText(ch.Equipment);

    }
    private void SetGauge(GameObject[] boxes, int value)
    {
        for (int i = 0; i < boxes.Length; i++)
            boxes[i].SetActive(i < value);
    }
    private string BuildEquipmentText(CharacterEquipment eq)
    {
        List<string> equipped = new List<string>();

        string weapon = ToKoreanWeapon(eq.Weapon);
        string armor = ToKoreanArmor(eq.Armor);
        string utility = ToKoreanUtility(eq.Utility);

        if (!string.IsNullOrEmpty(weapon)) equipped.Add(weapon);
        if (!string.IsNullOrEmpty(armor)) equipped.Add(armor);
        if (!string.IsNullOrEmpty(utility)) equipped.Add(utility);

        if (equipped.Count == 0) return "착용 장비 없음";
        return string.Join(", ", equipped) + " 착용중";
    }
    private string ToKoreanWeapon(WeaponType t)
    {
        switch (t)
        {
            case WeaponType.WoodenSpear: return "나무창";
            case WeaponType.StoneSpear: return "돌창";
            default: return "";
        }
    }
  
    private string ToKoreanArmor(ArmorType t)
    {
        switch (t)
        {
            case ArmorType.Cloth: return "옷";
            default: return "";
        }
    }

    private string ToKoreanUtility(UtilityType t)
    {
        switch (t)
        {
            case UtilityType.Fan: return "부채";
            default: return "";
        }
    }

}
