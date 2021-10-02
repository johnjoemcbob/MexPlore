using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[ExecuteInEditMode]
public class BuildingBlockGenerator : MonoBehaviour
{
    [Header( "Variables" )]
    public Vector2 RandomExtraBetweenBuildings;
    public float LeanMax = 10;

    [Header( "References" )]
    public Transform Sizer;
    public Transform Parent;

    [Header( "Assets" )]
    public GameObject[] Prefabs;

    void Update()
    {
        if ( !Application.isPlaying && Parent.childCount == 0 )
		{
            Generate();
		}
    }

    void Generate()
    {
        if ( RandomExtraBetweenBuildings == Vector2.zero ) return;

        float width = Sizer.localScale.x;
        float depth = Sizer.localScale.z;
        float x = -width / 2;
        float z = -depth / 2;

        //Instance( new Vector3( x, 0, z ) );
        //Instance( new Vector3( x + width, 0, z ) );
        //Instance( new Vector3( x, 0, z + depth ) );
        //Instance( new Vector3( x + width, 0, z + depth ) );

        // Start at one corner
        Vector3 pos = new Vector3( x, 0, z );

        // While has space on z axis
        float maxz = pos.z + depth;
        int iterate = 0;
        while ( pos.z <= maxz && iterate < 1000 )
        {
            // While has space on x axis
            float maxx = pos.x + width;
            while ( pos.x <= maxx && iterate < 1000 )
            {
                // Choose random building
                Instance( pos );

                // Get offset by size of first box collider
                // Add extra offset randomness
                pos.x += Random.Range( RandomExtraBetweenBuildings.x, RandomExtraBetweenBuildings.y );
                iterate++;
            }
            pos.x = x;
            pos.z += Random.Range( RandomExtraBetweenBuildings.x, RandomExtraBetweenBuildings.y );
        }
    }

    GameObject Instance( Vector3 pos )
    {
        var building = PrefabUtility.InstantiatePrefab( Prefabs[Random.Range( 0, Prefabs.Length )], Parent ) as GameObject;
        {
            building.transform.localPosition = pos;
            building.transform.localEulerAngles = new Vector3( Random.Range( -LeanMax, LeanMax ), Random.Range( -360, 360 ), Random.Range( -LeanMax, LeanMax ) );
            building.transform.localScale = Vector3.one;
        }
        return building;
    }
}
