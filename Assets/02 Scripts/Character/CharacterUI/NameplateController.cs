using UnityEngine;
using System.Collections.Generic;

public class NameplateController : MonoBehaviour
{
    [SerializeField] private Camera _worldCamera;
    [SerializeField] private RectTransform _canvasRect;
    [SerializeField] private NameplatePool _pool;
    [SerializeField] private Vector3 _headOffset = new(0f, 2f, 0f);

    private readonly Dictionary<CharacterEntity, NameplateUI> _active = new();
    private readonly List<CharacterEntity> _removeBuffer = new();

    private void LateUpdate()
    {
        List<CharacterEntity> characters = CharacterManager.Instance.ActiveCharacters;

        for (int i = 0; i < characters.Count; i++)
        {
            CharacterEntity character = characters[i];
            if (character == null || character.IsDead || character.Data == null) continue;

            Vector3 worldPos = character.transform.position + _headOffset;

            Vector3 viewport = _worldCamera.WorldToViewportPoint(worldPos);
            bool visible = viewport.z > 0f && viewport.x > 0f && viewport.x < 1f && viewport.y > 0f && viewport.y < 1f;

            if (!visible)
            {
                if (_active.TryGetValue(character, out NameplateUI hiddenUi))
                {
                    _pool.Release(hiddenUi);
                    _active.Remove(character);
                }
                continue;
            }

            if (!_active.TryGetValue(character, out NameplateUI ui))
            {
                ui = _pool.Get();
                _active.Add(character, ui);
            }

            ui.SetName(character.Data.Name);
            ui.SetMoodText(character.MoodHintText);

            Vector2 screenPos = RectTransformUtility.WorldToScreenPoint(_worldCamera, worldPos);
            RectTransformUtility.ScreenPointToLocalPointInRectangle(_canvasRect, screenPos, null, out Vector2 localPos);
            ui.Rect.anchoredPosition = localPos;

        }

        _removeBuffer.Clear();

        foreach (KeyValuePair<CharacterEntity, NameplateUI> characterToNameplate in _active)
        {
            CharacterEntity registeredCharacter = characterToNameplate.Key;

            if (registeredCharacter == null || registeredCharacter.IsDead || !characters.Contains(registeredCharacter))
            {
                _removeBuffer.Add(registeredCharacter);
            }
        }

        for (int i = 0; i < _removeBuffer.Count; i++)
        {
            CharacterEntity targetCharacter = _removeBuffer[i];
            if (!_active.TryGetValue(targetCharacter, out NameplateUI targetNameplate)) continue;

            _pool.Release(targetNameplate);
            _active.Remove(targetCharacter);
        }
    }


}
