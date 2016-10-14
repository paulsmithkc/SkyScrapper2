Shader "Custom/LaserWall"
{
	// Simple unlit transparent shader
	Properties
	{
		_Color("Color", Color) = (1,1,1,1)
		_MainTex("Texture", 2D) = "white" {}
		_AlphaAmplitude("AlphaAmplitude", Float) = 0.5
		_WaveAmplitude("WaveAmplitude", Float) = 0.5
		_WavePeriod("WavePeriod", Float) = 0.1
        _WaveOffset("WaveOffset", Float) = 0
		_WaveSpeed("WaveSpeed", Float) = 0.2
        _TexSpeedU("TexSpeedU", Float) = 0
        _TexSpeedV("TexSpeedV", Float) = 0.1
	}
	SubShader
	{
		Tags{ "RenderType" = "Transparent" "LightMode" = "ForwardBase" "Queue" = "Transparent" }
		LOD 100

		Pass
		{
			Name "FORWARD"
			Blend SrcAlpha OneMinusSrcAlpha
			Lighting Off
			Cull Off
			ZWrite Off
			ZTest On

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile_fog
			#include "UnityCG.cginc"

			uniform float4 _Color;
			uniform sampler2D _MainTex;
			uniform float4 _MainTex_ST;
			uniform float _AlphaAmplitude;
			uniform float _WaveAmplitude;
			uniform float _WavePeriod;
            uniform float _WaveOffset;
			uniform float _WaveSpeed;
            uniform float _TexSpeedU;
            uniform float _TexSpeedV;

			struct vert_input {
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
				float3 normal : NORMAL;
			};

			struct vert_output {
				float4 pos : SV_POSITION;
				float2 uv : TEXCOORD0;
				//float4 uv2 : TEXCOORD1;
				float4 color : COLOR;
				UNITY_FOG_COORDS(0)
			};

			vert_output vert(vert_input v) {
				vert_output o;
				float time = _Time.w;
				
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				
				float displacement = sin((time * _WaveSpeed + _WaveOffset + o.uv.y) / _WavePeriod);
				o.pos = v.vertex;
				o.pos.xyz += v.normal * (displacement * _WaveAmplitude);

				o.color = _Color;
				o.color.w *= (1.0f - 0.5f *_AlphaAmplitude) + (0.5f *_AlphaAmplitude * displacement);

                o.uv.x += time * _TexSpeedU;
                o.uv.y += time * _TexSpeedV;

				o.pos = mul(UNITY_MATRIX_MVP, o.pos);
			    UNITY_TRANSFER_FOG(o, o.pos);
				return o;
			}

			float4 frag(vert_output i) : COLOR {
				float time = _Time.w;
				fixed4 c = i.color * tex2D(_MainTex, i.uv);
				UNITY_APPLY_FOG(i.fogCoord, c);
				return c;
			}

			ENDCG
		}
	}
	FallBack "Diffuse"
}
