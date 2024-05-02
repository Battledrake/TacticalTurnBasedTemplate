using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BattleDrakeCreations.TTBTk
{
    public class GridTester : MonoBehaviour
    {
        [SerializeField] private Grid _grid;
        [SerializeField] private GameObject _spawnPrefab;

        private void Start()
        {
            for (int y = 0; y < 10; y++)
            {
                for (int x = 0; x < 10; x++)
                {
                    GameObject spawnedObject = Instantiate(_spawnPrefab);
                    spawnedObject.transform.position = _grid.GetCellCenterWorld(new Vector3Int(x, y, 0));
                }
            }
        }
    }
}
