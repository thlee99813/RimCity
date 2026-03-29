using TMPro;
using UnityEngine;
public class CharacterHeaderSlotUI : MonoBehaviour
{
    public TMP_Text NameText;
    public string CharacterId { get; private set; }

    public void SetData(string characterId, string characterName)
    {
        CharacterId = characterId;
        NameText.text = characterName;
    }
}