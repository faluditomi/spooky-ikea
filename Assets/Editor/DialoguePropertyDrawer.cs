using UnityEngine;
using UnityEditor;

//This script adds a button next to the dialogue fields so it can be opened in Inspector.
[CustomPropertyDrawer(typeof(Dialogue))]
public class DialoguePropertyDrawer : PropertyDrawer
{
    #region Editor Methods
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        using(new EditorGUI.ChangeCheckScope())
        {
            position.width -= 40f;

            EditorGUI.PropertyField(position, property, label);

            position.x = position.width + 20f;

            position.width = 40f;

            if(GUI.Button(position, new GUIContent("Edit"), EditorStyles.miniButton))
            {
                Selection.activeObject = property.objectReferenceValue;

                DialogueEditor.ShowEditorWindow();
            }
        }
    }
    #endregion
}

