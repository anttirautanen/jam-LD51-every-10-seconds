using System.Collections.Generic;
using UnityEngine;

public static class Utils
{
    public static List<Vector3Int> GetAllTilesInArea(Vector3Int cornerA, Vector3Int cornerB)
    {
        var (areaStart, areaEnd) = SortCorners(cornerA, cornerB);

        var allTilesInArea = new List<Vector3Int>();
        for (var x = areaStart.x; x <= areaEnd.x; ++x)
        {
            for (var y = areaStart.y; y <= areaEnd.y; ++y)
            {
                allTilesInArea.Add(new Vector3Int(x, y));
            }
        }

        return allTilesInArea;
    }

    public static (Vector3Int, Vector3Int) SortCorners(Vector3Int cornerA, Vector3Int cornerB)
    {
        var areaStartX = cornerA.x < cornerB.x ? cornerA.x : cornerB.x;
        var areaEndX = cornerA.x < cornerB.x ? cornerB.x : cornerA.x;
        var areaStartY = cornerA.y < cornerB.y ? cornerA.y : cornerB.y;
        var areaEndY = cornerA.y < cornerB.y ? cornerB.y : cornerA.y;

        return (new Vector3Int(areaStartX, areaStartY), new Vector3Int(areaEndX, areaEndY));
    }
}