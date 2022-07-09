using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class PathRequestManager : MonoBehaviour
{
    Queue<PathRequest> pathRequestQueue = new Queue<PathRequest>();
    static PathRequestManager instance;

    PathFinding pathFinding;

    bool isProcessingPath = false;

    PathRequest currentPathRequest;

    void Awake()
    {
        instance = this;
        pathFinding = GetComponent<PathFinding>();

    }
    public static void RequestPath(Vector3 pathStart, Vector3 pathEnd, Action<Vector3[], bool> callback)
    {
        Debug.Log("Request Path");
        PathRequest newRequest = new PathRequest(pathStart, pathEnd, callback);

        instance.pathRequestQueue.Enqueue(newRequest);

        instance.TryProcessNext();
    }

    void TryProcessNext()
    {
        Debug.Log("TryProcessNext");

        if (!isProcessingPath && pathRequestQueue.Count > 0)
        {
            Debug.Log("Processing");

            currentPathRequest = pathRequestQueue.Dequeue();
            isProcessingPath = true;

            pathFinding.StartFindPath(currentPathRequest.pathStart, currentPathRequest.pathEnd);
        }
    }

    public void FinishedProcessingPath(Vector3[] path, bool success)
    {
        currentPathRequest.callback(path, success);
        isProcessingPath = false;
        TryProcessNext();
    }

    struct PathRequest
    {
        public Vector3 pathStart;
        public Vector3 pathEnd;

        public Action<Vector3[], bool> callback;

        public PathRequest(Vector3 _pathStart, Vector3 _pathEnd, Action<Vector3[], bool> _callback)
        {
            pathStart = _pathStart;
            pathEnd = _pathEnd;
            callback = _callback;
        }
    }
}
