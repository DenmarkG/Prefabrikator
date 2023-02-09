using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Prefabrikator;
using System.Threading;

public class PoissonTest : MonoBehaviour
{
    private const int kMaxSamples = 30;

    [SerializeField] private int _targetCount = 10;
    [SerializeField] private Vector3 _center = new Vector3();
    [SerializeField] private Vector3 _size = new Vector3(10f, 0f, 10f);
    [SerializeField] private float _defaultRadius = 2f;

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
        List<Vector3> scatteredPoints = new();

        List<Vector3> activePoints = new(_targetCount);

        // #DG: Make this a user controlled variable
        //Vector3 initialSample = Extensions.GetRandomPointInBounds(new Bounds(_center, _size));
        Vector3 initialSample = new Vector3();

        activePoints.Add(initialSample);

        while (activePoints.Count > 0 && scatteredPoints.Count < _targetCount)
        {
            bool sampleFound = false;
            Vector3[] samplePoints = GenerateSampleSet(initialSample, _defaultRadius, 2f * _defaultRadius);
            foreach (Vector3 sample in samplePoints)
            {
                Vector3 testPosition = sample + initialSample;

                if (IsValidPoint(scatteredPoints, testPosition))
                {
                    activePoints.Add(testPosition);
                    scatteredPoints.Add(testPosition);

                    sampleFound = true;
                    break;
                }
            }

            if (!sampleFound)
            {
                activePoints.Remove(initialSample);
            }
            
            if (activePoints.Count > 0)
            {
                initialSample = activePoints[Random.Range(0, activePoints.Count)];
            }
        }

        Debug.Log($"{scatteredPoints.Count} points created");
        return scatteredPoints;
    }

    private bool IsValidPoint(List<Vector3> scatteredPoints, Vector3 testPoint)
    {
        Bounds testBounds = new Bounds(_center, _size);
        foreach (Vector3 point in scatteredPoints)
        {
            if (!testBounds.Contains(testPoint))
            {
                return false;
            }

            float distance = Vector3.Distance(point, testPoint);
            if (distance < _defaultRadius)
            {
                return false;
            }
        }

        return true;
    }

    private Vector3[] GenerateSampleSet(Vector3 center, float minRadius, float maxRadius)
    {
        Vector3[] samples = new Vector3[kMaxSamples];
        for (int i = 0; i < kMaxSamples; ++i)
        {
            Vector2 random = Random.insideUnitCircle;
            Vector3 direction = new Vector3(random.x, 0f, random.y);
            direction *= Random.Range(minRadius, maxRadius);
            samples[i] = direction;
        }

        return samples;
    }
}
