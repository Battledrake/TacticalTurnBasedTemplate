using UnityEngine;

namespace BattleDrakeCreations.TacticalTurnBasedTemplate
{
    public abstract class ActionBase : MonoBehaviour
    {
        public int actionValue = -1;

        protected PlayerActions _playerActions;

        public virtual void InitializeAction(PlayerActions playerActions) { _playerActions = playerActions; }

        public abstract bool ExecuteAction(GridIndex index);
    }
}
