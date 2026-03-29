using TMPro;
using UnityEngine;

public class PlayerInventoryUI : MonoBehaviour
{
    [SerializeField] private TMP_Text _berryText;
    [SerializeField] private TMP_Text _treeText;
    [SerializeField] private TMP_Text _rockText;

    private PlayerResourceInventory _inventory;

    private void OnEnable()
    {
        _inventory = GameManager.Instance.PlayerInventory;
        _inventory.OnChanged += Refresh;
        Refresh();
    }

    private void OnDisable()
    {
        _inventory.OnChanged -= Refresh;
    }

    private void Refresh()
    {
        _berryText.text = _inventory.Berry.ToString();
        _treeText.text = _inventory.Tree.ToString();
        _rockText.text = _inventory.Rock.ToString();
    }
}
