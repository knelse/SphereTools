using System;
using System.IO;
using Godot;

public partial class MeshInstance3D : Godot.MeshInstance3D
{
    // Called when the node enters the scene tree for the first time.
    public override void _Ready ()
    {
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process (double delta)
    {
        var file = File.ReadAllText(@"c:\_sphereDumps\localPing");
        var currentCoordsLine = file.Split(" ", StringSplitOptions.RemoveEmptyEntries);
        var x = float.Parse(currentCoordsLine[0]);
        var y = -float.Parse(currentCoordsLine[1]);
        var z = -float.Parse(currentCoordsLine[2]);
        var transform = Transform;
        transform.Origin = new Vector3(x, y, z);
        Transform = transform;
    }
}