﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Diagnostics;
using System;

public class PathFinding : MonoBehaviour
{

    PathRequestManager requestManager;

    Grid grid;

    void Awake()
    {
        grid = GetComponent<Grid>();
        requestManager = GetComponent<PathRequestManager>();

    }

    public void StartFindPath(Vector3 startPos, Vector3 targetPos)
    {
        StartCoroutine(FindPath(startPos, targetPos));
    }


    IEnumerator FindPath(Vector3 startPos, Vector3 targetPos)
    {
        Stopwatch sw = new Stopwatch();

        sw.Start();

        Vector3[] waypoints = new Vector3[0];
        bool pathSuccess = false;

        Node startNode = grid.GetNodeFromWorldPoint(startPos);
        Node targetNode = grid.GetNodeFromWorldPoint(targetPos);

        if (startNode.walkable && targetNode.walkable)
        {
            Heap<Node> openSet = new Heap<Node>(grid.MaxSize);
            HashSet<Node> closedSet = new HashSet<Node>();

            openSet.Add(startNode);

            while (openSet.Count > 0)
            {
                Node currentNode = openSet.RemoveFirst();

                closedSet.Add(currentNode);

                if (currentNode == targetNode)
                {
                    sw.Stop();
                    print("Path Found " + sw.ElapsedMilliseconds + " ms");
                    pathSuccess = true;

                    break;
                }

                foreach (Node neighbour in grid.GetNeighbours(currentNode))
                {
                    if (!neighbour.walkable || closedSet.Contains(neighbour))
                        continue;

                    int newMovementCostToNeighbour = currentNode.gCost + GetDistance(currentNode, neighbour);

                    if (newMovementCostToNeighbour < neighbour.gCost || !openSet.Contains(neighbour))
                    {
                        neighbour.gCost = newMovementCostToNeighbour;
                        neighbour.hCost = GetDistance(neighbour, targetNode);

                        neighbour.parent = currentNode;


                        if (!openSet.Contains(neighbour))
                            openSet.Add(neighbour);
                    }
                }

            }
        }

        yield return null;
        if (pathSuccess)
        {
            waypoints = RetracePath(startNode, targetNode);
        }

        requestManager.FinishedProcessingPath(waypoints, pathSuccess);

    }


    Vector3[] RetracePath(Node startNode, Node endNode)
    {
        List<Node> path = new List<Node>();

        Node currentNode = endNode;

        while (currentNode != startNode)
        {
            path.Add(currentNode);

            currentNode = currentNode.parent;
        }
        Vector3[] waypoints = SimplifyPath(path);
        Array.Reverse(waypoints);

        return waypoints;
    }

    Vector3[] SimplifyPath(List<Node> path)
    {
        List<Vector3> waypoints = new List<Vector3>();
        Vector2 directionOld = Vector2.zero;

        for (int i = 1; i < path.Count; i++)
        {
            Vector2 directionNew = new Vector2(path[i - 1].gridPosition.x - path[i].gridPosition.x, path[i - 1].gridPosition.y - path[i].gridPosition.y);
            if (directionNew != directionOld)
            {
                waypoints.Add(path[i].worldPosition);
            }

            directionOld = directionNew;
        }

        return waypoints.ToArray();
    }

    int GetDistance(Node nodeA, Node nodeB)
    {

        int distanceX = Mathf.Abs(nodeA.gridPosition.x - nodeB.gridPosition.x);
        int distanceY = Mathf.Abs(nodeA.gridPosition.y - nodeB.gridPosition.y);

        if (distanceX > distanceY)
            return 14 * distanceY + (10 * (distanceX - distanceY));
        else
            return 14 * distanceX + (10 * (distanceY - distanceX));

    }
}