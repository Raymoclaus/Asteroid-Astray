Shader "Custom/Arc"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_ArcAngle("Arc Angle", Range(0, 360)) = 60
		_MaxOpacity("Max Opacity", Range(0, 1)) = 0.5
		_TransparencyRollOff("Transparency RollOff", float) = 2
	}
	SubShader
	{
		BLEND SrcAlpha OneMinusSrcAlpha

		Tags
		{
			"Queue" = "Transparent"
		}

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile _ ETC1_EXTERNAL_ALPHA
			
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
			float _MaxOpacity;
			float _TransparencyRollOff;

			fixed4 frag (v2f i) : SV_Target
			{
				fixed4 col = tex2D(_MainTex, i.uv);
				float2 pos = float2(i.uv.x - 0.5, i.uv.y - 0.5);
				float angle = atan2(pos.x, pos.y) * 180 / 3.14159;
				angle *= sign(angle);
				float dist = sqrt(pos.x * pos.x + pos.y * pos.y);
				float alpha = pow(1 - step(_ArcAngle / 2.0, angle) - dist, _TransparencyRollOff) * _MaxOpacity;
				col.a = lerp(0, col.a, alpha);
				col.r = 0;
				col.g = 0.7;
				return col;
			}
			ENDCG
		}
	}
}
