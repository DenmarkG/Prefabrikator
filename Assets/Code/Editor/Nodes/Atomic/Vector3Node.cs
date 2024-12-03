using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace Prefabrikator.Editor
{
    public class Vector3Node : NodeView
    {
        private Vector3Field _vectorField = null;
        [SerializeField] private Vector3 _vector;
        
        public Vector3Node(Vector2 position) 
            : base("Vector3", position)
        {
            SetPosition(new Rect(position, DefaultSize));
            _vectorField = new Vector3Field();
            
            RefreshExpandedState();
        }

        protected override void CreateInputs()
        {
            //_inputs = new Port[1];
            //_inputs[0] = InstantiatePort(Orientation.Horizontal, Direction.Input, Port.Capacity.Single, typeof(Vector3));

            //_inputs[0].portName = "Input";
            //inputContainer.Add(_inputs[0]);
        }

        protected override void CreateOutputs()
        {
            _outPuts = new Port[1];
            _outPuts[0] = InstantiatePort(Orientation.Horizontal, Direction.Output, Port.Capacity.Multi, typeof(Vector3));
            _outPuts[0].portName = "Output";
            outputContainer.Add(_outPuts[0]);
            outputContainer.contentContainer.Add(_vectorField);

            RefreshExpandedState();
            RefreshPorts();
        }
    }
}
