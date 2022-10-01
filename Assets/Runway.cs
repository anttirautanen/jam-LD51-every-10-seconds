using UnityEngine;

public class Runway
{
    public readonly Vector3Int Start;
    public readonly Vector3Int End;
    
    public Runway(Vector3Int start, Vector3Int end)
    {
        Start = start;
        End = end;
    }
}