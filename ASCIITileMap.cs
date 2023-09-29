using CommunityToolkit.HighPerformance;
using Godot;
using System;
using System.Collections.Generic;

[Tool]
public partial class ASCIITileMap : TileMap
{
    private Texture2D _asciiTileSetTexture;
    [Export]
    public Texture2D AsciiTileSetTexture
    {
        get => _asciiTileSetTexture;
        set
        {
            _asciiTileSetTexture = value;
            if (_asciiTileSetTexture != null)
            {
                glyphSize = _asciiTileSetTexture.GetSize().ToVector2I() / 16;
                TileSet = GenerateTileset(_asciiTileSetTexture, glyphSize);
                // Set all child ASCIITileMapLayers TileSets to the one that was just generated.
                // Also set their MapSprite's shader params.
                foreach (var chunk in chunks.Values)
                {
                    chunk.SetAsciiTileSetTexture(_asciiTileSetTexture);
                    chunk.SetGlyphSize(glyphSize);
                }
            }
            else
            {
                // Return to defaults if no texture is supplied.
                TileSet = null;
                // Clear all chunk tileset textures.
                foreach (var chunk in chunks.Values)
                {
                    chunk.SetAsciiTileSetTexture(null);
                    chunk.SetGlyphSize(Vector2I.Zero);
                }
            }
        }
    }

    private Color _paintForegroundColor = Colors.White;
    [Export]
    public Color PaintForegroundColor
    {
        get => _paintForegroundColor;
        private set
        {
            _paintForegroundColor = value;
            if (_paintForegroundColor != null)
                paintForegroundColorAsInt = Util.ColorToInt(_paintForegroundColor);
            else
                paintForegroundColorAsInt = Util.ColorToInt(Colors.White);
        }
    }
    private int paintForegroundColorAsInt;

    private Color _paintBackgroundColor = Colors.Black;
    [Export]
    public Color PaintBackgroundColor
    {
        get => _paintBackgroundColor;
        private set
        {
            _paintBackgroundColor = value;
            if (_paintBackgroundColor != null)
                paintBackgroundColorAsInt = Util.ColorToInt(_paintBackgroundColor);
            else
                paintForegroundColorAsInt = Util.ColorToInt(Colors.Black);
        }
    }
    private int paintBackgroundColorAsInt;

    private Dictionary<Vector2I, Chunk> chunks = new Dictionary<Vector2I, Chunk>();

    public static readonly string[] GlyphNames = {
        "null", "white_smiling_face", "black_smiling_face", "black_heart_suit", "black_diamond_suit", "black_club_suit", "black_spade_suit", "bullet", "inverse_bullet", "white_circle", "inverse_white_circle", "male_sign", "female_sign", "eighth_note", "beamed_eighth_notes", "white_sun_with_rays",
        "black_right-pointing_pointer", "black_left-pointing_pointer", "up_down_arrow", "double_exclamation_mark", "pilcrow_sign", "section_sign", "black_rectangle", "leftwards_arrow_with_loop", "upwards_arrow", "downwards_arrow", "rightwards_arrow", "leftwards_arrow", "right_angle", "left_right_arrow", "black_up-pointing_triangle", "black_down-pointing_triangle",
        "space", "exclamation_mark", "quotation_mark", "hash", "dollar", "percent", "ampersand", "apostrophe", "open_bracket", "close_bracket", "asterisk", "plus", "comma", "dash", "full_stop", "slash",
        "zero", "one", "two", "three", "four", "five", "six", "seven", "eight", "nine", "colon", "semicolon", "less_than", "equals_sign", "greater_than", "question_mark",
        "at", "upper_case_A", "upper_case_B", "upper_case_C", "upper_case_D", "upper_case_E", "upper_case_F", "upper_case_G", "upper_case_H", "upper_case_I", "upper_case_J", "upper_case_K", "upper_case_L", "upper_case_M", "upper_case_N", "upper_case_O",
        "upper_case_P", "upper_case_Q", "upper_case_R", "upper_case_S", "upper_case_T", "upper_case_U", "upper_case_V", "upper_case_W", "upper_case_X", "upper_case_Y", "upper_case_Z", "open_square_bracket", "backslash", "close_square_bracket", "caret",
        "underscore", "grave_accent", "lower_case_a", "lower_case_b", "lower_case_c", "lower_case_d", "lower_case_e", "lower_case_f", "lower_case_g", "lower_case_h", "lower_case_i", "lower_case_j", "lower_case_k", "lower_case_l", "lower_case_m", "lower_case_n", "lower_case_o",
        "lower_case_p", "lower_case_q", "lower_case_r", "lower_case_s", "lower_case_t", "lower_case_u", "lower_case_v", "lower_case_w", "lower_case_x", "lower_case_y", "lower_case_z", "open_brace", "pipe", "close_brace", "tilde", "delete",
        "upper_case_C_with_cedilla", "lower_case_u_with_diaeresis", "lower_case_e_with_acute", "lower_case_a_with_circumflex", "lower_case_a_with_diaeresis", "lower_case_a_with_grave", "lower_case_a_with_ring_above", "lower_case_c_with_cedilla", "lower_case_e_with_circumflex", "lower_case_e_with_diaeresis", "lower_case_e_with_grave", "lower_case_i_with_diaeresis", "lower_case_i_with_circumflex", "lower_case_i_with_grave", "upper_case_A_with_diaeresis", "upper_case_A_with_ring_above",
        "upper_case_E_with_acute", "lower_case_ae", "upper_case_AE", "lower_case_o_with_circumflex", "lower_case_o_with_diaeresis", "lower_case_o_with_grave", "lower_case_u_with_circumflex", "lower_case_u_with_grave", "lower_case_y_with_diaeresis", "upper_case_O_with_diaeresis", "upper_case_U_with_diaeresis", "cent_sign", "pound_sign", "yen_sign", "paseta_sign", "lower_case_f_with_hook",
        "lower_case_a_with_acute", "lower_case_i_with_acute", "lower_case_o_with_acute", "lower_case_u_with_acute", "lower_case_n_with_tilde", "upper_case_N_with_tilde", "feminine_ordinal_indicator", "masculine_ordinal_indicator", "inverted_question_mark", "reversed_not_sign", "not_sign", "vulgar_fraction_one_half", "vulgar_fraction_one_quarter", "inverted_exclamation_mark", "left-pointing_double_angle_quotation_mark", "right-pointing_double_angle_quotation_mark",
        "light_shade", "medium_shade", "dark_shade", "box_drawings_light_vertical", "box_drawings_light_vertical_and_left", "box_drawings_vertical_single_and_left_double", "box_drawings_vertical_double_and_left_single", "box_drawings_down_double_and_left_single", "box_drawings_down_single_and_left_double", "box_drawings_double_vertical_and_left", "box_drawings_double_vertical", "box_drawings_double_down_and_left", "box_drawings_double_up_and_left", "box_drawings_up_double_and_left_single", "box_drawings_up_single_and_left_double", "box_drawings_light_down_and_left",
        "box_drawings_light_up_and_right", "box_drawings_light_up_and_horizontal", "box_drawings_light_down_and_horizontal", "box_drawings_light_vertical_and_right", "box_drawings_light_horizontal", "box_drawings_light_vertoical_and_horizontal", "box_drawings_vertical_single_and_right_double", "box_drawings_vertical_double_and_right_single", "box_drawings_double_up_and_right", "box_drawings_double_down_and_right", "box_drawings_double_up_and_horizontal", "box_drawings_double_down_and_horizontal", "box_drawings_double_vertical_and_right", "box_drawings_double_horizontal", "box_drawings_double_vertical_and_horizontal", "box_drawings_up_single_and_horizontal_double",
        "box_drawings_up_double_and_horizontal_single", "box_drawings_down_single_and_horizontal_double", "box_drawings_down_double_and_horizontal_single", "box_drawings_up_double_and_right_single", "box_drawings_up_single_and_right_double", "box_drawings_down_single_and_right_double", "box_drawings_down_double_and_right_single", "box_drawings_vertical_double_and_horizontal_single", "box_drawings_vertical_single_and_horizontal_double", "box_drawings_light_up_and_left", "box_drawings_light_down_and_right", "full_block", "lower_half_block", "left_half_block", "right_half_block", "upper_half_block",
        "greek_lower_case_alpha", "lower_case_sharp_s", "greek_upper_case_letter_gamma", "greek_lower_case_pi", "greek_upper_case_sigma", "greek_lower_case_sigma", "micro_sign", "greek_lower_case_tau", "greek_upper_case_letter_phi", "greek_upper_case_letter_theta", "greek_upper_case_letter_omega", "greek_lower_case_delta", "infinity", "greek_lower_case_phi", "greek_lower_case_epsilon", "intersection",
        "identical_to", "plus-minus_sign", "greater-than_or_equal_to", "less-than_or_equal_to", "top_half_integral", "bottom_half_integral", "division_sign", "almost_equal_to", "degree_sign", "bullet_operator", "middle_dot", "square_root", "subscript_lower_case_n", "subscript_two", "black_square", "no-break_space"
    };

    public Vector2I glyphSize;

    private bool drawn = false;
    private RandomNumberGenerator random = new RandomNumberGenerator();

    public ASCIITileMap()
    {
        TextureFilter = TextureFilterEnum.Nearest;
    }

    public override void _Ready()
    {
        base._Ready();
        Clear();
        // testing
        for (int col = 0; col < 16; col++)
        {
            for (int row = 0; row < 16; row++)
            {
                var coords = new Vector2I(col, row);
                AddChunk(coords, Chunk.DummyChunk());
            }
        }
        // var random = new RandomNumberGenerator();
        // for (int i = 0; i < 1000; i++)
        // {
        //     SetCell(0, new Vector2I(random.RandiRange(0, 100), random.RandiRange(0, 100)), (byte)random.RandiRange(0, byte.MaxValue), foregroundColor: Util.ColorToInt(Colors.White), backgroundColor: Util.ColorToInt(Colors.Black));
        // }
    }

    public override void _Process(double delta)
    {
        // testing
        if (!Engine.IsEditorHint())
        {
            // random test
            // for (int i = 0; i < 5000; i++)
            // {
            //     var position = new Vector2I(random.RandiRange(0, 160), random.RandiRange(0, 100));
            //     SetCell(0, position, characterId: random.RandiRange(0, byte.MaxValue), foregroundColor: Util.ColorToInt(Color.FromHsv(random.Randf(), 1, 1, random.Randf())), backgroundColor: Util.ColorToInt(Color.FromHsv(random.Randf(), 1, 1, random.Randf())));
            // }

            // read + write tests
            foreach (var item in chunks)
            {
                var chunkCoord = item.Key;
                var chunk = item.Value;

                var fgColors = new int[256];
                var fgColors2d = new Memory2D<int>(fgColors, Chunk.SIZE, Chunk.SIZE).Span;
                var bgColors = new int[256];
                var bgColors2d = new Memory2D<int>(bgColors, Chunk.SIZE, Chunk.SIZE).Span;

                for (byte col = 0; col < Chunk.SIZE; col++)
                {
                    for (byte row = 0; row < Chunk.SIZE; row++)
                    {
                        var fgColor = Util.ColorFromInt(chunk.CellForegroundColorsSpan2D[col, row]);
                        fgColor.H += 0.01f;
                        var bgColor = Util.ColorFromInt(chunk.CellBackgroundColorsSpan2D[col, row]);
                        bgColor.H += 0.01f;

                        var fgColorInt = Util.ColorToInt(fgColor);
                        var bgColorInt = Util.ColorToInt(bgColor);

                        // tilemap set cell (~57ms)
                        //SetCell(0, chunkCoord * Chunk.SIZE + new Vector2I(col, row), foregroundColor: fgColorInt, backgroundColor: bgColorInt);

                        // chunk set cell (~43ms)
                        //chunk.SetCell(col, row, fgColor: fgColorInt, bgColor: bgColorInt);
                        //QueueRedraw();

                        // chunk setting array (~33ms)
                        //fgColors2d[col, row] = fgColorInt;
                        //bgColors2d[col, row] = bgColorInt;

                        // chunk set cell individual (~42ms)
                        chunk.SetCellForegroundColor(col, row, fgColorInt);
                        chunk.SetCellBackgroundColor(col, row, bgColorInt);
                        //QueueRedraw();
                    }
                }
                // chunk setting array cont.
                //chunk.CellForegroundColors = fgColors;
                //chunk.CellBackgroundColors = bgColors;
                QueueRedraw();
            }

            // small redraw test

        }
    }

    public override void _Draw()
    {
        foreach (var entry in chunks)
        {
            var chunk = entry.Value;
            chunk.SendDirtyArrays();
        }
    }

    private void AddChunk(Vector2I coord, Chunk chunk)
    {
        chunk.SetAsciiTileSetTexture(AsciiTileSetTexture);
        chunk.SetGlyphSize(glyphSize);
        var size = glyphSize * Chunk.SIZE;
        var position = coord * size;
        var rect = new Rect2(position, size);
        chunk.DrawTo(GetCanvasItem(), rect);
        chunks[coord] = chunk;
    }

    // public override void _Input(InputEvent @event)
    // {
    //     base._Input(@event);
    //     if (@event is InputEventMouseButton eventMouse)
    //     {
    //         GD.Print(Name);
    //         if (eventMouse.Pressed && eventMouse.ButtonIndex == (int)ButtonList.Left)
    //         {
    //             GD.Print(Name);
    //         }
    //     }
    // }

    /// <summary>Returns the string 'ASCIITileMap'. Overrides the default get_class method.</summary>
    public string get_class()
    {
        return "ASCIITileMap";
    }

    public void SetCell(int layer, Vector2I coords, int characterId = -1, int foregroundPaletteColor = -1, int backgroundPaletteColor = -1, int foregroundColor = -1, int backgroundColor = -1)
    {
        var chunkCoords = coords / Chunk.SIZE;
        if (!chunks.ContainsKey(chunkCoords))
        {
            AddChunk(chunkCoords, new Chunk());
        }
        var chunk = chunks[chunkCoords];
        chunk.SetCell(coords % Chunk.SIZE, characterId, foregroundPaletteColor, backgroundPaletteColor, foregroundColor, backgroundColor);
        QueueRedraw();
    }

    public new void Clear()
    {
        chunks.Clear();
    }

    /// <summary>Needed because this is a tool script. This can get the mapSprite node before _Ready() is called.</summary>
    //private void MapSpriteFix() { if (mapSprite == null) mapSprite = GetNode<Sprite>("MapSprite"); }

    // public void SetAsciiTexture(Texture2D texture)
    // {
    //     _asciiTileSetTexture = texture;
    //     if (_asciiTileSetTexture != null)
    //     {
    //         glyphSize = new Vector2I((int)_asciiTileSetTexture.GetSize().X, (int)_asciiTileSetTexture.GetSize().Y) / 16;
    //         TileSet = GenerateTileset(_asciiTileSetTexture, glyphSize);
    //         // Set all child ASCIITileMapLayers TileSets to the one that was just generated.
    //         // Also set their MapSprite's shader params.
    //         asciiShaderMaterial.SetShaderParameter("tileset", _asciiTileSetTexture);
    //         asciiShaderMaterial.SetShaderParameter("glyph_size", glyphSize);
    //         //SetAllMapSpritesShaderParam("tileset", asciiTilesetTexture);
    //         //SetAllMapSpritesShaderParam("glyph_size", glyphSize);
    //     }
    //     else
    //     {
    //         // Return to defaults if no texture is supplied.
    //         TileSet = null;
    //         // Clear all MapSprite textures.
    //         foreach (ASCIITileMapSprite mapSprite in mapSprites.Values)
    //         {
    //             mapSprite.Texture = null;
    //         }
    //     }
    // }

    /// <summary>Generates a TileSet from a CP437 texture and a glyph size. All tiles are named based on <see cref="GlyphNames"></see></summary>
    public TileSet GenerateTileset(Texture2D texture, Vector2I glyphSize)
    {
        TileSet tileSet = new TileSet();
        tileSet.TileSize = glyphSize;
        var source = new TileSetAtlasSource();
        source.Texture = texture;
        source.TextureRegionSize = glyphSize;
        for (int i = 0; i < 256; i++)
        {
            var tileTextureCoord = new Vector2I(i % 16, i / 16);
            source.CreateTile(tileTextureCoord);
        }
        tileSet.AddSource(source);
        return tileSet;
    }

    // private void CreateUniformArrays(ShaderMaterial shaderMaterial)
    // {
    //     var tileIds = new int[256];
    //     var tileFgColors = new int[256];
    //     var tileBgColors = new int[256];
    //     //var usedRect = GetUsedRect();
    //     var usedRect = new Rect2I(0, 0, 16, 16);
    //     for (int y = 0; y < usedRect.Size.Y; y++)
    //     {
    //         for (int x = 0; x < usedRect.Size.X; x++)
    //         {
    //             int i = x + y * 16;
    //             var coords = new Vector2I(x, y);
    //             var cell = GetCellAtlasCoords(0, coords);
    //             //GD.Print(cell);

    //             // If cell is empty, add empty data to the arrays.
    //             if (cell == new Vector2I(-1, -1))
    //             {
    //                 tileIds[i] = 0;
    //                 tileFgColors[i] = DEFAULT_FOREGROUND_COLOR;
    //                 tileBgColors[i] = DEFAULT_BACKGROUND_COLOR;
    //                 continue;
    //             }

    //             // Add cell ID to tile IDs.
    //             GD.Print(cell);
    //             tileIds[i] = cell.X + cell.Y * 16;
    //             //tileIds[i] = i;

    //             // Foreground color.
    //             if (cellForegroundColors.ContainsKey(coords))
    //             {
    //                 GD.Print(cellForegroundColors[coords]);
    //             }
    //             var fgColor = cellForegroundColors.ContainsKey(coords) ? cellForegroundColors[coords] : defaultForegroundColor;
    //             tileFgColors[i] = ColorToInt(fgColor);

    //             // Background color.
    //             var bgColor = cellBackgroundColors.ContainsKey(coords) ? cellBackgroundColors[coords] : defaultBackgroundColor;
    //             tileBgColors[i] = ColorToInt(bgColor);
    //         }
    //     }
    //     GD.Print(tileFgColors[1]);
    //     shaderMaterial.SetShaderParameter("tile_ids", tileIds);
    //     shaderMaterial.SetShaderParameter("tile_fg_colors", tileFgColors);
    //     shaderMaterial.SetShaderParameter("tile_bg_colors", tileBgColors);
    // }

    // private void CreateMapSprite(Vector2I coord)
    // {
    //     ASCIITileMapSprite mapSprite = mapSpriteScene.Instantiate() as ASCIITileMapSprite;
    //     mapSprite.coord = coord;
    //     // Give the map sprite the ASCII tileset texture.
    //     (mapSprite.Material as ShaderMaterial).SetShaderParameter("tileset", asciiTilesetTexture);
    //     // Create images used within the tilemap shader.
    //     var mapSpriteRect = new Rect2I(coord * MapSpriteSize, MapSpriteSize);
    //     var imageRect = GetUsedRect().Intersection(mapSpriteRect);
    //     imageRect.Position %= MapSpriteSize;
    //     //mapSprite.CreateTextures(this);
    //     // Move mapSprite.
    //     mapSprite.Position = glyphSize * coord + imageRect.Position;
    //     // Add mapSprite to list of sprites.
    //     mapSprites.Add(coord, mapSprite);
    // }
}
