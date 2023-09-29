using System;
using System.Collections.Generic;
using CommunityToolkit.HighPerformance;
using Godot;

public class Chunk
{
    public enum CellColorType
    {
        None,
        TwoColor,
        Palette,
        Full,
        Gradient,
    }

    /// <summary>The number of cells in each chunk.</summary>
    public const int CELLS = 256;
    /// <summary>Width and height of each chunk.</summary>
    public const int SIZE = 16;

    private static readonly ShaderMaterial asciiUberShaderMaterial = ResourceLoader.Load<ShaderMaterial>("res://addons/ASCIITileMap/ascii_tilemap_uber_shader_material.tres");
    // private static readonly ShaderMaterial asciiOneBitShaderMaterial = ResourceLoader.Load<ShaderMaterial>("res://addons/ASCIITileMap/ascii_tilemap_uber_shader_material.tres");
    // private static readonly ShaderMaterial asciiTwoColorShaderMaterial = ResourceLoader.Load<ShaderMaterial>("res://addons/ASCIITileMap/ascii_tilemap_one_bit_shader_material.tres");
    // private static readonly ShaderMaterial asciiPaletteShaderMaterial = ResourceLoader.Load<ShaderMaterial>("res://addons/ASCIITileMap/ascii_tilemap_one_bit_shader_material.tres");
    // private static readonly ShaderMaterial asciiFullShaderMaterial = ResourceLoader.Load<ShaderMaterial>("res://addons/ASCIITileMap/ascii_tilemap_one_bit_shader_material.tres");
    // private static readonly ShaderMaterial asciiColorRegionShaderMaterial = ResourceLoader.Load<ShaderMaterial>("res://addons/ASCIITileMap/ascii_tilemap_one_bit_shader_material.tres");
    // private static readonly ShaderMaterial[] asciiShaderMaterials = new ShaderMaterial[5] { asciiOneBitShaderMaterial, asciiTwoColorShaderMaterial, asciiPaletteShaderMaterial, asciiFullShaderMaterial, asciiColorRegionShaderMaterial };

    // private CellColorType _dominantCellColorType = CellColorType.None;
    // public CellColorType DominantCellColorType
    // {
    //     get => _dominantCellColorType;
    //     private set
    //     {
    //         _dominantCellColorType = value;
    //         switch (_dominantCellColorType)
    //         {
    //             case CellColorType.None:
    //                 asciiShaderMaterial = asciiTwoColorShaderMaterial;
    //                 break;
    //             case CellColorType.TwoColor:
    //                 asciiShaderMaterial = asciiTwoColorShaderMaterial;
    //                 break;
    //             case CellColorType.Palette:
    //                 asciiShaderMaterial = asciiPaletteShaderMaterial;
    //                 break;
    //             case CellColorType.Full:
    //                 asciiShaderMaterial = asciiFullShaderMaterial;
    //                 break;
    //             case CellColorType.Gradient:
    //                 asciiShaderMaterial = asciiColorRegionShaderMaterial;
    //                 break;
    //         }
    //     }
    // }
    private bool render = true;
    private Rid canvasItem = RenderingServer.CanvasItemCreate();
    private ShaderMaterial asciiShaderMaterial = asciiUberShaderMaterial.Duplicate() as ShaderMaterial;
    // private ShaderMaterial AsciiShaderMaterial
    // {
    //     get => asciiShaderMaterial;
    //     set
    //     {
    //         asciiShaderMaterial = value;
    //         RenderingServer.CanvasItemSetMaterial(canvasItem, asciiShaderMaterial.GetRid());
    //     }
    // }
    private byte[] _cellGlyphs = new byte[SIZE * SIZE];
    public Span2D<byte> CellGlyphs => new Memory2D<byte>(_cellGlyphs, SIZE, SIZE).Span;
    private bool cellGlyphsDirty;
    private byte[] _cellForegroundPaletteColors = new byte[SIZE * SIZE];
    public Span2D<byte> CellForegroundPaletteColors => new Memory2D<byte>(_cellForegroundPaletteColors, SIZE, SIZE).Span;
    private bool cellForegroundPaletteColorsDirty;
    private byte[] _cellBackgroundPaletteColors = new byte[SIZE * SIZE];
    public Span2D<byte> CellBackgroundPaletteColors => new Memory2D<byte>(_cellBackgroundPaletteColors, SIZE, SIZE).Span;
    private bool cellBackgroundPaletteColorsDirty;
    private int[] _cellForegroundColors = new int[SIZE * SIZE];
    public int[] CellForegroundColors
    {
        private get => _cellForegroundColors;
        set
        {
            _cellForegroundColors = value;
            cellForegroundColorsDirty = true;
        }
    }
    public Span2D<int> CellForegroundColorsSpan2D
    {
        get => new Memory2D<int>(CellForegroundColors, SIZE, SIZE).Span;
        set
        {
            cellForegroundColorsDirty = true;
        }
    } 
    private bool cellForegroundColorsDirty;
    private int[] _cellBackgroundColors = new int[SIZE * SIZE];
    public int[] CellBackgroundColors
    {
        private get => _cellBackgroundColors;
        set
        {
            _cellBackgroundColors = value;
            cellBackgroundColorsDirty = true;
        }
    }
    public Span2D<int> CellBackgroundColorsSpan2D => new Memory2D<int>(CellBackgroundColors, SIZE, SIZE).Span;
    private bool cellBackgroundColorsDirty;

    // These 5 values represent how many cells in the chunk are of a certain color type.
    // private int noColorCells;
    // private int twoColorCells;
    // private int paletteCells;
    // private int fullColorCells;
    // private int gradientCells;


    /// <summary>Create empty chunk.</summary>
    public Chunk()
    {
        //noColorCells = 256;
        RenderingServer.CanvasItemSetMaterial(canvasItem, asciiShaderMaterial.GetRid());
    }

    /// <summary>Create chunk with cell glyph data.</summary>
    public Chunk(byte[,] cellGlyphs)
    {
        cellGlyphs.AsSpan2D().CopyTo(this.CellGlyphs);
        RenderingServer.CanvasItemSetMaterial(canvasItem, asciiShaderMaterial.GetRid());
    }

    /// <summary>Create chunk with cell glyph and palette color data.</summary>
    public Chunk(byte[,] cellGlyphs, byte[,] cellForegroundPaletteColors, byte[,] cellBackgroundPaletteColors)
    {
        cellGlyphs.AsSpan2D().CopyTo(this.CellGlyphs);
        // If there is a color in the palette color arrays, use the palette color mode.
        if (HasNonDefaultValue(cellForegroundPaletteColors.AsSpan2D()) || HasNonDefaultValue(cellBackgroundPaletteColors.AsSpan2D()))
        {
            //DominantCellColorType = CellColorType.Palette;
            cellForegroundPaletteColors.AsSpan2D().CopyTo(this.CellForegroundPaletteColors);
            cellBackgroundPaletteColors.AsSpan2D().CopyTo(this.CellBackgroundPaletteColors);
        }
        RenderingServer.CanvasItemSetMaterial(canvasItem, asciiShaderMaterial.GetRid());
    }

    /// <summary>Create chunk with cell glyph and color data.</summary>
    public Chunk(byte[,] cellGlyphs, int[,] cellForegroundColors, int[,] cellBackgroundColors)
    {
        cellGlyphs.AsSpan2D().CopyTo(this.CellGlyphs);
        // If there is a color in the color arrays, use the full color mode.
        if (HasNonDefaultValue(cellForegroundColors.AsSpan2D()) || HasNonDefaultValue(cellBackgroundColors.AsSpan2D()))
        {
            //DominantCellColorType = CellColorType.Full;
            cellForegroundColors.AsSpan2D().CopyTo(this.CellForegroundColors);
            cellBackgroundColors.AsSpan2D().CopyTo(this.CellBackgroundColors);
        }
        asciiShaderMaterial.SetShaderParameter("tileset_ids", _cellGlyphs);
        asciiShaderMaterial.SetShaderParameter("fg_colors", CellForegroundColors);
        asciiShaderMaterial.SetShaderParameter("bg_colors", CellBackgroundColors);
        RenderingServer.CanvasItemSetMaterial(canvasItem, asciiShaderMaterial.GetRid());
    }

    /// <summary>Create chunk with cell glyph and all color data.</summary>
    public Chunk(byte[,] cellGlyphs, byte[,] cellForegroundPaletteColors, byte[,] cellBackgroundPaletteColors, int[,] cellForegroundColors, int[,] cellBackgroundColors)
    {
        cellGlyphs.AsSpan2D().CopyTo(this.CellGlyphs);
        // If there is a color in the palette color arrays, use the palette color mode.
        if (HasNonDefaultValue(cellForegroundPaletteColors.AsSpan2D()) || HasNonDefaultValue(cellBackgroundPaletteColors.AsSpan2D()))
        {
            //DominantCellColorType = CellColorType.Palette;
            cellForegroundPaletteColors.AsSpan2D().CopyTo(this.CellForegroundPaletteColors);
            cellBackgroundPaletteColors.AsSpan2D().CopyTo(this.CellBackgroundPaletteColors);
        }
        // If there is a color in the color arrays, use the full color mode.
        if (HasNonDefaultValue(cellForegroundColors.AsSpan2D()) || HasNonDefaultValue(cellBackgroundColors.AsSpan2D()))
        {
            //DominantCellColorType = CellColorType.Full;
            cellForegroundColors.AsSpan2D().CopyTo(this.CellForegroundColors);
            cellBackgroundColors.AsSpan2D().CopyTo(this.CellBackgroundColors);
        }
        RenderingServer.CanvasItemSetMaterial(canvasItem, asciiShaderMaterial.GetRid());
    }

    // private void RecalculateCellColorTypeCounts()
    // {
    //     for (int col = 0; col < SIZE; col++)
    //     {
    //         for (int row = 0; row < SIZE; row++)
    //         {
    //             if (cellForegroundColors[col,row] != 0 || cellBackgroundColors[col,row] != 0)
    //                 fullColorCells++;
    //             else if (cellForegroundPaletteColors[col,row] != 0 || cellForegroundPaletteColors[col,row] != 0)
    //                 paletteCells++;
    //             else
    //                 noColorCells++;
    //         }
    //     }
    //     UpdateCellColorTypeCount(CellColorType.None);
    //     UpdateCellColorTypeCount(CellColorType.TwoColor);
    //     UpdateCellColorTypeCount(CellColorType.Palette);
    //     UpdateCellColorTypeCount(CellColorType.Full);
    //     UpdateCellColorTypeCount(CellColorType.Gradient);
    // }

    // private void UpdateCellColorTypeCount(CellColorType cellColorType)
    // {
    //     switch (cellColorType)
    //     {
    //         case CellColorType.None:
    //         case CellColorType.
    //         default:
    //     }
    // }

    private static bool HasNonDefaultValue<T>(Span2D<T> span2d)
    {
        foreach (var item in span2d)
        {
            if (!EqualityComparer<T>.Default.Equals(item, default(T)))
                return true;
        }
        return false;
    }

    /// <summary>Create chunk with dummy data.</summary>
    public static Chunk DummyChunk()
    {
        var cellGlyphs = new byte[SIZE, SIZE];
        var cellForegroundColors = new int[SIZE, SIZE];
        var cellBackgroundColors = new int[SIZE, SIZE];
        for (int col = 0; col < SIZE; col++)
        {
            for (int row = 0; row < SIZE; row++)
            {
                cellGlyphs[col, row] = (byte)(col + row * SIZE);
                var color = Color.FromHsv((float)(col + row) / (SIZE * 2), 1, 1);
                cellForegroundColors[col, row] = Util.ColorToInt(color);
                color.H += 0.5f;
                cellBackgroundColors[col, row] = Util.ColorToInt(color);
            }
        }
        return new Chunk(cellGlyphs, cellForegroundColors, cellBackgroundColors);
    }

    public void DrawTo(Rid parent, Rect2 rect)
    {
        RenderingServer.CanvasItemSetParent(canvasItem, parent);
        RenderingServer.CanvasItemAddRect(canvasItem, rect, Colors.White);
    }

    public void SetAsciiTileSetTexture(Texture2D texture)
    {
        asciiShaderMaterial.SetShaderParameter("tileset", texture);
    }

    public void SetGlyphSize(Vector2I glyphSize)
    {
        asciiShaderMaterial.SetShaderParameter("glyph_size", glyphSize);
    }

    public byte GetCellGlyph(Vector2I coord)
    {
        return CellGlyphs[coord.X, coord.Y];
    }

    public void SendDirtyArrays()
    {
        if (cellGlyphsDirty)
        {
            asciiShaderMaterial.SetShaderParameter("tileset_ids", _cellGlyphs);
            cellGlyphsDirty = false;
        }
        if (cellForegroundPaletteColorsDirty)
        {
            asciiShaderMaterial.SetShaderParameter("fg_palette_ids", _cellForegroundPaletteColors);
            cellForegroundPaletteColorsDirty = false;
        }
        if (cellBackgroundPaletteColorsDirty)
        {
            asciiShaderMaterial.SetShaderParameter("bg_palette_ids", _cellBackgroundPaletteColors);
            cellBackgroundPaletteColorsDirty = false;
        }
        if (cellForegroundColorsDirty)
        {
            asciiShaderMaterial.SetShaderParameter("fg_colors", CellForegroundColors);
            cellForegroundColorsDirty = false;
        }
        if (cellBackgroundColorsDirty)
        {
            asciiShaderMaterial.SetShaderParameter("bg_colors", CellBackgroundColors);
            cellBackgroundColorsDirty = false;
        }
    }

    public void SetCell(byte x, byte y, int glyph = -1, int fgPaletteColor = -1, int bgPaletteColor = -1, int fgColor = -1, int bgColor = -1)
    {
        if (glyph != -1)
        {
            CellGlyphs[y, x] = (byte)glyph;
            cellGlyphsDirty = true;
        }
        if (fgPaletteColor != -1)
        {
            CellForegroundPaletteColors[y, x] = (byte)fgPaletteColor;
            cellForegroundPaletteColorsDirty = true;
        }
        if (bgPaletteColor != -1)
        {
            CellBackgroundPaletteColors[y, x] = (byte)bgPaletteColor;
            cellBackgroundPaletteColorsDirty = true;
        }
        if (fgColor != -1)
        {
            CellForegroundColorsSpan2D[y, x] = fgColor;
            cellForegroundColorsDirty = true;
        }
        if (bgColor != -1)
        {
            CellBackgroundColorsSpan2D[y, x] = bgColor;
            cellBackgroundColorsDirty = true;
        }
    }

    public void SetCell(Vector2I coord, int glyph = -1, int fgPaletteColor = -1, int bgPaletteColor = -1, int fgColor = -1, int bgColor = -1)
    {
        if (glyph != -1)
        {
            CellGlyphs[coord.Y, coord.X] = (byte)glyph;
            cellGlyphsDirty = true;
        }
        if (fgPaletteColor != -1)
        {
            CellForegroundPaletteColors[coord.Y, coord.X] = (byte)fgPaletteColor;
            cellForegroundPaletteColorsDirty = true;
        }
        if (bgPaletteColor != -1)
        {
            CellBackgroundPaletteColors[coord.Y, coord.X] = (byte)bgPaletteColor;
            cellBackgroundPaletteColorsDirty = true;
        }
        if (fgColor != -1)
        {
            CellForegroundColorsSpan2D[coord.Y, coord.X] = fgColor;
            cellForegroundColorsDirty = true;
        }
        if (bgColor != -1)
        {
            CellBackgroundColorsSpan2D[coord.Y, coord.X] = bgColor;
            cellBackgroundColorsDirty = true;
        }
    }

    public void SetCellForegroundPaletteColor(Vector2I coord, byte color)
    {
        CellForegroundPaletteColors[coord.X, coord.Y] = color;
    }

    public void SetCellBackgroundPaletteColor(Vector2I coord, byte color)
    {
        CellBackgroundPaletteColors[coord.X, coord.Y] = color;
    }

    public void SetCellForegroundColor(Vector2I coord, int color)
    {
        CellForegroundColorsSpan2D[coord.Y, coord.X] = color;
        cellForegroundColorsDirty = true;
    }

    public void SetCellForegroundColor(int x, int y, int color)
    {
        CellForegroundColorsSpan2D[y, x] = color;
        cellForegroundColorsDirty = true;
    }

    public void SetCellBackgroundColor(Vector2I coord, int color)
    {
        CellBackgroundColorsSpan2D[coord.X, coord.Y] = color;
        cellBackgroundColorsDirty = true;
    }

    public void SetCellBackgroundColor(int x, int y, int color)
    {
        CellBackgroundColorsSpan2D[y, x] = color;
        cellBackgroundColorsDirty = true;
    }
}
