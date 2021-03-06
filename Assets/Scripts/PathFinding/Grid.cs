using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grid : MonoBehaviour
{

    public LayerMask unwalkableMask;
    public Vector2 gridWorldSize;
    public float nodeRadius;
    float nodeDiameter;

    public bool zAxis = true;

    Node[,] grid;
    Vector2Int gridSize;

    public bool displayGridGizmos;

    void Awake()
    {
        nodeDiameter = nodeRadius * 2;
        gridSize.x = Mathf.RoundToInt(gridWorldSize.x / nodeDiameter);
        gridSize.y = Mathf.RoundToInt(gridWorldSize.y / nodeDiameter);

        CreateGrid();
    }

    public int MaxSize
    {
        get
        {
            return gridSize.x * gridSize.y;
        }
    }
    void CreateGrid()
    {
        grid = new Node[gridSize.x, gridSize.y];
        Vector3 worldBottomLeft;
        Vector3 direction;

        if (zAxis)
            direction = Vector3.forward;
        else
            direction = Vector3.up;

        worldBottomLeft = transform.position - Vector3.right * gridWorldSize.x / 2 - direction * gridWorldSize.y / 2;

        for (int x = 0; x < gridSize.x; x++)
        {
            for (int y = 0; y < gridSize.y; y++)
            {
                Vector3 worldPoint = worldBottomLeft + Vector3.right * (x * nodeDiameter + nodeRadius) + direction * (y * nodeDiameter + nodeRadius);
                bool walkable = !(Physics.CheckSphere(worldPoint, nodeRadius, unwalkableMask));

                grid[x, y] = new Node(walkable, worldPoint, new Vector2Int(x, y));
            }
        }
    }

    public List<Node> GetNeighbours(Node centerNode)
    {
        List<Node> neighbours = new List<Node>();

        for (int x = -1; x <= 1; x++)
        {
            for (int y = -1; y <= 1; y++)
            {
                if (x == 0 && y == 0)
                    continue;

                int checkX = centerNode.gridPosition.x + x;
                int checkY = centerNode.gridPosition.y + y;

                if (checkX >= 0 && checkX < gridSize.x && checkY >= 0 && checkX < gridSize.y)
                {
                    neighbours.Add(grid[checkX, checkY]);
                }
            }
        }

        return neighbours;
    }


    public Node GetNodeFromWorldPoint(Vector3 worldPosition)
    {
        float percentX = Mathf.Clamp01((worldPosition.x + gridWorldSize.x * 0.5f) / gridWorldSize.x);
        float percentY = Mathf.Clamp01(((zAxis ? worldPosition.z : worldPosition.y) + gridWorldSize.y * 0.5f) / gridWorldSize.y);

        int x = Mathf.RoundToInt((gridSize.x - 1) * percentX);
        int y = Mathf.RoundToInt((gridSize.y - 1) * percentY);


        return grid[x, y];
    }
    void OnDrawGizmos()
    {
        if (zAxis)
            Gizmos.DrawWireCube(transform.position, new Vector3(gridWorldSize.x, 0, gridWorldSize.y));
        else
            Gizmos.DrawWireCube(transform.position, new Vector3(gridWorldSize.x, gridWorldSize.y, 0));

        if (grid != null && displayGridGizmos)
        {
            foreach (Node n in grid)
            {
                Gizmos.color = n.walkable ? Color.white : Color.red;
                Gizmos.DrawWireCube(n.worldPosition, Vector3.one * (nodeDiameter - 0.1f));
            }
        }
    }
}
