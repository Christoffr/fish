using UnityEngine;

public class FishGPUInstance : MonoBehaviour
{
    [System.Serializable]
    public struct InstanceData
    {
        public Vector3 position;
        public Vector3 direction;
        public Vector3 target;
    }

    [SerializeField] private ComputeShader computeShader;
    [SerializeField] private Mesh mesh;
    [SerializeField] private Material material;
    [SerializeField] private int instanceCount = 1000;
    [SerializeField] private Vector3 bounds = new Vector3(50f, 25f, 50f);
    [SerializeField] private float fishSpeed = 1f;
    [SerializeField] private Transform target;
    [SerializeField] private float separation = 1.0f;
    [SerializeField] private float alignment = 1.0f;
    [SerializeField] private float cohesion = 1.0f;

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
        computeShader.SetVector("target", target.position);
        computeShader.SetFloat("separationWeight", separation);
        computeShader.SetFloat("alignmentWeight", alignment);
        computeShader.SetFloat("cohesionWeight", cohesion);

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
            Vector3 position = Random.onUnitSphere;
            position.x *= bounds.x;
            position.y *= bounds.y;
            position.z *= bounds.z;

            Vector3 direction = Random.onUnitSphere;

            instances[i] = new InstanceData
            {
                position = position,
                direction = direction,
                target = target.position
            };
        }
    }

    private void SetupBuffers()
    {
        // Instance data buffer
        instanceBuffer = new ComputeBuffer(instanceCount, sizeof(float) * 9);
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

    void OnDrawGizmos()
    {
        // Visualize the bounds in the scene view
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(transform.position, bounds);
    }
}
