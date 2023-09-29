using Godot;

public partial class ASCIITileMapSprite : Sprite2D
{
    /// <summary>The ASCIITileMapLayer that the MapSprite is part of.</summary>
    public ASCIITileMapLayer layer;
    /// <summary>The coordinate of this ASCIITileMapSprite in an ASCIITileMap.</summary>
    public Vector2 coord = new Vector2();

    /// <summary>An Image used when creating the tileIdTexture that is passed into the ASCII shader.</summary>
    private Image tileIdImage = new Image();
    /// <summary>An Image used when creating the tileFgBgTexture that is passed into the ASCII shader.</summary>
    private Image tileFgBgImage = new Image();

    public override void _Ready()
    {
        base._Ready();

        layer = GetParent<ASCIITileMapLayer>();
    }

    /// <summary>Creates two textures from all cell data in a given ASCIITileMapLayer.</summary>
    /// <remarks>The first texture uses the L8 format and holds the ID of each tile in the corresponding pixel.
    /// The second texture uses the RGBA8 format and holds the foreground and background colors of each tile within two pixels.</remarks>
    public void CreateTextures(ASCIITileMapLayer tileMap)
    {
        var usedCells = tileMap.GetUsedCells(0);
        // Return if there aren't any cells.
        if (usedCells.Count == 0)
        {
            Texture = null;
            return;
        }
        // Create array of all tilemap data.
        // Each tile is split into 3 colors.
        // The first color is: r: cell_id g: cell_coord_x b: cell_coord_y a: 255.
        // The second color is the foreground color of the tile.
        // The third color is the background color of the tile.
        var tileIdByteStream = new StreamPeerBuffer();
        var tileColorByteStream = new StreamPeerBuffer();
        var usedRect = tileMap.GetUsedRect();
        for (int y = 0; y < usedRect.Size.Y; y++)
        {
            for (int x = 0; x < usedRect.Size.X; x++)
            {
                var coords = new Vector2I(x, y);
                int cell = tileMap.GetCellSourceId(0, coords);

                // If cell is empty, add empty data to the byte stream.
                if (cell == -1)
                {
                    tileIdByteStream.PutU8(0);
                    tileColorByteStream.PutU64(0);
                    continue;
                }

                // Add cell ID to tile ID byte stream.
                tileIdByteStream.PutU8((byte)cell);

                // Get foreground color.
                uint foregroundColorInt;
                if (tileMap.cellForegroundColors.ContainsKey(coords))
                {
                    foregroundColorInt = (uint)tileMap.cellForegroundColors[coords].ToRgba32();
                }
                else foregroundColorInt = 255;

                // Get background color.
                uint backgroundColorInt;
                if (tileMap.cellBackgroundColors.ContainsKey(coords))
                {
                    backgroundColorInt = (uint)tileMap.cellBackgroundColors[coords].ToRgba32();
                }
                else backgroundColorInt = 255;

                // Add foreground and background colors to tile color byte stream.
                tileColorByteStream.PutU8((byte)(foregroundColorInt >> 24));
                tileColorByteStream.PutU8((byte)(foregroundColorInt >> 16));
                tileColorByteStream.PutU8((byte)(foregroundColorInt >> 8));
                tileColorByteStream.PutU8((byte)(foregroundColorInt));
                tileColorByteStream.PutU8((byte)(backgroundColorInt >> 24));
                tileColorByteStream.PutU8((byte)(backgroundColorInt >> 16));
                tileColorByteStream.PutU8((byte)(backgroundColorInt >> 8));
                tileColorByteStream.PutU8((byte)(backgroundColorInt));
            }
            // Convert the streams into textures.
            tileIdImage = Image.CreateFromData(usedRect.Size.X, usedRect.Size.Y, false, Image.Format.L8, tileIdByteStream.DataArray);
            var tileIdTexture = ImageTexture.CreateFromImage(tileIdImage);
            tileFgBgImage = Image.CreateFromData(usedRect.Size.X, usedRect.Size.Y, false, Image.Format.Rgba8, tileIdByteStream.DataArray);
            var tileFgBgTexture = ImageTexture.CreateFromImage(tileFgBgImage);
            // Create new texture for mapSprite.
            var image = Image.Create(usedRect.Size.X * layer.map.glyphSize.X, usedRect.Size.Y * layer.map.glyphSize.Y, false, Image.Format.Rgba8);
            Texture = ImageTexture.CreateFromImage(image);
            // Move mapSprite to the top-left-most tile.
            //mapSprite.Position = usedRect.Position * glyphSize;
            // Send texture and tilemap size to shader which will use the data to draw the glyphs.
            (Material as ShaderMaterial).SetShaderParameter("tilemap_ids", tileIdTexture);
            (Material as ShaderMaterial).SetShaderParameter("tilemap_colors", tileFgBgTexture);
            (Material as ShaderMaterial).SetShaderParameter("tilemap_size", usedRect.Size);
        }
    }
}
