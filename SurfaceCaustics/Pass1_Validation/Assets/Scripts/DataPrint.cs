using UnityEngine;

/// <summary>
/// Utility class that can be placed on objects to view internal information
/// </summary>
public class DataPrint : MonoBehaviour
{
    /// <summary>
    /// The object's current world position
    /// </summary>
    public Vector3 WorldRotation;

    /// <summary>
    /// The object's current world position
    /// </summary>
    public Vector3 WorldPosition;

    // Update is called once per frame
    void Update()
    {
        this.WorldPosition = this.transform.position;
        this.WorldRotation = this.transform.rotation.eulerAngles;
    }
}
