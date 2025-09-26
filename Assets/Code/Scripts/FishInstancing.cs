
using UnityEngine;

public class FishInstancing : MonoBehaviour
{
    [Header("Rendering")]
    [SerializeField] private Material material;
    [SerializeField] private Mesh mesh;
    [SerializeField] private int instancies;

    [Header("Bounds")]
    [SerializeField] private Vector3 bounds;

    private Vector3[] positions;

    private void Update()
    {
        RenderParams renderParams = new RenderParams(material);
        renderParams.worldBounds = new Bounds(Vector3.zero, bounds);
        renderParams.matProps = new MaterialPropertyBlock();

        renderParams.matProps.SetVector("_Bounds", bounds);

        Graphics.RenderMeshPrimitives(renderParams, mesh, 0, instancies);
    }

    void OnDrawGizmosSelected()
    {
        // Visualize the bounds in the scene view
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(Vector3.zero, bounds);
    }
}
