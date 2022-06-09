# Result_ShadowSupport
Attempts to follow the algorithm as described in the article [Caustics Mapping: An Image-Space Technique for Real-Time Caustics](https://ieeexplore.ieee.org/document/4069236). This solution is capable of rendering the caustic effect onto receiving objects. Additionally, users can specify whether they would like shadows rendered as well.

## Key Source Details

### Assets\Resources\Scripts\LightSource.cs
Script responsible for generating the caustic cameras. Upon application start, the necessary caustic cameras are instantiated and added to the light source objects. The target textures for each camera are also created at runtime. Each light source receives a unique ID (starting at 0) which is later used by the shaders to determine which light source the light data is coming from.

### Assets\Resources\Scripts\LightCamera.cs
Handles the operational logic for a single caustic camera. This script is responsible for setting the camera information in the shaders such as the camera's transform, the camera's render texture, and more. 

### Assets\Resources\Shaders\CommonFunctions.cginc
This file contains the core logic for generating caustic effects. It defines the method
for calcluating the vertex estimation ("splat") position and calculating the refraction ray direciton

### Assets\Resources\Shaders\CausticFluxFunctions.cginc
This file is responsible for calcluating the flux values for each specular vertex. Shaders CausticFluxShader_0 and CausticFluxShader_1 use this file to compute flux. The main difference between CausticFluxShader_0 and CausticFluxShader_1 is the data passed to the methods defined in CausticFluxFunctions.cginc.

### Assets\Resources\Shaders\CausticFinalFunctions.cginc
This file contains the definitions for computing the final caustic effect for light sources. The methods here use data for flux, distance, color to compute the final caustic color.