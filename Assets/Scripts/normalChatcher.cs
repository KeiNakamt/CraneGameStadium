using System.Collections;
using System.Collections.Generic;
using UnityEditor.Timeline;
using UnityEngine;
using DG.Tweening;
using System.Runtime.InteropServices;

public class normalCatcher : MonoBehaviour
{
    [SerializeField] private Transform craneRoot;

    [Header("Parameters")]
    [SerializeField] private float moveSpeedXZ = 5f;
    [SerializeField] private float moveSpeedY = 3f;
    [SerializeField] private float descendLimitY = -5f;
    [SerializeField] private float waitSeconds = 3f;
    [SerializeField] private int backMode = 0;
    [SerializeField] private Vector3 secondTarget;

    [SerializeField] private GameObject ArmL;
    [SerializeField] private GameObject ArmR;

    private HingeJoint hingeL;
    private HingeJoint hingeR; 

    private Vector3 homePos;

    private Tween descendTween;
    private Tween currentTween;


    private bool isPushX = false;
    private bool isPushZ = false;
    private bool isSequence = false;


    // 一般的な、UFOキャッチャー(2本爪)を想定。
    void Awake()
    {
        if(!craneRoot) craneRoot = transform;
        homePos = craneRoot.position;

        hingeL = ArmL.GetComponent<HingeJoint>();
        hingeR = ArmR.GetComponent<HingeJoint>();
    }

    void OpenArm()
    {
        // アームを開く
        JointSpring jointSpringL = hingeL.spring;
        jointSpringL.targetPosition = 0f;
        hingeL.spring = jointSpringL;

        JointSpring jointSpringR = hingeR.spring;
        jointSpringR.targetPosition = 0f;
        hingeR.spring = jointSpringR;
    }

    void CloseArm()
    {
        // アームを閉じる
        JointSpring jointSpringL = hingeL.spring;
        jointSpringL.targetPosition = 50f;
        hingeL.spring = jointSpringL;

        JointSpring jointSpringR = hingeR.spring;
        jointSpringR.targetPosition = 50f;
        hingeR.spring = jointSpringR;
    }

    // Tweenで動かす際の関数を定義しておく
    // やらせることは、掴んで引き上げてX,Z軸移動後のメカの座標を引数に入れ、ホームポジションへ戻すこと
    public void StartDescend()
    {
        // 下降制限、爪と景品または壁の衝突、メカ本体の衝突時に停止

        float tDown = Mathf.Abs(craneRoot.position.y - descendLimitY) / moveSpeedY;

        OpenArm();
        descendTween = craneRoot.DOMoveY(descendLimitY, tDown).SetEase(Ease.InOutSine);

        descendTween.OnComplete(() =>
        {
            CatchAndReturn(backMode);
        });
    }
    public void CatchAndReturn(int backMode)
    {
        // この関数では、引数backModeの値に応じて、下降→上昇後の動きが変わる。
        // 0では現在位置から初期位置までの最短距離を直線状に進み、
        // 1では現在位置から初期位置まで、Z軸移動→X軸移動の順で戻る。
        // 2では現在位置から初期位置まで、X軸移動→Z軸移動の順で戻る。
        // 3では、SecondTargetへ移動後にアームを離し、その後初期位置へ直線状に進む。
        currentTween?.Kill();

        // 設定されていない場合は筐体の中心座標を設定。
        Vector3 secondTargetPos = new Vector3(0,0,0);

        float tUp = Mathf.Abs(descendLimitY - homePos.y) / moveSpeedY;
        
        float tBack = Vector3.Distance(new Vector3(craneRoot.position.x, 0, craneRoot.position.z), 
                                        new Vector3(homePos.x, 0,homePos.z)) / moveSpeedXZ;

        float tBack_Z = Vector3.Distance(new Vector3(craneRoot.position.x, 0, craneRoot.position.z),
                                         new Vector3(craneRoot.position.x, 0, homePos.z)) / moveSpeedXZ;

        float tBack_X = Vector3.Distance(new Vector3(craneRoot.position.x, 0, craneRoot.position.z),
                                         new Vector3(homePos.x, 0, craneRoot.position.z)) / moveSpeedXZ;
        
        float tMoveST = Vector3.Distance(new Vector3(craneRoot.position.x,0,craneRoot.position.z), 
                                         new Vector3(secondTarget.x, 0, secondTarget.z)) / moveSpeedXZ;

        Sequence seq = DOTween.Sequence();
        seq.AppendInterval(1f);
        // アームを閉じる
        seq.AppendCallback(() => CloseArm());

        // 待機
        seq.AppendInterval(waitSeconds);

        // 上昇
        seq.Append(craneRoot.DOMoveY(homePos.y, tUp).SetEase(Ease.InOutSine));

        // ここから分岐
        switch (backMode)
        {
            case 0:// 最短距離移動
                seq.Append(craneRoot.DOMoveX(homePos.x, tBack).SetEase(Ease.Linear));
                seq.Join(craneRoot.DOMoveZ(homePos.z, tBack).SetEase(Ease.Linear));
                break;
            case 1:// Z→Xの順に移動
                seq.Append(craneRoot.DOMoveZ(homePos.z, tBack_Z).SetEase(Ease.Linear));
                seq.Append(craneRoot.DOMoveX(homePos.x, tBack_X).SetEase(Ease.Linear));
                break;
            case 2:// X→Zの順に移動
                seq.Append(craneRoot.DOMoveX(homePos.x, tBack_X).SetEase(Ease.Linear));
                seq.Append(craneRoot.DOMoveZ(homePos.z, tBack_Z).SetEase(Ease.Linear));
                break;
            case 3:
                seq.Append(craneRoot.DOMoveX(homePos.x, tMoveST).SetEase(Ease.Linear));
                seq.Join(craneRoot.DOMoveZ(homePos.z, tMoveST).SetEase(Ease.Linear));

                seq.AppendCallback(() => OpenArm());
                seq.AppendInterval(1f);

                seq.AppendCallback(() => CloseArm());

                seq.Append(craneRoot.DOMoveX(homePos.x, tBack).SetEase(Ease.Linear));
                seq.Join(craneRoot.DOMoveZ(homePos.z, tBack).SetEase(Ease.Linear));
                break;
        }

        // 初期位置で開いて閉じる動作
        seq.AppendCallback(() => OpenArm());
        seq.AppendInterval(1f);
        seq.AppendCallback(() => CloseArm());

        seq.OnComplete(() =>
        {
            // Debug.Log("finish");
            isPushX = false;
            isPushZ = false;
            isSequence = false;
        });
        currentTween = seq;
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKey(KeyCode.N) && !isPushX && !isSequence)
        {
            transform.position -= new Vector3(moveSpeedXZ*Time.deltaTime,0,0);
        }
        if(Input.GetKeyUp(KeyCode.N))
        {
            isPushX = true;
        }


        if(Input.GetKey(KeyCode.M) && isPushX && !isPushZ && !isSequence)
        {
            transform.position += new Vector3(0,0,moveSpeedXZ*Time.deltaTime);
        }
        if(Input.GetKeyUp(KeyCode.M) && isPushX && !isPushZ)
        {
            isPushZ = true;
            isSequence = true;
        }
        // 下降ストップボタン
        if(Input.GetKeyDown(KeyCode.Comma) && isPushX && isPushZ)
        {
            descendTween.Kill();
            CatchAndReturn(backMode);
        }

        if(isSequence)
        {
            StartDescend();
            isSequence = false;
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Prize") || other.CompareTag("Equipment"))
        {
            if(descendTween != null && descendTween.IsActive() && descendTween.IsPlaying())
            {
                descendTween.Kill();
                CatchAndReturn(backMode);
            }
        }
    }
}
