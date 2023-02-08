using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Prefabrikator;

public class PoissonTest : MonoBehaviour
{
    private const int kMaxSamples = 30;
    private float _defaultRadius = 2f;

    [SerializeField] private int _targetCount = 10;
    [SerializeField] private Vector3 _center = new Vector3();
    [SerializeField] private Vector3 _size = new Vector3(10f, 0f, 10f);

    [SerializeField] private GameObject _prefab = null;

    private List<GameObject> _clones = null;

    private void Start()
    {
        _clones = new List<GameObject>(_targetCount);

        List<Vector3> positions = ScatterPoisson();
        foreach (Vector3 pos in positions)
        {
            _clones.Add(GameObject.Instantiate(_prefab, pos, Quaternion.identity));
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            foreach (GameObject go in _clones)
            {
                GameObject.Destroy(go);
            }

            _clones.Clear();

            List<Vector3> positions = ScatterPoisson();
            foreach (Vector3 pos in positions)
            {
                _clones.Add(GameObject.Instantiate(_prefab, pos, Quaternion.identity));
            }
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawCube(_center, _size);
    }

    private List<Vector3> ScatterPoisson()
    {
        float cellSize = _defaultRadius / Mathf.Sqrt(_targetCount);

        int[,] grid = new int[Mathf.CeilToInt(_size.x / cellSize), Mathf.CeilToInt(_size.y / cellSize)]; // #DG: Need to account for Region Size
        for (int x = 0; x < grid.GetLength(0); ++x)
        {
            for (int y = 0; y < grid.GetLength(1); ++y)
            {
                grid[x, y] = -1;
            }
        }

        List<Vector3> activePoints = new(_targetCount);

        Vector3 initialSample = Extensions.GetRandomPointInBounds(new Bounds(_center, _size));

        activePoints.Add(initialSample);

        for (int i = 1; i < _targetCount; ++i)
        {
            Vector3[] samplePoints = GenerateSampleSet(initialSample, _defaultRadius, 2f * _defaultRadius);
            foreach (Vector3 sample in samplePoints)
            {
                // #DG: Check if point is valid
                // optimize later
                bool isValid = true;
                Vector3 testPosition = sample + initialSample;
                foreach (Vector3 point in activePoints)
                {
                    //Debug.Log($"testing sample {sample}");
                    float distance = Vector3.Distance(point, testPosition);
                    if (distance < _defaultRadius)
                    {
                        isValid = false;
                        //Debug.Log($"sample {testPosition} is {distance} away from active point {point}");
                        break;
                    }
                }

                if (isValid)
                {
                    activePoints.Add(testPosition);
                    Debug.Log($"Adding {testPosition}");
                    break;
                }
            }

            initialSample = activePoints[Random.Range(0, activePoints.Count - 1)];
        }

        return activePoints;
    }

    private Vector3[] GenerateSampleSet(Vector3 center, float minRadius, float maxRadius)
    {
        Vector3[] samples = new Vector3[kMaxSamples];
        for (int i = 0; i < kMaxSamples; ++i)
        {
            Vector3 direction = Random.insideUnitSphere;
            direction *= Random.Range(minRadius, maxRadius);
            samples[i] = direction;
        }

        return samples;
    }
}
