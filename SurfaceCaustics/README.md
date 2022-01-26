# Surface Caustic Illumination

## Description
The projects contained in this directory attempt to replicate the findings by other researchers which attempt to render surface caustics in real-time

## Projects

### Pass 1 
Attempts to follow the algorithm as described in the article [Caustics Mapping: An Image-Space Technique for Real-Time Caustics](https://ieeexplore.ieee.org/document/4069236). This attempts to reproduce the results as described in the first pass of the algorithm which creates the following:

- A texture containing the world positions of the specular object as seen from a light source
- A texture containing the world normals of the specular object as seen from a light source
- A texture containing the world positions of the receiving object as seen from a light source

The scene in this project reads from these textures and renders geometry to validate the results.

### Pass 2
Attempts to follow the algorithm as described in the article [Caustics Mapping: An Image-Space Technique for Real-Time Caustics](https://ieeexplore.ieee.org/document/4069236). Attempts to reproduce the results as described in the second pass of the algorithm which attempts to create the caustic map texture. The scene in this project reads from the caustic map texture renders geometry to validate the results.