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
    public bool IsMainController = true;
    public float MoveSpeed = 5;
    public float BetweenMoveDelay = 0.1f;
    public float BodyHeightOffset = 1;
    public float LegLerpDuration = 1;

    [Header( "References" )]
    public GameObject Body;
    public LegArray[] Legs;

    private LegData[][] LegDatas;

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
            Body.transform.eulerAngles = new Vector3( 0, Camera.main.transform.eulerAngles.y, 0 );

            // Movement
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
            //Body.transform.position +=
            Body.GetComponent<Rigidbody>().MovePosition( Body.transform.position + dir * MoveSpeed );// * Time.deltaTime );
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
            for ( int leg = 0; leg < Legs[LEFT].Legs.Length; leg++ )
            {
                pos += LegDatas[LEFT][leg].Position;
                positions++;
            }
            for ( int leg = 0; leg < Legs[RIGHT].Legs.Length; leg++ )
            {
                pos += LegDatas[RIGHT][leg].Position;
                positions++;
            }
        }
        Body.transform.position = new Vector3( Body.transform.position.x, ( pos / positions ).y + BodyHeightOffset, Body.transform.position.z );
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

        NormaliseBodyPos();
    }

    public void TryMoveLeg( Transform leg, Vector3 pos )
    {
        int side = GetLegSide( leg );
        int other = GetOtherSide( side );
        int location = GetLegIndex( leg );
        bool canmove = !LegDatas[side][location].IsMoving && // Isn't already moving
            LegDatas[side][location].NextToMove && // Is next to move
            //( LegDatas[other][location].LastMoved + BetweenMoveDelay < Time.time );
            !LegDatas[other][location].IsMoving; // Other side leg isn't currently moving
        if ( canmove )
        {
            // Raycast on to ground
            RaycastHit hit;
            int mask = 1 << LayerMask.NameToLayer( "Ground" );
            float raydist = 10000;
            Vector3 start = pos + Vector3.up * raydist / 4;
            Vector3 raydir = -Vector3.up;
            if ( Physics.Raycast( start, raydir, out hit, raydist, mask ) )
            {
                pos = hit.point;
            }

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
                }
                Vector3 lerpedpos = Vector3.Lerp( legstartpos, target, progress * 2 );
                Legs[side].Legs[location].position = lerpedpos;
                //LegDatas[side][location].Position = lerpedpos;
                StoreNewLegPos();

                yield return new WaitForEndOfFrame();
            }
        }
        LegDatas[side][location].IsMoving = false;
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