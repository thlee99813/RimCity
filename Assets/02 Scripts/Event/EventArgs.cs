using System;
using UnityEngine;

[Serializable]
public class StageActivatedEventArgs : EventArgs
{
    public StageContext StageContext;
    public StageActivatedEventArgs(StageContext stageContext)
    {
        StageContext = stageContext;
    }
}
[Serializable]
public class BigTurnStartedEventArgs : EventArgs
{
    public int BigTurn;
    public BigTurnStartedEventArgs(int bigTurn)
    {
        BigTurn = bigTurn;
    }
}


