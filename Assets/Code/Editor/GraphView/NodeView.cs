using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace Prefabrikator.Editor
{
    public class NodeView : Node
    {
        private Port _input;
        private Port _outPut;

        private static readonly Vector2 DefaultSize = new Vector2(75, 50);

        public NodeView(string name, Vector2 position)
            : base()
        {
            this.title = name;

            SetPosition(new Rect(position, DefaultSize));
            style.left = position.x;
            style.top = position.y;

            CreateInputs();
            CreateOutputs();
        }

        public override void SetPosition(Rect newPos)
        {
            base.SetPosition(newPos);

        }

        private void CreateInputs()
        {
            _input = InstantiatePort(Orientation.Horizontal, Direction.Input, Port.Capacity.Single, typeof(bool));

            _input.portName = "Input";
            inputContainer.Add(_input);
        }

        private void CreateOutputs()
        {
            _outPut = InstantiatePort(Orientation.Horizontal, Direction.Output, Port.Capacity.Single, typeof(bool));
            _outPut.portName = "Output";
            outputContainer.Add(_outPut);
        }
    }
}
