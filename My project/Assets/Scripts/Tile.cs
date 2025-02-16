using UnityEngine;

public class Tile : MonoBehaviour
{
    public Vector2Int cords;

    GridManager gridManager;

    private void Start()
    {
        gridManager = FindObjectOfType<GridManager>();
        int x = (int)transform.position.x;
        int z = (int)transform.position.z;

        cords = new Vector2Int(x / gridManager.UnityGridSize, z / gridManager.UnityGridSize);
    }
}
