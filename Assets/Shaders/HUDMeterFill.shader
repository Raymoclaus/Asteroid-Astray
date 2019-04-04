Shader "Custom/HUDMeterFill"
{
	Properties
	{
		_MainTex("Main Texture", 2D) = "white" {}
		_FullColor("Full Color", Color) = (0, 1, 0, 1)
		_LowColour("Low Color", Color) = (1, 0, 0, 1)
		_BackgroundColor("Background Color", Color) = (0, 0, 0, 1)
		_FillAmount("Fill Amount", Range(0, 1)) = 1
		_DamageColor("Damage Color", Color) = (1, 1, 0, 1)
		_DamageFillAmount("Damage Fill Amount", Range(0, 1)) = 0
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
			fixed4 _FullColor, _LowColour, _BackgroundColor, _DamageColor;
			float _FillAmount, _DamageFillAmount;

			fixed4 frag (v2f i) : SV_Target
			{
				fixed4 col = tex2D(_MainTex, i.uv);
				fixed4 meterCol = lerp(_LowColour, _FullColor, _FillAmount);
				float brightness = (col.r + col.g + col.b) / 3.01;
				float isDamaged = step(_DamageFillAmount, brightness);
				fixed4 fillCol = lerp(_DamageColor, _BackgroundColor, isDamaged);
				float isFilled = step(_FillAmount, brightness);
				fixed4 pixelCol = lerp(meterCol, fillCol, isFilled);
				pixelCol.a *= col.a;
				return pixelCol;
			}
			ENDCG
		}
	}
}
