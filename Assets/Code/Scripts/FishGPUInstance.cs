using UnityEngine;

public class FishGPUInstance : MonoBehaviour
{
    [System.Serializable]
    public struct InstanceData
    {
        public Vector3 position;
        public Vector3 direction;
    }

    [System.Serializable]
    public struct CellData
    {
        public uint fishIndex;
        public uint cellIndex;
    }

    [SerializeField] private ComputeShader _computeShader;
    [SerializeField] private BoidsSettingsOS _settings;

    private ComputeBuffer instanceBuffer;
    private ComputeBuffer cellDataBuffer;
    private ComputeBuffer sortedCellDataBuffer;
    private ComputeBuffer cellStartBuffer;
    private ComputeBuffer cellEndBuffer;

    private InstanceData[] instances;
    private CellData[] cellDataArray;

    private int csMainKernel;
    private int assignCellsKernel;
    private int buildCellRangesKernel;

    private int currentInstanceCount = 0;
    private int totalCells = 0;

    private void Start()
    {
        // Find all kernels
        csMainKernel = _computeShader.FindKernel("CSMain");
        assignCellsKernel = _computeShader.FindKernel("AssignCells");
        buildCellRangesKernel = _computeShader.FindKernel("BuildCellRanges");

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
        _computeShader.SetFloat("cellSize", _settings.CellSize);

        int threadGroups = Mathf.CeilToInt(_settings.InstanceCount / 64f);

        // Step 1: Assign each fish to a cell
        _computeShader.Dispatch(assignCellsKernel, threadGroups, 1, 1);

        // Step 2: Sort cell data (CPU for now)
        SortCellData();

        // Step 3: Build cell start/end indices
        // Clear buffers first
        ClearCellBuffers();
        _computeShader.Dispatch(buildCellRangesKernel, threadGroups, 1, 1);

        // Step 4: Execute main boids update
        _computeShader.Dispatch(csMainKernel, threadGroups, 1, 1);

        // Render all instances
        RenderParams renderParams = new RenderParams(_settings.Material);
        Graphics.RenderMeshPrimitives(renderParams, _settings.Mesh, 0, _settings.InstanceCount);
    }

    private void SortCellData()
    {
        // Copy from GPU to CPU
        cellDataBuffer.GetData(cellDataArray);

        // Sort by cellIndex
        System.Array.Sort(cellDataArray, (a, b) => a.cellIndex.CompareTo(b.cellIndex));

        // Copy back to GPU
        sortedCellDataBuffer.SetData(cellDataArray);
    }

    private void ClearCellBuffers()
    {
        int[] clearData = new int[totalCells];
        for (int i = 0; i < totalCells; i++)
            clearData[i] = -1;

        cellStartBuffer.SetData(clearData);
        cellEndBuffer.SetData(clearData);
    }

    private void OnDestroy()
    {
        instanceBuffer?.Release();
        cellDataBuffer?.Release();
        sortedCellDataBuffer?.Release();
        cellStartBuffer?.Release();
        cellEndBuffer?.Release();
    }

    private void InitializeInstances()
    {
        // Release old buffers
        instanceBuffer?.Release();
        cellDataBuffer?.Release();
        sortedCellDataBuffer?.Release();
        cellStartBuffer?.Release();
        cellEndBuffer?.Release();

        // Store current count
        currentInstanceCount = _settings.InstanceCount;

        // Calculate grid dimensions
        Vector3Int gridRes = new Vector3Int(
            Mathf.FloorToInt(_settings.Bounds.x / _settings.CellSize),
            Mathf.FloorToInt(_settings.Bounds.y / _settings.CellSize),
            Mathf.FloorToInt(_settings.Bounds.z / _settings.CellSize)
        );
        totalCells = gridRes.x * gridRes.y * gridRes.z;

        // Initialize instance data
        instances = new InstanceData[currentInstanceCount];
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

        // Create buffers
        instanceBuffer = new ComputeBuffer(currentInstanceCount, sizeof(float) * 6);
        instanceBuffer.SetData(instances);

        cellDataBuffer = new ComputeBuffer(currentInstanceCount, sizeof(uint) * 2);
        sortedCellDataBuffer = new ComputeBuffer(currentInstanceCount, sizeof(uint) * 2);
        cellStartBuffer = new ComputeBuffer(totalCells, sizeof(int));
        cellEndBuffer = new ComputeBuffer(totalCells, sizeof(int));

        // Array for sorting
        cellDataArray = new CellData[currentInstanceCount];

        // Set buffers on CSMain kernel
        _computeShader.SetBuffer(csMainKernel, "instanceBuffer", instanceBuffer);
        _computeShader.SetBuffer(csMainKernel, "sortedCellDataBuffer", sortedCellDataBuffer);
        _computeShader.SetBuffer(csMainKernel, "cellStartBuffer", cellStartBuffer);
        _computeShader.SetBuffer(csMainKernel, "cellEndBuffer", cellEndBuffer);

        // Set buffers on AssignCells kernel
        _computeShader.SetBuffer(assignCellsKernel, "instanceBuffer", instanceBuffer);
        _computeShader.SetBuffer(assignCellsKernel, "cellDataBuffer", cellDataBuffer);

        // Set buffers on BuildCellRanges kernel
        _computeShader.SetBuffer(buildCellRangesKernel, "sortedCellDataBuffer", sortedCellDataBuffer);
        _computeShader.SetBuffer(buildCellRangesKernel, "cellStartBuffer", cellStartBuffer);
        _computeShader.SetBuffer(buildCellRangesKernel, "cellEndBuffer", cellEndBuffer);

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