using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[ExecuteInEditMode]
public class SpecificMaterialReplacer : MonoBehaviour
{
    public string Find = "";
    public Material Replace;
    public bool Active = false;

    void Update()
    {
        if ( !Active ) return;

		// For every mesh renderer in child
		foreach ( MeshRenderer renderer in GetComponentsInChildren<MeshRenderer>() )
        {
			for ( int ind = 0; ind < renderer.sharedMaterials.Length; ind++ )
			{
                var mat = renderer.sharedMaterials[ind];
                // If it has a material which is not of the correct flatkit shader
                Debug.Log( mat.name + " " + mat.name.Contains( Find ) + " " + ind );
                if ( mat.name.Contains( Find ) )
                {
                    // Assign it
                    Material[] mats = renderer.sharedMaterials;
                    {
                        mats[ind] = Replace;
                    }
                    renderer.sharedMaterials = mats;
                }
            }
        }
    }
}
