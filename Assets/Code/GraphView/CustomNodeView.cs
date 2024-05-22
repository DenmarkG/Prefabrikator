using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class CustomNodeView : Node
{
    private Port _input;
    private Port _outPut;

    public CustomNodeView(string name)
        : base()
    {
        this.title = name;

        style.left = this.GetPosition().x;
        style.top = this.GetPosition().y;

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
