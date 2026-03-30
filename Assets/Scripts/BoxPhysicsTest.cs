using Unity.VisualScripting;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class BoxPhysicsTest : MonoBehaviour
{
    [Header("力の強さ")]
    [SerializeField] private float pushForce = 10f;

    private Rigidbody rb;
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.F))
        {
            ApplySideForce();
        }
    }

    void ApplySideForce()
    {
        Vector3 forceDirection = transform.right;
        rb.AddForce(forceDirection * pushForce, ForceMode.Impulse);        
    }
}
