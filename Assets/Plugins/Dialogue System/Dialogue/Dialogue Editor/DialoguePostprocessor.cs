using System.Linq;
using UnityEditor;

public class DialoguePostprocessor : AssetPostprocessor
{
    //Automatically called by Unity after assets are imported/created. Creates the root node for new Dialogue assets.
    static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
    {
        foreach(string path in importedAssets)
        {
            Dialogue dialogue = AssetDatabase.LoadAssetAtPath<Dialogue>(path);

            if(dialogue != null && !dialogue.GetAllNodes().Any())
            {
                EditorApplication.delayCall += () =>
                {
                    if(dialogue != null)
                    {
                        dialogue.Init();

                        EditorUtility.SetDirty(dialogue);
                        AssetDatabase.SaveAssets();
                    }
                };
            }
        }
    }
}
