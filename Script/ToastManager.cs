using Godot;
using System;

public partial class ToastManager : Node
{
    private CanvasLayer _canvasLayer;

    public override void _Ready()
    {
        base._Ready();

        _canvasLayer = new CanvasLayer();
        AddChild(_canvasLayer);
    }

}
