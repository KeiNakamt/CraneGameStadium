using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class threeArmCatcher : MonoBehaviour
{
    // コントローラーでの操作を可能にする。
    private CraneInputs craneInputs;
    private Vector2 moveInput;
    [SerializeField] public GameObject mecha;
    [SerializeField] public GameObject ropeJoint;
    [SerializeField] private Transform craneHead;

    public GameObject Arm1;
    public GameObject Arm2;
    public GameObject Arm3;

    private Transform arm1_transform;
    private Transform arm2_transform;
    private Transform arm3_transform;
    ConfigurableJoint configurableJoint_mecha;
    ConfigurableJoint configurableJoint_rope;

    public bool isUp = false;

    [Header("Params")]

    [SerializeField] private int maxPrice = 5;
    [Tooltip("確率(天井)を設定します")]

    private int counter = 0;
    // プレイ回数

    [SerializeField] private float moveSpeedXZ = 0.7f;

    [SerializeField] public int defaultSpring = 100000;
    [Tooltip("デフォルトのスプリング値")]
    [SerializeField] public int defaultDamper = 100;
    [Tooltip("デフォルトの減衰値")]

    [SerializeField] public int isDownSpring = 0;
    [Tooltip("キャッチャー下降時のスプリング値")]

    [SerializeField] public int isDownDamper = 100;
    [Tooltip("キャッチャー下降時の減衰値")]


    [SerializeField] public int isUpSpring = 100000;
    [Tooltip("機体が糸を巻き上げる時のスプリング値")]

    [SerializeField] public int isUpDamper = 100000;
    [Tooltip("機体が糸を巻き上げる時の減衰値")]

    [SerializeField] public int powerRemoveAmount = 13;
    [Tooltip("キャッチャーがパワーを抜く際の開き具合")]

    [SerializeField] public int powerRemoveTime = 1;
    [Tooltip("キャッチャーが上昇してからパワーを抜くまでの時間")]

    [SerializeField] public int totalUpTime = 3;
    [Tooltip("キャッチャーが上昇しきるまでの時間(要調整)")]

    private Vector3 homePos;

    private Tween currentTween;
    // Start is called before the first frame update
    void Start()
    {
        configurableJoint_mecha = mecha.GetComponent<ConfigurableJoint>();
        configurableJoint_rope = ropeJoint.GetComponent<ConfigurableJoint>();

        arm1_transform = Arm1.transform;
        arm2_transform = Arm2.transform;
        arm3_transform = Arm3.transform;

        homePos = craneHead.position;

        counter = 0;
    }
    void Awake()
    {
        if(!craneHead) craneHead = transform;
        homePos = craneHead.position;

        craneInputs = new CraneInputs();

        craneInputs.Player.Move.performed += context => moveInput = context.ReadValue<Vector2>();

        craneInputs.Player.Move.canceled += context => moveInput = Vector2.zero;
    }

    void OnEnable()
    {
        craneInputs.Enable();
    }

    void OnDisable()
    {
        craneInputs.Disable();
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.S) || craneInputs.Player.Drop.WasPressedThisFrame())
        {
            //configurableJoint_mecha.xMotion = ConfigurableJointMotion.Free;
            //configurableJoint.yMotion = ConfigurableJointMotion.Free;
            //configurableJoint.zMotion = ConfigurableJointMotion.Free;

            // positionSpringを0に設定。バネが戻ろうとする力は0になるため、ローカルx軸に沿ってキャッチャーは落下
            JointDrive drive = configurableJoint_mecha.xDrive;
            drive.positionSpring = isDownSpring;
            drive.positionDamper = isDownDamper;
            configurableJoint_mecha.xDrive = drive;

            configurableJoint_mecha.angularXMotion = ConfigurableJointMotion.Free;
            configurableJoint_mecha.angularYMotion = ConfigurableJointMotion.Free;
            configurableJoint_mecha.angularZMotion = ConfigurableJointMotion.Free;

            StartCoroutine("ropeUp");
        }
    }

    IEnumerator ropeUp()
    {
        yield return new WaitForSeconds(2f);
        // 各ツメを閉じる
        arm1_transform.DOLocalRotate(new Vector3(0,0,-25),0.3f,RotateMode.LocalAxisAdd);
        arm2_transform.DOLocalRotate(new Vector3(0,0,-25),0.3f,RotateMode.LocalAxisAdd);
        arm3_transform.DOLocalRotate(new Vector3(0,0,-25),0.3f,RotateMode.LocalAxisAdd);

        yield return new WaitForSeconds(1.5f);
        // positionSpringを1000に設定。ローカルx軸に沿ってキャッチャーは上昇
        JointDrive drive = configurableJoint_mecha.xDrive;
        drive.positionDamper = isUpDamper;
        configurableJoint_mecha.xDrive = drive;

        //yield return new WaitForSeconds(0.5f);

        drive.positionSpring = isUpSpring;
        configurableJoint_mecha.xDrive = drive;

        // パワーを抜く
        StartCoroutine("powerRemover");
        yield return new WaitForSeconds(totalUpTime);

        // クレーンの設定を基に戻す処理
        configurableJoint_mecha.angularXMotion = ConfigurableJointMotion.Locked;
        configurableJoint_mecha.angularYMotion = ConfigurableJointMotion.Locked;
        configurableJoint_mecha.angularZMotion = ConfigurableJointMotion.Locked;
        drive.positionSpring = defaultSpring;
        drive.positionDamper = defaultDamper;
        configurableJoint_mecha.xDrive = drive;

        // クレーンが戻る動作
        PlaySequenceTo(new Vector3(craneHead.transform.position.x,0,craneHead.transform.position.z));

        yield return new WaitForSeconds(1f);

        // クレーンを元の開き幅に戻す
        arm1_transform.DOLocalRotate(new Vector3(0,0,25),0.3f,RotateMode.LocalAxisAdd);
        arm2_transform.DOLocalRotate(new Vector3(0,0,25),0.3f,RotateMode.LocalAxisAdd);
        arm3_transform.DOLocalRotate(new Vector3(0,0,25),0.3f,RotateMode.LocalAxisAdd);
        if(counter<maxPrice)counter++;
    }

    IEnumerator powerRemover()
    {
        yield return new WaitForSeconds(powerRemoveTime);

        // 確率機特有の力抜き。counterがmax_priceと等しければパワーを抜かない。
        if(counter != maxPrice){
        // 開く
        arm1_transform.DOLocalRotate(new Vector3(0,0,powerRemoveAmount),0.3f,RotateMode.LocalAxisAdd);
        arm2_transform.DOLocalRotate(new Vector3(0,0,powerRemoveAmount),0.3f,RotateMode.LocalAxisAdd);
        arm3_transform.DOLocalRotate(new Vector3(0,0,powerRemoveAmount),0.3f,RotateMode.LocalAxisAdd);
        
        yield return new WaitForSeconds(1f);

        // 閉じる
        arm1_transform.DOLocalRotate(new Vector3(0,0,-powerRemoveAmount),0.3f,RotateMode.LocalAxisAdd);
        arm2_transform.DOLocalRotate(new Vector3(0,0,-powerRemoveAmount),0.3f,RotateMode.LocalAxisAdd);
        arm3_transform.DOLocalRotate(new Vector3(0,0,-powerRemoveAmount),0.3f,RotateMode.LocalAxisAdd);
        }
    }

    public void PlaySequenceTo(Vector3 targetXZ)
    {
        currentTween?.Kill();

        Vector3 targetPos = new Vector3(targetXZ.x,homePos.y, targetXZ.z);

        float tBack = Vector3.Distance(new Vector3(targetPos.x, 0, targetPos.z),
                                       new Vector3(homePos.x, 0, homePos.z)) / moveSpeedXZ;

        Sequence seq = DOTween.Sequence();
        // 帰還
        seq.Append(craneHead.DOMoveX(homePos.x, tBack).SetEase(Ease.Linear));
        seq.Join  (craneHead.DOMoveZ(homePos.z, tBack).SetEase(Ease.Linear));

        currentTween = seq;

    }
}
