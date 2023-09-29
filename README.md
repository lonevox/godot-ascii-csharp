# godot-ascii

godot-ascii is an ASCII renderer backed by a Godot Tilemap node. Each glyph can have full-color foreground and background colors.

![image](https://github.com/lonevox/godot-ascii/assets/38600896/bec0d76b-5f4d-4661-bb7a-c83ca1a9c64f)


## Performance
Rendering is done with a single shader and in chunks of 16x16 glyphs, so draw calls can be batched allowing for very fast draw time. Setting glyphs in the map is optimised using C# spans, though rendering is so fast that modifying the map will likely be the bottleneck in a project. The biggest performance issue currently is sending the ASCII cell data from C# to Godot, which could be optimized in the future with this: https://github.com/godotengine/godot-proposals/issues/7842
