using System.Collections.Generic;
using UnityEngine;

public class Utils
{
    public static List<Vector3Int> GetAllTilesInArea(Vector3Int cornerA, Vector3Int cornerB)
    {
        var areaStartX = cornerA.x < cornerB.x ? cornerA.x : cornerB.x;
        var areaEndX = cornerA.x < cornerB.x ? cornerB.x : cornerA.x;
        var areaStartY = cornerA.y < cornerB.y ? cornerA.y : cornerB.y;
        var areaEndY = cornerA.y < cornerB.y ? cornerB.y : cornerA.y;

        var allTilesInArea = new List<Vector3Int>();
        for (var x = areaStartX; x <= areaEndX; ++x)
        {
            for (var y = areaStartY; y <= areaEndY; ++y)
            {
                allTilesInArea.Add(new Vector3Int(x, y));
            }
        }

        return allTilesInArea;
    }
}