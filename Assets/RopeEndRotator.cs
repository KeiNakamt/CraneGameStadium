using UnityEngine;
public class RopeEndRotator : MonoBehaviour
{
    [SerializeField] LineRenderer lineRenderer;
    [SerializeField] private Transform StartPos;
    [SerializeField] private Transform EndPos;

    void Update()
    {
        var positions = new Vector3[] {StartPos.position, EndPos.position, };
        lineRenderer?.SetPositions(positions);
    }
}