shader_type canvas_item;

const lowp uvec2 tilemap_size = uvec2(16);

uniform sampler2D tileset;
uniform lowp uvec2 glyph_size;
uniform lowp uint tile_ids[256];
uniform uint fg_color;
uniform uint bg_color;

void fragment() {
	lowp uvec2 tilemap_coord = uvec2(UV * vec2(tilemap_size));
	lowp uint tilemap_index = tilemap_coord.x + tilemap_coord.y * tilemap_size.y;
	// Get tile ID in tileset from tile_ids
	lowp uint tile_id = tile_ids[tilemap_index];
	// Get either white or black from the tileset
	lowp uvec2 tileset_coord = uvec2(tile_id % 16u, uint(float(tile_id) / 16.));
	lowp uvec2 position_within_glyph = uvec2(UV * vec2(tilemap_size) * vec2(glyph_size)) % glyph_size;
	vec4 tileset_color = texelFetch(tileset, ivec2(tileset_coord * glyph_size + position_within_glyph), 0);
	// Draw either the foreground color or background color based on tileset_color
	vec4 fg_color_unpacked = unpackUnorm4x8(fg_color);
	vec4 bg_color_unpacked = unpackUnorm4x8(bg_color);
	COLOR = mix(fg_color_unpacked, bg_color_unpacked, tileset_color.r);
}
