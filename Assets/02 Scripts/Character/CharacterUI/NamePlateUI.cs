using TMPro;
using UnityEngine;

public class NameplateUI : MonoBehaviour
{
    [SerializeField] private TMP_Text _nameText;
    public RectTransform Rect => (RectTransform)transform;

    public void SetName(string value)
    {
        _nameText.text = value;
    }
}
