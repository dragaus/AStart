using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCMovement : MonoBehaviour
{
    public float speed = 1.0f;
    public Queue<Vector2> mWayPoints = new Queue<Vector2>();

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(MoveTo());
    }

    public void AddWayPoint(Vector2 point)
    {
        mWayPoints.Enqueue(point);
    }

    public void SetDestination(RectGridBuilder map, RectGridCellA destination)
    {
        //Path finding here

        AddWayPoint(destination.Value);
    }

    // Update is called once per frame
    void Update()
    {
        
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
