using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace Prefabrikator.Editor
{
    public class OutputNode : NodeView
    {
        public OutputNode(Vector2 position)
            : base("Output", position)
        {
            SetPosition(new Rect(position, DefaultSize));
        }


        protected override void CreateOutputs() { }

        protected override void CreateInputs() 
        {
            _outPuts = new Port[1];
            _outPuts[0] = InstantiatePort(Orientation.Horizontal, Direction.Input, Port.Capacity.Multi, typeof(Object[]));

            _outPuts[0].portName = "Object";
            inputContainer.Add(_outPuts[0]);
        }
    }
}