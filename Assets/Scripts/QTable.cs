using System.Collections.Generic;
using System.IO;
using UnityEngine;

/// <summary>
/// You should save your Q-table regularly so that you are preserving your 
/// learning. This Q-table should be able to be reloaded at any point to see optimal agent performance.
/// </summary>
public class QTable
{
    public QTableEntry[,] _qTable = new QTableEntry[20,20];

    public QTable()
    {
        for (int i = 0; i < 20; i++)
        {
            for (int j = 0; j < 20; j++)
            {
                var initialMoveValues = new float[4];
                for (int k = 0; k < 4; k++)
                {
                    // index 0: Up, index 1: Down, index 2: Left, index 3: Right
                    initialMoveValues[k] = Random.Range(0.001f, .01f);
                }

                // reverse j and i to mimick the player moving left to right and then up a row
                _qTable[i, j] = new QTableEntry(new Vector3(j, i, 0), initialMoveValues);
            }
        }
    }

    #region SaveAndLoad
    public void SaveQTable()
    {
        var streamWriter = new StreamWriter(Application.dataPath+"/"+"qTableValues.txt");
        for (var i = 0; i < _qTable.GetLength(0); i++)
        for (var j = 0; j < _qTable.GetLength(1); j++)
        {
            var stateString = _qTable[i, j].state.x+","+_qTable[i, j].state.y+","+_qTable[i, j].state.z;
            var valuesString = "";
            foreach (var value in _qTable[i,j].moveValues)
            {
                valuesString += value.ToString();
            }
            streamWriter.WriteLine(stateString+"/"+valuesString);
        }
    }

    public QTable LoadQTable()
    {
        var loadedQTable = new QTable();
        var sr = new StreamReader(Application.dataPath+"/"+"qTableValues.txt");
        var stateValues = new List<string>();
        var moveValues = new List<float[]>();
        while (sr.Peek() > 0)
        {
            var line = sr.ReadLine()?.Split("/");
            stateValues.Add(line[0]);
            var moveString = line[1];
            var moveArray = new float[4];
            for (var i = 0; i < moveArray.Length; i++)
            {
                moveArray[i] = float.Parse(moveString[i].ToString());
            }
            moveValues.Add(moveArray);
        }

        var stateCounter = 0;
        for (int i = 0; i < 20; i++)
        {
            for (int j = 0; j < 20; j++)
            {
                Debug.Log($"State: {stateValues[stateCounter]}, Moves: {moveValues[stateCounter]}");
            }
        }

        return loadedQTable;
    }
    #endregion
}

public struct QTableEntry
{
    public Vector3 state;
    public float[] moveValues;

    public QTableEntry(Vector3 state, float[] moveValues)
    {
        this.state = state;
        this.moveValues = moveValues;
    }
}

