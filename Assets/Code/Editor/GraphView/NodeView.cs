using Codice.CM.Client.Differences.Graphic;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace Prefabrikator.Editor
{
    public abstract class NodeView : Node
    {
        protected Port[] _inputs;
        protected Port[] _outPuts;

        protected virtual Vector2 DefaultSize { get; } = new Vector2(75, 50);
        public string Name { get; }

        public NodeView(string name, Vector2 position)
            : base()
        {
            Name = name;
            this.title = Name;

            CreateInputs();
            CreateOutputs();
        }

        public override sealed void SetPosition(Rect newPos)
        {
            style.position = Position.Relative;
            style.left = newPos.x;
            style.top = newPos.y;
            style.width = newPos.width;
            style.height = newPos.height;
        }

        protected virtual void CreateInputs()
        {
            _inputs = new Port[1];
            _inputs[0] = InstantiatePort(Orientation.Horizontal, Direction.Input, Port.Capacity.Single, typeof(bool));

            _inputs[0].portName = "Input";
            inputContainer.Add(_inputs[0]);
        }

        protected virtual void CreateOutputs()
        {
            _outPuts = new Port[1];
            _outPuts[0] = InstantiatePort(Orientation.Horizontal, Direction.Output, Port.Capacity.Single, typeof(bool));
            _outPuts[0].portName = "Output";
            outputContainer.Add(_outPuts[0]);
        }
    }
}
