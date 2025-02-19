using UnityEngine;

public class Tile : MonoBehaviour
{
    public Vector2Int cords;
    public bool walkable = true;

    GridManager gridManager;

    private void Start()
    {
        gridManager = FindObjectOfType<GridManager>();
        int x = (int)transform.position.x;
        int z = (int)transform.position.z;

        cords = new Vector2Int(x / gridManager.UnityGridSize, z / gridManager.UnityGridSize);

        // Optionally, register this tile with the grid manager
        gridManager.RegisterTile(this);
    }

    public void SetWalkable(bool isWalkable)
    {
        walkable = isWalkable;
        // Change appearance based on walkability
        GetComponent<Renderer>().material.color = isWalkable ? Color.white : Color.black;

        // Update the corresponding node in the grid manager
        gridManager.UpdateNodeWalkability(cords, isWalkable);
    }
}
