/// <summary>
/// Defines the types of light cameras which are expected to store data to a render texture
/// </summary>
public enum LightCameraType 
{
    /// <summary>
    /// Value indicates that the camera is expected to store the world positions of seen refractive objects
    /// </summary>
    REFRACTIVE_POSITION,

    /// <summary>
    /// Value indicates that the camera is expected to store the world normals of the seen refractive objects
    /// </summary>
    REFRACTIVE_NORMAL,

    /// <summary>
    /// Value indicates that the camera is expected to store the world positions of the receiving objects
    /// </summary>
    RECEIVING_POSITION,

    /// <summary>
    /// Value indicates that the camera is expected to store caustic position information
    /// </summary>
    CAUSTIC,
}
