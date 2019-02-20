Shader "Custom/Spotlight"
{
	Properties
	{
		_MainTex("Texture", 2D) = "white" {}
		_Radius("Radius", float) = 0.2
		_SoftRadius("Soft Radius", float) = 0.05
		_PositionX("X Position", float) = 0.5
		_PositionY("Y Position", float) = 0.5
	}
	SubShader
	{
		// No culling or depth
		Cull Off ZWrite Off ZTest Always

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
			float _Radius, _SoftRadius;
			float _PositionX, _PositionY;

			fixed4 frag (v2f i) : SV_Target
			{
				fixed4 col = tex2D(_MainTex, i.uv);
				fixed4 black = fixed4(0, 0, 0, 0);
				float scl = _ScreenParams.y / _ScreenParams.x;
				float2 pos = float2(i.uv.x, i.uv.y);
				float2 difference = float2(pos.x - _PositionX, (pos.y - _PositionY) * scl);
				float dist = sqrt(difference.x * difference.x + difference.y * difference.y);
				float outside = step(_Radius, dist);
				float soft = clamp((dist - _Radius + _SoftRadius) / _SoftRadius, 0, 1);
				col = lerp(col, black, clamp(0, 1, outside + soft));
				return col;
			}
			ENDCG
		}
	}
}
