using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UIElements;

public class ToolController : MonoBehaviour
{
    public UIDocument uiDocument;
    public Tool currentTool = Tool.None;
    public readonly Dictionary<Tool, Tile> TileTypes = new();
    public Tile dirtTile;
    public Tile tarmacTile;
    public Tile runwayTile;
    public Tile taxiwayTile;
    public Tile gateTile;

    private readonly Dictionary<Tool, Button> _toolButtons = new();

    private void Start()
    {
        TileTypes.Add(Tool.Dirt, dirtTile);
        TileTypes.Add(Tool.Tarmac, tarmacTile);
        TileTypes.Add(Tool.Runway, runwayTile);
        TileTypes.Add(Tool.Taxiway, taxiwayTile);
        TileTypes.Add(Tool.Gate, gateTile);

        var root = uiDocument.rootVisualElement;

        _toolButtons.Add(Tool.None, root.Q<Button>("NoToolButton"));
        _toolButtons.Add(Tool.Dirt, root.Q<Button>("DirtButton"));
        _toolButtons.Add(Tool.Tarmac, root.Q<Button>("TarmacButton"));
        _toolButtons.Add(Tool.Runway, root.Q<Button>("RunwayButton"));
        _toolButtons.Add(Tool.Taxiway, root.Q<Button>("TaxiwayButton"));
        _toolButtons.Add(Tool.Gate, root.Q<Button>("GateButton"));

        _toolButtons[Tool.None].RegisterCallback(OnSelectTool(Tool.None));
        _toolButtons[Tool.Dirt].RegisterCallback(OnSelectTool(Tool.Dirt));
        _toolButtons[Tool.Tarmac].RegisterCallback(OnSelectTool(Tool.Tarmac));
        _toolButtons[Tool.Runway].RegisterCallback(OnSelectTool(Tool.Runway));
        _toolButtons[Tool.Taxiway].RegisterCallback(OnSelectTool(Tool.Taxiway));
        _toolButtons[Tool.Gate].RegisterCallback(OnSelectTool(Tool.Gate));
    }

    private EventCallback<ClickEvent> OnSelectTool(Tool selectedTool)
    {
        const string activeClassName = "active";
        return _ =>
        {
            currentTool = selectedTool;
            foreach (var (tool, button) in _toolButtons)
            {
                if (tool == currentTool)
                {
                    button.AddToClassList(activeClassName);
                }
                else
                {
                    button.RemoveFromClassList(activeClassName);
                }
            }
        };
    }
}