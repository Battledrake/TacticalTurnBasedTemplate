using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridTester : MonoBehaviour
{
    [SerializeField] private Grid _grid;
    [SerializeField] private GameObject _spawnPrefab;

    private void Start()
    {
        for(int x = 0; x < 10; x++)
        {
            for(int y = 0; y < 10; y++)
            {
                GameObject spawnedObject = Instantiate(_spawnPrefab);
                spawnedObject.transform.position = _grid.GetCellCenterWorld(new Vector3Int(x, y));
            }
        }
    }
}
