Shader "Custom/ItemPopup"
{
	Properties
	{
		_MainTex("Main Texture", 2D) = "white" {}
		_Tint("Tint", Color) = (1, 1, 1, 0.5)
		_Radius("Radius", Range(0.0, 1.0)) = 0.0
		_EdgeWidth("Edge Width", Range(0, 1)) = 0.1
		_EdgeRoundness("Edge Roundness", Range(0, 1)) = 0.5
		_Padding("Padding", Range(0, 1)) = 0.2
		_Flash("Flash", Range(0, 1)) = 1
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
			float _Flash;

			fixed4 frag (v2f i) : SV_Target
			{
				//white used for glow
				fixed4 white = 1;
				//clear used for empty space
				fixed4 clear = 0;
				//sampling from different place in the texture helps the transition
				float posFromTex = float2(i.uv.x - (1 - _Radius * 0.9), i.uv.y);
				fixed4 col = tex2D(_MainTex, posFromTex);
				//treat the center of the image as the origin instead of bottom left
				float2 pos = float2(i.uv.x - 1, i.uv.y - 0.5) / (1 - _Padding);
				//distance from center
				float dist = sqrt(pos.x * pos.x + pos.y * pos.y * 4);
				//angle from center (with adjustments)
				float angle = pow(abs(2 * pos.y * pos.x), _Radius + _EdgeRoundness + 0.5);
				//compared with distance so we get something more rectangular instead of circular
				float sharpenedRadius = lerp(_Radius, 1.414, saturate(angle * _Radius));
				//fade from _Tint to clear on the right
				_Tint.a *= abs(pos.x) * (1 - _Padding);
				_Tint.a *= _Tint.a;
				//anything with a distance too large is set to clear
				col = lerp(clear, _Tint * col, step(dist, sharpenedRadius));
				//calculate how much a pixel should be whitened for use with glow
				float glow = lerp(1, _EdgeWidth, _Radius);
				float edgeDist = (-abs(dist - sharpenedRadius) + glow * (1 - angle)) / glow;
				col = lerp(col, white, saturate(edgeDist));
				//fade in when increasing _Radius
				col.a = col.a * _Radius;
				//Tint to white based on flash parameter
				float flash = lerp(0, 1, (_Radius - 0.666) * 6);
				flash = lerp(0, flash, step(0, flash));
				float flip = step(1, flash);
				flash = lerp (flash, -flash, flip);
				flash += 2 * flip;
				col = lerp(col, white, step(0.0000001, col.a) * flash * step(0.5, _Flash));
				return col;
			}
			ENDCG
		}
	}
}
