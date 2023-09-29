using Godot;
using System;
using System.Collections.Generic;

public partial class ASCIITileMapLayer : TileMap
{
    
    public ASCIITileMap map;

    /// <summary>Holds the foreground color of every cell.</summary>
    public readonly Dictionary<Vector2, Color> cellForegroundColors;
    /// <summary>Holds the background color of every cell.</summary>
    public readonly Dictionary<Vector2, Color> cellBackgroundColors;

    public override void _Ready()
    {
        base._Ready();

        map = GetParent<ASCIITileMap>();

        // Hide self so that only the tilemap's children (ASCIITileMapSprites) are visible.
        SelfModulate = new Color(1, 1, 1, 0);

        // Create map sprites from the saved tilemap.
        // Rect2 usedRect = GetUsedRect();
        // foreach (Vector2I cellCoord in GetUsedCells(0))
        // {
        //     var mapSpriteCoord = cellCoord / MapSpriteSize;
        //     if (!mapSprites.ContainsKey(mapSpriteCoord))
        //     {
        //         CreateMapSprite(mapSpriteCoord);
        //     }
        // }
    }

    //Called every frame. 'delta' is the elapsed time since the previous frame.
    // public override void _Process(float delta)
    // {
    //     // Check if textures need to be remade (they will if any tiles have changed).
    //     foreach (ASCIITileMapSprite mapSprite in mapSpritesToUpdate)
    //     {
    //         mapSprite.CreateTextures(this);
    //     }
    //     mapSpritesToUpdate.Clear();
    // }

    

    

    public void ClearMapSpriteTextures()
    {

    }
}