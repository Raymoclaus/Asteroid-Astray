Shader "Custom/PixelDisplacement"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_Radius("Radius", Range(0, 3)) = 0
		_RippleWidth("RippleWidth", Range(0, 1)) = 0.1
		_DistortionAmplitude("_DistortionAmplitude", Range(0, 1)) = 0.01
		_Tint("Tint", Color) = (1, 1, 1, 1)
		_PosX("PosX", float) = 0.5
		_PosY("PosY", float) = 0.5
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
			float _Radius;
			float _RippleWidth;
			float _DistortionAmplitude;
			fixed4 _Tint;
			float _PosX, _PosY;

			fixed4 frag (v2f i) : SV_Target
			{
				float2 center = float2(_PosX, _PosY);
				float scl = _ScreenParams.y / _ScreenParams.x;
				float2 screenPos = float2(i.uv.x, (i.uv.y - 0.5) * scl + 0.5);
				float dis = sqrt(pow(screenPos.x - center.x, 2) + pow(screenPos.y - center.y, 2));
				float inDistortion = step(dis, _Radius + _RippleWidth / 2.0) * step(_Radius - _RippleWidth / 2.0, dis);
				float diff = (1.0 - abs(dis - _Radius) / (_RippleWidth / 2.0)) * _DistortionAmplitude * inDistortion;
				float2 diffUV = normalize(i.uv - center) * diff;
				fixed4 col = tex2D(_MainTex, i.uv - diffUV);
				_Tint.rgb *= _Tint.a;
				col += _Tint * diff / _DistortionAmplitude;
				return col;
			}
			ENDCG
		}
	}
}
