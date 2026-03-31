using System.Collections;
using UnityEngine;

public class EncounterEventController : MonoBehaviour
{
    [SerializeField] private CharacterGenerator _characterGenerator;
    [SerializeField] private EncounterUIController _encounterUI;

    [Header("Encounter Rule")]
    [SerializeField] private int _encounterIntervalBigTurn = 5;

    private bool _isWaiting;
    private CharacterData _candidate;
    public IEnumerator RunEncounter()
    {
        _candidate = _characterGenerator.CreateEncounterCandidate();
        _isWaiting = true;

        _encounterUI.Open(_candidate, OnAcceptWithName, OnReject);
        yield return new WaitUntil(() => _isWaiting == false);
    }


    public IEnumerator RunEncounterIfNeeded(int currentBigTurn)
    {
        if (currentBigTurn <= 0) yield break;
        if (currentBigTurn % _encounterIntervalBigTurn != 0) yield break;

        yield return RunEncounter();
    }

    private void OnAcceptWithName(string enteredName)
    {
        CharacterEntity spawned = _characterGenerator.SpawnCharacterFromData(_candidate, enteredName);

        if (spawned != null)
        UIManager.Instance.AddCharacterSlot(spawned);


        _isWaiting = false;
    }

    private void OnReject()
    {
        _isWaiting = false;
    }
}
