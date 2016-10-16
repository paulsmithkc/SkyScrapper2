Shader "Custom/MiniMap_Line"
{
	// Simple unlit transparent shader
	Properties
	{
		_Color("Color", Color) = (1,1,1,1)
	}
	SubShader
	{
		Tags { "RenderType" = "Opaque" "LightMode" = "ForwardBase" }
		LOD 100

		Pass
		{
			Name "FORWARD"
			Blend One Zero
			Lighting Off
			Cull Off
			ZWrite On
			ZTest Off

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
			};

			vert_output vert(vert_input v) {
				vert_output o;
				o.pos = mul(UNITY_MATRIX_MVP, v.vertex);
				return o;
			}

			float4 frag(vert_output i) : COLOR {
				fixed4 c = _Color;
				return c;
			}

			ENDCG
		}
	}
	FallBack "Diffuse"
}
