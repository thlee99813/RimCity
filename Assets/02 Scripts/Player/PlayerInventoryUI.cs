using TMPro;
using UnityEngine;

public class PlayerInventoryUI : MonoBehaviour
{
    [SerializeField] private TMP_Text _berryText;
    [SerializeField] private TMP_Text _treeText;
    [SerializeField] private TMP_Text _rockText;
    [SerializeField] private TMP_Text _fiberText;

    [SerializeField] private TMP_Text _clothText;
    [SerializeField] private TMP_Text _woodenSpearText;
    [SerializeField] private TMP_Text _stoneSpearText;
    [SerializeField] private TMP_Text _fanText;
    [SerializeField] private TMP_Text _bandageText;
    [SerializeField] private TMP_Text _medkitText;



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
        _fiberText.text = _inventory.Fiber.ToString();
        _clothText.text = _inventory.Cloth.ToString();
        _woodenSpearText.text = _inventory.WoodenSpear.ToString();
        _stoneSpearText.text = _inventory.StoneSpear.ToString();
        _fanText.text = _inventory.Fan.ToString();
        _bandageText.text = _inventory.Bandage.ToString();
        _medkitText.text = _inventory.Medkit.ToString();
    }
}
