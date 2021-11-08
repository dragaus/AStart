using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PathFinding;
using System;

public class NPCMovement : MonoBehaviour
{
    public float speed = 1.0f;
    public Queue<Vector2> mWayPoints = new Queue<Vector2>();

    PathFinder<Vector2Int> pathFinder = new AStarPathFinder<Vector2Int>();

    // Start is called before the first frame update
    void Start()
    {
        pathFinder.onSuccess += OnSuccesPathFinding;
        pathFinder.onFailure += OnFailurePathFinding;
        pathFinder.HeuristicCost = RectGridBuilder.GetManhattanCost;
        pathFinder.NodeTraversalCost = RectGridBuilder.GetEuclidianCost;

        StartCoroutine(MoveTo());
    }

    private void OnFailurePathFinding()
    {
        Debug.LogError("Theres no path to destination");
    }

    private void OnSuccesPathFinding()
    {
        PathFinder<Vector2Int>.PathFinderNode node = pathFinder.CurrentNode;

        List<Vector2Int> reverseIndices = new List<Vector2Int>();

        while (node != null)
        {
            reverseIndices.Add(node.Location.Value);
            node = node.Parent;
        }

        for (int i = reverseIndices.Count - 1; i >= 0; i--) 
        {
            AddWayPoint(new Vector2(reverseIndices[i].x, reverseIndices[i].y));
        }
    }

    private void OnDestroy()
    {
        pathFinder.onSuccess -= OnSuccesPathFinding;
        pathFinder.onFailure -= OnFailurePathFinding;
        pathFinder.HeuristicCost = null;
        pathFinder.NodeTraversalCost = null;
    }

    public void AddWayPoint(Vector2 point)
    {
        mWayPoints.Enqueue(point);
    }

    public void SetDestination(RectGridBuilder map, RectGridCellA destination)
    {
        //Path finding here

        //AddWayPoint(destination.Value);

        if (pathFinder.Status == PathFinderStatus.RUNNING) { return; }

        mWayPoints.Clear();

        RectGridCellA start = map.GetRectGridCellA(
            (int)transform.position.x,
            (int)transform.position.y);

        if (start == null) return;

        pathFinder.Initialize(start, destination);
        StartCoroutine(FindPathSteps());
    }

    IEnumerator FindPathSteps()
    {
        while (pathFinder.Status == PathFinderStatus.RUNNING) 
        {
            pathFinder.Step();
            yield return null;
        }
    }

    IEnumerator MoveTo()
    {
        while (true)
        {
            while (mWayPoints.Count > 0)
            {
                yield return StartCoroutine(MoveToPoint(mWayPoints.Dequeue(), speed));
            }

            yield return null;
        }
    }

    IEnumerator MoveToPoint(Vector2 point, float speed)
    {
        Vector3 endPos = new Vector3(point.x, point.y, transform.position.z);
        float duration = (transform.position - endPos).magnitude / speed;

        yield return StartCoroutine(MoveOverSeconds(transform.gameObject, endPos, duration));
    }

    IEnumerator MoveOverSeconds(GameObject movableObject, Vector3 endPos, float duration)
    {
        float elapsedTime = 0f;

        Vector3 startingPos = movableObject.transform.position;

        while (elapsedTime < duration)
        {
            movableObject.transform.position = Vector3.Lerp(
                startingPos,
                endPos,
                (elapsedTime / duration));

            elapsedTime += Time.deltaTime;

            yield return new WaitForEndOfFrame();
        }

        movableObject.transform.position = endPos;
    }
}
