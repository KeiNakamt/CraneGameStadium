using System.ComponentModel;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class MovingCeiling : MonoBehaviour
{
    public float amplitude = 1.0f;   // 揺れ幅
    public float frequency = 0.5f;   // 揺れの速さ(Hz)
    Vector3 basePos;
    Rigidbody rb;

    [SerializeField] LineRenderer lineRenderer;
    [SerializeField] Transform startPoint;
    [SerializeField] Transform endPoint;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.isKinematic = true;
        basePos = rb.position;
    }

    void FixedUpdate()
    {
        float t = Time.fixedTime;
        Vector3 target = basePos + new Vector3(amplitude * Mathf.Sin(2f * Mathf.PI * frequency * t), 0, 0);
        //rb.MovePosition(target);

        var positions = new Vector3[] {startPoint.position, endPoint.position, };
        lineRenderer?.SetPositions(positions);
    }
}