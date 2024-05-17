using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BattleDrakeCreations.TacticalTurnBasedTemplate
{
    public static class AbilityStatics
    {
        public static bool HasLineOfSight(TileData origin, TileData target, float height, float distance)
        {
            Vector3 startPosition = origin.tileMatrix.GetPosition();
            startPosition.y += height;
            Vector3 targetPosition = target.tileMatrix.GetPosition();
            targetPosition.y += height;
            Vector3 targetDirection = targetPosition - startPosition;

            if(Physics.Raycast(startPosition, targetDirection, out RaycastHit hitInfo, distance))
            {
                return true;
            }
            return false;
        }
    }
}