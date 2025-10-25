using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEditor;

[CreateAssetMenu(fileName = "New Dialogue", menuName = "Dialogue", order = 0)]
public class Dialogue : ScriptableObject, ISerializationCallbackReceiver
{
    #region Attributes
    [SerializeField] List<DialogueNode> nodes = new List<DialogueNode>();

    Dictionary<string, DialogueNode> nodeLookup = new Dictionary<string, DialogueNode>();
    #endregion

    //Is called when a value is changed in the inspector or when a scriptable object is loaded.
    private void OnValidate()
    {
        if(nodeLookup != null)
        {
            nodeLookup.Clear();
        }

        foreach(DialogueNode node in GetAllNodes())
        {
            if(node)
            {
                nodeLookup[node.name] = node;
            }
        }
    }

    public IEnumerable<DialogueNode> GetAllNodes() { return nodes; }

    public DialogueNode GetRandomRootNode()
    {
        if(nodes.Count == 0) return null;

        List<DialogueNode> rootNodes = new List<DialogueNode>();

        foreach(DialogueNode node in nodes)
        {
            if(node.IsRootNode()) rootNodes.Add(node);
        }

        if(rootNodes.Count > 0)
        {
            int i = UnityEngine.Random.Range(0, rootNodes.Count);

            return rootNodes[i];
        }

        return nodes[0];
    }

    public void GetChildren(DialogueNode parentNode, List<DialogueNode> children)
    {
        children.Clear();

        foreach(string childID in parentNode.GetChildren())
        {
            if(nodeLookup.ContainsKey(childID))
            {
                children.Add(nodeLookup[childID]);
            }
        }
    }

    //Runs the methods if only its an Editor window.
    #if UNITY_EDITOR
    public DialogueNode CreateNode(DialogueNode parent)
    {
        DialogueNode newNode = MakeNode(parent);

        Undo.RegisterCreatedObjectUndo(newNode, "Created Dialogue Node.");
        Undo.RecordObject(this, "Added Dialogue Node.");

        return AddNode(newNode);
    }

    public DialogueNode CreateRootNode()
    {
        float rootColumnX = (nodes.Count > 0) ? nodes[0].GetRect().position.x : 0f;

        Vector2 finalPosition = FindNextAvailablePosition(rootColumnX);

        DialogueNode newNode = CreateInstance<DialogueNode>();

        newNode.name = System.Guid.NewGuid().ToString();
        newNode.SetIsRootNode(true);
        newNode.SetPosition(finalPosition);

        Undo.RegisterCreatedObjectUndo(newNode, "Created Root Node");
        Undo.RecordObject(this, "Added Root Node");

        return AddNode(newNode);
    }

    public void Init()
    {
        if(nodes.Count > 0) return;

        CreateRootNode();
    }

    public void DeleteNode(DialogueNode nodeToDelete)
    {
        Undo.RecordObject(this, "Deleted Dialogue Node.");

        nodes.Remove(nodeToDelete);

        foreach(DialogueNode node in GetAllNodes())
        {
            if(node.GetChildren().Contains(nodeToDelete.name))
            {
                Undo.RecordObject(node, "Removed Dialogue Node Child.");

                node.RemoveChild(nodeToDelete.name);
            }
        }

        OnValidate();

        Undo.DestroyObjectImmediate(nodeToDelete);
    }

    private DialogueNode AddNode(DialogueNode newNode)
    {
        nodes.Add(newNode);

        OnValidate();

        return newNode;
    }

    private Vector2 FindNextAvailablePosition(float columnX)
    {
        const float gridSize = DialogueEditor.backgroundSize;
        const float verticalSpacing = DialogueEditor.verticalSpacing;
        const float startY = 0f;

        var occupiedSnappedYPositions = new HashSet<float>();

        float snappedColumnX = Mathf.Round(columnX / gridSize) * gridSize;

        foreach(DialogueNode node in nodes)
        {
            float nodeSnappedX = Mathf.Round(node.GetRect().position.x / gridSize) * gridSize;

            if(Mathf.Approximately(nodeSnappedX, snappedColumnX))
            {
                float nodeSnappedY = Mathf.Round(node.GetRect().position.y / gridSize) * gridSize;

                occupiedSnappedYPositions.Add(nodeSnappedY);
            }
        }

        int slotIndex = 0;

        while(true)
        {
            float targetY = startY + (slotIndex * verticalSpacing);

            if(!occupiedSnappedYPositions.Contains(targetY))
            {
                return new Vector2(snappedColumnX, targetY);
            }

            slotIndex++;
        }
    }

    private DialogueNode MakeNode(DialogueNode parent)
    {
        DialogueNode newNode = CreateInstance<DialogueNode>();

        newNode.name = Guid.NewGuid().ToString();

        if(parent)
        {
            float childColumnX = parent.GetRect().position.x + DialogueEditor.horizontalSpacing;

            Vector2 finalPosition = FindNextAvailablePosition(childColumnX);

            newNode.SetPosition(finalPosition);
            parent.AddChild(newNode.name);
        }
        else
        {
            newNode.SetPosition(FindNextAvailablePosition(0f));
        }

        return newNode;
    }
#endif

    public void OnBeforeSerialize()
    {
        #if UNITY_EDITOR
        if(AssetDatabase.GetAssetPath(this) != "")
        {
            foreach(DialogueNode node in GetAllNodes())
            {
                if(AssetDatabase.GetAssetPath(node) == "")
                {
                    AssetDatabase.AddObjectToAsset(node, this);
                }
            }
        }
        #endif
    }

    #region ISerializationCallbackReceiver Methods
    //Needed because the Script derives from ISerializationCallbackReceiver.
    public void OnAfterDeserialize() { }
    #endregion
}
