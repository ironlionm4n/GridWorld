using System;
using System.Collections.Generic;

/// <summary>The e-table is tracking the eligibility for each space to 
/// be updated, so it should be set to 1 when that state/action pair is visited and reduced by lambda each step 
///     taken afterwards. For example, if the agent is in state 4 and takes action 1, that e(s,a) would be set to 1 (or have 
/// 1 added to it, feel free to experiment) for that update step. After the next action is taken, the e(s4,a1) would be 
/// diminished by lambda, so it may hold a value of 0.90, and then the next action taken would reduce this to 0.81, 
/// etc.). The e-table is reset each time an agent is restarted, but the Q-table is persistent.
/// </summary>
public class ETable
{
    private Dictionary<int, Dictionary<Direction, double>> eTable; 
    private int _numberOfStates;

    public ETable(int numberOfStates)
    {
        this._numberOfStates = numberOfStates;
        eTable = new Dictionary<int, Dictionary<Direction, double>>();
        for (int i = 0; i < numberOfStates; i++)
        {
            eTable[i] = new Dictionary<Direction, double>();
            foreach (Direction action in Enum.GetValues(typeof(Direction)))
            {
                eTable[i][action] = 0;
            }
        }
    }

    public void ClearETable(){
        for (int i = 0; i < _numberOfStates; i++)
        {
            foreach (Direction action in Enum.GetValues(typeof(Direction)))
            {
                eTable[i][action] = 0;
            }
        }
    }

    public void UpdateETable(int state, Direction action, double value)
    {
        eTable[state][action] = value;
    }

    public double GetETableValue(int state, Direction action)
    {
        return eTable[state][action];
    }

    public double this[int state, Direction action]
    {
        get => eTable[state][action];
        set => eTable[state][action] = value;
    }
}