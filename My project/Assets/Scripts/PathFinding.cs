using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pathfinding : MonoBehaviour
{
    [SerializeField] Vector2Int startCords;
    public Vector2Int StartCords { get { return startCords; } }

    [SerializeField] Vector2Int targetCords;
    public Vector2Int TargetCords { get { return targetCords; } }

    private Node startNode;
    private Node targetNode;
    private Node currentNode;

    private Queue<Node> frontier = new Queue<Node>();
    private Dictionary<Vector2Int, Node> reached = new Dictionary<Vector2Int, Node>();

    private GridManager gridManager;
    private Dictionary<Vector2Int, Node> grid = new Dictionary<Vector2Int, Node>();

    private Vector2Int[] searchOrder = {
        Vector2Int.right, Vector2Int.left, Vector2Int.up, Vector2Int.down,
        new Vector2Int(1, 1), new Vector2Int(1, -1), new Vector2Int(-1, 1), new Vector2Int(-1, -1)
    };

    private List<Node> path = new List<Node>();
    private LineRenderer lineRenderer;
    private GameObject destinationMarker;

    private void Awake()
    {
        gridManager = FindObjectOfType<GridManager>();
        if (gridManager != null)
        {
            grid = gridManager.Grid;
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

    public List<Node> GetNewPath(Vector2Int? coordinates = null)
    {
        if (!grid.TryGetValue(startCords, out startNode) || !grid.TryGetValue(targetCords, out targetNode))
        {
            Debug.LogError("Invalid start or target node!");
            return new List<Node>();
        }

        gridManager.ResetNodes();
        BreadthFirstSearch(coordinates ?? startCords);
        path = BuildPath();
        UpdateLineRenderer();
        return path;
    }

    private void BreadthFirstSearch(Vector2Int coordinates)
    {
        if (!grid.TryGetValue(coordinates, out Node start))
        {
            Debug.LogError("Invalid start position!");
            return;
        }

        startNode.walkable = true;
        targetNode.walkable = true;

        frontier.Clear();
        reached.Clear();

        frontier.Enqueue(start);
        reached.Add(coordinates, start);

        while (frontier.Count > 0)
        {
            currentNode = frontier.Dequeue();
            currentNode.explored = true;

            if (currentNode.cords == targetCords)
            {
                return;
            }

            ExploreNeighbors();
        }
    }

    private void ExploreNeighbors()
    {
        foreach (Vector2Int direction in searchOrder)
        {
            Vector2Int neighborCords = currentNode.cords + direction;

            if (!grid.TryGetValue(neighborCords, out Node neighbor) || !neighbor.walkable || reached.ContainsKey(neighborCords))
            {
                continue;
            }

            if (Mathf.Abs(direction.x) == 1 && Mathf.Abs(direction.y) == 1)
            {
                Vector2Int horizontal = new Vector2Int(direction.x, 0);
                Vector2Int vertical = new Vector2Int(0, direction.y);

                if (!grid.ContainsKey(currentNode.cords + horizontal) || !grid[currentNode.cords + horizontal].walkable ||
                    !grid.ContainsKey(currentNode.cords + vertical) || !grid[currentNode.cords + vertical].walkable)
                {
                    continue;
                }
            }

            neighbor.connectTo = currentNode;
            reached.Add(neighborCords, neighbor);
            frontier.Enqueue(neighbor);
        }
    }

    private List<Node> BuildPath()
    {
        List<Node> path = new List<Node>();
        Node current = targetNode;

        if (!reached.ContainsKey(targetCords))
        {
            Debug.LogWarning("No path found!");
            return path;
        }

        while (current != null)
        {
            path.Add(current);
            current.path = true;
            current = current.connectTo;
        }

        path.Reverse();
        return path;
    }

    private void UpdateLineRenderer()
    {
        if (path == null || path.Count == 0)
        {
            ClearPathVisualization();
            return;
        }

        lineRenderer.positionCount = path.Count;
        for (int i = 0; i < path.Count; i++)
        {
            lineRenderer.SetPosition(i, new Vector3(path[i].cords.x, 0.75f, path[i].cords.y));
        }

        destinationMarker.SetActive(true);
        destinationMarker.transform.position = new Vector3(targetCords.x, 0.75f, targetCords.y);
    }

    public void NotifyReceivers()
    {
        BroadcastMessage("RecalculatePath", false, SendMessageOptions.DontRequireReceiver);
    }

    public void SetNewDestination(Vector2Int startCoordinates, Vector2Int targetCoordinates)
    {
        startCords = startCoordinates;
        targetCords = targetCoordinates;
        GetNewPath();
    }

    public void OnUnitReachedDestination()
    {
        ClearPathVisualization();
    }

    private void ClearPathVisualization()
    {
        lineRenderer.positionCount = 0;
        destinationMarker.SetActive(false);
    }
}
