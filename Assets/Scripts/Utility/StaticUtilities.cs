using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace BattleDrakeCreations.TacticalTurnBasedTemplate
{
    public static class StaticUtilities
    {
        public static Transform FindTransform(GameObject parentObject, string transformName)
        {
            return parentObject.GetComponentsInChildren<Transform>().FirstOrDefault(t => t.name == transformName);
        }

        public static int MinMaxRandom(Vector2Int minMax)
        {
            if(minMax.x <= minMax.y)
            {
                return Random.Range(minMax.x, minMax.y + 1);
            }
            else
            {
                return Random.Range(minMax.y, minMax.x + 1);
            }
        }
    }
}