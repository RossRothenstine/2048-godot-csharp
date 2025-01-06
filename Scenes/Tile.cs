using Godot;
using System;

public partial class Tile : Node2D
{
    private int _value;

    [Export]
    public int Value
    {
        get => _value;
        set {
            _value = value;
            UpdateColor();
            UpdateLabel();
        }
    }

    public override void _Ready() {
        UpdateColor();
        UpdateLabel();
    }

    private void UpdateLabel() {
        if (!IsInsideTree()) {
            return;
        }
        var label = GetNode<Label>("Label");
        label.Text = Value.ToString();
    }

    private void UpdateColor() {
        if (!IsInsideTree()) {
            return;
        }
        Color color = default;
        switch (Value) {
        case 2:
            color = new Color("bb7777");
            break;
        case 4:
            color = new Color("776688");
            break;
        case 8:
            color = new Color("889999");
            break;
        case 16:
            color = new Color("99aa55");
            break;
        case 32:
            color = new Color("668855");
            break;
        case 64:
            color = new Color("336644");
            break;
        case 128:
            color = new Color("334444");
            break;
        case 256:
            color = new Color("332233");
            break;
        case 512:
            color = new Color("221122");
            break;
        case 1024:
            color = new Color("cc9966");
            break;
        case 2048:
            color = new Color("aa6677");
            break;
        }

        var polygon2D = GetNode<Polygon2D>("Polygon2D");
        polygon2D.Color = color;
    }
}
