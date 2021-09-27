using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class MexPlore
{
    public enum SOUND
    {
        HELI_BLADES,
        HELI_DOCK,
        HELI_UNDOCK,

        MECH_TURN,
        MECH_LEG_RAISE,
        MECH_LEG_LOWER,
        MECH_FOOTSTEP,

        ENGINE_ON,
        ENGINE_LOOP,
        ENGINE_OFF,

        COUNT,
    }

    public static float[] Volume;
    public static Vector3[] Pitch;

    public static void InitVolumes()
	{
        Volume = new float[(int) SOUND.COUNT];
        Pitch = new Vector3[(int) SOUND.COUNT];
		for ( int i = 0; i < Pitch.Length; i++ )
		{
            Pitch[i] = new Vector3( 1, 1, 0 );
		}

        Volume[(int) SOUND.HELI_BLADES] = 0.5f;
        Volume[(int) SOUND.HELI_DOCK] = 0.5f;
        Volume[(int) SOUND.HELI_UNDOCK] = 0.5f;

        Volume[(int) SOUND.MECH_TURN] = 0.2f;
        Volume[(int) SOUND.MECH_LEG_RAISE] = 0.15f;
        Pitch[(int) SOUND.MECH_LEG_RAISE] = new Vector3( 0.95f, 1.05f, 0.05f );
        Volume[(int) SOUND.MECH_LEG_LOWER] = 0.15f;
        Pitch[(int) SOUND.MECH_LEG_LOWER] = new Vector3( 0.95f, 1.05f, 0.05f );
        Volume[(int) SOUND.MECH_FOOTSTEP] = 0.5f;
        Pitch[(int) SOUND.MECH_FOOTSTEP] = new Vector3( 0.8f, 1.2f, 0.2f );

        Volume[(int) SOUND.ENGINE_ON] = 0.3f;
        Volume[(int) SOUND.ENGINE_LOOP] = 0.2f;
        Volume[(int) SOUND.ENGINE_OFF] = 0.3f;
    }

    public static float GetVolume( SOUND sound )
	{
        return Volume[(int) sound];
	}

    public static Vector3 GetPitchRange( SOUND sound )
	{
        return Pitch[(int) sound];
	}

    public static Vector3 GetCameraDirectionalInput()
    {
        // Camera
        Vector3 forward = Camera.main.transform.forward;
        forward.y = 0;
        Vector3 right = Camera.main.transform.right;
        right.y = 0;

        // Input
        var hor = Input.GetAxis( "Horizontal" );
        var ver = Input.GetAxis( "Vertical" );

        // Together
        Vector3 dir = forward * ver + right * hor;
        {
            // Weird and bad normalisation because < 1 is valid for directional force? or something?
            if ( dir.magnitude > 1 )
            {
                dir /= dir.magnitude;
            }
        }
        return dir;
    }

    public static Vector3 RaycastToGround( Vector3 pos )
    {
        // Raycast on to ground
        RaycastHit hit;
        int mask = 1 << LayerMask.NameToLayer( "Ground" );
        float raydist = 1000;
        float updist = 10;
        Vector3 start = pos + Vector3.up * updist;
        Vector3 raydir = -Vector3.up;
        if ( Physics.Raycast( start, raydir, out hit, updist + raydist, mask ) )
        {
            pos = hit.point;
        }
        return pos;
    }

    public static RaycastHit RaycastToGroundHit( Vector3 pos )
    {
        // Raycast on to ground
        RaycastHit hit;
        int mask = 1 << LayerMask.NameToLayer( "Ground" );
        float raydist = 10;
        float updist = 10;
        Vector3 start = pos + Vector3.up * updist;
        Vector3 raydir = -Vector3.up;

        Physics.Raycast( start, raydir, out hit, updist + raydist, mask );
        return hit;
    }
}
