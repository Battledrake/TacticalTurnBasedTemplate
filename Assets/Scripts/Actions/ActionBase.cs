using UnityEngine;

namespace BattleDrakeCreations.TacticalTurnBasedTemplate
{
    public abstract class ActionBase : MonoBehaviour
    {
        public int actionValue;

        protected PlayerActions _playerActions;

        public void InitializeAction(PlayerActions playerActions) { _playerActions = playerActions; }

        public abstract bool ExecuteAction(GridIndex index);
    }
}
