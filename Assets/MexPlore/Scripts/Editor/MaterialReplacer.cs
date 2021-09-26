using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[ExecuteInEditMode]
public class MaterialReplacer : MonoBehaviour
{
    void Update()
    {
		// For every mesh renderer in child
		foreach ( MeshRenderer renderer in GetComponentsInChildren<MeshRenderer>() )
        {
            int ind = 0;
            foreach ( var mat in renderer.sharedMaterials )
            {
                // If it has a material which is not of the correct flatkit shader
                //Debug.Log( mat.shader.name );
                if ( mat.shader.name == "Standard" )
                {
                    Material material = mat;

                    string matname = mat.name.Replace( " (Instance)", "" ) + " AutoFlat";
                    string name = renderer.name + "_" + matname;
                    string dir = "Assets/MexPlore/Materials/Auto/";
                    var assets = AssetDatabase.FindAssets( name );
                    if ( assets.Length > 0 )
					{
                        // Load existing material for this model + part
                        GUID id;
                        if ( GUID.TryParse( assets[0], out id ) )
                        {
                            material = AssetDatabase.LoadAssetAtPath<Material>( AssetDatabase.GUIDToAssetPath( id ) );
                        }
					}
					else
                    {
                        // Create new one with the same name in a folder with the model name
                        material = new Material( Shader.Find( "FlatKit/Stylized Surface With Outline" ) );
                        {
                            material.name = matname;

                            material.SetColor( "_Color", mat.color );
                            Color darker = mat.color;
                            {
                                float mult = 0.7f;
                                darker.r *= mult;
                                darker.g *= mult;
                                darker.b *= mult;
                            }
                            material.SetColor( "_ColorDim", darker );

                            material.SetFloat( "_OutlineWidth", 2 );
                            material.SetColor( "_OutlineColor", new Color( 144 / 255.0f, 126 / 255.0f, 126 / 255.0f ) );
                        }
                        AssetDatabase.CreateAsset( material, dir + name + ".mat" );
                    }

                    // Assign it
                    Material[] mats = renderer.sharedMaterials;
                    {
                        mats[ind] = material;
                    }
                    renderer.sharedMaterials = mats;

                    ind++;
                }
            }
        }
    }
}
