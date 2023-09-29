extends EditorInspectorPlugin

const RandomIntEditor = preload("res://addons/ASCIITileMap/RandomIntEditor.gd")


func can_handle(object):
	# Only handles ASCIITileMaps.
	return object.get_class() == "ASCIITileMap"


func parse_begin(object):
	add_property_editor("_asciiTilesetTexture", RandomIntEditor.new())
	#add_property_editor("layers", RandomIntEditor.new())


func parse_category(object, category):
	pass
	#print(category)
	#if category == "TileMap":



func parse_property(object, type, path, hint, hint_text, usage):
	# We handle properties of type integer.
	print("Path3D: " + path)
	match path:
		"tile_set":
			add_property_editor(path, RandomIntEditor.new())
		"compatibility_mode":
			return true
		"centered_textures":
			return true
		"cell_clip_uv":
			return true
	return false
