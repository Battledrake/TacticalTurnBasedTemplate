using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace BattleDrakeCreations.TacticalTurnBasedTemplate
{
    public static class DataManager
    {
        public static GridShapeData GetGridShapeData(GridShape gridShape)
        {
            GridShapeData[] _gridData = Resources.LoadAll<GridShapeData>("Data/Grid");
            return _gridData.FirstOrDefault<GridShapeData>(data => data.gridShape == gridShape);
        }

        public static UnitData GetUnitDataFromId(UnitId unitId)
        {
            UnitData[] unitData = GetAllUnitData();
            return unitData.FirstOrDefault<UnitData>(data => data.unitId == unitId);
        }

        public static UnitData[] GetAllUnitData()
        {
            return Resources.LoadAll<UnitData>("Data/Unit");
        }
    }
}
