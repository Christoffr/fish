using UnityEngine;

public class FishGPUInstance : MonoBehaviour
{
    [System.Serializable]
    public struct InstanceData
    {
        public Vector3 position;
        public Vector3 direction;
    }

    [SerializeField] private ComputeShader _computeShader;
    [SerializeField] private BoidsSettingsOS _settings;

    private ComputeBuffer instanceBuffer;
    private InstanceData[] instances;
    private int kernelID;
    private int currentInstanceCount = 0;

    private void Start()
    {
        InitializeInstances();
    }

    private void Update()
    {
        if (currentInstanceCount != _settings.InstanceCount)
            InitializeInstances();

        // Update compute shader with parameters
        _computeShader.SetFloat("deltaTime", Time.deltaTime);
        _computeShader.SetInt("instanceCount", _settings.InstanceCount);
        _computeShader.SetVector("bounds", _settings.Bounds);
        _computeShader.SetFloat("fishSpeed", _settings.FishSpeed);
        _computeShader.SetFloat("separationWeight", _settings.Separation);
        _computeShader.SetFloat("alignmentWeight", _settings.Alignment);
        _computeShader.SetFloat("cohesionWeight", _settings.Cohesion);

        // Execute compute shader
        int threadGroups = Mathf.CeilToInt(_settings.InstanceCount / 64f);
        _computeShader.Dispatch(kernelID, threadGroups, 1, 1);

        // Render all instances
        RenderParams renderParams = new RenderParams(_settings.Material);
        Graphics.RenderMeshPrimitives(renderParams, _settings.Mesh, 0, _settings.InstanceCount);

    }

    private void OnDestroy()
    {
        instanceBuffer?.Release();
    }

    private void InitializeInstances()
    {
        // Release the old buffer
        instanceBuffer?.Release();

        // Store current count
        currentInstanceCount = _settings.InstanceCount;

        // Initialize the array
        instances = new InstanceData[currentInstanceCount];

        // Initialize positions and directions
        for (int i = 0; i < currentInstanceCount; i++)
        {
            Vector3 position = Random.onUnitSphere;
            position.x *= _settings.Bounds.x;
            position.y *= _settings.Bounds.y;
            position.z *= _settings.Bounds.z;

            Vector3 direction = Random.onUnitSphere;

            instances[i] = new InstanceData
            {
                position = position,
                direction = direction,
            };
        }

        // Create new buffer with correct size
        instanceBuffer = new ComputeBuffer(currentInstanceCount, sizeof(float) * 6);
        instanceBuffer.SetData(instances);

        // Set buffer on compute shader
        _computeShader.SetBuffer(kernelID, "instanceBuffer", instanceBuffer);

        // Set buffer on material
        _settings.Material.SetBuffer("instanceBuffer", instanceBuffer);
        _settings.Material.SetVector("_Bounds", _settings.Bounds);
    }

    void OnDrawGizmos()
    {
        if (_settings != null)
        {
            // Visualize the bounds in the scene view
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireCube(transform.position, _settings.Bounds);
        }
    }
}
