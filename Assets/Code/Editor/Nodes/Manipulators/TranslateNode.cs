using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace Prefabrikator.Editor
{
    public class TranslateNode : NodeView
    {
        private Vector3Field _vectorField;
        public TranslateNode(Vector2 position) 
            : base("Translate", position)
        {
            SetPosition(new Rect(position, DefaultSize));

            var spacer = new VisualElement();
            spacer.style.height = 10;
            mainContainer.Add(spacer);
            
            _vectorField = new Vector3Field();
            mainContainer.Add(_vectorField);
        }

        protected override void CreateInputs()
        {
            _inputs = new Port[1];
            _inputs[0] = InstantiatePort(Orientation.Horizontal, Direction.Input, Port.Capacity.Single, typeof(Vector3));

            _inputs[0].portName = "Input";
            inputContainer.Add(_inputs[0]);
        }

        protected override void CreateOutputs()
        {
            _outPuts = new Port[1];
            _outPuts[0] = InstantiatePort(Orientation.Horizontal, Direction.Output, Port.Capacity.Multi, typeof(Vector3));
            _outPuts[0].portName = "Output";
            outputContainer.Add(_outPuts[0]);
        }
    }
}
