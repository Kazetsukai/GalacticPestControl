using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

[Serializable]
public class AsepriteAnimation
{
    public AsepriteFrame[] frames;
    public AsepriteMeta meta;
}

#region Aseprite Animation Cell
[Serializable]
public class AsepriteFrame
{
    public string filename;
    public AsepriteCellFrame frame;
    public bool rotated;
    public bool framed;
    public AsepriteSpriteSourceSize spriteSourceSize;
    public AsepriteSourceSize sourceSize;
    public int duration;
}

[Serializable]
public class AsepriteCellFrame
{
    public int x;
    public int y;
    public int w;
    public int h;
}

[Serializable]
public class AsepriteSpriteSourceSize
{
    public int x;
    public int y;
    public int w;
    public int h;
}

[Serializable]
public class AsepriteSourceSize
{
    public int w;
    public int h;
}
#endregion
#region Aseprite Animation Meta
[Serializable]
public class AsepriteMeta
{
    public string app;
    public string version;
    public string image;
    public string format;
    public AsepriteSize size;
    public float scale;
    public AsepriteAnim[] frameTags;
    public AsepriteLayer[] layers;
}

[Serializable]
public class AsepriteSize
{
    public int w;
    public int h;
}

[Serializable]
public class AsepriteAnim
{
    public string name;
    public int from;
    public int to;
    public string direction;
}

[Serializable]
public class AsepriteLayer
{
    public string name;
    public int opacity;
    public string blendMode;
}
#endregion

