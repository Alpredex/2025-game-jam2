using Unity.VisualScripting;
using UnityEditor.Rendering.HighDefinition;
using UnityEngine;

public class UnitController : MonoBehaviour
{
    [SerializeField] float movementSpeed = 1f;
    Transform selectedUnit;
    bool unitSelected = false;

    GridManager gridManager;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        gridManager = FindAnyObjectByType<GridManager>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            bool hasHit = Physics.Raycast(ray, out hit);

            if (hasHit)
            {
                Debug.Log($"has hit {hit.transform.gameObject.name}");
                if (hit.transform.tag == "Tile") {
                    if (unitSelected) { 
                        Vector2Int targetCords = hit.transform.GetComponent<Labeller>().cords;
                        Vector2Int startCords = new Vector2Int((int)selectedUnit.position.x, (int)selectedUnit.position.y) / gridManager.UnityGridSize;
                        selectedUnit.transform.position = new Vector3(targetCords.x, selectedUnit.position.y, targetCords.y);
                    }
                }
                if (hit.transform.tag == "Unit") { selectedUnit = hit.transform; unitSelected = true; }
            }
        }
    }
}
