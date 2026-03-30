using TMPro;
using UnityEngine;

public class NameplateUI : MonoBehaviour
{
    [SerializeField] private TMP_Text _nameText;
    [SerializeField] private RectTransform _rect;
    [SerializeField] private TMP_Text _moodText;

    public RectTransform Rect => _rect;

    private void Awake()
    {
    }

    public void SetName(string nameText)
    {
        _nameText.text = nameText;
    }

    public void SetMoodText(string text)
    {
        _moodText.text = string.IsNullOrEmpty(text) ? "" : text;
    }
}
