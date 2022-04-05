Shader "Custom/AdvancingTextureOverlay"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_AtlasXPosition("Atlas X Position", Range(0, 1)) = 0
		_AtlasYPosition("Atlas Y Position", Range(0, 1)) = 0
		_AtlasCellXScale("Atlas Cell X Scale", Range(0, 1)) = 1
		_AtlasCellYScale("Atlas Cell Y Scale", Range(0, 1)) = 1
		_OverlayTextures("Overlay Textures", 2D) = "white" {}
		_OverlayTexWidth("Overlay Texture Width", float) = 256
		_OverlayTexHeight("Overlay Texture Height", float) = 256
		_CellWidth("Cell Width", Range(0, 1)) = 1
		_CellHeight("Cell Height", Range(0, 1)) = 1
		_Frame("Frame", int) = 0
		_Opacity("Opacity", Range(0, 1)) = 1
		_AlphaCutout("Alpha Cutout", Range(0, 1)) = 1
		_OverlayDarkenFactor("Overlay Darken Factor", float) = 1.0
		_PivotX("Pivot X", Range(0, 1)) = 0.5
		_PivotY("Pivot Y", Range(0, 1)) = 0.5
		_PivotXOffset("Pivot X Offset", Range(-0.5, 0.5)) = 0
		_PivotYOffset("Pivot Y Offset", Range(-0.5, 0.5)) = 0
		_ScaleXFromPivot("Scale X", float) = 1.0
		_ScaleYFromPivot("Scale Y", float) = 1.0
		_Angle("Angle", float) = 0
		_AngleStretch("Angle Stretch", float) = 1.0
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
			#pragma enable_d3d11_debug_symbols

			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float4 uv : TEXCOORD0;
			};

			struct v2f
			{
				float4 uv : TEXCOORD0;
				float4 vertex : SV_POSITION;
			};

			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = v.uv;
				return o;
			}

			sampler2D _MainTex, _OverlayTextures;
			float4 _MainTex_ST, _OverlayTextures_ST;
			static const float pi = 3.14159265359;
			float _AtlasXPosition, _AtlasYPosition, _AtlasCellXScale, _AtlasCellYScale,
				_CellWidth, _CellHeight, _Opacity, _AlphaCutout, _OverlayDarkenFactor,
				_PivotX, _PivotY, _PivotXOffset, _PivotYOffset, _Angle, _ScaleXFromPivot,
				_ScaleYFromPivot, _OverlayTexWidth, _OverlayTexHeight, _AngleStretch;
			uint _Frame;

			fixed4 border(float2 uv)
			{
				fixed bottomGap = 1.0 % _CellHeight;
				uv.y -= bottomGap;
				fixed borderWidth = 0.005;
				fixed cellBorder = 1 - step(borderWidth, uv.x % _CellWidth);
				cellBorder += 1 - step(borderWidth, uv.y % _CellHeight);
				cellBorder += step(_CellWidth - borderWidth, uv.x % _CellWidth);
				cellBorder += step(_CellHeight - borderWidth, uv.y % _CellHeight);
				fixed rightGap = 1.0 - 1.0 % _CellWidth;
				fixed isWithinFullCell = step(rightGap, uv.x);
				cellBorder += isWithinFullCell;
				cellBorder = saturate(cellBorder);
				return fixed4(cellBorder, 0, 0, cellBorder);
			}

			fixed2 currentFrameMinPosition()
			{
				uint columnCount = floor(1 / _CellWidth);
				uint rowCount = floor(1 / _CellHeight);
				uint frameColumn = _Frame % columnCount;
				uint frameRow = floor(_Frame / columnCount);
				fixed2 currentFrameMin = fixed2(
					frameColumn * _CellWidth,
					1.0 - frameRow * _CellHeight - _CellHeight);
				return currentFrameMin;
			}

			fixed4 highlightCurrentFrame(float2 uv, fixed4 tint)
			{
				uint columnCount = floor(1 / _CellWidth);
				uint rowCount = floor(1 / _CellHeight);
				uint frameColumn = _Frame % columnCount;
				uint frameRow = floor(_Frame / columnCount);
				fixed2 currentFrameMin = currentFrameMinPosition();
				fixed isInColumn = step(uv.x - currentFrameMin.x, _CellWidth) * step(0, uv.x - currentFrameMin.x);
				fixed isInRow = step(uv.y - currentFrameMin.y, _CellHeight) * step(0, uv.y - currentFrameMin.y);
				fixed isInCell = isInColumn * isInRow;
				return isInCell * tint;
			}

			float distance(float2 a, float2 b)
			{
				float xDist = a.x - b.x;
				float yDist = a.y - b.y;
				return sqrt(xDist * xDist + yDist * yDist);
			}

			fixed4 drawCircle(float2 uv, float2 pos, fixed4 tint, fixed circleRadius)
			{
				fixed aspectRatio = _CellWidth / _CellHeight;
				fixed2 vectorToPos = uv - pos;
				vectorToPos.y *= aspectRatio;
				fixed distanceToPos = distance(vectorToPos, fixed2(0, 0));
				fixed isInRadius = step(distanceToPos, circleRadius);
				return isInRadius * tint;
			}

			fixed4 pointToColour(float2 pos)
			{
				return fixed4(pos.x, pos.y, (pos.x + pos.y) / 2.0, 1.0);
			}

			fixed degreesToRadians(fixed degrees)
			{
				return degrees * pi / 180;
			}

			fixed4 frag (v2f i) : SV_Target
			{
				fixed radians = degreesToRadians(_Angle);
				fixed2 cellScale = fixed2(_CellWidth, _CellHeight);
				fixed cellAspectRatio = _CellWidth / _CellHeight;
				fixed overlayTexAspectRatio = _OverlayTexWidth / _OverlayTexHeight;
				fixed2 imageScale = fixed2(_ScaleXFromPivot, _ScaleYFromPivot);

				fixed4 col = fixed4(0, 0, 0, 0);
				i.uv.xy = i.uv.xy * _MainTex_ST.xy + _MainTex_ST.zw;
				//get colour from main texture
				fixed4 originalCol = tex2D(_MainTex, i.uv);
				//calculate number of columns and rows of cells in texture (only works assuming all cells are the same size)
				uint columnCount = floor(1 / cellScale.x);
				//get the int x/y of current frame as a grid (frame 0 at top left)
				uint frameColumn = _Frame % columnCount;
				uint frameRow = floor(_Frame / columnCount);
				//convert the main UVs of the cell to 0/0 to 1/1 values
				float2 mainScale = float2(_AtlasCellXScale, _AtlasCellYScale);
				float2 mainOffset = float2(_AtlasXPosition, _AtlasYPosition);
				float2 adjustedMainUV = (i.uv - mainOffset) / mainScale;
				//get the UVs from the overlay atlas frame (not in 0/0 to 1/1, use adjustedMainUV for that)
				float2 frameUVOffset = float2(frameColumn * cellScale.x, 1.0 - frameRow * cellScale.y - cellScale.y);
				float2 overlayUV = frameUVOffset + adjustedMainUV * cellScale;
				overlayUV = overlayUV * _OverlayTextures_ST.xy + _OverlayTextures_ST.zw;
				//define pivot point based on parameters
				fixed2 pivotPoint = fixed2(_PivotX, _PivotY);
				fixed2 pivotOffset = fixed2(-_PivotXOffset, -_PivotYOffset) * cellScale / imageScale;
				//scaling
				fixed2 pivotPointTextureCoordinate = frameUVOffset + pivotPoint * cellScale + pivotOffset;
				fixed2 uvToPPVector = (overlayUV - pivotPointTextureCoordinate) / imageScale;
				overlayUV = pivotPointTextureCoordinate + uvToPPVector;
				//offset
				overlayUV += pivotOffset / imageScale;
				//rotation
				fixed aspectRatioDelta = pow(-abs(2.0 * (fmod(radians, pi)) / pi - 1.0) + 1.0, _AngleStretch);
				uvToPPVector.x = lerp(uvToPPVector.x, uvToPPVector.x * overlayTexAspectRatio, aspectRatioDelta);
				uvToPPVector.y = lerp(uvToPPVector.y, uvToPPVector.y / overlayTexAspectRatio, aspectRatioDelta);
				fixed angleOfUV = -atan2(uvToPPVector.y, uvToPPVector.x) + pi / 2.0;
				fixed uvDistanceToPivot = distance(uvToPPVector, fixed2(0.0, 0.0));
				fixed2 rotatedVector = normalize(fixed2(sin(angleOfUV - radians), cos(angleOfUV - radians)));
				overlayUV = pivotPointTextureCoordinate + rotatedVector * uvDistanceToPivot;
				//get colour from overlay texture using calculated UVs
				fixed4 overlayCol = tex2D(_OverlayTextures, overlayUV);
				//checks to see if the UV is in the correct frame row/column
				fixed isInColumn = step(overlayUV.x - frameUVOffset.x, cellScale.x) * step(0, overlayUV.x - frameUVOffset.x);
				fixed isInRow = step(overlayUV.y - frameUVOffset.y, cellScale.y) * step(0, overlayUV.y - frameUVOffset.y);
				fixed isInCell = isInColumn * isInRow;
				//cuts out the overlay colour if the original colour is transparent
				overlayCol.a = lerp(overlayCol.a * isInCell, originalCol.a, (1.0 - originalCol.a) * _AlphaCutout);
				//darkens the overlay colour based on the brightness of the original colour
				overlayCol.rgb *= pow((originalCol.r + originalCol.g + originalCol.b) / 3.0, _OverlayDarkenFactor);
				//fades the overlay based on opacity parameter
				col = lerp(originalCol, overlayCol, overlayCol.a * _Opacity);

				//DEBUG: tint current frame
				//fixed4 currentFrameTint = highlightCurrentFrame(overlayUV, fixed4(0, 0, 1, 0.7));
				//col = lerp(col, currentFrameTint, currentFrameTint.a);

				//DEBUG: border each frame
				//fixed4 borders = border(overlayUV);
				//col = lerp(col, borders, borders.a);

				//DEBUG: draw pivot point of current frame
				fixed4 drawPivot = drawCircle(overlayUV, pivotPointTextureCoordinate, fixed4(0, 1.0, 0, 1.0), 0.005);
				col = lerp(col, drawPivot, drawPivot.a);
				
				//test
				//col = pointToColour(aspectRatioDelta);
				return col;
			}
			ENDCG
		}
	}
}
