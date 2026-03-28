using TMPro;
using UnityEngine;
public class CharacterHeaderSlotUI : MonoBehaviour
{
    [SerializeField] private TMP_Text _nameText;

    public void SetName(string characterName)
    {
        _nameText.text = characterName;
    }
}