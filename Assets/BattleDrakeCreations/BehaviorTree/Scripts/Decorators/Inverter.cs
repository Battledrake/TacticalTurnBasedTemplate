using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BattleDrakeCreations.BehaviorTree
{
    public class Inverter : DecoratorNode
    {
        public override string title { get => "Inverter"; }

        protected override void OnStart()
        {
        }

        protected override void OnStop()
        {
        }

        protected override NodeResult OnEvaluate()
        {
            _result = _child.Evaluate();
            if (_result == NodeResult.Succeeded)
                _result = NodeResult.Failed;
            else if (_result == NodeResult.Failed)
                _result = NodeResult.Succeeded;

            return _result;
        }
    }
}