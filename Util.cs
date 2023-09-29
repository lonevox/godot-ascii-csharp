using System;
using Godot;

public static class Util
{
    public static Color ColorFromInt(int color)
    {
        byte[] channels = BitConverter.GetBytes(color);
        return Color.Color8(channels[0], channels[1], channels[2], channels[3]);
    }

    public static int ColorToInt(Color color)
    {
        return unchecked((int)color.ToAbgr32());
    }
}

public static class Extensions
{
    public static Vector2I ToVector2I(this Vector2 vector2)
    {
        return new Vector2I((int)vector2.X, (int)vector2.Y);
    }
}
