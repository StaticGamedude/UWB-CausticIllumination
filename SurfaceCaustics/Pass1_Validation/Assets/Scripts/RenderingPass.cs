using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Utility enum to help focus on a specific rendering pass. The render passes are described here:
/// https://ieeexplore.ieee.org/document/4069236
/// </summary>
public enum RenderingPass 
{
    /// <summary>
    /// Represents the initial pass of the caustic rendering algorithm. This pass is expected to create
    /// the receiving object (world) position texture, the specular object (world) position texture, 
    /// and the specular object (world) normal texture
    /// </summary>
    PASS_1,

    /// <summary>
    /// Represents the second pass of the caustic rendering algorithm. This pass is expected to create
    /// the caustic map texture
    /// </summary>
    PASS_2,

    /// <summary>
    /// Represents the final pass of the caustic rendering algorithm. This pass is expected to compute the
    /// final illumination on the receiving object
    /// </summary>
    PASS_3
}
