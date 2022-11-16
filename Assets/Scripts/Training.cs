using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Training : MonoBehaviour
{
    [SerializeField] private Agent agent;
    [SerializeField] private float gamma = .20f;
    [SerializeField] private float learningRate;
    private ETable _eTable;
    public ETable ETable => _eTable;
    public float lambda = .2f;


    private void Awake()
    {
        _eTable = new ETable(400);
    }

    /// <summary>
    /// Update the QTable and ETable given the current state-action, next state-action, nextReward
    /// </summary>
    /// <param name="currentPosition"></param>
    /// <param name="nextPosition"></param>
    /// <param name="reward"></param>
    /// <param name="currentAction"></param>
    /// <param name="nextAction"></param>
    public void UpdateQTableAndETable(Vector3 currentPosition, Vector3 nextPosition, float reward, Direction currentAction, Direction nextAction)
    {
        var currentETableState = (int) (20 * currentPosition.x + currentPosition.y);
        var predictedQTableEntry = agent.QTable._qTable[(int) currentPosition.x, (int) currentPosition.y];
        var targetQTableEntry = agent.QTable._qTable[(int) nextPosition.x, (int) nextPosition.y];
        var delta = reward + gamma * (targetQTableEntry.moveValues[(int) nextAction]) - predictedQTableEntry.moveValues[(int) currentAction];
        _eTable[currentETableState, currentAction] += 1;
        for (int i = 0; i < 20; i++)
        {
            for (int j = 0; j < 20; j++)
            {
                for (int k = 0; k < 4; k++)
                {
                    agent.QTable._qTable[i, j].moveValues[k] += learningRate * delta * (float) _eTable[20*i+j, (Direction) k];
                    _eTable[20*i + j, (Direction) k] *= (gamma * lambda);
                }
            }
        }
    }
}