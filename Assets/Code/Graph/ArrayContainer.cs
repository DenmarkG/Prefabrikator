using UnityEngine;

namespace Prefabrikator
{

    public class ArrayContainer : MonoBehaviour
    {
        private NodeGraph _graph;
        private void Awake()
        {
            _graph = new();
            Debug.Log(_graph.Evaluate());
            
        }
    }
}
