using UnityEngine;

/// <summary>
/// Marks an object as a valid scan target.
/// </summary>
public class ScanTarget : MonoBehaviour
{
    [Tooltip("Optional override for the scan point. If empty, the transform position is used.")]
    [SerializeField] private Transform scanPoint;

    public Vector3 ScanPoint
    {
        get
        {
            return scanPoint != null ? scanPoint.position : transform.position;
        }
    }
}
