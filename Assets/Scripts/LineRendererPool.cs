using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LineRendererPool : MonoBehaviour
{
    public static LineRendererPool Instance;

    [SerializeField] private LineRenderer _lineRendererPrefab;
    [SerializeField] private Transform _instanceContainer;
    [SerializeField] private int _initialPoolCount;

    private Dictionary<int, LineRenderer> _pooledLineRenderers = new Dictionary<int, LineRenderer>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
        }
        else
        {
            Instance = this;
        }
        DontDestroyOnLoad(this.gameObject);

        for (int i = 0; i < _initialPoolCount; i++)
        {
            LineRenderer lineRenderer = Instantiate(_lineRendererPrefab, _instanceContainer);
            lineRenderer.gameObject.SetActive(false);
            _pooledLineRenderers.TryAdd(i, lineRenderer);
        }
    }

    public List<LineRenderer> BorrowInstances(int amount)
    {
        List<LineRenderer> objectsToLend = new List<LineRenderer>();

        int objectCount = 0;
        if (_pooledLineRenderers.Count > amount)
        {
            foreach (KeyValuePair<int, LineRenderer> pooledPair in _pooledLineRenderers)
            {
                if (objectCount == amount)
                    return objectsToLend;
                if (!pooledPair.Value.gameObject.activeInHierarchy)
                {
                    objectCount++;
                    pooledPair.Value.gameObject.SetActive(true);
                    objectsToLend.Add(pooledPair.Value);
                }
            }
        }
        return objectsToLend;
    }

    public void ReturnInstances(List<LineRenderer> returnList)
    {
        if (this == null)
            return;

        for (int i = 0; i < returnList.Count; i++)
        {
            returnList[i].gameObject.SetActive(false);
        }
    }
}
