//
// Copyright © Daniel Shervheim, 2019
// www.danielshervheim.com
//

Shader "Custom/CampfireShader" {
	Properties {
		_Albedo1 ("Albedo1", Color) = (1,1,1,1)
		_Albedo2 ("Albedo2", Color) = (1,1,1,1)
		_AlbedoAdd ("AlbedoAdd", float) = 0.0
		_AlbedoMult ("AlbedoMult", float) = 1.0
		[HDR]
		_Emission1 ("Emission1", Color) = (0,0,0,1)
		[HDR]
		_Emission2 ("Emission2", Color) = (0,0,0,1)
		_EmissionAdd ("EmissionAdd", float) = 0.0
		_EmissionMult ("EmissionMult", float) = 1.0
	}
	SubShader {
		Tags { "RenderType"="Opaque" }
		LOD 200

		CGPROGRAM
		// Physically based Standard lighting model, and enable shadows on all light types
		#pragma surface surf Standard fullforwardshadows vertex:vert

		// Use shader model 3.0 target, to get nicer looking lighting
		#pragma target 3.0

		struct Input {
			float3 worldPosition;
		};

		float4 _Albedo1;
		float4 _Albedo2;
		float _AlbedoAdd;
		float _AlbedoMult;
		float4 _Emission1;
		float4 _Emission2;
		float _EmissionAdd;
		float _EmissionMult;

		void vert(inout appdata_full v, out Input data) {
			data.worldPosition = mul(unity_ObjectToWorld, v.vertex).xyz;
		}

		void surf (Input IN, inout SurfaceOutputStandard o) {

			float at = saturate(_AlbedoMult*IN.worldPosition.y + _AlbedoAdd);
			float ot = saturate(_EmissionMult*IN.worldPosition.y + _EmissionAdd);

			o.Albedo = lerp(_Albedo1, _Albedo2, at);
			o.Emission = lerp(_Emission1, _Emission2, ot);
			o.Metallic = 0;
			o.Smoothness = 0;
		}
		ENDCG
	}
	FallBack "Diffuse"
}
