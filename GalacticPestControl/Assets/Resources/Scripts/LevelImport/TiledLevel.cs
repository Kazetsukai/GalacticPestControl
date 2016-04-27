
using System;

[Serializable]
public class TiledLevel
{
    [NonSerialized]
    public string path;

    public int height;
    public Layer[] layers;
    public Tileset[] tilesets;
}

[Serializable]
public class Layer
{
    public int[] data;
    public string name;
    public int height;
    public int width;
}

[Serializable]
public class Tileset
{
    public string image;
    public int tilecount;
    public int firstgid;
    public string source;
}
