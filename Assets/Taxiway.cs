using UnityEngine;

public class Taxiway
{
    public readonly Vector3Int Start;
    public readonly Vector3Int End;

    public Taxiway(Vector3Int start, Vector3Int end)
    {
        Start = start;
        End = end;
    }
}