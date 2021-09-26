using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Game : MonoBehaviour
{
    public static Game Instance;
    public static Transform RuntimeParent;

    void Awake()
    {
        Instance = this;
        RuntimeParent = transform;
    }

    void Start()
    {
        
    }

    void Update()
    {
        
    }
}
