using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace BattleDrakeCreations.TTBTk
{
    public static class DataManager
    {
        public static GridShapeData GetShapeData(GridShape gridShape)
        {
            GridShapeData[] _gridData = Resources.LoadAll<GridShapeData>("Data/Grid");
            return _gridData.FirstOrDefault<GridShapeData>(data => data.gridShape == gridShape);
        }
    }
}
