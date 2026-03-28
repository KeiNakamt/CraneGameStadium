using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using Unity.VisualScripting;

public class normalChatcher : MonoBehaviour
{
    [SerializeField] private Transform craneRoot;

    [Header("Parameters")]
    [SerializeField] private float moveSpeedXZ = 5f;
    [SerializeField] private float moveSpeedY = 3f;
    [SerializeField] private float descendLimitY = -5f;
    [SerializeField] private float waitSeconds = 3f;

    private Vector3 homePos;
    private Tween currentTween;


    // 一般的な、UFOキャッチャー(2本爪)を想定。
    void Awake()
    {
        if(!craneRoot) craneRoot = transform;
        homePos = craneRoot.position;
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKey(KeyCode.N))
        {
            
        }
        if(Input.GetKey(KeyCode.M))
        {
            
        }
    }
}
