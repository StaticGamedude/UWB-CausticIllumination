# Experimentation project for determining the accumulation of flux values in a texture

## Description
There are two scenes in the project which can both be found in the Scenes directory:

- SampleScene
- BlendScene

### SampleScene
This scene attempts to read values from a texture that is currently being rendered. The TestLightCam object uses a replacement shader (like the caustic cameras) and outputs the results to a render texture. This render texture is then passed as a reference to the shader (through a material). A material is created from this shader and used on an object ("Plane (1)")

### BlendScene
This scene test some of the blend methods as defined by [Unity's Blend Command](https://docs.unity3d.com/Manual/SL-Blend.html). Simply, this project attempts to blend color from the source texture to a color hard coded in a shader