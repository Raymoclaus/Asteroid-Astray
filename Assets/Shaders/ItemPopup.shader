Shader "Custom/ItemPopup"
{
	Properties
	{
		_MainTex("Main Texture", 2D) = "white" {}
		_Tint("Tint", Color) = (1, 1, 1, 0.5)
		_Radius("Radius", Range(0, 1)) = 1
		_EdgeWidth("Edge Width", Range(0, 1)) = 0.1
		_EdgeRoundness("Edge Roundness", Range(0, 1)) = 0.5
		_Padding("Padding", Range(0, 1)) = 0.2
	}
	SubShader
	{
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
			fixed4 _Tint;
			float _Radius;
			fixed _EdgeWidth;
			float _EdgeRoundness;
			float _Padding;

			fixed4 frag (v2f i) : SV_Target
			{
				fixed4 white = fixed4(1, 1, 1, 1);
				fixed4 clear = fixed4(0, 0, 0, 0);
				float posFromTex = float2(i.uv.x - (1 - _Radius), i.uv.y);
				fixed4 col = tex2D(_MainTex, posFromTex);
				float2 pos = float2(i.uv.x - 1, i.uv.y - 0.5) / (1 - _Padding);
				float dist = sqrt(pos.x * pos.x + pos.y * pos.y * 4);
				float angle = pow(abs(2 * pos.y * pos.x), _Radius + _EdgeRoundness + 0.5);
				float sharpenedRadius = lerp(_Radius, 1.414, saturate(angle * _Radius));
				_Tint.a *= abs(pos.x) * (1 - _Padding);
				_Tint.a *= _Tint.a;
				col = lerp(clear, _Tint * col, step(dist, sharpenedRadius));
				float glow = lerp(1, _EdgeWidth, _Radius);
				float edgeDist = (-abs(dist - sharpenedRadius) + glow * (1 - angle)) / glow;
				col = lerp(col, white, saturate(edgeDist));
				col.a = col.a * _Radius;
				return col;
			}
			ENDCG
		}
	}
}
