using UnityEngine;

[System.Serializable]
public class FishType
{
    public BoidsSettingsOS settings;
    public ComputeBuffer instanceBuffer;
    public ComputeBuffer cellDataBuffer;
    public ComputeBuffer sortedCellDataBuffer;
    public ComputeBuffer cellStartBuffer;
    public ComputeBuffer cellEndBuffer;
    public FishStruct.CellData[] cellDataArray;
}
