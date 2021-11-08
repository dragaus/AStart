using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PathFinding;

public class RectGridBuilder : MonoBehaviour
{
    //Estas son las columnas que voy a tener
    public int columns;

    //Estos los rows
    public int rows;

    [SerializeField]
    GameObject rectGridCellPrefab;

    GameObject[,] mRectGridCellGameObjects;

    protected Vector2Int[,] mIndices;

    protected RectGridCellA[,] rectGridCellAs;

    //Construimos nuestro grid
    protected void Construct(int numX, int numY)
    {
        columns = numX;
        rows = numY;

        mIndices = new Vector2Int[columns, rows];
        mRectGridCellGameObjects = new GameObject[columns, rows];
        rectGridCellAs = new RectGridCellA[columns, rows];

        for (int x = 0; x < columns; ++x)
        {
            for (int y = 0; y < rows; ++y)
            {
                mIndices[x, y] = new Vector2Int(x, y);

                mRectGridCellGameObjects[x, y] = Instantiate(
                    rectGridCellPrefab,
                    new Vector3(x, y, 0.0f),
                    Quaternion.identity);

                mRectGridCellGameObjects[x, y].transform.SetParent(transform);

                mRectGridCellGameObjects[x, y].name = $"cell_{x}_{y}";

                rectGridCellAs[x, y] = new RectGridCellA(this, mIndices[x, y]);

                RectGridCellC rectGridCellC =
                    mRectGridCellGameObjects[x, y].GetComponent<RectGridCellC>();

                if (rectGridCellC != null)
                {
                    rectGridCellC.gridCellA = rectGridCellAs[x, y];
                }
            }
        }
    }

    public List<Node<Vector2Int>> GetNeighboursCells(Node<Vector2Int> location)
    {
        var neighbours = new List<Node<Vector2Int>>();

        int x = location.Value.x;
        int y = location.Value.y;

        //Este revisa el de arriba
        if (y < rows - 1)
        {
            int x1 = x;
            int y1 = y + 1;
            if (rectGridCellAs[x1, y1].isWalkable)
            {
                neighbours.Add(rectGridCellAs[x1, y1]);
            }
        }

        //Este revisa el de abajo
        if (y > 0)
        {
            int x1 = x;
            int y1 = y - 1;
            if (rectGridCellAs[x1, y1].isWalkable)
            {
                neighbours.Add(rectGridCellAs[x1, y1]);
            }
        }

        //Este revisa a la derecha
        if (x < columns - 1)
        {
            int x1 = x + 1;
            int y1 = y;
            if (rectGridCellAs[x1, y1].isWalkable)
            {
                neighbours.Add(rectGridCellAs[x1, y1]);
            }
        }

        //Este revisa a la izquierda
        if (x > 0)
        {
            int x1 = x - 1;
            int y1 = y;
            if (rectGridCellAs[x1, y1].isWalkable)
            {
                neighbours.Add(rectGridCellAs[x1, y1]);
            }
        }

        //Arriba y derecha
        if (y < rows - 1 && x < columns - 1)
        {
            int x1 = x + 1;
            int y1 = y + 1;
            if (rectGridCellAs[x1, y1].isWalkable)
            {
                neighbours.Add(rectGridCellAs[x1, y1]);
            }
        }

        //Arriba e izquierda
        if (y < rows - 1 && x > 0)
        {
            int x1 = x - 1;
            int y1 = y + 1;
            if (rectGridCellAs[x1, y1].isWalkable)
            {
                neighbours.Add(rectGridCellAs[x1, y1]);
            }
        }

        //abajo e izquireda
        if (y >0 && x > 0)
        {
            int x1 = x - 1;
            int y1 = y - 1;
            if (rectGridCellAs[x1, y1].isWalkable)
            {
                neighbours.Add(rectGridCellAs[x1, y1]);
            }
        }

        //abajo y derecha
        if (y > 0 && x < columns - 1)
        {
            int x1 = x + 1;
            int y1 = y - 1;
            if (rectGridCellAs[x1, y1].isWalkable)
            {
                neighbours.Add(rectGridCellAs[x1, y1]);
            }
        }

        return neighbours;
    }

    void ResetCamera()
    {
        Camera.main.orthographicSize = rows / 2.0f + 1f;

        Camera.main.transform.position = new Vector3(columns / 2f - 0.5f, rows / 2f - 0.5f, -100f);
    }

    // Start is called before the first frame update
    void Start()
    {
        Construct(columns, rows);

        ResetCamera();
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(1))
        {
            RaycastAndToogleWalkable();
        }
    }

    void RaycastAndToogleWalkable()
    {

        Vector2 rayPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        RaycastHit2D hit = Physics2D.Raycast(rayPos, Vector2.zero, Mathf.Infinity);

        if (hit)
        {
            GameObject obj = hit.transform.gameObject;

            var cellC = obj.GetComponent<RectGridCellC>();

            
        }
    }




}
