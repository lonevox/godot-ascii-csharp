shader_type canvas_item;

uniform sampler2D tileset;
uniform uvec2 glyph_size;
uniform uint tile_ids[256];
uniform uint tile_fg_colors[256];
uniform uint tile_bg_colors[256];

const uvec2 tilemap_size = uvec2(16);

void fragment() {
	uvec2 tilemap_coord = uvec2(UV * vec2(tilemap_size));
	lowp uint tilemap_index = tilemap_coord.x + tilemap_coord.y * tilemap_size.y;
	// Get tile ID in tileset from tile_ids
	lowp uint tile_id = tile_ids[tilemap_index];
	// Get foreground and background colors
	uint fg = tile_fg_colors[tilemap_index];
	uint bg = tile_bg_colors[tilemap_index];
	// Get either black or white from the tileset
	// Choose the foreground color if white, background color if black
	uvec2 tileset_coord = uvec2(tile_id % 16u, uint(float(tile_id) / 16.));
	uvec2 position_within_glyph = uvec2(UV * vec2(tilemap_size) * vec2(glyph_size)) % glyph_size;
	vec4 tileset_color = texelFetch(tileset, ivec2(tileset_coord * glyph_size + position_within_glyph), 0);
	if (tileset_color == vec4(1.)) {
		COLOR = unpackUnorm4x8(fg);
	} else {
		COLOR = unpackUnorm4x8(bg);
	}
}
