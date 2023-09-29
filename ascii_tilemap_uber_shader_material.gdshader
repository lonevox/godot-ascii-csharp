shader_type canvas_item;

const lowp uvec2 tilemap_size = uvec2(16);

uniform lowp uint render_type = 3;	// 0: 1-bit, 1: Two color, 2: Palette, 3: Full color, 4: Gradient
uniform sampler2D tileset;
uniform lowp uvec2 glyph_size;
uniform lowp uint tileset_ids[256];
uniform vec4 fg_single_color = vec4(1, 0, 1, 1);
uniform vec4 bg_single_color = vec4(0, 0, 0, 1);
uniform sampler2D palette;
uniform lowp uint fg_palette_ids[256];
uniform lowp uint bg_palette_ids[256];
uniform uint fg_colors[256];
uniform uint bg_colors[256];

void fragment() {
	lowp uvec2 tilemap_coord = uvec2(UV * vec2(tilemap_size));
	lowp uint tilemap_index = tilemap_coord.x + tilemap_coord.y * tilemap_size.y;
	// Get tile ID in tileset from tile_ids
	lowp uint tileset_id = tileset_ids[tilemap_index];
	// Get either white or black from the tileset
	lowp uvec2 tileset_coord = uvec2(tileset_id % 16u, uint(float(tileset_id) / 16.));
	lowp uvec2 position_within_glyph = uvec2(UV * vec2(tilemap_size) * vec2(glyph_size)) % glyph_size;
	vec4 tileset_color = texelFetch(tileset, ivec2(tileset_coord * glyph_size + position_within_glyph), 0);
	
	switch (render_type) {
		// 1-bit (~0.3ms)
		case 0:
			COLOR = tileset_color;
			break;
		// Two color (~0.3ms)
		case 1:
			COLOR = mix(bg_single_color, fg_single_color, tileset_color.r);
			break;
		// Palette (~?ms)
		case 2:
			lowp uint fg_palette_index = fg_palette_ids[tilemap_index];
			lowp uint bg_palette_index = bg_palette_ids[tilemap_index];
			vec4 fg_palette_color = texelFetch(palette, ivec2(int(fg_palette_index), 0), 0);
			vec4 bg_palette_color = texelFetch(palette, ivec2(int(bg_palette_index), 0), 0);
			COLOR = mix(bg_palette_color, fg_palette_color, tileset_color.r);
			break;
		// Full color (~0.38ms)
		case 3:
			vec4 fg_color = unpackUnorm4x8(fg_colors[tilemap_index]);
			vec4 bg_color = unpackUnorm4x8(bg_colors[tilemap_index]);
			COLOR = mix(bg_color, fg_color, tileset_color.r);
			break;
	}
}
