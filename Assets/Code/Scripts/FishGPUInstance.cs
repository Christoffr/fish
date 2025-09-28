using UnityEngine;

public class FishGPUInstance : MonoBehaviour
{
    [System.Serializable]
    public struct InstanceData
    {
        public Vector3 position;
        public Vector3 direction;
    }

    [SerializeField] private ComputeShader computeShader;
    [SerializeField] private Mesh mesh;
    [SerializeField] private Material material;
    [SerializeField] private int instanceCount = 1000;
    [SerializeField] private Vector3 bounds = new Vector3(50f, 25f, 50f);
    [SerializeField] private float fishSpeed = 1f;

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
        // Update compute shader with parameters
        computeShader.SetFloat("deltaTime", Time.deltaTime);
        computeShader.SetInt("instanceCount", instanceCount);
        computeShader.SetVector("bounds", bounds);
        computeShader.SetFloat("fishSpeed", fishSpeed);

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
            Vector3 position = Random.insideUnitSphere;
            position.x *= bounds.x;
            position.y *= bounds.y;
            position.z *= bounds.z;

            Vector3 direction = Random.insideUnitSphere;

            instances[i] = new InstanceData
            {
                position = position,
                direction = direction
            };
        }
    }

    private void SetupBuffers()
    {
        // Instance data buffer
        instanceBuffer = new ComputeBuffer(instanceCount, sizeof(float) * 6);
        instanceBuffer.SetData(instances);

        kernelID = computeShader.FindKernel("CSMain");
        computeShader.SetBuffer(kernelID, "instanceBuffer", instanceBuffer);
    }

    private void SetupMaterial()
    {
        // Set the instance buffer on the material
        material.SetBuffer("instanceBuffer", instanceBuffer);

        // Pass the bounds to the shader
        material.SetVector("_Bounds", bounds);
    }

    void OnDrawGizmosSelected()
    {
        // Visualize the bounds in the scene view
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(transform.position, bounds);
    }
}
