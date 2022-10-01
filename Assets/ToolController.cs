using UnityEngine;
using UnityEngine.UIElements;

public class ToolController : MonoBehaviour
{
    public UIDocument uiDocument;
    public Tool currentTool = Tool.None;
    
    private void Start()
    {
        var root = uiDocument.rootVisualElement;
        var button = root.Q<Button>("DirtButton");
        button.RegisterCallback(OnSelectTool(Tool.Dirt));
    }

    private EventCallback<ClickEvent> OnSelectTool(Tool tool)
    {
        return @event =>
        {
            Debug.Log(@event.target);
        };
    }
}