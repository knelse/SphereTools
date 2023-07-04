using Godot;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using sphModelInfo;

public partial class ModelImport : MeshInstance3D
{
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		var path = @"c:\Games\Sfera\models\";
		var files = Directory.EnumerateFiles(path, "*.mdl");

		foreach (var file in files)
		{
			var modelInfo = SphModelInfo.GetModelInfo(file);

			if (modelInfo is null)
			{
				Console.WriteLine($"Skipping {file}");

				continue;
			}

			var surfaceArray = new Godot.Collections.Array();
			surfaceArray.Resize((int) Mesh.ArrayType.Max);
			var vertices = new List<Vector3>();
			var uvs = new List<Vector2>();
			var normals = new List<Vector3>();
			var indices = new List<int>();

			foreach (var vertex in modelInfo.Vertices)
			{
				vertices.Add(new Vector3(vertex.X, -vertex.Y, vertex.Z));
				normals.Add(new Vector3(vertex.NormalX, -vertex.NormalY, vertex.NormalZ));
				uvs.Add(new Vector2(vertex.TextureU, vertex.TextureV));
			}

			foreach (var triangle in modelInfo.Triangles)
			{
				indices.Add(triangle.Vertex1);
				indices.Add(triangle.Vertex2);
				indices.Add(triangle.Vertex3);
			}

			surfaceArray[(int) Mesh.ArrayType.Vertex] = vertices.ToArray();
			surfaceArray[(int) Mesh.ArrayType.TexUv] = uvs.ToArray();
			surfaceArray[(int) Mesh.ArrayType.Normal] = normals.ToArray();
			surfaceArray[(int) Mesh.ArrayType.Index] = indices.ToArray();

			var arrayMesh = new ArrayMesh();
			arrayMesh.AddSurfaceFromArrays(Mesh.PrimitiveType.Triangles, surfaceArray);

			ResourceSaver.Save(arrayMesh, $"res://{Path.GetFileNameWithoutExtension(file)}.tres", ResourceSaver.SaverFlags.Compress);
		}
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
