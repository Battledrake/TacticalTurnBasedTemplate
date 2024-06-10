using System.Collections;
using UnityEngine;

namespace BattleDrakeCreations.TacticalTurnBasedTemplate
{
    public class UnitAI : MonoBehaviour
    {
        private Unit _unit;

        private void Start()
        {
            _unit = this.GetComponentInParent<Unit>();
        }
        public void RunAILogic()
        {
            IEnumerator Pause()
            {
                yield return new WaitForSeconds(1f);
                _unit.AIEndTurn();
            }
            StartCoroutine(Pause());
        }
    }
}