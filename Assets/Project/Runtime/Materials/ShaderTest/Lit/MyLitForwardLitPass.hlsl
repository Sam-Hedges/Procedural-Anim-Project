// Pull in URP library functions and our own common functions
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

// This file contains the vertex and fragment functions for the forward lit pass
// This is the shader pass that computes visible colours for a material
// by reading material, light, shadow, etc. data
TEXTURE2D(_ColourMap); SAMPLER(sampler_ColourMap);
float4 _ColourMap_ST; // Automatically set by Unity. Used in TRANSFORM_TEX to apply UV tiling
float4 _ColourTint;

// This attributes struct receives data about the mesh we're currently rendering
// Data automatically populates fields according to their semantic
struct Attributes
{
    float3 positionOS : POSITION; // Position in object space
    float2 uv : TEXCOORD0; // Material texture UVs
};

// This struct is output by the vertex function and input to the fragment function
// Note that fields will be transformed by the intermediary rasterization stage
struct Interpolators
{
    // This value should contain the position in clip space when output from the
    // vertex function. It will be transformed into the pixel position of the
    // current fragment on the screen when read from the fragment function
    float4 positionCS : SV_POSITION;

    // The following variables will retain their values from the vertex stage, except the
    // rasterizer will interpolate them between vertices
    float2 uv : TEXCOORD0; // Material texture UVs
};

// The vertex function which runs for each vertex on the mesh.
// It must output the position on the screen, where each vertex should appear,
// as well as any data the fragment function will need
Interpolators Vertex(Attributes input)
{
    Interpolators output;

    // These helper functions, found in URP/ShaderLib/ShaderVariablesFunctions.hlsl
    // transform object space values into world and clip space
    VertexPositionInputs posnInputs = GetVertexPositionInputs(input.positionOS);

    // Pass position and orientation data to the fragment function
    output.positionCS = posnInputs.positionCS;
    output.uv = TRANSFORM_TEX(input.uv, _ColourMap);

    return output;
}

// The fragment function runs once per fragment, akin to a pixel on the screen but virtualized
// It must output the final colour of this pixel hence the function is a float4
// The function is tagged with a semantic so that the return value is interpreted in a specific way
float4 Fragment(Interpolators input) : SV_TARGET
{
    float2 uv = input.uv;
    
    // Sample the colour map
    float4 colourSample = SAMPLE_TEXTURE2D(_ColourMap, sampler_ColourMap, uv);
    
    return colourSample * _ColourTint;
}
