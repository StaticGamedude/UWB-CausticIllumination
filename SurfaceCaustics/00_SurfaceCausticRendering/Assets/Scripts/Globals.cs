using System.Collections;
using System.Collections.Generic;
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


    #region Shader parameter names

    /// <summary>
    /// Represents the parameter which stores vertex positions as seen from a light source
    /// </summary>
    public readonly static string SHADER_PARAM_POSITION_TEXTURE = "_SpecularPosTexture";

    /// <summary>
    /// Represents the parameter which stores vertex normals as seen from a light source
    /// </summary>
    public readonly static string SHADER_PARAM_NORMAL_TEXTURE = "_SpecularNormTexture";

    /// <summary>
    /// Represents the parameter which stores the transformation to convert positions/normals into
    /// the light's space.
    /// </summary>
    public readonly static string SHADER_PARAM_LIGHT_MATRIX = "LightMatrix";

    #endregion
}
