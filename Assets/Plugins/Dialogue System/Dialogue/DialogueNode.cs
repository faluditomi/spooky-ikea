using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class DialogueNode : ScriptableObject, ISerializationCallbackReceiver
{
    #region Attributes
    [TextArea(10,15)] [SerializeField] string dialogueText;

    [HideInInspector] [SerializeField] List<string> children = new List<string>();

    [HideInInspector] [SerializeField] Rect rect = new Rect(0, 0, 200, 100);

    [SerializeField] public AudioClip voiceAudioClip;

    [SerializeField] List<string> triggerActions = new List<string>();

    [HideInInspector][SerializeField] private bool isRootNode = false;
    #endregion

    #region Getters
    public Rect GetRect() { return rect; }

    public string GetDialogueText() { return dialogueText; }

    public List<string> GetChildren() { return children; }

    public List<string> GetTriggerActions() { return triggerActions; }

    public bool IsRootNode() { return isRootNode; }
    #endregion

    #region Setters
#if UNITY_EDITOR
    public void SetPosition(Vector2 newPosition)
    {
        Undo.RecordObject(this, "Moved Dialogue Node.");

        rect.position = newPosition;

        EditorUtility.SetDirty(this);
    }

    public void SetText(string newDialogueText)
    {
        if(newDialogueText != dialogueText)
        {
            Undo.RecordObject(this, "Updated Dialogue Text.");

            dialogueText = newDialogueText;

            EditorUtility.SetDirty(this);
        }
    }

    public void AddChild(string childID)
    {
        Undo.RecordObject(this, "Added Dialogue Link.");

        children.Add(childID);

        EditorUtility.SetDirty(this);
    }

    public void RemoveChild(string childID)
    {
        Undo.RecordObject(this, "Removed Dialogue Link.");

        children.Remove(childID);

        EditorUtility.SetDirty(this);
    }

    public void SetIsRootNode(bool isRoot)
    {
        Undo.RecordObject(this, "Set as Root Node");

        isRootNode = isRoot;

        EditorUtility.SetDirty(this);
    }
#endif
    #endregion

    #region ISerializationCallbackReceiver Methods
    //Needed because the Script derives from ISerializationCallbackReceiver.
    public void OnBeforeSerialize() { }

    public void OnAfterDeserialize() { }
    #endregion
}
