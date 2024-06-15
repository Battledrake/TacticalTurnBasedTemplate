using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BattleDrakeCreations.BehaviorTree;

public class SpawnPrefab : TaskNode
{
    [SerializeField] private string _stringKey = "SpawnPrefab";
    [SerializeField] private float _objectLifetime = 2f;

    public override string title { get => "Spawn Prefab"; }

    private BlackboardKey _blackboardKey;

    protected override void OnStart() 
    {
        _blackboardKey = _blackboard.GetOrRegisterKey(_stringKey);
    }

    protected override void OnStop() 
    {
    }

    protected override NodeResult OnEvaluate() 
    {
        _result = NodeResult.Running;

        if(_blackboard.TryGetValue(_blackboardKey, out GameObject spawnPrefab))
        {
            GameObject newObject = Instantiate(spawnPrefab, _agent.Unit.transform.position, Quaternion.identity);
            Destroy(newObject, _objectLifetime);
            _result = NodeResult.Succeeded;
        }
        else
        {
            _result = NodeResult.Failed;
        }

        return _result;
    }
}
