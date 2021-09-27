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
    public float BodyHeightNormaliseLerpSpeed = 1;
    public float LegLerpDuration = 1;

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
            Vector3 dir = MexPlore.GetCameraDirectionalInput();
            LastDirection = dir;
            Vector3 pos = Body.transform.position + dir * MoveSpeed * Time.deltaTime;
                pos.y = NormalisedHeight;
            Body.GetComponent<MechBody>().SetTargetPos( pos );

            NormaliseBodyPos();
        }
        else if ( DisabledStillNormaliseBodyHeight )
        {
            // If not in control, still normalise body height relative to feet
            Vector3 pos = Body.transform.position;
                pos.y = NormalisedHeight;
            Body.GetComponent<MechBody>().SetTargetPos( pos );
        }

        UpdateLegs();
    }
    #endregion

    #region Body
    void NormaliseBodyPos()
    {
        int positions = 0;
        Vector3 pos = Vector3.zero;
        {
            // Find the IK of each leg target to get the ACTUAL hand/foot pos rather than desired!
            for ( int leg = 0; leg < Legs[LEFT].Legs.Length; leg++ )
            {
                pos += Legs[LEFT].Legs[leg].GetComponentInParent<InverseKinematics>().hand.position;
                positions++;
            }
            for ( int leg = 0; leg < Legs[RIGHT].Legs.Length; leg++ )
            {
                pos += Legs[RIGHT].Legs[leg].GetComponentInParent<InverseKinematics>().hand.position;
                positions++;
            }
        }
        float target = ( pos / positions ).y + BodyHeightOffset;
        float dist = Mathf.Abs( target - NormalisedHeight );
        NormalisedHeight = Mathf.Lerp( NormalisedHeight, target, Time.deltaTime * BodyHeightNormaliseLerpSpeed * dist );

        // Clamp to height of mech from ground
        Vector3 ground = MexPlore.RaycastToGround( Body.transform.position );
        // Ground is min, ground+height max
        float bodyheight = 10;
        NormalisedHeight = Mathf.Clamp( NormalisedHeight, ground.y, ground.y + bodyheight );
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

    public void TryMoveLeg( Transform leg, Vector3 pos )
    {
        // Add current direction of movement to the target pos as overshoot
        pos += LastDirection * OvershootMultiplier;

        int side = GetLegSide( leg );
        int other = GetOtherSide( side );
        int location = GetLegIndex( leg );
        bool canmove = IsMainController && // Is active!
            !LegDatas[side][location].IsMoving && // Isn't already moving
            LegDatas[side][location].NextToMove && // Is next to move
            //( LegDatas[other][location].LastMoved + BetweenMoveDelay < Time.time );
            !LegDatas[other][location].IsMoving; // Other side leg isn't currently moving
        if ( canmove )
        {
            pos = MexPlore.RaycastToGround( pos );

            // Start lerp
            LegDatas[side][location].TargetPosition = pos;
            StartCoroutine( MoveLeg( side, location, pos ) );

            LegDatas[side][location].LastMoved = Time.time;
            LegDatas[side][location].NextToMove = false;
            LegDatas[other][location].NextToMove = true;
        }
    }

    IEnumerator MoveLeg( int side, int location, Vector3 pos )
    {
        // Play leg raise sound
        TryPlaySound( SoundBankLegRaise, Legs[side].Legs[location].position, side, MexPlore.SOUND.MECH_LEG_RAISE );

        bool lowered = false;

        LegDatas[side][location].IsMoving = true;
        {
            Transform leg = Legs[side].Legs[location];
            Vector3 legstartpos = leg.position;
            float starttime = Time.time;
            while ( Time.time <= ( starttime + LegLerpDuration ) )
            {
                float elapsed = Time.time - starttime;
                float progress = elapsed / LegLerpDuration;
                // Move to halfway between the old and new positions, and upwards
                Vector3 target = legstartpos + ( pos - legstartpos ) / 2 + Vector3.up * 1;
                // Unitl half way through the move duration
                if ( progress >= 0.5f )
                {
                    // Store current leg position as new lerp start point
                    legstartpos = leg.position;
                    // Then move to the new position
                    target = pos;
                    progress -= 0.5f; // Normalised to 0.5 for lerp progress

                    // Play leg raise sound
                    if ( !lowered )
                    {
                        TryPlaySound( SoundBankLegLower, Legs[side].Legs[location].position, side, MexPlore.SOUND.MECH_LEG_LOWER );
                        lowered = true;
                    }
                }
                Vector3 lerpedpos = Vector3.Lerp( legstartpos, target, progress * 2 );
                Legs[side].Legs[location].position = lerpedpos;
                //LegDatas[side][location].Position = lerpedpos;
                StoreNewLegPos();

                yield return new WaitForEndOfFrame();
            }
        }
        LegDatas[side][location].IsMoving = false;

        // Play footstep sound
        TryPlaySound( SoundBankFootstep, pos, side, MexPlore.SOUND.MECH_FOOTSTEP );

        // Play particle effect
        StaticHelpers.EmitParticleDust( pos );
    }

    float GetLegPitch( MexPlore.SOUND sound, int side )
	{
        Vector3 range = MexPlore.GetPitchRange( sound );
        return UnityEngine.Random.Range( range.x, range.y ) + ( side + 1 ) * range.z;
    }

    void TryPlaySound( AudioClip[] bank, Vector3 pos, int side, MexPlore.SOUND sound )
	{
        if ( bank.Length > 0 )
        {
            StaticHelpers.GetOrCreateCachedAudioSource( bank[UnityEngine.Random.Range( 0, bank.Length )], pos, GetLegPitch( sound, side ), MexPlore.GetVolume( sound ) );
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
}
