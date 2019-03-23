Shader "Custom/Grid"
{
	Properties
	{
		[PerRendererData] _MainTex ("Texture", 2D) = "white" {}
		_Size("Size", Range(0, 1)) = 0.05
		_CellHeightMatchWidth("Height Match Width", Range(0, 1)) = 0
		_BackgroundColor("Background Color", Color) = (0, 0, 0, 1)
		_LineColor("Line Color", Color) = (1, 1, 1, 1)
		_LineGlowColor("Line Glow Color", Color) = (1, 1, 1, 1)
		_LineWidth("Line Width", Range(0, 1)) = 0.1
		_WaveSpeed("Wave Speed", float) = 1
		_WaveDelay("Wave Delay", float) = 1
		_WaveAmplitude("Wave Amplitude", float) = 1
	}
	SubShader
	{
		Tags { "QUEUE" = "Transparent" "IGNOREPROJECTOR" = "true" "RenderType" = "Transparent"
			"PreviewType" = "Plane" "CanUseSpriteAtlas" = "true" }
		ZWrite Off
		Cull Off
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
				float4 color : COLOR;
			};

			struct Input
			{
				float2 uv_MainTex;
				fixed4 color;
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				float4 vertex : SV_POSITION;
				float4 diff : COLOR0;
			};

			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = v.uv;
				o.diff = v.color;
				return o;
			}
			
			sampler2D _MainTex;
			float _LineWidth, _CellHeightMatchWidth, _WaveSpeed, _WaveDelay, _WaveAmplitude;
			float _Size;
			fixed4 _BackgroundColor, _LineColor, _LineGlowColor;

			fixed4 frag (v2f i) : SV_Target
			{
				fixed4 col = tex2D(_MainTex, i.uv);
				float PI = 3.14159;
				float2 pos = i.uv;
				float width = _ScreenParams.x;
				float height = _ScreenParams.y;
				float aspectRatio = width / height;
				pos.y *= lerp(1, height / width, _CellHeightMatchWidth);
				float time = _Time.y * _WaveSpeed;
				float waveTime = time - pos.x / _Size - pos.y * aspectRatio / _Size;
				float waveTimeModPI = fmod(waveTime, PI * 2 * _WaveDelay);
				float readyDelay = step(waveTimeModPI, PI * 2);
				float cosTime = -cos(waveTime * readyDelay) / 2 + 0.5;
				pos.y -= cosTime * _Size * _WaveAmplitude;

				float wrapX = ((pos.x + width) / _Size) % 1;
				float wrapY = ((pos.y + height) / _Size) % 1;
				float gridLineY = step(wrapX, _LineWidth / 2);
				float gridLineNY = step(1 - _LineWidth / 2, wrapX);
				float gridLineX = step(wrapY, _LineWidth / 2);
				float gridLineNX = step(1 - _LineWidth / 2, wrapY);
				fixed4 lineCol = lerp(_LineColor, _LineGlowColor, cosTime);
				col *= lerp(_BackgroundColor, lineCol, clamp(0, 1, (gridLineY + gridLineNY) * (gridLineX + gridLineNX)));
				col.a *= i.diff.a;
				return col;
			}
			ENDCG
		}
	}
}
