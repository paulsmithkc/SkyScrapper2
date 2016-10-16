Shader "Custom/UnlitTransparentColor"
{
	// Simple unlit transparent shader
	Properties
	{
		_Color("Color", Color) = (1,1,1,1)
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

			struct vert_input {
				float4 vertex : POSITION;
			};
			struct vert_output {
				float4 pos : SV_POSITION;
				UNITY_FOG_COORDS(0)
			};

			vert_output vert(vert_input v) {
				vert_output o;
				o.pos = mul(UNITY_MATRIX_MVP, v.vertex);
				UNITY_TRANSFER_FOG(o, o.pos);
				return o;
			}

			float4 frag(vert_output i) : COLOR {
				fixed4 c = _Color;
				UNITY_APPLY_FOG(i.fogCoord, c);
				return c;
			}

			ENDCG
		}
	}
	FallBack "Diffuse"
}
