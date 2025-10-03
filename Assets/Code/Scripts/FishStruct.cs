using UnityEngine;

public static class FishStruct
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
}
