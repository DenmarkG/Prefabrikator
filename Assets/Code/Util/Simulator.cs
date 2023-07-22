using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class Simulator : MonoBehaviour
{
    [SerializeField] private bool _simulate = false;

    private void Update()
    {
        if (_simulate)
        {
            Simulate();
        }
        else
        {
            Physics.autoSimulation = false;
        }
    }

    public void Simulate()
    {
        Physics.Simulate(Time.fixedDeltaTime);
    }
}
