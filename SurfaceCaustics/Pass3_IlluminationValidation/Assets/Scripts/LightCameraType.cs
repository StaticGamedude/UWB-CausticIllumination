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

    CAUSTIC_REFRACTION_RAY,

    /// <summary>
    /// Value indicates that the camera is expected to store color information of the specular object
    /// </summary>
    CAUSTIC_COLOR,

    /// <summary>
    /// Value indicates that the camera is expected to store caustic flux values
    /// </summary>
    CAUSTIC_FLUX,

    /// <summary>
    /// Value indicates that the camera is expected to store caustic intensity values
    /// </summary>
    CAUSTIC_INTENSITY,

    /// <summary>
    /// Value indicates that the camera is expected to store the final caustic data needed to support resulting caustic effect
    /// </summary>
    CAUSTIC_FINAL,

    /// <summary>
    /// Value used when testing miscellaneous properties
    /// </summary>
    OTHER,
}
