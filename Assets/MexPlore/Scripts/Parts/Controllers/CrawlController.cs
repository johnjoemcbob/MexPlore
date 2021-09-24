using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CrawlController : BaseController
{
    [Header( "Variables" )]
    public float Distance = 5;
    public float IdleDistance = 1;
    public float HoverDistance = 1;

    [Header( "References" )]
    public GameObject Body;
    public GameObject ElbowTarget;
    public GameObject HandTarget;

    [Header( "Assets" )]
    public AudioClip SoundReach;
    public AudioClip SoundGrab;
    public AudioClip SoundPull;

    private Vector3 LastDirection = Vector3.forward;
    private Vector3 LastSoundDirection = Vector3.zero;
    private Vector3 FreezePos = Vector3.zero;
    private Vector3 FreezeHandPos = Vector3.zero;

    void Update()
    {
        // Elbow global up?
        ElbowTarget.transform.position = Body.transform.position + Vector3.up * 4;

        Vector3 forward = Camera.main.transform.forward;
        forward.y = 0;
        Vector3 right = Camera.main.transform.right;
        right.y = 0;
        var hor = Input.GetAxis( "Horizontal" );
        var ver = Input.GetAxis( "Vertical" );
        Vector3 dir = forward * ver + right * hor;
        if ( dir.magnitude > 1 )
        {
            dir /= dir.magnitude;
        }
        Vector3 target = dir * Distance;

        bool hover = false;
        bool raycast = true;
        if ( Input.GetButtonDown( "Jump" ) )
        {
            FreezePos = Body.transform.position;
            FreezeHandPos = HandTarget.transform.position;

            if ( SoundGrab != null )
			{
                AudioSource.PlayClipAtPoint( SoundGrab, transform.position );
			}
        }
        if ( Input.GetButton( "Jump" ) )
        {
            // Freeze the hand, instead move the body towards the distance
            Body.transform.position = FreezePos + ( FreezeHandPos - Body.transform.position ) * ( 1 - dir.magnitude );
            //Body.GetComponent<Rigidbody>().MovePosition( FreezePos + ( FreezeHandPos - Body.transform.position ) * ( 1 - dir.magnitude ) );
            HandTarget.transform.position = FreezePos + ( FreezeHandPos - Body.transform.position ) * ( dir.magnitude ) / 2;
        }
        else
        {
            if ( target == Vector3.zero )
            {
                // Hover near last direction to avoid jank
                HandTarget.transform.position = Body.transform.position + LastDirection * IdleDistance + Vector3.up * HoverDistance;
                raycast = false;
            }
            else
            {
                LastDirection = dir;

                // Move the hand in the direction
                HandTarget.transform.position = Body.transform.position + target;
                Body.GetComponent<MechBody>().SetTargetDirection( dir.normalized );

                hover = true;
            }
        }

        if ( raycast )
        {
            // Raycast to ground
            RaycastHit hit;
            int mask = 1 << LayerMask.NameToLayer( "Ground" );
            float dist = 10000;
            Vector3 start = HandTarget.transform.position + Vector3.up * dist / 4;
            Vector3 raydir = -Vector3.up;
            if ( Physics.Raycast( start, raydir, out hit, dist, mask ) )
            {
                HandTarget.transform.position = hit.point;
            }

            // Hover if hovering
            if ( hover )
            {
                HandTarget.transform.position += Vector3.up * HoverDistance;
            }
        }

        // Sounds
        float change = dir.magnitude - LastSoundDirection.magnitude;
        if ( Mathf.Abs( change ) > 0.5f )
		{
            if ( change > 0 )
            {
                if ( SoundGrab != null )
                {
                    AudioSource.PlayClipAtPoint( SoundReach, transform.position );
                }
            }
            else
            {
                if ( SoundGrab != null )
                {
                    AudioSource.PlayClipAtPoint( SoundPull, transform.position );
                }
            }

            LastSoundDirection = dir;
        }
    }
}
