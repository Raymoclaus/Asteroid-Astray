Shader "Custom/Wormhole"
{
	Properties
	{
		[PerRendererData] _MainTex ("Texture", 2D) = "white" {}
		_PositionX ("Position X", float) = 0.5
		_PositionY ("Position Y", float) = 0.5
		_Radius ("Radius", float) = 0.2
		_DistortionAmplitude ("Distortion Amplitude", float) = 0.1
		_RotationSpeed ("Rotation Speed", float) = 8
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
			float _PositionX, _PositionY, _Radius, _DistortionAmplitude, _RotationSpeed;
			float _Arb, _Arb2;

			fixed4 frag (v2f i) : SV_Target
			{
				float pi = 3.14159265359;
				float radius = clamp(_Radius, 0.001, 1);
				float width = _ScreenParams.x;
				float height = _ScreenParams.y;
				float aspectRatio = width / height;
				float2 pos = i.uv;
				float2 distortionCenterPos = float2(_PositionX, _PositionY);

				float rise = pos.y - distortionCenterPos.y;
				float run = pos.x - distortionCenterPos.x;
				float distanceFromPos = sqrt(run * run + rise * rise);
				run *= aspectRatio;
				float distanceFromAdjustedPos = sqrt(run * run + rise * rise);
				float distortionDelta = distanceFromPos / radius;
				distortionDelta = pow(distortionDelta, _DistortionAmplitude);
				float isInRadius = step(distanceFromAdjustedPos, radius);

				float angleFromDistortion = atan2(pos.y - distortionCenterPos.y, pos.x - distortionCenterPos.x);
				float2 distortionPos = distortionCenterPos;
				float adjustedAngle = angleFromDistortion + distortionDelta * _DistortionAmplitude - _Time.x * _RotationSpeed;
				float2 adjustedPos = float2(sin(adjustedAngle + pi / 2), -cos(adjustedAngle + pi / 2));
				distortionPos += adjustedPos * distanceFromPos;


				pos = lerp(pos, distortionPos, isInRadius);
				fixed4 col = tex2D(_MainTex, pos);
				return col;
			}
			ENDCG
		}
	}
}
