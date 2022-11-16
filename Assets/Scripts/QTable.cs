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
    public QTable(Vector3[,] states)
    {
        for (int i = 0; i < 20; i++)
        {
            for (int j = 0; j < 20; j++)
            {
                var initialMoveValues = new float[4];
                for (int k = 0; k < 4; k++)
                {
                    // index 0: Up, index 1: Down, index 2: Left, index 3: Right
                    initialMoveValues[k] = Random.Range(0.0001f, .0005f);
                }

                // reverse j and i to mimick the player moving left to right and then up a row
                _qTable[i, j] = new QTableEntry(new Vector3(j, i, 0), initialMoveValues);
            }
        }
    }

    private QTable(List<Vector3> stateValues, List<float[]> moveValues)
    {
        for (int i = 0; i < 20; i++)
        {
            for (int j = 0; j < 20; j++)
            {
                // reverse j and i to mimick the player moving left to right and then up a row
                _qTable[i, j] = new QTableEntry(stateValues[20*i+j], moveValues[20*i+j]);
            }
        }
    }

    #region SaveAndLoad
    public void SaveQTable()
    {
        var streamWriter = new StreamWriter(Application.dataPath+"/"+"qTableValues.txt");
        for (var i = 0; i < 20; i++)
        {
            for (var j = 0; j < 20; j++)
            {
                var stateString = _qTable[i, j].state.x+","+_qTable[i, j].state.y+","+_qTable[i, j].state.z;
                var valuesString = "";
                foreach (var value in _qTable[i,j].moveValues)
                {
                    var moveValue = value + ",";
                    valuesString += moveValue;
                }
                streamWriter.WriteLine(stateString+"/"+valuesString);
            }
        }
        // Would not finish writing all of the lines to the text file if left open
        streamWriter.Close();
        Debug.Log("Finished Saving QTable");
    }

    public QTable LoadQTable()
    {
        var sr = new StreamReader(Application.dataPath+"/"+"qTableValues.txt");
        var stateValues = new List<Vector3>();
        var moveValues = new List<float[]>();
        while (sr.Peek() > 0)
        {
            var line = sr.ReadLine()?.Split("/");
            var vectorThreeString = line[0].Split(','); // x,y,z
            stateValues.Add(new Vector3(float.Parse(vectorThreeString[0]), float.Parse(vectorThreeString[1])
                , float.Parse(vectorThreeString[2])));
            var moveString = line[1].Split(','); // up,down,left,right,
            var moveArray = new float[4];
            for (var i = 0; i < moveArray.Length; i++)
            {
                moveArray[i] = float.Parse(moveString[i]);
            }
            moveValues.Add(moveArray);
        }

        var loadedQTable = new QTable(stateValues, moveValues);
        
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

