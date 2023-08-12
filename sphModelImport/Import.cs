using System;
using Godot;
using System.Collections.Generic;
using System.IO;
using System.Text;
using sphModelInfo;

public partial class Import : MeshInstance3D
{
	private PackedScene landscapeScene;
	private Node3D meshLibraryRoot;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		// var path = @"c:\Games\Sfera\models\";
		// var modelFiles = Directory.EnumerateFiles(path, "*.mdl");
		//
		// foreach (var file in modelFiles)
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
		// 		surfaceArray[(int) Mesh.ArrayType.TexUV] = uvs.ToArray();
		// 		surfaceArray[(int) Mesh.ArrayType.Normal] = normals.ToArray();
		// 		surfaceArray[(int) Mesh.ArrayType.Index] = indices.ToArray();
		//
		// 		arrayMesh.AddSurfaceFromArrays(Mesh.PrimitiveType.Triangles, surfaceArray);
		// 	}
		//
		// 	ResourceSaver.Save(arrayMesh, $"res://models/{Path.GetFileNameWithoutExtension(file)}.tres", ResourceSaver.SaverFlags.Compress);
		// }

		// landscapeScene = new PackedScene();
		// meshLibraryRoot = new Node3D
		// {
		// 	Name = "MeshLibraryRoot"
		// };
		// ImportLandscapeFiles(@"landscape");
		// ImportLandscapeFiles("landscape_hr");
		// ImportLandscapeFiles("landscape_ph");
		// ImportLandscapeFiles("landscape_rd");
		// landscapeScene.Pack(meshLibraryRoot);
		// var error = ResourceSaver.Save(landscapeScene, $"res://TerrainMeshLibrary.tscn", ResourceSaver.SaverFlags.Compress);
		// if (error != Error.Ok)
		// {
		// 	GD.Print(error);
		// }
		
		// using var mapFile = File.OpenRead(@"c:\Games\Sfera_std\landscape\map.bin");
		// using var reader = new BinaryReader(mapFile);
		// var terrainIndex = 0;
		// var nodeNames = new Dictionary<string, int>();
		// while (reader.BaseStream.Position < reader.BaseStream.Length && terrainIndex < 300)
		// {
		// 	var geometryNameBytes = reader.ReadBytes(20);
		// 	var variantBytes = reader.ReadBytes(2);
		// 	var variant = $"{variantBytes[0]:0}{variantBytes[1]:0}";
		// 	var sb = new StringBuilder();
		// 	foreach (var nameByte in geometryNameBytes)
		// 	{
		// 		if (nameByte == 0)
		// 		{
		// 			break;
		// 		}
		// 		sb.Append((char)nameByte);
		// 	}
		//
		// 	var geometryName = sb.ToString().ToLower();
		// 	string resourcePath = "";
		// 	if (geometryName.Contains("_hr"))
		// 	{
		// 		resourcePath = $"landscape_hr/{geometryName}_{variant}.tres";
		// 	}
		// 	else if (geometryName.Contains("_ph"))
		// 	{
		// 		resourcePath = $"landscape_ph/{geometryName}_{variant}.tres";
		// 	}
		// 	else if (geometryName.Contains("_rd"))
		// 	{
		// 		resourcePath = $"landscape_rd/{geometryName}_{variant}.tres";
		// 	}
		// 	else
		// 	{
		// 		resourcePath = $"landscape/{geometryName}_{variant}.tres";
		// 	}
		//
		// 	try
		// 	{
		// 		var resource = ResourceLoader.Load<ArrayMesh>(resourcePath);
		// 		var collisionNode = new StaticBody3D();
		// 		var terrain = new MeshInstance3D();
		// 		terrain.Mesh = resource;
		// 		var transform = terrain.Transform;
		// 		transform.Origin =
		// 			new Vector3(100.0f * (terrainIndex % 80), transform.Origin.Y, 100.0f * terrainIndex / 80);
		// 		transform = transform.RotatedLocal(Vector3.Up, -Mathf.Pi / 2);
		// 		terrain.Transform = transform;
		// 		nodeNames.TryAdd(geometryName, 0);
		// 		nodeNames[geometryName]++;
		// 		collisionNode.Name = $"{geometryName}_{nodeNames[geometryName]}";
		// 		collisionNode.AddChild(terrain);
		// 		terrain.CreateConvexCollision();
		// 		terrain.Owner = collisionNode;
		// 		AddChild(collisionNode);
		// 		collisionNode.Owner = this;
		// 	}
		// 	catch (Exception ex)
		// 	{
		// 		GD.Print(ex.Message);
		// 		GD.Print(resourcePath);
		// 	}
		//
		// 	terrainIndex++;
		// }
		//
		// var scene = new PackedScene();
		// scene.Pack(this);
		// var error = ResourceSaver.Save(scene, $"res://testterrain.tscn", ResourceSaver.SaverFlags.Compress);
		// if (error != Error.Ok)
		// {
		// 	GD.Print(error);
		// }
	}

	private void ImportLandscapeFiles(string continent)
	{
		var path = $@"c:\Games\Sfera_std\{continent}\";
		var files = Directory.EnumerateFiles(path, "*.lnd");
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

			var name = Path.GetFileNameWithoutExtension(filePath);
			var material = new StandardMaterial3D
			{
				AlbedoTexture = ResourceLoader.Load<Texture2D>($"res://{continent}/textures/{name}.tga")
			};
			arrayMesh.SurfaceSetMaterial(0, material);
			var terrain = new MeshInstance3D
			{
				Name = name
			};
			terrain.Mesh = arrayMesh;
			var transform = terrain.Transform;
			transform = transform.RotatedLocal(Vector3.Up, -Mathf.Pi / 2);
			terrain.Transform = transform;
			meshLibraryRoot.AddChild(terrain);
			terrain.Owner = meshLibraryRoot;
			terrain.CreateTrimeshCollision();
		}
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
