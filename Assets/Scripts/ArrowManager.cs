using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;

public class ArrowManager : MonoBehaviour
{
    [SerializeField] private GameObject upArrowPrefab;
    [SerializeField] private GameObject downArrowPrefab;
    [SerializeField] private GameObject leftArrowPrefab;
    [SerializeField] private GameObject rightArrowPrefab;

    public void PlaceArrow(float value, Direction direction, Vector3 placementPosition)
    {
        var xYScaleValue = value;
        if (value < .5f)
        {
            xYScaleValue = .5f;
        }
        else if (value < .8f)
        {
            xYScaleValue = .75f;
        }
        else if (value > .9f)
        {
            xYScaleValue = 1f;
        }

        switch (direction)
        {
            case Direction.Up:
            {
                var arrow = Instantiate(upArrowPrefab, placementPosition, quaternion.identity);
                arrow.transform.localScale = new Vector3(xYScaleValue, xYScaleValue, 1);
                break;
            }
            case Direction.Down:
            {
                var arrow = Instantiate(downArrowPrefab, placementPosition, quaternion.identity);
                arrow.transform.localScale = new Vector3(xYScaleValue, xYScaleValue, 1);
                break;
            }
            case Direction.Left:
            {
                var arrow = Instantiate(leftArrowPrefab, placementPosition, quaternion.identity);
                arrow.transform.localScale = new Vector3(xYScaleValue, xYScaleValue, 1);
                break;
            }
            case Direction.Right:
            {
                var arrow = Instantiate(rightArrowPrefab, placementPosition, quaternion.identity);
                arrow.transform.localScale = new Vector3(xYScaleValue, xYScaleValue, 1);
                break;
            }
        }
    }

    public void DestroyArrowsOnRestart()
    {
        var arrows = GameObject.FindGameObjectsWithTag("Arrow").ToList();
        foreach (var arrow in arrows)
        {
            Destroy(arrow);
        }
    }
}
