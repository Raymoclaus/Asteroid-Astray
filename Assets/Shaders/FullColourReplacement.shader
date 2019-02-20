Shader "Custom/FullColourReplacement"
{
	Properties
	{
		_MainTex("Texture", 2D) = "white" {}
		_Colour("Colour", Color) = (1, 1, 1, 1)
		_BlendAmount("BlendAmount", Range(0, 1)) = 0
	}
	SubShader
	{
		BLEND SrcAlpha OneMinusSrcAlpha

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				float4 vertex : SV_POSITION;
			};

			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = v.uv;
				return o;
			}

			sampler2D _MainTex;
			fixed4 _Colour;
			float _BlendAmount;

			fixed4 frag (v2f i) : SV_Target
			{
				fixed4 col = tex2D(_MainTex, i.uv);
				fixed4 adjustedCol = lerp(col, _Colour, _BlendAmount);
				adjustedCol.a = col.a;
				return adjustedCol;
			}
			ENDCG
		}
	}
}
