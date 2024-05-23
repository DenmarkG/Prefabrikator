using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace Prefabrikator.Editor
{
    public class SetPositionNode : NodeView
    {
        public SetPositionNode(Vector2 position)
            : base("Set Position", position)
        {
            SetPosition(new Rect(position, DefaultSize));
        }


        protected override void CreateInputs() 
        {
            _inputs = new Port[2];
            _inputs[0] = InstantiatePort(Orientation.Horizontal, Direction.Input, Port.Capacity.Single, typeof(Object));

            _inputs[0].portName = "Object";
            inputContainer.Add(_inputs[0]);

            _inputs[1] = InstantiatePort(Orientation.Horizontal, Direction.Input, Port.Capacity.Single, typeof(Vector3));

            _inputs[1].portName = "Position";
            inputContainer.Add(_inputs[1]);
        }

        protected override void CreateOutputs()
        {
            _outPuts = new Port[1];
            _outPuts[0] = InstantiatePort(Orientation.Horizontal, Direction.Output, Port.Capacity.Multi, typeof(Object));

            _outPuts[0].portName = "Object";
            outputContainer.Add(_outPuts[0]);
        }
    }
}