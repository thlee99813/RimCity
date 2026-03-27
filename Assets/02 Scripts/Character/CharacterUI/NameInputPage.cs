using UnityEngine;
using TMPro;

public class NameInputPage : MonoBehaviour
{
    [SerializeField] private TMP_InputField nameInput;
    [SerializeField] private GameObject currentPage;
    [SerializeField] private GameObject nextPage;


    private void OnEnable()
    {
        nameInput.onSubmit.AddListener(OnSubmit); // Enter 입력 시 호출
    }

    private void OnDisable()
    {
        nameInput.onSubmit.RemoveListener(OnSubmit);
    }

    private void OnSubmit(string _)
    {
        currentPage.SetActive(false);
        nextPage.SetActive(true);
    }









}
