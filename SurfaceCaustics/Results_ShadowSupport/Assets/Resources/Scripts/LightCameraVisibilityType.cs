/// <summary>
/// Defines the types of an object the camera is expcted to "see"
/// </summary>
public enum LightCameraVisibilityType 
{
    /// <summary>
    /// Indicates the the camera is supposed to see specular objects
    /// </summary>
    SPECULAR,

    /// <summary>
    /// Indicates that the camera is supposed to see receiving objects
    /// </summary>
    RECEIVER,

    /// <summary>
    /// (Deprecated) Indicates that the camera is supposed to see some other object for testing
    /// </summary>
    OTHER
}
