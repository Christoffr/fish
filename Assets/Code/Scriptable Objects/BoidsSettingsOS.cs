using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(fileName = "BoidsSettingsOS", menuName = "Scriptable Objects/BoidsSettingsOS")]
public class BoidsSettingsOS : ScriptableObject
{
    public Mesh Mesh;
    public Material Material;
    [Range(100, 10000)]
    public int InstanceCount;
    public Vector3 Bounds = new Vector3(50f, 25f, 50f);
    public float FishSpeed = 1f;
    public float Separation = 1.0f;
    public float Alignment = 1.0f;
    public float Cohesion = 1.0f;
    public int CellSize = 5;
}
