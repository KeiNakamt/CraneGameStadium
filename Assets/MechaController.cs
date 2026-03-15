using System.Collections;
using System.Collections.Generic;
using UnityEditor.Timeline;
using UnityEngine;
using DG.Tweening;
using System.Runtime.InteropServices;

public class MechaController : MonoBehaviour
{
    [SerializeField] private Transform craneHead;
    
    [Header("MechaMode")]
    [Tooltip("For example, Ring Setting CraneGame must be false.")]
    [SerializeField] public bool ArmMode = false;

    [Header("Params")]
    [SerializeField] private float moveSpeedXZ = 5f;
    [SerializeField] private float moveSpeedY = 3f;
    [SerializeField] private float descendLimitY = -5f;
    [SerializeField] private float waitSeconds = 3f;

    private Vector3 homePos;
    private Tween currentTween;
    
    // Start is called before the first frame update
    void Awake()
    {
        if(!craneHead) craneHead = transform;
        homePos = craneHead.position;
    }

    public void PlaySequenceTo(Vector3 targetXZ)
    {
        currentTween?.Kill();

        Vector3 targetPos = new Vector3(targetXZ.x,homePos.y, targetXZ.z);

        float t1 = Vector3.Distance(new Vector3(craneHead.position.x, 0, craneHead.position.z),
                                    new Vector3(targetPos.x, 0, targetPos.z)) / moveSpeedXZ;
        float tDown = Mathf.Abs(craneHead.position.y - descendLimitY) / moveSpeedY;
        float tUp   = Mathf.Abs(descendLimitY - homePos.y) / moveSpeedY;

        float tBack = Vector3.Distance(new Vector3(targetPos.x, 0, targetPos.z),
                                       new Vector3(homePos.x, 0, homePos.z)) / moveSpeedXZ;

        // Sequenceで工程表を書くナ
        Sequence seq = DOTween.Sequence();

        // 1) XZで目標へ
        //seq.Append(craneHead.DOMoveX(targetPos.x, t1).SetEase(Ease.Linear));
        //seq.Join  (craneHead.DOMoveZ(targetPos.z, t1).SetEase(Ease.Linear));

        // 2) 下降
        seq.Append(craneHead.DOMoveY(descendLimitY, tDown).SetEase(Ease.InOutSine));

        // 3) 待つ
        seq.AppendInterval(waitSeconds);

        // 4) 上昇（ホームのYへ）
        seq.Append(craneHead.DOMoveY(homePos.y, tUp).SetEase(Ease.InOutSine));

        // 5) XZで帰還
        seq.Append(craneHead.DOMoveX(homePos.x, tBack).SetEase(Ease.Linear));
        seq.Join  (craneHead.DOMoveZ(homePos.z, tBack).SetEase(Ease.Linear));

        // 終了処理（任意）ナ
        seq.OnComplete(() =>
        {
            // Debug.Log("Sequence finished!");
        });

        currentTween = seq;

    }

    public void EmergencyStop()
    {
        currentTween?.Kill();
    }


    // Update is called once per frame
    void Update()
    {
        if (Input.GetKey(KeyCode.RightArrow))
        {
            transform.position += new Vector3(moveSpeedXZ*Time.deltaTime,0,0);
        }
        if (Input.GetKey(KeyCode.LeftArrow))
        {
            transform.position -= new Vector3(moveSpeedXZ*Time.deltaTime,0,0);
        }
        if (Input.GetKey(KeyCode.UpArrow))
        {
            transform.position += new Vector3(0,0,moveSpeedXZ*Time.deltaTime);
        }
        if (Input.GetKey(KeyCode.DownArrow))
        {
           transform.position -= new Vector3(0,0,moveSpeedXZ*Time.deltaTime); 
        }

        if(Input.GetKeyDown(KeyCode.A) && !ArmMode)
        {
            PlaySequenceTo(new Vector3(transform.position.x,0,transform.position.z));
        }
    }
}
