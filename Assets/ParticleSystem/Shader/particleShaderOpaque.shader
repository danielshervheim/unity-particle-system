//
// Copyright © Daniel Shervheim, 2019
// www.danielshervheim.com
//

Shader "Custom/particleShaderOpaque" {
	Properties {
		// these are all set in Emitter.cs
		_AlbedoBirth ("Albedo (birth)", Color) = (0,0,0,1)
		_AlbedoDeath ("Albedo (death)", Color) = (0,0,0,1)
		_AlbedoTexture ("Albedo (texture)", 2D) = "defaulttexture" {}

		[Normal]
		_NormalTexture ("Normal (texture)", 2D) = "defaulttexture" {}

		[HDR]
		_EmissionBirth ("Emission (birth)", Color) = (0,0,0,1)
		[HDR]
		_EmissionDeath ("Emission (death)", Color) = (0,0,0,1)
		[HDR]
		_EmissionTexture ("Emission (texture)", 2D) = "defaulttexture" {}

		_SizeBirth ("Size (birth)", float) = 1.0
		_SizeDeath ("Size (death)", float) = 0.0

		_Smoothness ("Smoothness", Range(0,1)) = 0.5
		_Metallic ("Metallic", Range(0,1)) = 0.0

		[MaterialToggle]
		_FollowVelocity ("Follow Velocity", Range(0, 1)) = 0
	}
	SubShader {
		Tags { "RenderType"="Opaque" }
		LOD 200

		/* We must prevent culling or else the shadow calculations aren't done for 
		when the quads are not technically "facing" us, even though they are via
		vertex rotation. */
		Cull Off

		CGPROGRAM

		/* vertex:vert specifices a custom vertex function, and addshadow forces
		shadow recalculations so particles correctly shadow, and are shaded. */
		#pragma surface surf Standard vertex:vert nolightmap addshadow
		
		/* Enables instancing support for this shader. */
		#pragma instancing_options procedural:setup assumeuniformscaling

		/* Target shader model 5.0, for instancing. */
		#pragma target 5.0

		fixed4 _AlbedoBirth;
		fixed4 _AlbedoDeath;
		sampler2D _AlbedoTexture;

		sampler2D _NormalTexture;

		half4 _EmissionBirth;
		half4 _EmissionDeath;
		sampler2D _EmissionTexture;

		float _SizeBirth;
		float _SizeDeath;

		half _Smoothness;
		half _Metallic;

		int _FollowVelocity;

		struct Input {
			float3 position;
			float3 velocity;
			float lifetime;
			float age;

			float2 uv_AlbedoTexture;
			float2 uv_EmissionTexture;
			float2 uv_NormalTexture;
			float2 uv_AlphaTexture;
		};

		#ifdef UNITY_PROCEDURAL_INSTANCING_ENABLED
		#include "Utilities.cginc"
		struct Particle {
			float3 position;
			float3 originalPosition;
			float3 velocity;
			float3 originalVelocity;
			float lifetime;
			float age;
		};
       	StructuredBuffer<Particle> particleBuffer;
    	#endif

       	void vert(inout appdata_full v, out Input data) {
       		UNITY_INITIALIZE_OUTPUT(Input, data);
            #ifdef UNITY_PROCEDURAL_INSTANCING_ENABLED
            // set the frag parameters
       		data.position = particleBuffer[unity_InstanceID].position;
       		data.velocity = particleBuffer[unity_InstanceID].velocity;
       		data.lifetime = particleBuffer[unity_InstanceID].lifetime;
       		data.age = particleBuffer[unity_InstanceID].age;

       		// get the forward look direction vector
       		float3 forward = mul((float3x3)unity_CameraToWorld, float3(0,0,1));

       		// and the up vector
       		float3 up = (_FollowVelocity == 1) ? normalize(particleBuffer[unity_InstanceID].velocity) : float3(0,1,0);

       		// create a rotation so the mesh is facing the viewer, and pointed in the correct direction
       		float4 quat = lookRotation(forward, up);

       		// rotate the vertex according to the look rotation.
       		float3 positionRotated = rotateVector(v.vertex.xyz, quat);

       		// scale the vertex and set it its new position, offset by the particle position.
       		float size = lerp(_SizeBirth, _SizeDeath, saturate(particleBuffer[unity_InstanceID].age / particleBuffer[unity_InstanceID].lifetime));
       		v.vertex.xyz = positionRotated*size + particleBuffer[unity_InstanceID].position;

       		// rotate the normal to match the rotated point.
		  	v.normal = rotateVector(v.normal, quat);
			// v.normal = float3(0, 1, 0);
			#endif
		}

		// required for DrawMeshInstancedIndirect().
		void setup () {}

		void surf (Input IN, inout SurfaceOutputStandard o) {
			float t = saturate(IN.age/IN.lifetime);
			o.Albedo = tex2D(_AlbedoTexture, IN.uv_AlbedoTexture) * lerp(_AlbedoBirth, _AlbedoDeath, t);
			o.Emission = tex2D(_EmissionTexture, IN.uv_EmissionTexture) * lerp(_EmissionBirth, _EmissionDeath, t);
			o.Normal = tex2D(_NormalTexture, IN.uv_NormalTexture);
			o.Metallic = _Metallic;
			o.Smoothness = _Smoothness;
		}

		ENDCG
	}
	
	FallBack "Diffuse"
}