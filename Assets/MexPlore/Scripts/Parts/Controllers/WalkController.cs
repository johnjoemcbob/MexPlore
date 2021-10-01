using System;
using System.Collections;
using UnityEngine;

public class WalkController : BaseController
{
    [Serializable]
    public struct LegArray
    {
        public Transform[] Legs;
    }

    public struct LegData
    {
        public Vector3 Position;
        public Vector3 TargetPosition;
        public float LastMoved;
        public bool NextToMove;
        public bool IsMoving;
    }

    const short LEFT = 0;
    const short RIGHT = 1;

    [Header( "Variables" )]
    public bool DisabledStillNormaliseBodyHeight = false;
    public float MoveSpeed = 5;
    public float OvershootMultiplier = 1;
    public float BetweenMoveDelay = 0.1f;
    public float BodyHeightOffset = 1;
    public float BodyLegRaiseHeightOffset = 0.5f;
    public float BodyHeightNormaliseLerpSpeed = 1;
    public float LegLerpDuration = 1;
    public float MaxMoveMultiplier = 1;

    [Header( "References" )]
    public GameObject Body;
    public LegArray[] Legs;

    [Header( "Assets" )]
    public AudioClip[] SoundBankLegRaise;
    public AudioClip[] SoundBankLegLower;
    public AudioClip[] SoundBankFootstep;

    private LegData[][] LegDatas;
    private float NormalisedHeight = 0;
    private Vector3 LastDirection = Vector3.zero;

    #region MonoBehaviour
    void OnEnable()
    {
        // Store initial leg target positions
        LegDatas = new LegData[2][];
        LegDatas[LEFT] = new LegData[Legs[LEFT].Legs.Length];
        LegDatas[RIGHT] = new LegData[Legs[RIGHT].Legs.Length];
        StoreNewLegPos();

        // Initialise next moves
        for ( int leg = 0; leg < LegDatas[LEFT].Length; leg++ )
        {
            LegDatas[LEFT][leg].LastMoved = 0;
            LegDatas[LEFT][leg].NextToMove = leg % 2 == 0; // Even
            LegDatas[LEFT][leg].IsMoving = false;
        }
        for ( int leg = 0; leg < LegDatas[RIGHT].Length; leg++ )
        {
            LegDatas[RIGHT][leg].LastMoved = 0;
            LegDatas[RIGHT][leg].NextToMove = leg % 2 != 0; // Odd
            LegDatas[RIGHT][leg].IsMoving = false;
        }
    }

    void LateUpdate()
    {
        if ( IsMainController )
        {
            // Face camera forward
            //Body.transform.eulerAngles = new Vector3( 0, Camera.main.transform.eulerAngles.y, 0 );
            Vector3 camdir = Camera.main.transform.forward;
            camdir.y = 0;
            Body.GetComponent<MechBody>().SetTargetDirection( camdir.normalized );

            // Movement
            TryMove( MexPlore.CAST_SPHERE_UP );
        }
        else if ( DisabledStillNormaliseBodyHeight )
        {
            // If not in control, still normalise body height relative to feet
            Vector3 pos = Body.transform.position;
                NormalisedHeight = NormaliseBodyPos( pos );
                pos.y = Mathf.Lerp( pos.y, NormalisedHeight, Time.deltaTime * BodyHeightNormaliseLerpSpeed );
            Body.GetComponent<MechBody>().SetTargetPos( pos );
        }

        UpdateLegs();
    }

    // Recursive function!
    void TryMove( float updist )
	{
        // Get input + required height at new position (for this updist)
        Vector3 dir = MexPlore.GetCameraDirectionalInput();
        LastDirection = dir;
        float lastnormal = NormalisedHeight;
        Vector3 pos = Body.transform.position + dir * MoveSpeed * Time.deltaTime;
        {
            NormalisedHeight = NormaliseBodyPos( pos );
            pos.y = Mathf.Lerp( pos.y, NormalisedHeight, Time.deltaTime * BodyHeightNormaliseLerpSpeed );
        }

        // Verify that this new position is within range on one leg stride (in case massive height difference)
        float dist = Vector3.Distance( Body.transform.position, pos );
        float leglength = GetComponentInChildren<InverseKinematics>().ArmLength * MaxMoveMultiplier * Time.deltaTime;

        // Verify that this new position doesn't pass the torso through solid objects
        Vector3 offset = Vector3.up * 3;
        Vector3 start = Body.transform.position + offset;
        Vector3 testpos = start + dir * MoveSpeed * Time.deltaTime;
        Vector3 castpos = MexPlore.RaycastSphere( start, testpos, 0.2f );
        bool wayblocked = start != castpos;

        // Debug
        DebugLastSphereCast = castpos;
        DebugLastSphereCastHit = testpos;

        // If it's blocked, undo the move and try a lower ground level
        if ( dist >= leglength || wayblocked )
        {
            pos = Body.transform.position;
            if ( updist > 0 )
            {
                TryMove( updist - 1 );
            }
        }
        Body.GetComponent<MechBody>().SetTargetPos( pos );
    }
    #endregion

    #region Body
    float NormaliseBodyPos( Vector3 pos, float updist = MexPlore.CAST_SPHERE_UP )
    {
        Vector3 ground = MexPlore.RaycastToGroundSphere( pos, updist );
        float target = ground.y + BodyHeightOffset + GetRaisedLegCount() * BodyLegRaiseHeightOffset;
        //float normalised = Mathf.Lerp( NormalisedHeight, target, Time.deltaTime * BodyHeightNormaliseLerpSpeed );
        float normalised = target;

        //DebugLastSphereCast = Body.transform.position + Vector3.up * updist;
        //DebugLastSphereCastHit = ground;

        return normalised;
    }
    #endregion

    #region Legs
    void UpdateLegs()
    {
        // Keep the leg target positions static despite being child of the body
        for ( int leg = 0; leg < Legs[LEFT].Legs.Length; leg++ )
        {
            Legs[LEFT].Legs[leg].position = LegDatas[LEFT][leg].Position;
        }
        for ( int leg = 0; leg < Legs[RIGHT].Legs.Length; leg++ )
        {
            Legs[RIGHT].Legs[leg].position = LegDatas[RIGHT][leg].Position;
        }
    }

    public void StoreNewLegPos()
    {
        for ( int leg = 0; leg < Legs[LEFT].Legs.Length; leg++ )
        {
            LegDatas[LEFT][leg].Position = Legs[LEFT].Legs[leg].position;
        }
        for ( int leg = 0; leg < Legs[RIGHT].Legs.Length; leg++ )
        {
            LegDatas[RIGHT][leg].Position = Legs[RIGHT].Legs[leg].position;
        }
    }

    float GetLegPitch( MexPlore.SOUND sound, int side )
	{
        Vector3 range = MexPlore.GetPitchRange( sound );
        return UnityEngine.Random.Range( range.x, range.y ) + ( side + 1 ) * range.z;
    }

    public void TryPlaySound( AudioClip[] bank, Vector3 pos, int side, MexPlore.SOUND sound )
    {
        if ( bank.Length > 0 )
        {
            StaticHelpers.GetOrCreateCachedAudioSource( bank[UnityEngine.Random.Range( 0, bank.Length )], pos, GetLegPitch( sound, side ), MexPlore.GetVolume( sound ) );
        }
    }

    public void TryPlaySound( AudioClip[] bank, InverseKinematics ik, MexPlore.SOUND sound )
    {
        TryPlaySound( bank, ik.target.position, GetLegSide( ik.target ), sound );
    }

    int GetRaisedLegCount()
    {
        int count = 0;
        {
			for ( int side = 0; side <= LEFT + RIGHT; side++ )
            {
                for ( int leg = 0; leg < Legs[side].Legs.Length; leg++ )
                {
                    count += ( ( Time.time - LegDatas[side][leg].LastMoved ) < 0.1f ) ? 1 : 0;
                }
            }
        }
        return count;
    }
    #endregion

    #region Move Leg
    public Vector3 GetOvershoot()
	{
        return LastDirection * OvershootMultiplier;
    }

    public void TryMoveLeg( Transform leg, Vector3 pos, bool force = false )
    {
        // Add current direction of movement to the target pos as overshoot
        //pos += LastDirection * OvershootMultiplier;

        int side = GetLegSide( leg );
        int other = GetOtherSide( side );
        int location = GetLegIndex( leg );
        bool canmove = 
            //IsMainController && // Is active!
            //!LegDatas[side][location].IsMoving && // Isn't already moving
            ( LegDatas[side][location].NextToMove ) && // && LegDatas[side].Length == 1 ) && // Is next to move
            ( LegDatas[side][location].LastMoved + LegLerpDuration + BetweenMoveDelay < Time.time ) && // Isn't already moving
            (
				( LegDatas[other][location].LastMoved + LegLerpDuration < Time.time ) || // Other side leg isn't currently moving
                force
			) &&
		    true; // For easy commenting of other lines :)
        if ( canmove || force )
        {
            Vector3 ground = MexPlore.RaycastToGround( pos );

            var ik = Legs[side].Legs[location].GetComponentInParent<InverseKinematics>();
            float dist = Vector3.Distance( ik.transform.position, ground );
            bool valid = ( dist <= ik.ArmLength );
            if ( valid || force )
            {
                // Start lerp
                LegDatas[side][location].TargetPosition = ground;
                if ( !ik.IsWalking() || force )
                {
                    ik.StartWalking( ground );

                    Debug.DrawRay( ground, Vector3.up );
                    LegDatas[side][location].LastMoved = Time.time;
                }
            }

            LegDatas[side][location].NextToMove = false;
            LegDatas[other][location].NextToMove = true;
            //int next = location + 1;
            //if ( next >= LegDatas[other].Length )
            //{
            //    next = 0;
            //}
            //LegDatas[other][next].NextToMove = true;
        }
    }
    #endregion

    #region Find Leg
    int GetLegSide( Transform legform )
    {
        for ( int leg = 0; leg < Legs[LEFT].Legs.Length; leg++ )
        {
            if ( Legs[LEFT].Legs[leg] == legform )
            {
                return LEFT;
            }
        }
        return RIGHT;
    }

    int GetOtherSide( int side )
    {
        if ( side == LEFT )
        {
            return RIGHT;
        }
        return LEFT;
    }

    int GetLegIndex( Transform legform )
    {
        for ( int leg = 0; leg < Legs[LEFT].Legs.Length; leg++ )
        {
            if ( Legs[LEFT].Legs[leg] == legform )
            {
                return leg;
            }
        }
        for ( int leg = 0; leg < Legs[RIGHT].Legs.Length; leg++ )
        {
            if ( Legs[RIGHT].Legs[leg] == legform )
            {
                return leg;
            }
        }
        return -1;
    }
    #endregion

    #region Debug
    public bool DEBUG = true;
    private Vector3 DebugLastSphereCast = Vector3.zero;
    private Vector3 DebugLastSphereCastHit = Vector3.zero;

    private void OnDrawGizmos()
    {
        if ( DEBUG )
        {
            if ( DebugLastSphereCast != Vector3.zero )
            {
                Gizmos.color = Color.red;
                Gizmos.DrawSphere( DebugLastSphereCast, MexPlore.CAST_SPHERE_RADIUS );
            }
            if ( DebugLastSphereCastHit != Vector3.zero )
            {
                Gizmos.color = Color.blue;
                Gizmos.DrawSphere( DebugLastSphereCastHit, MexPlore.CAST_SPHERE_RADIUS );
            }
        }
    }
	#endregion
}
