using UnityEngine;

public class Globals
{
    /// <summary>
    /// Shadow/light bias as described here: 
    /// https://learnopengl.com/Advanced-Lighting/Shadows/Shadow-Mapping#:~:text=We%20can%20solve%20this%20issue%20with%20a%20small%20little%20hack%20called%20a%20shadow%20bias 
    /// </summary>
    public readonly static Matrix4x4 BIAS = new Matrix4x4() {
        m00 = 0.5f, m01 = 0,    m02 = 0,    m03 = 0.5f,
        m10 = 0,    m11 = 0.5f, m12 = 0,    m13 = 0.5f,
        m20 = 0,    m21 = 0,    m22 = 0.5f, m23 = 0.5f,
        m30 = 0,    m31 = 0,    m32 = 0,    m33 = 1,
    };

    /// <summary>
    /// Tag used for objects which are to be labled as "specular" objects
    /// </summary>
    public readonly static string SPECULAR_OBJECT_SHADER_TAG = "SpecularObj";

    /// <summary>
    /// Tag used for objects which are to receiving caustic effects on them
    /// </summary>
    public readonly static string RECEIVING_OBJECT_SHADER_TAG = "SpecularReceiver";

    #region Shader parameter names

    /// <summary>
    /// Represents the parameter which stores vertex positions of a specular object as seen from a light source
    /// </summary>
    public readonly static string SHADER_PARAM_REFRACTION_POSITION_TEXTURE = "_SpecularPosTexture";

    /// <summary>
    /// Represents the parameter which stores vertex normals of a specular object as seen from a light source
    /// </summary>
    public readonly static string SHADER_PARAM_REFRACTION_NORMAL_TEXTURE = "_SpecularNormTexture";

    /// <summary>
    /// Represents the parameter which stores vertex positions of a receiving object seen from a light source
    /// </summary>
    public readonly static string SHADER_PARAM_RECEIVING_POSITION_TEXTURE = "_ReceivingPosTexture";

    /// <summary>
    /// Represents the parameter which stores the caustic map details
    /// </summary>
    public readonly static string SHADER_PARAM_CAUSTIC_MAP_TEXTURE = "_CausticMapTexture";

    public readonly static string SHADER_PARAM_CAUSTIC_REFRACTION_RAY_TEXTURE = "_CausticRefractionRayTexture";

    /// <summary>
    /// Represents the parameter which stores the caustic map details
    /// </summary>
    public readonly static string SHADER_PARAM_CAUSTIC_COLOR_MAP_TEXTURE = "_CausticColorMapTexture";

    /// <summary>
    /// Represents the parameter which stores the caustic flux values
    /// </summary>
    public readonly static string SHADER_PARAM_CAUSTIC_FLUX_TEXTURE = "_CausticFluxTexture";

    /// <summary>
    /// Represents the parameter which stores the caustic intensity values
    /// </summary>
    public readonly static string SHADER_PARAM_CAUSTIC_INTENSITY_TEXTURE = "_CausticIntensityTexture";

    /// <summary>
    /// Represents the parameter which stores the final caustic details
    /// </summary>
    public readonly static string SHADER_PARAM_CAUSTIC_FINAL_MAP_TEXTURE = "_CausticColorMapTexture";

    /// <summary>
    /// Represents the parameter which stores the transformation to convert positions/normals into
    /// the light's space.
    /// </summary>
    public readonly static string SHADER_PARAM_LIGHT_MATRIX = "_LightMatrix";

    /// <summary>
    /// Represents the parameter which stores the world to camera matrix
    /// </summary>
    public readonly static string SHADER_PARAM_LIGHT_CAMERA_MATRIX = "_LightCamMatrix";

    /// <summary>
    /// Represents the parameter which stores the light camera's view project matrix;
    /// </summary>
    public readonly static string SHADER_PARAM_LIGHT_VIEW_PROJECTION_MATRIX = "_LightViewProjectionMatrix";

    /// <summary>
    /// Represents the parameter which stores the distance of how far the light camera can see
    /// </summary>
    public readonly static string SHADER_PARAM_LIGHT_CAMERA_FAR = "_LightCam_Far";

    public readonly static string SHADER_PARAM_LIGHT_WORLD_POS = "_LightWorldPosition";

    #endregion
}
