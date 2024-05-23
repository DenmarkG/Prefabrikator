using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Prefabrikator.Editor
{
    public class ObjectPickerNode : NodeView
    {
        private ObjectField _objectField;

        public ObjectPickerNode(Vector2 position)
            : base("Object Picker", position)
        {
            SetPosition(new Rect(position, DefaultSize));

            _objectField = new ObjectField();
            mainContainer.Add(_objectField);
        }


        protected override void CreateInputs() { }
        protected override void CreateOutputs()
        {
            _outPuts = new Port[1];
            _outPuts[0] = InstantiatePort(Orientation.Horizontal, Direction.Output, Port.Capacity.Multi, typeof(Vector3));
            
            _outPuts[0].portName = "Object";
            outputContainer.Add(_outPuts[0]);
        }
    }
}