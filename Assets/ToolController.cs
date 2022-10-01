using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class ToolController : MonoBehaviour
{
    public UIDocument uiDocument;
    public Tool currentTool = Tool.None;
    private readonly Dictionary<Tool, Button> _toolButtons = new();

    private void Start()
    {
        var root = uiDocument.rootVisualElement;

        _toolButtons.Add(Tool.None, root.Q<Button>("NoToolButton"));
        _toolButtons.Add(Tool.Dirt, root.Q<Button>("DirtButton"));
        _toolButtons.Add(Tool.Tarmac, root.Q<Button>("TarmacButton"));

        _toolButtons[Tool.None].RegisterCallback(OnSelectTool(Tool.None));
        _toolButtons[Tool.Dirt].RegisterCallback(OnSelectTool(Tool.Dirt));
        _toolButtons[Tool.Tarmac].RegisterCallback(OnSelectTool(Tool.Tarmac));
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