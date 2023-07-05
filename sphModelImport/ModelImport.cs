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
		// var path = @"c:\Games\Sfera\models\";
		// var files = Directory.EnumerateFiles(path, "*.mdl");
		//
		// foreach (var file in files)
		// {
		// 	var modelInfo = SphModelInfo.GetModelInfo(file);
		//
		// 	if (modelInfo is null)
		// 	{
		// 		Console.WriteLine($"Skipping {file}");
		//
		// 		continue;
		// 	}
		//
		// 	var arrayMesh = new ArrayMesh();
		//
		// 	foreach (var surface in modelInfo.Surfaces)
		// 	{
		// 		var surfaceArray = new Godot.Collections.Array();
		// 		surfaceArray.Resize((int) Mesh.ArrayType.Max);
		// 		var vertices = new List<Vector3>();
		// 		var uvs = new List<Vector2>();
		// 		var normals = new List<Vector3>();
		// 		var indices = new List<int>();
		//
		// 		for (var i = surface.FirstVertexIndex; i < surface.FirstVertexIndex + surface.VertexCount; i++)
		// 		{
		// 			var vertex = modelInfo.Vertices[i];
		// 			vertices.Add(new Vector3(vertex.X, -vertex.Y, vertex.Z));
		// 			normals.Add(new Vector3(vertex.NormalX, -vertex.NormalY, vertex.NormalZ));
		// 			uvs.Add(new Vector2(vertex.TextureU, vertex.TextureV));
		// 		}
		//
		// 		for (var i = surface.FirstTriangleIndex; i < surface.FirstTriangleIndex + surface.TriangleCount; i++)
		// 		{
		// 			var triangle = modelInfo.Triangles[i];
		// 			indices.Add(triangle.Vertex1);
		// 			indices.Add(triangle.Vertex2);
		// 			indices.Add(triangle.Vertex3);
		// 		}
		//
		// 		surfaceArray[(int) Mesh.ArrayType.Vertex] = vertices.ToArray();
		// 		surfaceArray[(int) Mesh.ArrayType.TexUv] = uvs.ToArray();
		// 		surfaceArray[(int) Mesh.ArrayType.Normal] = normals.ToArray();
		// 		surfaceArray[(int) Mesh.ArrayType.Index] = indices.ToArray();
		//
		// 		arrayMesh.AddSurfaceFromArrays(Mesh.PrimitiveType.Triangles, surfaceArray);
		// 	}
		//
		// 	ResourceSaver.Save(arrayMesh, $"res://{Path.GetFileNameWithoutExtension(file)}.tres", ResourceSaver.SaverFlags.Compress);
		// }

		var landscapePath = @"c:\Games\Sfera\landscape\";
		var files = Directory.EnumerateFiles(landscapePath, "*.lnd");

		using var file = File.OpenRead(files.First());
		using var reader = new BinaryReader(file);
		var magicBytes = reader.ReadBytes(4);
		var totalVertexCount = reader.ReadUInt16();

		file.Seek(0x1b20, SeekOrigin.Begin);

		var vertices = new List<Vector3>();
		var normals = new List<Vector3>();
		var uvs = new List<Vector2>();

		while (file.Position < file.Length - 12)
		// for (var i = 0; i < totalVertexCount; i++)
		{
			// var vertex = reader.ReadStruct<Vertex>();
			// Console.WriteLine($"{vertex.X} {vertex.Y} {vertex.Z} {vertex.NormalX} {vertex.NormalY} {vertex.NormalZ} {vertex.TextureU} {vertex.TextureV}");
			var x = reader.ReadSingle();
			var y = reader.ReadSingle();
			var z = reader.ReadSingle();
			vertices.Add(new Vector3(x, -y, z));
			// var xN = reader.ReadSingle();
			// var yN = reader.ReadSingle();
			// var zN = reader.ReadSingle();
			normals.Add(new Vector3(x, -y, z));
			uvs.Add(new Vector2(0, 0));
		}

		var arrayMesh = new ArrayMesh();

		var surfaceArray = new Godot.Collections.Array();
		surfaceArray.Resize((int) Mesh.ArrayType.Max);
		var indices = new List<int>();

		for (var i = 0; i < vertices.Count; i++)
		{
			indices.Add(i);
			// indices.Add(i + 1);
			// indices.Add(i + 2);
		}

		surfaceArray[(int) Mesh.ArrayType.Vertex] = vertices.ToArray();
		surfaceArray[(int) Mesh.ArrayType.TexUv] = uvs.ToArray();
		surfaceArray[(int) Mesh.ArrayType.Normal] = normals.ToArray();
		surfaceArray[(int) Mesh.ArrayType.Index] = indices.ToArray();

		arrayMesh.AddSurfaceFromArrays(Mesh.PrimitiveType.Points, surfaceArray);

		ResourceSaver.Save(arrayMesh, $"res://testlnd.tres", ResourceSaver.SaverFlags.Compress);

	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
