using Unity.VisualScripting;
using UnityEngine;

public class Fish : MonoBehaviour
{
    [System.Serializable]
    public struct InstanceData
    {
        public Vector3 position;
    }

    [SerializeField] private ComputeShader computeShader;
    [SerializeField] private Mesh mesh;
    [SerializeField] private Material material;
    [SerializeField] private int instanceCount = 1000;

    private ComputeBuffer instanceBuffer;
    private InstanceData[] instances;
    private int kernelID;

    private void Start()
    {
        InitializeData();
        SetupBuffers();
        SetupMaterial();
    }

    private void Update()
    {
        // Update compute shader with time
        computeShader.SetFloat("deltaTime", Time.time);

        // Execute compute shader
        int threadGroups = Mathf.CeilToInt(instanceCount / 64f);
        computeShader.Dispatch(kernelID, threadGroups, 1, 1);

        // Render all instances
        RenderParams renderParams = new RenderParams(material);
        Graphics.RenderMeshPrimitives(renderParams, mesh, 0, instanceCount);
    }

    private void OnDestroy()
    {
        instanceBuffer?.Release();
    }

    private void InitializeData()
    {
        instances = new InstanceData[instanceCount];

        // Initialize the positions
        for (int i = 0; i < instanceCount; i++)
        {
            instances[i] = new InstanceData
            {
                position = new Vector3(
                    Random.Range(-25f, 25f),
                    Random.Range(-10f, 10f),
                    Random.Range(-25f, 25f)
                )
            };
        }
    }

    private void SetupBuffers()
    {
        // Instance data buffer
        instanceBuffer = new ComputeBuffer(instanceCount, sizeof(float) * 3);
        instanceBuffer.SetData(instances);

        kernelID = computeShader.FindKernel("CSMain");
        computeShader.SetBuffer(kernelID, "instanceBuffer", instanceBuffer);
    }

    private void SetupMaterial()
    {
        // Set the instance buffer on the material
        material.SetBuffer("instanceBuffer", instanceBuffer);
    }

    void OnDrawGizmosSelected()
    {
        // Visualize the bounds in the scene view
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(Vector3.zero, new Vector3(50, 20, 50));
    }
}
