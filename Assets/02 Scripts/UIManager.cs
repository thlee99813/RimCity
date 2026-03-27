using UnityEngine;
using TMPro;

public class UIManager : Singleton<UIManager>
{
    protected override void Init()
    {
        _currentPage.SetActive(true);
        _nextPage.SetActive(false);

    }    
    
    [SerializeField] private TMP_InputField _nameInput;
    [SerializeField] private GameObject _currentPage;
    [SerializeField] private GameObject _nextPage;


    
}
