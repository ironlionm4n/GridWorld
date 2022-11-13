using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Training : MonoBehaviour
{
    [SerializeField] private Agent agent;
    [SerializeField] private float lambda = .90f;
    [SerializeField] private float gamma = .20f;
    private ETable _eTable;
    [SerializeField] private float learningRate;


    private void Awake()
    {
        _eTable = new ETable(400);
    }

    // update the agents qTable - current state, next state, reward, current action, next action
    public void UpdateQTable(Vector3 currentPosition, Vector3 nextPosition,
        float reward, Direction currentAction, Direction nextAction)
    {
        var currentETableState = (int) (20 * currentPosition.y + currentPosition.x);
        // update each entry of the q table
        var predictedQTableEntry = agent.QTable._qTable[(int) currentPosition.x, (int) currentPosition.y];
        // Debug.Log($"nextPosX: {nextPosition.x}, nextPosY: {nextPosition.y}");
        var targetQTableEntry = agent.QTable._qTable[(int) nextPosition.x, (int) nextPosition.y];
        var delta = reward + gamma * (predictedQTableEntry.moveValues[(int) currentAction] -
                                      targetQTableEntry.moveValues[(int) nextAction]);
        _eTable[currentETableState, currentAction]++;
        for (int i = 0; i < 20; i++)
        {
            for (int j = 0; j < 20; j++)
            {
                for (int k = 0; k < 4; k++)
                {
                    agent.QTable._qTable[i, j].moveValues[k] += learningRate * (delta * (float) _eTable[20*j+i, (Direction) k]);
                    _eTable[20*j + i, (Direction) k] *= (gamma * lambda);
                }
            }
        }
    }
}