using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

[ExecuteAlways]
public class Labeller : MonoBehaviour
{
    TextMeshPro label;
    public Vector2Int cords = new Vector2Int();
    GridManager gridManager;

    [SerializeField] Color defaultColor = Color.white;
    [SerializeField] Color blockedColor = Color.red;
    [SerializeField] Color exploredColor = Color.yellow;
    [SerializeField] Color pathColor = new Color(1f, 0.5f, 0f);

    private void Awake()
    {
        gridManager = FindObjectOfType<GridManager>();
        label = GetComponentInChildren<TextMeshPro>();
        label.enabled = false;
        DisplayCords();
        CheckAboveTile();
    }

    private void Update()
    {
        if (!Application.isPlaying)
        {
            label.enabled = true;
        }

        DisplayCords();
        transform.name = cords.ToString();

        ToggleLabels();
        SetLabelColor();
    }

    void SetLabelColor()
    {
        if (gridManager == null) { return; }

        Node node = gridManager.GetNode(cords);

        if (node == null) { return; }

        if (!node.walkable)
        {
            label.color = blockedColor;
        }
        else if (node.path)
        {
            label.color = pathColor;
        }
        else if (node.explored)
        {
            label.color = exploredColor;
        }
        else
        {
            label.color = defaultColor;
        }
    }

    private void DisplayCords()
    {
        if (!gridManager) { return; }
        cords.x = Mathf.RoundToInt(transform.position.x / gridManager.UnityGridSize);
        cords.y = Mathf.RoundToInt(transform.position.z / gridManager.UnityGridSize);
        label.text = $"{cords.x}, {cords.y}";
    }

    void ToggleLabels()
    {
        if (Input.GetKeyDown(KeyCode.C))
        {
            label.enabled = !label.IsActive();
        }
    }

    void CheckAboveTile()
    {
        // Start from slightly above the tile's surface
        Vector3 startPosition = transform.position + new Vector3(0f, 0.1f, 0f);
        float checkDistance = 2.0f; // Adjust this value if needed
        RaycastHit hit;

        // Visualize the raycast
        Debug.DrawRay(startPosition, Vector3.up * checkDistance, Color.green, 5.0f);

        // Cast a ray upwards
        if (Physics.Raycast(startPosition, Vector3.up, out hit, checkDistance))
        {
            Debug.Log($"Raycast hit: {hit.transform.name} with tag {hit.transform.tag}");

            if (hit.transform.CompareTag("wall"))
            {
                gridManager.BlockNode(cords);

                // Update the node and its visual representation
                Node node = gridManager.GetNode(cords);
                if (node != null)
                {
                    node.walkable = false;
                }
                // Change tile appearance if desired
                GetComponent<Renderer>().material.color = Color.red;
            }
        }
        else
        {
            Debug.Log("No wall detected above this tile.");
        }
    }
}
