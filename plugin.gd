@tool
extends EditorPlugin

var gui: Control

var inspector_plugin

var _editor_selection: EditorSelection


func _enter_tree():
	name = "ASCIITileMapPlugin"

	# Get editor selection and connect its "selection_changed" signal.
	_editor_selection = get_editor_interface().get_selection()
	_editor_selection.connect("selection_changed", Callable(self, "_on_selection_changed"))

	# Add ASCIITileMap node type.
	add_custom_type("ASCIITileMap", "TileMap", preload("res://addons/ASCIITileMap/ASCIITileMap.cs"), preload("res://addons/ASCIITileMap/ASCIITileSetIcon.png"))
	
	# Editor GUI
	#gui = preload("res://addons/ASCIITileMap/ASCIITileMapGUI.tscn").instance()
	#add_control_to_container(EditorPlugin.CONTAINER_CANVAS_EDITOR_SIDE_LEFT, gui)

	# Inspector GUI
	inspector_plugin = preload("res://addons/ASCIITileMap/inspector_plugin.gd").new()
	add_inspector_plugin(inspector_plugin)


func _exit_tree():
	remove_custom_type("ASCIITileMap")
	#remove_control_from_container(EditorPlugin.CONTAINER_CANVAS_EDITOR_SIDE_LEFT, gui)
	#gui.queue_free()
	remove_inspector_plugin(inspector_plugin)


func _get_plugin_name():
	return "ASCIITileMap"


func _on_selection_changed():
	# If an ASCIITileMap has editor focus, then set mapWithEditorFocus in globals to that map.
	var selected_nodes = _editor_selection.get_selected_nodes()
	if selected_nodes.size() == 1 and selected_nodes[0].get_class() == "ASCIITileMap":
		#ascii_tile_map_globals.mapWithEditorFocus = selected_nodes[0]
		pass
	else:
		#ascii_tile_map_globals.mapWithEditorFocus = null
		pass
