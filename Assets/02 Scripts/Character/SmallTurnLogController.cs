using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class SmallTurnLogController : MonoBehaviour
{
    [SerializeField] private TMP_Text _smallTurnLogText;
    [SerializeField] private int _maxLogLines = 5;

    private readonly List<string> _logLines = new List<string>();

    public void AddLog(string line)
    {
        _logLines.Add(line);

        if (_logLines.Count > _maxLogLines)
        {
            _logLines.RemoveAt(0);
        }

        _smallTurnLogText.text = string.Join("\n", _logLines);
    }

    public void ClearLogs()
    {
        _logLines.Clear();
        _smallTurnLogText.text = string.Empty;
    }
}
