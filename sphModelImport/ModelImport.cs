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
		var modelFiles = Directory.EnumerateFiles(path, "*.mdl");
		
		foreach (var file in modelFiles)
		{
			var modelInfo = SphModelInfo.GetModelInfo(file);
		
			if (modelInfo is null)
			{
				Console.WriteLine($"Skipping {file}");
		
				continue;
			}
		
			var arrayMesh = new ArrayMesh();
		
			foreach (var surface in modelInfo.Surfaces)
			{
				var surfaceArray = new Godot.Collections.Array();
				surfaceArray.Resize((int) Mesh.ArrayType.Max);
				var vertices = new List<Vector3>();
				var uvs = new List<Vector2>();
				var normals = new List<Vector3>();
				var indices = new List<int>();
		
				for (var i = surface.FirstVertexIndex; i < surface.FirstVertexIndex + surface.VertexCount; i++)
				{
					var vertex = modelInfo.Vertices[i];
					vertices.Add(new Vector3(vertex.X, -vertex.Y, vertex.Z));
					normals.Add(new Vector3(vertex.NormalX, -vertex.NormalY, vertex.NormalZ));
					uvs.Add(new Vector2(vertex.TextureU, vertex.TextureV));
				}
		
				for (var i = surface.FirstTriangleIndex; i < surface.FirstTriangleIndex + surface.TriangleCount; i++)
				{
					var triangle = modelInfo.Triangles[i];
					indices.Add(triangle.Vertex1);
					indices.Add(triangle.Vertex2);
					indices.Add(triangle.Vertex3);
				}
		
				surfaceArray[(int) Mesh.ArrayType.Vertex] = vertices.ToArray();
				surfaceArray[(int) Mesh.ArrayType.TexUV] = uvs.ToArray();
				surfaceArray[(int) Mesh.ArrayType.Normal] = normals.ToArray();
				surfaceArray[(int) Mesh.ArrayType.Index] = indices.ToArray();
		
				arrayMesh.AddSurfaceFromArrays(Mesh.PrimitiveType.Triangles, surfaceArray);
			}
		
			ResourceSaver.Save(arrayMesh, $"res://models/{Path.GetFileNameWithoutExtension(file)}.tres", ResourceSaver.SaverFlags.Compress);
		}

		var landscapePath = @"c:\Games\Sfera_std\landscape\";
		var files = Directory.EnumerateFiles(landscapePath, "*.lnd");
		foreach (var filePath in files)
		{

			using var file = File.OpenRead(filePath);
			using var reader = new BinaryReader(file);
			var magicBytes = reader.ReadBytes(4);
			var totalVertexCount = reader.ReadUInt16();

			file.Seek(0x68c0, SeekOrigin.Begin);

			var vertices = new List<Vector3>();
			var normals = new List<Vector3>();
			var uvs = new List<Vector2>();

			for (var i = 0; i < totalVertexCount; i++)
			{
				var x = reader.ReadSingle();
				var y = reader.ReadSingle();
				var z = reader.ReadSingle();
				var xN = reader.ReadSingle();
				var yN = reader.ReadSingle();
				var zN = reader.ReadSingle();
				var u = reader.ReadSingle();
				var v = reader.ReadSingle();
				var unknown = reader.ReadBytes(8);
				vertices.Add(new Vector3(x, -y, z));
				normals.Add(new Vector3(xN, -yN, zN));
				uvs.Add(new Vector2(u, v));
			}

			var arrayMesh = new ArrayMesh();

			var surfaceArray = new Godot.Collections.Array();
			surfaceArray.Resize((int)Mesh.ArrayType.Max);
			var indices = new List<int>();

			while (file.Position < file.Length)
			{
				var v1 = reader.ReadUInt16();
				var v2 = reader.ReadUInt16();
				var v3 = reader.ReadUInt16();
				reader.ReadBytes(22);
				indices.Add(v1);
				indices.Add(v2);
				indices.Add(v3);
			}

			surfaceArray[(int)Mesh.ArrayType.Vertex] = vertices.ToArray();
			surfaceArray[(int)Mesh.ArrayType.TexUV] = uvs.ToArray();
			surfaceArray[(int)Mesh.ArrayType.Normal] = normals.ToArray();
			surfaceArray[(int)Mesh.ArrayType.Index] = indices.ToArray();

			arrayMesh.AddSurfaceFromArrays(Mesh.PrimitiveType.Triangles, surfaceArray);

			ResourceSaver.Save(arrayMesh, $"res://landscape/{Path.GetFileNameWithoutExtension(filePath)}.tres",
				ResourceSaver.SaverFlags.Compress);
		}

	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
