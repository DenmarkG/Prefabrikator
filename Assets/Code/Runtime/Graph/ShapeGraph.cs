using System;
using System.Collections.Generic;

namespace Prefabrikator
{

    public class Node
    {
        public List<Node> Inputs { get; private set; } = new();
        public HashSet<Type> Outputs { get; private set; }

        public void AddInput(Node input)
        {
            if (input != null)
            {
                Inputs.Add(input);
            }
        }

        public void Execute()
        {
            //
        }
    }

    public class ShapeGraph
    {
        //
    }
}