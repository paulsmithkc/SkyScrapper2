// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'
// Upgrade NOTE: replaced '_World2Object' with 'unity_WorldToObject'

Shader "Rubberband/ArmyMen" {
	Properties {
		_Color ("Main Color", Color) = (1,1,1,1)
		_SpecColor ("Specular Color", Color) = (1,1,1,1)
		_Ramp ("Toon Ramp (RGB)", 2D) = "gray" {} 
		_Shininess ("Shininess", Float) = 3
		_OutlineColor("Outline Color", Color) = (0,0,0,1)
		_OutlineWidth("Outline Width", Float) = 3
	}
	SubShader {
		Tags { "RenderType" = "Opaque" "LightMode" = "ForwardBase" }
		LOD 200
		
		Pass {
			Name "FORWARD"
			Blend One Zero
			Lighting On
			Cull Back
			ZWrite On

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile_fog
			#pragma multi_compile_fwdbase
			#include "UnityCG.cginc"
			#include "AutoLight.cginc"
			
			uniform float4 _Color;
			uniform float4 _SpecColor;
			uniform sampler2D _Ramp;
			uniform float _Shininess;
			
			uniform float4 _LightColor0;
			
			struct vert_input {
				float4 vertex : POSITION;
				float3 normal : NORMAL;
			};

#ifdef SHADER_API_D3D9
			struct vert_output {
				float4 pos : SV_POSITION;
				float3 normal : TEXCOORD0;
				float4 posWorld : TEXCOORD1;
				UNITY_FOG_COORDS(2)
				LIGHTING_COORDS(3, 4)
			};
#else
			struct vert_output {
				float4 pos : SV_POSITION;
				float3 normal : NORMAL;
				float4 posWorld : TEXCOORD0;
				UNITY_FOG_COORDS(1)
				LIGHTING_COORDS(2, 3)
			};
#endif
			
			vert_output vert(vert_input v) {
				vert_output o;
				o.pos = mul(UNITY_MATRIX_MVP, v.vertex);
				o.normal = normalize(mul(float4(v.normal, 0.0), unity_WorldToObject).xyz);
				o.posWorld = mul(unity_ObjectToWorld, v.vertex);
				UNITY_TRANSFER_FOG(o, o.pos);
				TRANSFER_VERTEX_TO_FRAGMENT(o);
				return o;
			}
			
			float4 frag(vert_output i) : COLOR {
				float3 normalDirection = i.normal;
				float3 viewDirection = normalize(_WorldSpaceCameraPos.xyz - i.posWorld.xyz);
				
				float3 lightDirecton;
				if (_WorldSpaceLightPos0.w == 0.0) {
					// Directional lights
					lightDirecton = normalize(_WorldSpaceLightPos0.xyz);
				} else {
					// Point lights
					lightDirecton = _WorldSpaceLightPos0.xyz - i.posWorld.xyz;
					lightDirecton = normalize(lightDirecton);
				}

				float atten = LIGHT_ATTENUATION(i);
				float diffuseStrength = saturate(dot(normalDirection, lightDirecton));
				float specularStrength = saturate(dot(reflect(-lightDirecton, normalDirection), viewDirection));
				float ramp = tex2D(_Ramp, float2(diffuseStrength * 0.5 + 0.5, 0.5)).r;
				float3 diffuseColor = atten * ramp * _LightColor0.rgb;
				float3 specularColor = atten * diffuseStrength * pow(specularStrength, _Shininess);
				float3 totalLighting = diffuseColor + specularColor + UNITY_LIGHTMODEL_AMBIENT.rgb;				

				float4 c = float4(totalLighting, 1.0) * _Color;
				UNITY_APPLY_FOG(i.fogCoord, c);
				return c;
			}
			
			ENDCG
		}
		Pass {
			Name "OUTLINE"
			Lighting Off
			Cull Front
			ZWrite On
			ColorMask RGB

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile_fog
			#pragma multi_compile_fwdbase
			#include "UnityCG.cginc"
			#include "AutoLight.cginc"

			uniform float4 _OutlineColor;
			uniform float _OutlineWidth;

			struct vert_input {
				float4 vertex : POSITION;
				float3 normal : NORMAL;
			};
			struct vert_output {
				float4 pos : SV_POSITION;
				UNITY_FOG_COORDS(0)
			};

			vert_output vert(vert_input v) {
				vert_output o;
				o.pos = mul(UNITY_MATRIX_MVP, v.vertex);

				float3 norm = normalize(mul((float3x3)UNITY_MATRIX_IT_MV, v.normal));
				float2 offset = TransformViewToProjection(norm.xy);
				o.pos.xy += offset * o.pos.z * _OutlineWidth * 0.001f;

				UNITY_TRANSFER_FOG(o, o.pos);
				return o;
			}

			float4 frag(vert_output i) : COLOR {
				float4 c = _OutlineColor;
				UNITY_APPLY_FOG(i.fogCoord, c);
				return c;
			}

			ENDCG
		}
	} 
	FallBack "Diffuse"
}
