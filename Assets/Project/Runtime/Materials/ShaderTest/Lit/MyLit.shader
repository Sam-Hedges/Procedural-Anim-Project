Shader "SamHedges/MyLit" {
	
	// Properties are options set per material, exposed by the material inspector
		Properties {
			[Header(Surface Options)]
			// [MainColor] & [MainTexture] allows Material.color & Material.mainTexture in C# to use the correct properties
			[MainTexture] _ColourMap("Albedo", 2D) = "white" {}
			[MainColor] _ColourTint("Colour", Color) = (1,1,1,1)
		}
	
	// Sub-shaders allow for different behaviour and options for different pipelines and platforms
	Subshader {
		// Tags are shared by all passed in this subshader
		Tags {"RenderPipeline" = "UniversalPipeline"}

		// Shader can have several passes which are used to render different data about the material and
		// each pass has it's own vertex and fragment function and shader variant keywords
		Pass {
			Name "ForwardLit" // For debugging 
			Tags {"LightMode" = "UniversalForward"} // Pass specific tags
			// "UniversalForward" tells unity this is the main lighting pass of this shader

			HLSLPROGRAM // Begin HLSL code

			// Register our programmable stage functions
			// #pragma has variety of uses related to shader metadata
			// vertex and fragment sub-commands register the corresponding functions
			// to the containing pass; the names MUST MATCH those within the hlsl file
			#pragma vertex Vertex
			#pragma fragment Fragment

			// Include our hlsl file
			#include "MyLitForwardLitPass.hlsl"

			ENDHLSL
		}
	}
}
