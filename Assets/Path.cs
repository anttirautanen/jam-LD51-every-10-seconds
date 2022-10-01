using System.Collections.Generic;
using UnityEngine;

public class Path
{
    public List<Vector3Int> TilePositions;
    
    public Path(List<Vector3Int> tilePositions)
    {
        TilePositions = tilePositions;
    }
}