Shader "Custom/Energy Shield"
{
	Properties
	{
		[PerRendererData] _MainTex ("Texture", 2D) = "white" {}
		_Color("Color", Color) = (1, 1, 1, 1)
		_ForceLocationX("Force X Location", Range(-2, 2)) = 0.2
		_ForceLocationY("Force Y Location", Range(-2, 2)) = 0.8
		_ForceRadius("Force Radius", Range(0, 1)) = 0.5
		_ForceColor("Force Color", Color) = (1, 1, 1, 1)
		_DistortionAmplitude("Distortion Amplitude", float) = 1
		_DistortionRadius("Distortion Radius", Range(0, 1)) = 0.2
		_RippleAngle("Ripple Angle", float) = 0
		_RippleProgress("Ripple Progress", Range(0, 1)) = 0.5
		_RippleWidth("Ripple Width", float) = 0.1
		_RippleColor("Ripple Color", Color) = (1, 1, 1, 1)
	}
	SubShader
	{
		Tags
		{
			"QUEUE" = "Transparent"
			"IGNOREPROJECTOR" = "true"
			"RenderType" = "Transparent"
			"PreviewType" = "Plane"
			"CanUseSpriteAtlas" = "true"
		}
		Cull Off
		ZWrite Off
		ZTest Always
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
			fixed4 _Color, _ForceColor, _RippleColor;
			float _ForceLocationX, _ForceLocationY, _ForceRadius, _DistortionAmplitude, _DistortionRadius;
			float _RippleAngle, _RippleProgress, _RippleWidth;

			fixed4 frag (v2f i) : SV_Target
			{
				float2 center = float2(0.5, 0.5);
				float2 pos = i.uv - center;

				float2 forceLocation = float2(_ForceLocationX, _ForceLocationY) / 2;
				float rise = forceLocation.y - pos.y;
				float run = forceLocation.x - pos.x;
				float distFromForceLocation = sqrt(rise * rise + run * run);
				float forceDistDelta = distFromForceLocation / max(0.001, _ForceRadius);
				float inForceRadius = step(forceDistDelta, 1);
				float forceAmountDelta = (1 - forceDistDelta) * inForceRadius;

				float distortionRadiusDelta = distFromForceLocation / max(0.001, _DistortionRadius);
				float2 distortedPos = float2(pos.x + run * distortionRadiusDelta * _DistortionAmplitude,
					pos.y + rise * distortionRadiusDelta * _DistortionAmplitude);
				float2 adjustedPos = lerp(pos, distortedPos, 1 - clamp(distortionRadiusDelta, 0, 1));

				float2 texPos = adjustedPos + center;
				fixed4 col = tex2D(_MainTex, texPos);

				col.a *= step(0, texPos.x) * step(0, texPos.y) * step(texPos.x, 1) * step(texPos.y, 1);

				fixed4 shieldCol = _Color;
				shieldCol.a *= col.a;
				fixed4 shieldPlusForce = shieldCol + _ForceColor;
				shieldPlusForce.a *= shieldCol.a;
				fixed4 forceAdjustedCol = lerp(shieldCol, shieldPlusForce, forceAmountDelta);

				float a = _RippleAngle;
				float topMeasure = (adjustedPos.x + sin(a)) * tan(a) + cos(a);
				float bottomMeasure = (adjustedPos.x - sin(a)) * tan(a) - cos(a);
				float ripplePos = (adjustedPos.y - bottomMeasure) / (topMeasure - bottomMeasure);
				float bottomOfRipple = _RippleProgress - _RippleWidth / 2;
				float topOfRipple = _RippleProgress + _RippleWidth / 2;
				float inRipple = step(bottomOfRipple, ripplePos) * step(ripplePos, topOfRipple);
				float rippleDelta = (1 - abs(ripplePos - _RippleProgress) / (_RippleWidth / 2));

				fixed4 rippleCol = forceAdjustedCol + _RippleColor;
				rippleCol.a *= forceAdjustedCol.a;
				fixed4 rippleAdjustedCol = lerp(forceAdjustedCol, rippleCol, rippleDelta * inRipple);

				return rippleAdjustedCol;
			}
			ENDCG
		}
	}
}
