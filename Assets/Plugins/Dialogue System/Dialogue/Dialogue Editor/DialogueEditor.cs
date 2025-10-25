using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;

public class DialogueEditor : EditorWindow
{
    #region Attributes
    Dialogue selectedDialogue = null;
    [NonSerialized] DialogueNode creatingNode = null;
    [NonSerialized] DialogueNode deletingNode = null;
    [NonSerialized] DialogueNode linkingParentNode = null;

    [NonSerialized] GUIStyle nodeStyle, selectedNodeStyle;
    [NonSerialized] GUIStyle triggerActionsNodeStyle, selectedTriggerActionsNodeStyle;
    [NonSerialized] GUIStyle rootNodeStyle, selectedRootNodeStyle;
    [NonSerialized] GUIStyle scaledNodeStyle, scaledSelectedNodeStyle;
    [NonSerialized] GUIStyle scaledTriggerActionsNodeStyle, scaledSelectedTriggerActionsNodeStyle;
    [NonSerialized] GUIStyle scaledRootNodeStyle, scaledSelectedRootNodeStyle;
    [NonSerialized] GUIStyle scaledTextFieldStyle, scaledButtonStyle;

    [NonSerialized] Texture2D backgroundTex;

    private List<DialogueNode> reusableChildrenList = new List<DialogueNode>();


    [NonSerialized] Dictionary<DialogueNode, Vector2> dragNodeOffsets;

    [NonSerialized] Vector2 marqueeStartPosition;
    private Vector2 scrollPosition;

    [NonSerialized] bool isDraggingSelection = false;
    [NonSerialized] bool isMarqueeSelecting = false;
    [NonSerialized] bool hasDragged = false;

    const float canvasSize = 4000;
    public const float backgroundSize = 50;
    public const float verticalSpacing = 100f;
    public const float horizontalSpacing = 250f;
    private float zoomScale = 1f;
    private float previousZoomScale = -1f;
    #endregion

    #region Editor Methods
    [MenuItem("Dialogue/Dialogue Editor")]
    public static DialogueEditor ShowEditorWindow()
    {
        return ShowEditorWindow(null);
    }

    public static DialogueEditor ShowEditorWindow(Dialogue dialogueToEdit)
    {
        DialogueEditor window = GetWindow<DialogueEditor>(false, "Dialogue Editor");

        if(dialogueToEdit != null)
        {
            window.selectedDialogue = dialogueToEdit;
            window.Repaint();
        }

        return window;
    }

    [OnOpenAsset(1)]
    public static bool OnOpenAsset(int instanceID, int line)
    {
        Dialogue dialogue = EditorUtility.InstanceIDToObject(instanceID) as Dialogue;

        if(dialogue != null)
        {
            ShowEditorWindow(dialogue);

            return true;
        }

        return false;
    }

    //Layouts the DialogueNodes and styles them.
    private void OnEnable()
    {
        Selection.selectionChanged += OnSelectionChanged;

        backgroundTex = Resources.Load("DialogueEditorBackground") as Texture2D;

        nodeStyle = new GUIStyle();
        nodeStyle.normal.background = EditorGUIUtility.Load("node0") as Texture2D;
        nodeStyle.normal.textColor = Color.white;
        nodeStyle.padding = new RectOffset(20, 20, 20, 20);
        nodeStyle.border = new RectOffset(12, 12, 12, 12);
        nodeStyle.fontSize = 12;
        selectedNodeStyle = new GUIStyle(nodeStyle);
        selectedNodeStyle.normal.background = EditorGUIUtility.Load("node0 on") as Texture2D;

        triggerActionsNodeStyle = new GUIStyle(nodeStyle);
        triggerActionsNodeStyle.normal.background = EditorGUIUtility.Load("node4") as Texture2D;
        selectedTriggerActionsNodeStyle = new GUIStyle(nodeStyle);
        selectedTriggerActionsNodeStyle.normal.background = EditorGUIUtility.Load("node4 on") as Texture2D;

        rootNodeStyle = new GUIStyle(nodeStyle);
        rootNodeStyle.normal.background = EditorGUIUtility.Load("node5") as Texture2D;
        selectedRootNodeStyle = new GUIStyle(nodeStyle);
        selectedRootNodeStyle.normal.background = EditorGUIUtility.Load("node5 on") as Texture2D;

        scaledNodeStyle = new GUIStyle(nodeStyle);
        scaledSelectedNodeStyle = new GUIStyle(selectedNodeStyle);
        scaledTriggerActionsNodeStyle = new GUIStyle(triggerActionsNodeStyle);
        scaledSelectedTriggerActionsNodeStyle = new GUIStyle(selectedTriggerActionsNodeStyle);
        scaledRootNodeStyle = new GUIStyle(rootNodeStyle);
        scaledSelectedRootNodeStyle = new GUIStyle(selectedRootNodeStyle);
        scaledTextFieldStyle = new GUIStyle(EditorStyles.textField);
        scaledButtonStyle = new GUIStyle(EditorStyles.miniButton);
    }

    //Redraws the UI.
    private void OnSelectionChanged()
    {
        Dialogue newDialogue = Selection.activeObject as Dialogue;

        if(newDialogue)
        {
            selectedDialogue = newDialogue;

            Repaint();
        }
    }

    //Draws the UI of the Editor.
    private void OnGUI()
    {
        if(selectedDialogue == null)
        {
            EditorGUILayout.LabelField("No Dialogue Selected", EditorStyles.centeredGreyMiniLabel);

            return;
        }

        if(zoomScale != previousZoomScale)
        {
            RecalculateScaledStyles();

            previousZoomScale = zoomScale;
        }

        ProcessEvents();

        if(backgroundTex != null)
        {
            float scaledBackgroundSize = backgroundSize * zoomScale;

            Rect texCoords = new Rect(
                scrollPosition.x / scaledBackgroundSize,
                -scrollPosition.y / scaledBackgroundSize,
                position.width / scaledBackgroundSize,
                position.height / scaledBackgroundSize);

            GUI.DrawTextureWithTexCoords(new Rect(0, 0, position.width, position.height), backgroundTex, texCoords);
        }

        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

        Rect canvas = GUILayoutUtility.GetRect(canvasSize, canvasSize);

        IEnumerable<DialogueNode> allNodes = selectedDialogue.GetAllNodes();

        foreach(DialogueNode node in allNodes) { DrawConnections(node, zoomScale); }
        foreach(DialogueNode node in allNodes) { DrawNode(node, zoomScale); }

        EditorGUILayout.EndScrollView();

        if(isMarqueeSelecting)
        {
            Rect marqueeRect = new Rect(
                (marqueeStartPosition.x * zoomScale) - scrollPosition.x,
                (marqueeStartPosition.y * zoomScale) - scrollPosition.y,
                (Event.current.mousePosition.x - (marqueeStartPosition.x * zoomScale) + scrollPosition.x),
                (Event.current.mousePosition.y - (marqueeStartPosition.y * zoomScale) + scrollPosition.y));

            GUI.Box(marqueeRect, "", "SelectionRect");
        }

        if(creatingNode)
        {
            selectedDialogue.CreateNode(creatingNode);

            creatingNode = null;
        }

        if(deletingNode)
        {
            selectedDialogue.DeleteNode(deletingNode);

            deletingNode = null;
        }

        Rect buttonRect = new Rect(position.width - 160f, position.height - 40f, 150f, 30f);

        if(GUI.Button(buttonRect, "Create Root Node"))
        {
            selectedDialogue.CreateRootNode();
        }
    }

    //Allows the nodes to be dragged.
    private void ProcessEvents()
    {
        if(Event.current.type == EventType.ScrollWheel && Event.current.control)
        {
            zoomScale -= Event.current.delta.y * 0.01f;
            zoomScale = Mathf.Clamp(zoomScale, 0.5f, 1.0f);

            GUI.changed = true;
            Event.current.Use();
        }

        Vector2 mousePositionInCanvas = (Event.current.mousePosition + scrollPosition) / zoomScale;

        if(Event.current.type == EventType.MouseDown && Event.current.button == 0)
        {
            hasDragged = false;

            DialogueNode nodeUnderMouse = GetNodeAtPoint(mousePositionInCanvas);

            if(nodeUnderMouse != null)
            {
                if(Event.current.shift)
                {
                    var selection = new List<UnityEngine.Object>(Selection.objects);

                    if(selection.Contains(nodeUnderMouse))
                    {
                        selection.Remove(nodeUnderMouse);
                    }
                    else
                    {
                        selection.Add(nodeUnderMouse);

                    }
                    Selection.objects = selection.ToArray();
                }
                else
                {
                    if(!IsNodeSelected(nodeUnderMouse))
                    {
                        Selection.objects = new UnityEngine.Object[] { nodeUnderMouse };
                    }
                }

                isDraggingSelection = true;

                dragNodeOffsets = new Dictionary<DialogueNode, Vector2>();

                foreach(var obj in Selection.objects)
                {
                    if(obj is DialogueNode node)
                    {
                        dragNodeOffsets[node] = node.GetRect().position - mousePositionInCanvas;
                    }
                }
            }
            else
            {
                Rect buttonRect = new Rect(position.width - 160f, position.height - 40f, 150f, 30f);

                if(buttonRect.Contains(Event.current.mousePosition)) { }
                else
                {
                    isMarqueeSelecting = true;

                    marqueeStartPosition = mousePositionInCanvas;

                    Selection.objects = new UnityEngine.Object[0];
                }
            }
        }
        else if(Event.current.type == EventType.MouseDrag)
        {
            hasDragged = true;

            if(isDraggingSelection)
            {
                foreach(var obj in Selection.objects)
                {
                    if(obj is DialogueNode node && dragNodeOffsets.ContainsKey(node))
                    {
                        Vector2 newPosition = mousePositionInCanvas + dragNodeOffsets[node];

                        float snappedX = Mathf.Round(newPosition.x / backgroundSize) * backgroundSize;
                        float snappedY = Mathf.Round(newPosition.y / backgroundSize) * backgroundSize;

                        node.SetPosition(new Vector2(snappedX, snappedY));
                    }
                }

                GUI.changed = true;
                Event.current.Use();
            }
            else if(isMarqueeSelecting)
            {
                Repaint();

                Event.current.Use();
            }
        }
        else if(Event.current.type == EventType.MouseUp && Event.current.button == 0)
        {
            if(isMarqueeSelecting)
            {
                Rect marqueeRect = new Rect(marqueeStartPosition, mousePositionInCanvas - marqueeStartPosition);

                var newSelection = new List<UnityEngine.Object>();

                foreach(DialogueNode node in selectedDialogue.GetAllNodes())
                {
                    if(marqueeRect.Overlaps(node.GetRect(), true))
                    {
                        newSelection.Add(node);
                    }
                }

                Selection.objects = newSelection.ToArray();
            }

            if(hasDragged || isMarqueeSelecting) Event.current.Use();

            isDraggingSelection = false;
            isMarqueeSelecting = false;
            hasDragged = false;
        }
    }
    #endregion

    #region Normal Methods
    private bool IsNodeSelected(DialogueNode node)
    {
        return Selection.objects.Contains(node);
    }

    private void DrawNode(DialogueNode node, float zoom)
    {
        GUIStyle finalScaledStyle;

        if(node.GetTriggerActions().Count > 0)
        {
            finalScaledStyle = (IsNodeSelected(node)) ? scaledSelectedTriggerActionsNodeStyle : scaledTriggerActionsNodeStyle;
        }
        else if(node.IsRootNode())
        {
            finalScaledStyle = (IsNodeSelected(node)) ? scaledSelectedRootNodeStyle : scaledRootNodeStyle;
        }
        else
        {
            finalScaledStyle = (IsNodeSelected(node)) ? scaledSelectedNodeStyle : scaledNodeStyle;
        }

        Rect scaledRect = new Rect(node.GetRect().position * zoom, node.GetRect().size * zoom);

        GUILayout.BeginArea(scaledRect, finalScaledStyle);

        node.SetText(EditorGUILayout.TextField(node.GetDialogueText(), scaledTextFieldStyle));

        GUILayout.BeginHorizontal();

        if(GUILayout.Button("Add", scaledButtonStyle))
        {
            creatingNode = node;
        }

        DrawLinkButtons(node, scaledButtonStyle);

        if(GUILayout.Button("Delete", scaledButtonStyle))
        {
            deletingNode = node;
        }

        GUILayout.EndHorizontal();
        GUILayout.EndArea();
    }

    private void RecalculateScaledStyles()
    {
        ApplyScalingToStyle(scaledNodeStyle, nodeStyle, zoomScale);
        ApplyScalingToStyle(scaledSelectedNodeStyle, selectedNodeStyle, zoomScale);
        ApplyScalingToStyle(scaledTriggerActionsNodeStyle, triggerActionsNodeStyle, zoomScale);
        ApplyScalingToStyle(scaledSelectedTriggerActionsNodeStyle, selectedTriggerActionsNodeStyle, zoomScale);
        ApplyScalingToStyle(scaledRootNodeStyle, rootNodeStyle, zoomScale);
        ApplyScalingToStyle(scaledSelectedRootNodeStyle, selectedRootNodeStyle, zoomScale);
        ApplyScalingToStyle(scaledTextFieldStyle, EditorStyles.textField, zoomScale);
        ApplyScalingToStyle(scaledButtonStyle, EditorStyles.miniButton, zoomScale);
    }

    private void ApplyScalingToStyle(GUIStyle styleToScale, GUIStyle baseStyle, float zoom)
    {
        styleToScale.fontSize = Mathf.Max(1, (int)(baseStyle.fontSize * zoom));

        RectOffset padding = styleToScale.padding;
        padding.left = (int)(baseStyle.padding.left * zoom);
        padding.right = (int)(baseStyle.padding.right * zoom);
        padding.top = (int)(baseStyle.padding.top * zoom);
        padding.bottom = (int)(baseStyle.padding.bottom * zoom);

        RectOffset border = styleToScale.border;
        border.left = (int)(baseStyle.border.left * zoom);
        border.right = (int)(baseStyle.border.right * zoom);
        border.top = (int)(baseStyle.border.top * zoom);
        border.bottom = (int)(baseStyle.border.bottom * zoom);
    }

    //Linking and unlinking nodes.
    private void DrawLinkButtons(DialogueNode node, GUIStyle buttonStyle)
    {
        if(!linkingParentNode)
        {
            if(GUILayout.Button("Link", buttonStyle))
            {
                linkingParentNode = node;
            }
        }
        else if(linkingParentNode == node)
        {
            if(GUILayout.Button("Cancel", buttonStyle))
            {
                linkingParentNode = null;
            }
        }
        else if(linkingParentNode.GetChildren().Contains(node.name))
        {
            if(GUILayout.Button("Unlink", buttonStyle))
            {
                linkingParentNode.RemoveChild(node.name);

                linkingParentNode = null;
            }
        }
        else
        {
            if(GUILayout.Button("Child", buttonStyle))
            {
                linkingParentNode.AddChild(node.name);

                linkingParentNode = null;
            }
        }
    }

    //Draws the connections between nodes with Bezier curves.
    private void DrawConnections(DialogueNode node, float zoom)
    {
        Vector3 startPosition = new Vector2(node.GetRect().center.x, node.GetRect().center.y) * zoom;

        selectedDialogue.GetChildren(node, reusableChildrenList);

        foreach(DialogueNode childNode in reusableChildrenList)
        {
            Vector3 endPosition = new Vector2(childNode.GetRect().center.x, childNode.GetRect().center.y) * zoom;
            Vector3 controlPointOffset = endPosition - startPosition;

            controlPointOffset.y = 0f;
            controlPointOffset.x *= 0.8f;

            Handles.DrawBezier(startPosition, endPosition, startPosition + controlPointOffset, endPosition - controlPointOffset, IsNodeSelected(node) ? Color.white : Color.gray, null, 4f);
        }
    }

    private DialogueNode GetNodeAtPoint(Vector2 point)
    {
        DialogueNode foundNode = null;

        List<DialogueNode> allNodes = selectedDialogue.GetAllNodes() as List<DialogueNode>;

        if(allNodes != null)
        {
            for(int i = allNodes.Count - 1; i >= 0; i--)
            {
                if(allNodes[i].GetRect().Contains(point))
                {
                    foundNode = allNodes[i];

                    break;
                }
            }
        }

        return foundNode;
    }
}
#endregion
