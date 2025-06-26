using UnityEngine;
using System.Collections.Generic;

namespace Prefabrikator
{
    public interface INode<Tout>
    {
        public Tout Evaluate();
    }

    public abstract class Node
    {
        public abstract NodePort[] InputContainer { get; }
        public abstract NodePort[] OutputContainer { get; }
    }

    public class NodeConnection
    {
        public Node OutNode;
        public Node InNode;
    }

    public class NodePort
    {
        public enum PortDirection
        {
            Input,
            Output,
        }

        public NodeConnection Input => _input;
        [SerializeField] private NodeConnection _input;
        public NodeConnection[] Outputs => _outputs;
        [SerializeField] private NodeConnection[] _outputs;

        public PortDirection Direction { get; }

        private NodePort(PortDirection direction, int outputSize = 0)
        {
            Direction = direction;

            _input = direction == PortDirection.Input ? new NodeConnection() : null;
            _outputs = direction == PortDirection.Output ? new NodeConnection[outputSize] : null;
        }

        public static NodePort CreateInputPort() => new NodePort(PortDirection.Input);
        public static NodePort CreateOutputPort(int size) => new NodePort(PortDirection.Output, size);
    }

    public class FloatNode : Node, INode<float>
    {
        public override NodePort[] InputContainer => null;
        public override NodePort[] OutputContainer => _outputContainer;
        [SerializeField] private NodePort[] _outputContainer = new NodePort[] { NodePort.CreateOutputPort(1) };
        
        [SerializeField] private float _data;

        public float Evaluate() => _data;

        public void Set(float f) => _data = f;
    }

    public class SumNode : Node, INode<float>
    {
        public override NodePort[] InputContainer => throw new System.NotImplementedException();
        private NodePort[] _inputs = new NodePort[]
            {
                NodePort.CreateInputPort(),
                NodePort.CreateInputPort(),
            };

        public override NodePort[] OutputContainer => _outputs; 
        [SerializeField] private NodePort[] _outputs;

        // #TEMP
        public List<Node> Input = new(2);
        public Node Output;

        public float Evaluate()
        {
            float sum = 0;
            foreach (Node node in Input)
            {
                sum += ((INode<float>)node).Evaluate();
            }

            return sum;
        }
    }

    public class NodeGraph // Make a variety of graphs with different output types
    {
        private FloatNode _inputA = new();
        private FloatNode _inputB = new();
        private SumNode _sum = new();

        public NodeGraph()
        {
            _inputA.Set(100f);
            _inputB.Set(200f);
            _sum.Input.Add(_inputA);
            _sum.Input.Add(_inputB);
        }

        public float Evaluate()
        {
            return _sum.Evaluate();
        }
    }
}