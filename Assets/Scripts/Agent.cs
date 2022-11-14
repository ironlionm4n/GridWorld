using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using Debug = UnityEngine.Debug;
using Random = UnityEngine.Random;

public enum Direction {Up, Down, Left, Right}

public class Agent : MonoBehaviour
{
    [SerializeField]
    private float explorationExploitationThreshold = .99f;
    [SerializeField] private float waitTime;
    [SerializeField] private float maxEpisodes;
    [SerializeField] private GameObject goal;
    [SerializeField] private List<Transform> wallsInGrid;
    [SerializeField] private Slider timeSlider;
    [SerializeField] private Button saveButton;
    [SerializeField] private Button stopTrainingButton;
    [SerializeField] private Button loadButton;
    [SerializeField] private Training training;
    [SerializeField] private TMP_Text episodeText;
    [SerializeField] private int maxStepsPerEpisode;
    private QTable _qTable;
    public QTable QTable => _qTable;
    private bool _hasHitWall = false;
    private bool _hasHitGoal = false;
    private Direction _currentAction;
    private int _episodeCounter = 0;
    private int _stepCounter = 0;
    private bool shouldTrain = true;

    private void Awake()
    {
        _qTable = new QTable();
    }

    public void StopGame()
    {
        StopAllCoroutines();
    }

    public void ResumeGame()
    {
        StartCoroutine(RestartGridWorld());
    }

    private void Start()
    {
        Debug.Log(timeSlider.value);
        waitTime = timeSlider.value;
        StartCoroutine(BeginGridWorld());
    }

    private IEnumerator BeginGridWorld()
    {
        SetAgentStartingPosition();
        while (_episodeCounter < maxEpisodes)
        {
            if (_stepCounter >= maxStepsPerEpisode)
            {
                StopAllCoroutines();
                StartCoroutine(RestartGridWorld());
            }
            var reward = ExploreOrExploit();
            _stepCounter++;
            if (reward == -1)
            {
                Debug.LogWarning("Agent Hit Wall");
                _hasHitWall = true;
                // need to do q table stuff
                // restart agent with new position
                StopAllCoroutines();
                StartCoroutine(RestartGridWorld());
            }
            if (reward == 10)
            {
                Debug.Log("Agent Found Goal");
                _hasHitGoal = true;
                StopAllCoroutines();
                StartCoroutine(RestartGridWorld());
            }
            //explorationExploitationThreshold = Mathf.Log( (_episodeCounter / maxEpisodes * 100) + 1f, maxEpisodes);
            //explorationExploitationThreshold += .15f;
            if (_episodeCounter < 100) explorationExploitationThreshold = 0;
            if (_episodeCounter < 200) explorationExploitationThreshold = 0.1f;
            if (_episodeCounter < 300) explorationExploitationThreshold = 0.25f;
            if (_episodeCounter < 400) explorationExploitationThreshold = 0.45f;
            if (_episodeCounter < 500) explorationExploitationThreshold = 0.55f;
            if (_episodeCounter < 600) explorationExploitationThreshold = 0.65f;
            if (_episodeCounter < 700) explorationExploitationThreshold = 0.75f;
            if (_episodeCounter >= 700) explorationExploitationThreshold = .9f;
            yield return new WaitForSeconds(waitTime);
        }
    }

    private void UpdateEpisodeText()
    {
        episodeText.text = $"Episode #: {_episodeCounter.ToString()}";
    }

    private IEnumerator RestartGridWorld()
    {
        _episodeCounter++;
        UpdateEpisodeText();
        Debug.Log("Restarting");
        _stepCounter = 0;
        training.ETable.ClearETable();
        yield return new WaitForSeconds(.5f);
        StartCoroutine(BeginGridWorld());
    }

    private void SetAgentStartingPosition()
    {
        bool isValidPosition = false;
        Vector3 agentStartPosition;
        do
        {
            agentStartPosition = GetPositionInsideGrid();
            isValidPosition = !PlayerHitWallInGrid(agentStartPosition) && !PlayerOutsideGridBounds(agentStartPosition);
            // Debug.Log($"Agent placed in an invalid position: {agentStartPosition.ToString()}, isValidPosition: {isValidPosition}");
        } while (!isValidPosition);
        transform.position = agentStartPosition;
    }

    private bool PlayerOutsideGridBounds(Vector3 agentPosition)
    {
        return (agentPosition.x is < 0 or > 19 || agentPosition.y is < 0 or > 19);
    }

    public void UpdateWaitTime()
    {
        waitTime = timeSlider.value;
    }
    
    private bool PlayerOutsideGridBounds()
    {
        var agentPosition = transform.position;
        return (agentPosition.x is < 0 or > 19 || agentPosition.y is < 0 or > 19);
    }

    private Vector3 GetPositionInsideGrid()
    {
        var xCoordinate = Random.Range(0, 20);
        var yCoordinate = Random.Range(0, 20);
       return new Vector3(xCoordinate, yCoordinate, 0f);
    }

    private int ExploreOrExploit()
    {
        var explorationOrExploitation = Random.Range(0f, 1f);
        if(shouldTrain)
            return explorationOrExploitation < explorationExploitationThreshold ? Exploit() : Explore();
        
        return Exploit();
    }



    /// <summary>
    /// Checks the position of the agent to see if they are outside the bounds of the grid, on the goal tile, or a regular tile
    /// </summary>
    /// <returns></returns>
    private int CheckPositionForReward()
    {
        var agentPosition = transform.position;
        var playerOutOfBounds = agentPosition.x is < 0 or > 19 || agentPosition.y is < 0 or > 19;
        if (playerOutOfBounds || PlayerHitWallInGrid())
        {
            return -1;
        }

        if (Vector3.Distance(agentPosition, goal.transform.position) < .001f)
        {
            return 10;
        }

        return 0;
    }
    
    /// <summary>
    /// Checks the position of the agent to see if they are outside the bounds of the grid, on the goal tile, or a regular tile
    /// </summary>
    /// <returns></returns>
    private int CheckPositionForReward(Vector3 agentPosition)
    {
        var playerOutOfBounds = agentPosition.x < 0 || agentPosition.x > 19 || agentPosition.y < 0 || agentPosition.y > 19;
        
        if (playerOutOfBounds || PlayerHitWallInGrid())
        {
            return -1;
        }

        if (Vector3.Distance(agentPosition, goal.transform.position) < .001f)
        {
            return 10;
        }

        return 0;
    }

    /// <summary>
    /// Determine if the player has made a bad move
    /// </summary>
    /// <returns>Boolean based off if the player is in a tile with a wall</returns>
    private bool PlayerHitWallInGrid()
    {
        var agentPosition = transform.position;
        foreach (var wall in wallsInGrid)
        {
            if (Vector3.Distance(agentPosition, wall.position) < .001f)
            {
                // Debug.Log($"agentPosition: {agentPosition}, wallPosition: {wall.position}, distance eval: {Vector3.Distance(agentPosition, wall.position) < .001f}");
                // Debug.Break();
                return true;
            }
        }
        return false;
    }
    
    private bool PlayerHitWallInGrid(Vector3 agentPosition)
    {
        foreach (var wall in wallsInGrid)
        {
            if (Vector3.Distance(agentPosition, wall.position) < .001f)
            {
                // Debug.Log($"agentPosition: {agentPosition}, wallPosition: {wall.position}, distance eval: {Vector3.Distance(agentPosition, wall.position) < .001f}");
                // Debug.Break();
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// Get which direction to move the agent in the environment
    /// </summary>
    /// <returns>Direction</returns>
    private Direction GetRandomMove()
    {
        switch(Random.Range(0, 4))
        {
            case 0:
            {
                return Direction.Up;
            }
            case 1:
            {
                return Direction.Down;
            }
            case 2:
            {
                return Direction.Left;
            }
            case 3:
            {
                return Direction.Right;
            }
            default:
            {
                Debug.LogError("Get Random Move Default Hit");
                Debug.Break();
                return Direction.Up;
            }
        }
    }

    #region ExploreExploit
    /// <summary>
    /// Try a random move
    /// </summary>
    private int Explore()
    {
        Debug.Log("~Exploring~");
        var movementDirection = GetRandomMove();
        var nextAction = movementDirection;
        var currentPos = transform.position;
        var nextPos = GetNextPosition(movementDirection);
        if (_currentAction != null)
        {
            training.UpdateQTableAndETable(currentPos, nextPos, CheckPositionForReward(nextPos),
                _currentAction, nextAction);
        }
        if (!(Vector3.Distance(nextPos, currentPos) < .01))
        {
            UpdatePositionOnGrid(movementDirection);
        }
        _currentAction = nextAction;
        return CheckPositionForReward();
    }

    /// <summary>
    /// Get the best move from the Q-Table
    /// </summary>
    private int Exploit()
    {
        Debug.Log("^Exploiting^");
        var position = transform.position;
        var qTableEntry = _qTable._qTable[(int) position.x, (int) position.y];
        var nextDirection = GetBestMoveFromQTable(qTableEntry);
        var nextAction = nextDirection;
        var nextPosition = GetNextPosition(nextDirection);
        var reward = CheckPositionForReward();
        if (_currentAction != null)
        {
            training.UpdateQTableAndETable(position, nextPosition, reward, _currentAction, nextAction);
        }
        if (!(Vector3.Distance(nextPosition, position) < .01))
        {
            UpdatePositionOnGrid(nextDirection);
        }

        _currentAction = nextAction;
        return CheckPositionForReward();
    }

    private Vector3 GetNextPosition(Direction nextDirection)
    {
        var nextXPos = transform.position.x;
        var nextYPos = transform.position.y;
        switch (nextDirection)
        {
            case Direction.Up:
            {
                // if the next move takes me outside the grid bounds then stay at that y spot
                nextYPos = Mathf.Min(transform.position.y + 1, 19);
                break;
            }
            case Direction.Down:
            {
                nextYPos = Mathf.Max(transform.position.y - 1, 0);
                break;
            }
            case Direction.Left:
            {
                nextXPos = Mathf.Max(transform.position.x - 1, 0);
                break;
            }
            case Direction.Right:
            {
                nextXPos = Mathf.Min(transform.position.x + 1, 19);
                break;
            }
            default:
            {
                Debug.LogWarning("Default case of GetNextPosition");
                return Vector3.zero;
            }
        }
        return new Vector3(nextXPos, nextYPos);
    }

    private Direction GetBestMoveFromQTable(QTableEntry qTableEntry)
    {
        var index = Array.IndexOf(qTableEntry.moveValues, qTableEntry.moveValues.Max());
        /*var moveValues = qTableEntry.moveValues;
        var highestAction = moveValues[0];
        for (int i = 1; i < moveValues.Length; i++)
        {
            if (moveValues[i] > highestAction)
            {
                highestAction = moveValues[i];
                index = i;
            }
        }*/
        switch (index)
        {
            case 0:
            {
                return Direction.Up;
            }
            case 1:
            {
                return Direction.Down;
            }
            case 2:
            {
                return Direction.Left;
            }
            case 3:
            {
                return Direction.Right;
            }
            default:
            {
                return Direction.Up;
            }
        }
    }

    #endregion

    private void UpdatePositionOnGrid(Direction direction)
    {
        switch (direction)
        {
            case Direction.Up:
            {
                transform.position += Vector3.up;
                break;
            }
            case Direction.Down:
            {
                transform.position += Vector3.down;
                break;
            }
            case Direction.Left:
            {
                transform.position += Vector3.left;
                break;
            }
            case Direction.Right:
            {
                transform.position += Vector3.right;
                break;
            }
        }
    }

    public void SaveQTable()
    {
        _qTable.SaveQTable();
    }

    public void HandleLoadQTableButtonClick()
    {
        StopAllCoroutines();
        _qTable = LoadQTable();
        StartCoroutine(RestartGridWorld());
    }
    
    private QTable LoadQTable()
    {
        var newQTable = _qTable.LoadQTable();
        Debug.Log("QTable Loaded");
        return newQTable;
    }

    public void HandleStopTraining()
    {
        StopAllCoroutines();
        shouldTrain = !shouldTrain;
        StartCoroutine(RestartGridWorld());
    }
}
