using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateAndPulse : MonoBehaviour
{
    public float RotateSpeed = 5;
    public float ScaleSpeed = 5;
    public float ScaleMult = 0.2f;
    public float UpSpeed = 5;
    public float UpMult = 0.2f;

    void Update()
    {
        transform.localPosition = Vector3.up * ( Mathf.Sin( Time.time * UpSpeed ) * UpMult );
        transform.localEulerAngles += new Vector3( 0, 1, 0 ) * Time.deltaTime * RotateSpeed;
        transform.localScale = Vector3.one * ( 1 + ( Mathf.Sin( Time.time * ScaleSpeed ) * ScaleMult ) );
    }
}
