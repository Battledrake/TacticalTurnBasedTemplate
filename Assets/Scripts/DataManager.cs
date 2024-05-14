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

        public static UnitData GetUnitDataFromType(UnitType unitType)
        {
            UnitData[] unitData = GetAllUnitData();
            return unitData.FirstOrDefault<UnitData>(data => data.unitType == unitType);
        }

        public static UnitData[] GetAllUnitData()
        {
            return Resources.LoadAll<UnitData>("Data/Unit");
        }
    }
}
