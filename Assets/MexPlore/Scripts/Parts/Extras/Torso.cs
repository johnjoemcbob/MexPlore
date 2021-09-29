using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Torso : MonoBehaviour
{
    [Serializable]
    public struct Spine
	{
        public Transform Pivot;
        public Vector2 MaxAngles;
	}

    [Header( "Variables" )]
    public float AngleDistanceMultiplier = -1;
    public float GlobalSpineMaxMultiplier = 1;

    [Header( "References" )]
    public Spine[] Spines;

    [HideInInspector]
    public float CurrentLean = 0;

    void Start()
    {
        
    }

    void Update()
    {
        float dist = CurrentLean;
        if ( GetComponentInParent<MechBody>().IsMainController )
        {
            dist = Camera.main.transform.forward.y;
            CurrentLean = dist;
        }
        foreach ( var spine in Spines )
        {
            float ang = Mathf.Clamp( dist * AngleDistanceMultiplier, spine.MaxAngles.x * GlobalSpineMaxMultiplier, spine.MaxAngles.y * GlobalSpineMaxMultiplier );
            spine.Pivot.localEulerAngles = new Vector3( 0, 0, 1 ) * ang;
        }
    }
}
