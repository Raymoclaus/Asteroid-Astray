Shader "Custom/CircleFill"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_ArcAngle("Arc Angle", Range(0, 360)) = 60
		_Rotation("Rotation", float) = 90
		_ArcTint("Tint", Color) = (1, 1, 1, 1)
		_Radius("Radius", float) = 1
		_Width("Width", float) = 0.1
		_BackgroundColor("Background Color", Color) = (0, 0, 0, 1)
	}
	SubShader
	{
		// No culling or depth
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
			float _ArcAngle;
			float _Rotation;
			fixed4 _ArcTint;
			float _Radius;
			float _Width;
			fixed4 _BackgroundColor;

			fixed4 frag (v2f i) : SV_Target
			{
				fixed4 col = tex2D(_MainTex, i.uv);
				float2 pos = float2(i.uv.x - 0.5, i.uv.y - 0.5);
				float dist = sqrt(pos.x * pos.x + pos.y * pos.y);
				float inRadiusWidth = step(_Radius - _Width, dist) * step(dist, _Radius + _Width);
				float angle = atan2(pos.y, pos.x) * -180 / 3.14159 + _Rotation;
				angle += 360 * step(sign(angle), 0);
				float inAngle = step(angle, _ArcAngle);
				col *= lerp(_BackgroundColor, _ArcTint, inRadiusWidth * inAngle);
				return col;
			}
			ENDCG
		}
	}
}
