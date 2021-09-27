using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MechColourScheme : MonoBehaviour
{
	[Serializable]
    public struct MaterialLink
	{
        public Material Default;
        public Material Replacement;
	}

	public MaterialLink[] MaterialLinks;

	void Start()
    {
		foreach ( var renderer in GetComponentsInChildren<MeshRenderer>() )
		{
			for ( int ind = 0; ind < renderer.materials.Length; ind++ )
			{
				var mat = renderer.materials[ind];
				foreach ( var link in MaterialLinks )
				{
					if ( mat == link.Default || mat.name.Replace( " (Instance)", "" ) == link.Default.name )
					{
						var mats = renderer.materials;
						{
							mats[ind] = link.Replacement;
						}
						renderer.materials = mats;
					}
				}
			}
		}
    }
}
