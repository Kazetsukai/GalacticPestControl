#if UNITY_EDITOR

using UnityEngine;
using System.Collections;
using System.IO;
using System;
using System.Linq;
using System.Xml;
using System.Collections.Generic;

using UnityEditor;
using Assets.Resources.Scripts;

[CustomEditor(typeof(TiledLevelImporter))]
public class TiledLevelImporterEditor : Editor
{
    public Dictionary<int, bool> walls;

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        if (GUILayout.Button("Import Tiled Level"))
        {
            string filename = EditorUtility.OpenFilePanel("Select Tiled level JSON", ".", "json");

            var file = new StreamReader(File.OpenRead(filename));
            var fileText = file.ReadToEnd();

            var obj = JsonUtility.FromJson<TiledLevel>(fileText);

            obj.path = new FileInfo(filename).DirectoryName;

            CreateLevel(obj);


        }
    }

    private void CreateLevel(TiledLevel obj)
    {
        var material = ((TiledLevelImporter)target).SpriteMaterial;
        var spriteMesh = ((TiledLevelImporter)target).SpriteMesh;

        walls = new Dictionary<int, bool>();

        // If any tilesets are remote references, go find those tilesets and load them
        FixTilesets(obj.tilesets, obj.path);

        var tileCount = obj.tilesets.Sum(t => t.tilecount) + 1;
        Texture2D[] tileSprites = new Texture2D[tileCount];
        Material[] tileMaterials = new Material[tileCount];

        int i = 0;

        // Load tilesets
        foreach (var tileset in obj.tilesets)
        {
            Debug.Log("Reading tileset...");
            var tilesetToUse = tileset;

            i = tilesetToUse.firstgid;

            var query = tilesetToUse.image.Split('\\', '/').Last().Split('.').First() + " t:sprite";
            Debug.Log(query);
            var assetPath = AssetDatabase.FindAssets(query);
            Debug.Log(assetPath.First());


            if (assetPath.Any())
            {
                var spriteSheet = AssetDatabase.GUIDToAssetPath(assetPath.First());

                Sprite[] sprites = AssetDatabase.LoadAllAssetsAtPath(spriteSheet)
                    .OfType<Sprite>().ToArray();

                foreach (var sprite in sprites)
                {
                    // Turn sprite into texture
                    Texture2D spriteTexture = new Texture2D((int)sprite.rect.width, (int)sprite.rect.height);
                    spriteTexture.filterMode = FilterMode.Point;

                    var pixels = sprite.texture.GetPixels((int)sprite.rect.x, (int)sprite.rect.y, (int)sprite.rect.width, (int)sprite.rect.height);
                    spriteTexture.SetPixels(pixels);
                    spriteTexture.Apply();

                    // And make a material out of that
                    tileMaterials[i] = new Material(material);
                    tileMaterials[i].mainTexture = spriteTexture;

                    tileSprites[i++] = spriteTexture;
                }
            }
            else
            {
                Debug.LogError("Couldn't find sprites - " + tilesetToUse.image);
            }

        }

        // Kill old level
        var parent = ((MonoBehaviour)target).transform;
        List<Transform> killList = new List<Transform>(parent.Cast<Transform>());
        foreach (var t in killList)
            DestroyImmediate(t.gameObject);

        var importedLevel = new GameObject("Tiled Level");
        importedLevel.transform.parent = parent;

        // Load the layers.
        for (int layerId = obj.layers.Length - 1; layerId >= 0; layerId--)
        {
            var layer = obj.layers[layerId];

            GameObject layerObj = new GameObject(layer.name);
            layerObj.transform.parent = importedLevel.transform;

            var renderer = layerObj.AddComponent<MeshRenderer>();
            renderer.material = material;

            // Each layer gets its own mesh.
            Mesh mesh = new Mesh();
            var vertices = new Vector3[layer.height * layer.width * 6];
            var uvs = new Vector2[layer.height * layer.width * 6];
            var triangles = new List<int>();

            // If the layer name can be parsed as an int, use that as the layer's height.
            int layerHeight;
            if (!int.TryParse(layer.name, out layerHeight))
            {
                layerHeight = 0;
            }

            for (int y = 0; y < layer.height; y++)
            {
                for (int x = 0; x < layer.width; x++)
                {
                    var index = (layer.width * y + x) * 6;

                    var dataIndex = layer.width * y + x;
                    var spriteIndex = layer.data[dataIndex];

                    if (spriteIndex == 0)
                        continue;

                    // Positions need to be adjusted by the square root of two, to line up pixels perfectly with the 45 degree orthographic camera.
                    var pos = new Vector3(x, (-y - layerHeight) * Constants.SQRT_TWO, -layerHeight * Constants.SQRT_TWO);

                    if (!walls[spriteIndex])
                    {
                        vertices[index + 0] = pos + (Vector3.up * Constants.SQRT_TWO + Vector3.left) / 2f;
                        vertices[index + 1] = pos + (Vector3.up * Constants.SQRT_TWO + Vector3.right) / 2f;
                        vertices[index + 2] = pos + (Vector3.down * Constants.SQRT_TWO + Vector3.left) / 2f;

                        vertices[index + 3] = pos + (Vector3.up * Constants.SQRT_TWO + Vector3.right) / 2f;
                        vertices[index + 4] = pos + (Vector3.down * Constants.SQRT_TWO + Vector3.right) / 2f;
                        vertices[index + 5] = pos + (Vector3.down * Constants.SQRT_TWO + Vector3.left) / 2f;
                    }
                    else
                    {
                        vertices[index + 0] = pos + (Vector3.up * Constants.SQRT_TWO + Vector3.left) / 2f;
                        vertices[index + 1] = pos + (Vector3.up * Constants.SQRT_TWO + Vector3.right) / 2f;
                        vertices[index + 2] = pos + Vector3.forward * Constants.SQRT_TWO + (Vector3.up * Constants.SQRT_TWO + Vector3.left) / 2f;

                        vertices[index + 3] = pos + (Vector3.up * Constants.SQRT_TWO + Vector3.right) / 2f;
                        vertices[index + 4] = pos + Vector3.forward * Constants.SQRT_TWO + (Vector3.up * Constants.SQRT_TWO + Vector3.right) / 2f;
                        vertices[index + 5] = pos + Vector3.forward * Constants.SQRT_TWO + (Vector3.up * Constants.SQRT_TWO + Vector3.left) / 2f;
                    }

                    uvs[index + 0] = new Vector2(0, 0);
                    uvs[index + 1] = new Vector2(1, 0);
                    uvs[index + 2] = new Vector2(0, 1);

                    uvs[index + 3] = new Vector2(1, 0);
                    uvs[index + 4] = new Vector2(1, 1);
                    uvs[index + 5] = new Vector2(0, 1);

                    triangles.Add(index + 0);
                    triangles.Add(index + 1);
                    triangles.Add(index + 2);

                    triangles.Add(index + 3);
                    triangles.Add(index + 4);
                    triangles.Add(index + 5);

                    //renderer.sortingOrder = zDepths[spriteIndex];

                    // Anything on layer 0 should have a collider
                    /*if (zDepths[spriteIndex] == 0)
                    {
                        var collider = tile.AddComponent<BoxCollider2D>();
                        collider.size = new Vector2(1, 1.4f);
                        
                    }*/
                }
            }

            // Construct mesh
            mesh.vertices = vertices;
            mesh.uv = uvs;
            mesh.triangles = triangles.ToArray();

            mesh.RecalculateNormals();
            mesh.RecalculateBounds();

            // Add the mesh to the layer object
            var meshFilter = layerObj.AddComponent<MeshFilter>();
            meshFilter.mesh = mesh;
        }
    }

    private void FixTilesets(Tileset[] tilesets, string path)
    {
        for (int i = 0; i < tilesets.Length; i++)
        {
            if (!string.IsNullOrEmpty(tilesets[i].source))
            {
                var firstgid = tilesets[i].firstgid;
                tilesets[i] = LoadTileset(Path.Combine(path, tilesets[i].source), firstgid);
            }
        }

    }

    private Tileset LoadTileset(string source, int firstgid)
    {
        Debug.Log(source);

        var tileset = new Tileset();

        var contents = new StreamReader(File.OpenRead(source)).ReadToEnd();

        var xdoc = new XmlDocument();
        xdoc.LoadXml(contents);
        var tsNode = xdoc.GetElementsByTagName("tileset")[0];

        tileset.tilecount = int.Parse(tsNode.Attributes.GetNamedItem("tilecount").Value);
        tileset.image = tsNode["image"].Attributes.GetNamedItem("source").Value;

        var tileNodes = xdoc.GetElementsByTagName("tile");
        foreach (XmlNode tileNode in tileNodes)
        {
            var id = int.Parse(tileNode.Attributes.GetNamedItem("id").Value) + firstgid;
            var zNode = tileNode["properties"].ChildNodes.Cast<XmlNode>().Where(x => x.Name == "property" && x.Attributes.GetNamedItem("name").Value == "z").FirstOrDefault();
            var wallNode = tileNode["properties"].ChildNodes.Cast<XmlNode>().Where(x => x.Name == "property" && x.Attributes.GetNamedItem("name").Value == "wall").FirstOrDefault();
            if (wallNode != null)
            {
                walls[id] = true;
            }
            else
            {
                walls[id] = false;
            }

        }

        tileset.firstgid = firstgid;

        return tileset;
    }
}


#endif