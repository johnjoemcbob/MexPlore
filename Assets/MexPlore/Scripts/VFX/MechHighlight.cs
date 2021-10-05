using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MechHighlight : MonoBehaviour
{
    public int Index = 0;

    public Material[] Highlights;

    void Start()
    {
        Apply();
    }

    public void Apply()
	{
        GetComponent<MechColourScheme>().MaterialLinks[2].Replacement = Highlights[Index];
        GetComponent<MechColourScheme>().Apply();
    }

    public void Set( int index )
	{
        Index = index;
        Apply();
	}
}
