using System.Collections.Generic;
using UnityEngine;

public class MultiFishManager : MonoBehaviour
{
    [SerializeField] private ComputeShader _computeShader;
    [SerializeField] private List<FishType> fishTypes = new List<FishType>();

    private int csMainKernel;
    private int assignCellsKernel;
    private int buildCellRangesKernel;

    private void Start()
    {
        csMainKernel = _computeShader.FindKernel("CSMain");
        assignCellsKernel = _computeShader.FindKernel("AssignCells");
        buildCellRangesKernel = _computeShader.FindKernel("BuildCellRanges");

        foreach (var fishType in fishTypes)
        {
            InitializeFishType(fishType);
        }
    }

    private void Update()
    {
        // Update each fish type independently
        foreach (var fishType in fishTypes)
        {
            // Check if instance count changed and reinitialize if needed
            if (fishType.settings.InstanceCount != fishType.settings.currentInstanceCount)
            {
                ReleaseFishTypeBuffers(fishType);
                InitializeFishType(fishType);
            }

            UpdateFishType(fishType);
            RenderFishType(fishType);
        }
    }

    private void InitializeFishType(FishType fishType)
    {
        // Store current instance count
        fishType.settings.currentInstanceCount = fishType.settings.InstanceCount;

        fishType.instanceBuffer = new ComputeBuffer(fishType.settings.InstanceCount, sizeof(float) * 6);
        fishType.cellDataBuffer = new ComputeBuffer(fishType.settings.InstanceCount, sizeof(uint) * 2);
        fishType.sortedCellDataBuffer = new ComputeBuffer(fishType.settings.InstanceCount, sizeof(uint) * 2);
        fishType.cellStartBuffer = new ComputeBuffer(fishType.settings.TotalCells(), sizeof(uint));
        fishType.cellEndBuffer = new ComputeBuffer(fishType.settings.TotalCells(), sizeof(uint));
        fishType.cellDataArray = new FishStruct.CellData[fishType.settings.InstanceCount];

        // Initialize instance data
        FishStruct.InstanceData[] instances = new FishStruct.InstanceData[fishType.settings.InstanceCount];
        for (int i = 0; i < fishType.settings.InstanceCount; i++)
        {
            Vector3 position = Random.onUnitSphere;
            position.x *= fishType.settings.Bounds.x * 0.5f;
            position.y *= fishType.settings.Bounds.y * 0.5f;
            position.z *= fishType.settings.Bounds.z * 0.5f;

            instances[i] = new FishStruct.InstanceData
            {
                position = position,
                direction = Random.onUnitSphere * fishType.settings.FishSpeed
            };
        }
        fishType.instanceBuffer.SetData(instances);

        // Set buffer on material
        fishType.settings.Material.SetBuffer("instanceBuffer", fishType.instanceBuffer);
        fishType.settings.Material.SetVector("_Bounds", fishType.settings.Bounds);
    }

    private void ReleaseFishTypeBuffers(FishType fishType)
    {
        fishType.instanceBuffer?.Release();
        fishType.cellDataBuffer?.Release();
        fishType.sortedCellDataBuffer?.Release();
        fishType.cellStartBuffer?.Release();
        fishType.cellEndBuffer?.Release();
    }

    private void UpdateFishType(FishType fishType)
    {
        // Set global parameters
        _computeShader.SetFloat("deltaTime", Time.deltaTime);
        _computeShader.SetVector("bounds", fishType.settings.Bounds);
        _computeShader.SetFloat("cellSize", fishType.settings.CellSize);
        _computeShader.SetInt("instanceCount", fishType.settings.InstanceCount);

        // Set fish-specific parameters
        _computeShader.SetFloat("fishSpeed", fishType.settings.FishSpeed);
        _computeShader.SetFloat("separationWeight", fishType.settings.SeparationWeight);
        _computeShader.SetFloat("alignmentWeight", fishType.settings.AlignmentWeight);
        _computeShader.SetFloat("cohesionWeight", fishType.settings.CohesionWeight);

        int threadGroups = Mathf.CeilToInt(fishType.settings.InstanceCount / 64f);

        // Step 1: Assign cells
        _computeShader.SetBuffer(assignCellsKernel, "instanceBuffer", fishType.instanceBuffer);
        _computeShader.SetBuffer(assignCellsKernel, "cellDataBuffer", fishType.cellDataBuffer);
        _computeShader.Dispatch(assignCellsKernel, threadGroups, 1, 1);

        // Step 2: Sort
        SortCellData(fishType);

        // Step 3: Build cell ranges
        ClearCellBuffers(fishType);
        _computeShader.SetBuffer(buildCellRangesKernel, "sortedCellDataBuffer", fishType.sortedCellDataBuffer);
        _computeShader.SetBuffer(buildCellRangesKernel, "cellStartBuffer", fishType.cellStartBuffer);
        _computeShader.SetBuffer(buildCellRangesKernel, "cellEndBuffer", fishType.cellEndBuffer);
        _computeShader.Dispatch(buildCellRangesKernel, threadGroups, 1, 1);

        // Step 4: Update boids
        _computeShader.SetBuffer(csMainKernel, "instanceBuffer", fishType.instanceBuffer);
        _computeShader.SetBuffer(csMainKernel, "sortedCellDataBuffer", fishType.sortedCellDataBuffer);
        _computeShader.SetBuffer(csMainKernel, "cellStartBuffer", fishType.cellStartBuffer);
        _computeShader.SetBuffer(csMainKernel, "cellEndBuffer", fishType.cellEndBuffer);
        _computeShader.Dispatch(csMainKernel, threadGroups, 1, 1);
    }

    private void SortCellData(FishType fishType)
    {
        fishType.cellDataBuffer.GetData(fishType.cellDataArray);
        System.Array.Sort(fishType.cellDataArray, (a, b) => a.cellIndex.CompareTo(b.cellIndex));
        fishType.sortedCellDataBuffer.SetData(fishType.cellDataArray);
    }

    private void ClearCellBuffers(FishType fishType)
    {
        int[] clearData = new int[fishType.settings.TotalCells()];
        for (int i = 0; i < fishType.settings.TotalCells(); i++)
            clearData[i] = -1;

        fishType.cellStartBuffer.SetData(clearData);
        fishType.cellEndBuffer.SetData(clearData);
    }

    private void RenderFishType(FishType fishType)
    {
        RenderParams renderParams = new RenderParams(fishType.settings.Material);
        Graphics.RenderMeshPrimitives(renderParams, fishType.settings.Mesh, 0, fishType.settings.InstanceCount);
    }

    private void OnDestroy()
    {
        foreach (var fishType in fishTypes)
        {
            ReleaseFishTypeBuffers(fishType);
        }
    }

    void OnDrawGizmos()
    {
        if (fishTypes == null || fishTypes.Count == 0)
            return;

        foreach (var fishType in fishTypes)
        {
            // Check if fishType and its settings exist
            if (fishType == null || fishType.settings == null)
                continue;

            Gizmos.color = Color.yellow;
            Gizmos.DrawWireCube(transform.position, fishType.settings.Bounds);
        }
    }
}