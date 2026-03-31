using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CharacterHeaderSlotUI : MonoBehaviour
{
    public TMP_Text NameText;
    public string CharacterId { get; private set; }

    private CharacterEntity _character;
    public Image _hpFillImage;

    private void Awake()
    {

        _hpFillImage.type = Image.Type.Filled;
        _hpFillImage.fillMethod = Image.FillMethod.Horizontal;
        _hpFillImage.fillOrigin = (int)Image.OriginVertical.Bottom;
        _hpFillImage.fillAmount = 1f;
    }

    public void Bind(CharacterEntity character)
    {
        _character = character;
        CharacterId = character.Data.Id;
        NameText.text = character.Data.Name;
        Refresh();
    }

    public void Refresh()
    {
        float ratio = Mathf.Clamp01(_character.Status.Health / _character.Data.MaxHealth);
        _hpFillImage.fillAmount = ratio;
    }
}
