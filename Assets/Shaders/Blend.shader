Shader "Custom/Blend"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_Overlay ("Texture", 2D) = "white" {}
		_Alpha ("Alpha", Range(0, 1)) = 1
	}
	SubShader
	{
		// No culling or depth
		Cull Off ZWrite Off ZTest Always
		Blend SrcAlpha OneMinusSrcAlpha

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
			sampler2D _Overlay;
			float _Alpha;

			fixed4 frag (v2f i) : SV_Target
			{
				fixed4 col1 = tex2D(_MainTex, i.uv);
				fixed4 col2 = tex2D(_Overlay, i.uv);
				col1.rgb = col2.rgb * col2.a + col1.rgb * (1-col2.a);
				col1.a *= _Alpha;
				return col1;
			}
			ENDCG
		}
	}
}
