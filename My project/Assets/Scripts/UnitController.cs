using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UnitController : MonoBehaviour
{
    [SerializeField] float movementSpeed = 1f;
    [SerializeField] public Button moveButton; // UI Button to trigger movement

    Transform selectedUnit;
    bool unitSelected = false;
    Queue<List<Node>> moveQueue = new Queue<List<Node>>();
    bool isMoving = false;

    GridManager gridManager;
    Pathfinding pathFinder;
    LineRenderer lineRenderer;
    GameObject destinationMarker;

    private List<Vector3> linePositions = new List<Vector3>();  // Store line renderer positions

    void Start()
    {
        gridManager = FindObjectOfType<GridManager>();
        pathFinder = FindObjectOfType<Pathfinding>();

        if (moveButton)
        {
            moveButton.onClick.AddListener(StartMoveQueue);
        }

        lineRenderer = gameObject.AddComponent<LineRenderer>();
        lineRenderer.startWidth = 0.1f;
        lineRenderer.endWidth = 0.1f;
        lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        lineRenderer.startColor = Color.green;
        lineRenderer.endColor = Color.green;
        lineRenderer.positionCount = 0;

        destinationMarker = GameObject.CreatePrimitive(PrimitiveType.Cube);
        destinationMarker.transform.localScale = new Vector3(0.3f, 0.3f, 0.3f);
        destinationMarker.GetComponent<Renderer>().material.color = Color.red;
        destinationMarker.SetActive(false);
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit))
            {
                if (hit.transform.CompareTag("Tile"))
                {
                    if (unitSelected)
                    {
                        Vector2Int targetCords = hit.transform.GetComponent<Tile>().cords;
                        Vector2Int startCords;

                        if (moveQueue.Count > 0)
                        {
                            // Get the last path in the queue
                            List<Node> lastPath = moveQueue.ToArray()[moveQueue.Count - 1];
                            // Get the last node in that path
                            Node lastNode = lastPath[lastPath.Count - 1];
                            startCords = lastNode.cords;
                        }
                        else
                        {
                            // If no moves are queued, start from the unit's current position
                            startCords = new Vector2Int(
                                Mathf.RoundToInt(selectedUnit.position.x / gridManager.UnityGridSize),
                                Mathf.RoundToInt(selectedUnit.position.z / gridManager.UnityGridSize)
                            );
                        }

                        pathFinder.SetNewDestination(startCords, targetCords);
                        List<Node> newPath = pathFinder.GetNewPath(startCords);
                        if (newPath.Count > 0)
                        {
                            moveQueue.Enqueue(newPath);
                            AppendToLineRenderer(newPath);
                        }
                    }
                }
                else if (hit.transform.CompareTag("Unit"))
                {
                    selectedUnit = hit.transform;
                    unitSelected = true;
                }
            }
        }
    }

    void StartMoveQueue()
    {
        if (!isMoving && moveQueue.Count > 0)
        {
            StartCoroutine(ProcessMoveQueue());
        }
    }

    IEnumerator ProcessMoveQueue()
    {
        isMoving = true;
        while (moveQueue.Count > 0)
        {
            List<Node> currentPath = moveQueue.Dequeue();
            yield return StartCoroutine(FollowPath(currentPath));
        }
        isMoving = false;
        ClearPathVisualization();
    }

    IEnumerator FollowPath(List<Node> path)
    {
        for (int i = 1; i < path.Count; i++)
        {
            Vector3 startPosition = selectedUnit.position;
            Vector3 endPosition = gridManager.GetPositionFromCoordinates(path[i].cords);
            float travelPercent = 0f;

            selectedUnit.LookAt(endPosition);

            while (travelPercent < 1f)
            {
                travelPercent += Time.deltaTime * movementSpeed;
                selectedUnit.position = Vector3.Lerp(startPosition, endPosition, travelPercent);
                yield return null;
            }
        }
    }

    void AppendToLineRenderer(List<Node> newPath)
    {
        if (linePositions.Count == 0)
        {
            // If linePositions is empty, add the unit's current position
            linePositions.Add(selectedUnit.position);
        }

        // Add new path positions without resetting linePositions
        foreach (var node in newPath)
        {
            Vector3 worldPosition = gridManager.GetPositionFromCoordinates(node.cords) + new Vector3(0, 0.75f, 0);
            linePositions.Add(worldPosition);
        }

        lineRenderer.positionCount = linePositions.Count;
        lineRenderer.SetPositions(linePositions.ToArray());

        if (linePositions.Count > 1)
        {
            Vector3 lastPosition = linePositions[linePositions.Count - 1];
            destinationMarker.SetActive(true);
            destinationMarker.transform.position = lastPosition;
        }
    }

    void ClearPathVisualization()
    {
        linePositions.Clear();
        lineRenderer.positionCount = 0;
        destinationMarker.SetActive(false);
    }
}
