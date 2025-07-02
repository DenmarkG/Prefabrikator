using Prefabrikator.Runtime;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Graphs;
using UnityEngine;

public class ArrayAnimator : MonoBehaviour
{
    [SerializeField] private float _incrementAmount = 2f;
    [SerializeField] private Vector3 direction;
    [SerializeField] private Line _lineArray;


    void Update()
    {
        float step = Time.deltaTime * _incrementAmount;
        Vector3 offset = _lineArray.Offset + (direction * step);
        _lineArray.Offset = offset;
    }
}
