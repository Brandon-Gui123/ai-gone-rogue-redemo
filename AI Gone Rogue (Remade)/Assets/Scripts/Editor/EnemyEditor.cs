#if UNITY_EDITOR

using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Enemy), true)]
public class EnemyEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        Enemy enemyScript = (Enemy) target;

        if (GUILayout.Button("Inherit Layers from Weapon"))
        {
            // first, we check if the enemy does have a weapon
            if (!enemyScript.HasArmedWeapon())
            {
                // since we don't have an armed weapon, we can't proceed any further
                // tell the dev about it
                EditorUtility.DisplayDialog("No weapon attached", "This enemy has no weapons attached!\nLayers cannot be inherited from a non-existant weapon.", "Ok");
                return;
            }

            bool canProceed = EditorUtility.DisplayDialog(
                "Inheriting layers",
                "This will replace the enemy's layer mask with the one from the weapon, as well as whether triggers are involved.\nAre you sure?",
                "Yes",
                "Cancel"
            );

            if (canProceed)
            {
                enemyScript.InheritLayerSettingsFromWeapon();

                // record modifications made to the Enemy script component of the prefab
                PrefabUtility.RecordPrefabInstancePropertyModifications(enemyScript);

                // signal to the editor that there are changes to be saved
                EditorUtility.SetDirty(enemyScript);
            }

        }
    }
}

#endif