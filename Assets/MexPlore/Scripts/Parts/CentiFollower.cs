using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CentiFollower : MonoBehaviour
{
    public float MaxDist = 0;
    public float LerpSpeed = 5;
    public float LerpAngleSpeed = 5;

    public Transform ToFollow;

    private float InitialDist;
    private Vector3 InitialOffset;

    void Start()
    {
        InitialDist = Vector3.Distance( transform.position, ToFollow.position );
        InitialOffset = ToFollow.position - transform.position;
    }

    void Update()
    {
        float dist = Vector3.Distance( transform.position, ToFollow.position );
        if ( dist > InitialDist + MaxDist )
        {
            // Move towards the target
            Vector3 dir = ( ToFollow.position - transform.position ).normalized;
            Vector3 target = ToFollow.position + InitialOffset;// dir * ( dist - ( InitialDist + MaxDist ) );
            transform.position = Vector3.Lerp( transform.position, target, Time.deltaTime * LerpSpeed );

            // Raycast?


            // While moving, it can also turn
            transform.rotation = Quaternion.Lerp( transform.rotation, ToFollow.rotation, Time.deltaTime * LerpAngleSpeed );
        }
    }
}
