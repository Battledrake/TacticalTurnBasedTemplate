using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BattleDrakeCreations.TTBTk
{
    public class Player : MonoBehaviour
    {
        [SerializeField] private GameObject _textObjectToSpam;

        void Start()
        {
            for(int i = 0; i < 50; i++)
            {
                for(int j = 0; j < 50; j++)
                {
                    Vector3 spawnPosition = new Vector3(j * 2, 4, i * 2);
                    Quaternion spawnRotation = Quaternion.Euler(90f, 0f, 0f);
                    Instantiate(_textObjectToSpam, spawnPosition, spawnRotation, this.transform);
                }
            }
        }

        // Update is called once per frame
        void Update()
        {

        }
    }
}
