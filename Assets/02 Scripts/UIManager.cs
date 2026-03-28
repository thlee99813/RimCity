using UnityEngine;
using TMPro;

public class UIManager : Singleton<UIManager>
{
    protected override void Init()
    {
        _currentPage.SetActive(true);
        _nextPage.SetActive(false);
        _smallTurnPage.SetActive(false);
        _bigTurnPage.SetActive(false);
        _ingameBackGround.SetActive(false);

    }    
    
    [SerializeField] private TMP_InputField _nameInput;
    [SerializeField] private GameObject _currentPage;
    [SerializeField] private GameObject _nextPage;
    [SerializeField] private GameObject _smallTurnPage;
    [SerializeField] private GameObject _bigTurnPage;
    [SerializeField] private GameObject _ingameBackGround;



    
}
