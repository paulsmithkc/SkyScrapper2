Shader "Rubberband/Rubberband"
{
	// Simple unlit transparent shader
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
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

			uniform sampler2D _MainTex;
			uniform float4 _MainTex_ST;
			uniform float _Speed;

			struct vert_input {
				float4 vertex : POSITION;
				float4 uv : TEXCOORD0;
			};
			struct vert_output {
				float4 pos : SV_POSITION;
				float2 uv : TEXCOORD0;
				UNITY_FOG_COORDS(1)
			};

			vert_output vert(vert_input v) {
				vert_output o;
				o.pos = mul(UNITY_MATRIX_MVP, v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				UNITY_TRANSFER_FOG(o, o.pos);
				return o;
			}

			float4 frag(vert_output i) : COLOR {
				fixed4 c = tex2D(_MainTex, i.uv);
				UNITY_APPLY_FOG(i.fogCoord, c);
				return c;
			}

			ENDCG
		}
	}
	FallBack "Diffuse"
}
