using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(fileName = "BoidsSettingsOS", menuName = "Scriptable Objects/BoidsSettingsOS")]
public class BoidsSettingsOS : ScriptableObject
{
    public Mesh Mesh;
    public Material Material;
    [Range(1,10000)]
    public int InstanceCount;
    public int currentInstanceCount;
    public Vector3 Bounds = new Vector3(50f, 25f, 50f);
    public float FishSpeed = 1f;
    public float SeparationWeight = 1.5f;
    public float AlignmentWeight = 1.0f;
    public float CohesionWeight = 1.0f;
    public int CellSize = 5;

    public int TotalCells()
    {
        int totalCells;

        Vector3Int gridSize = new Vector3Int(
            Mathf.FloorToInt(Bounds.x / CellSize),
            Mathf.FloorToInt(Bounds.y / CellSize),
            Mathf.FloorToInt(Bounds.z / CellSize)
            );

        totalCells = gridSize.x * gridSize.y * gridSize.z;

        return totalCells;
    }
}
