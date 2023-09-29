using Godot;

[Tool]
public partial class ASCIITileMapPlugin : EditorPlugin
{
    EditorSelection editorSelection;

    public override void _EnterTree()
    {
        base._EnterTree();

        Name = "ASCIITileMapPlugin";

        // Get editor selection and connect its "selection_changed" signal.
        editorSelection = GetEditorInterface().GetSelection();
        Connect("selection_changed", new Callable(this, nameof(_OnSelectionChanged)));

        // Add ASCIITileMap node type.
        AddCustomType("ASCIITileMap",
                    "TileMap",
                    ResourceLoader.Load<Script>("res://addons/ASCIITileMap/ASCIITileMap.cs"),
                    ResourceLoader.Load<Texture2D>("res://addons/ASCIITileMap/ASCIITileSetIcon.png"));
    }

    public override void _ExitTree()
    {
        base._ExitTree();

        RemoveCustomType("ASCIITileMap");
    }

    public override string _GetPluginName()
    {
        return "ASCIITileMap";
    }

    private void _OnSelectionChanged()
    {
        Godot.Collections.Array<Godot.Node> selectedNodes = editorSelection.GetSelectedNodes();
        GD.Print("yoooooo");
        if (selectedNodes.Count == 1 && ((Node)selectedNodes[0]).GetClass() == "ASCIITileMap")
        {
        }
    }
}
