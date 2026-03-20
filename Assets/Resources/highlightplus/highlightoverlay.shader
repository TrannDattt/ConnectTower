Shader "HighlightPlus/Geometry/Overlay" {
	Properties {
		_MainTex ("Texture", any) = "white" {}
		_Color ("Color", Vector) = (1,1,1,1)
		_OverlayColor ("Overlay Color", Vector) = (1,1,1,1)
		_OverlayBackColor ("Overlay Back Color", Vector) = (1,1,1,1)
		_OverlayData ("Overlay Data", Vector) = (1,0.5,1,1)
		_OverlayHitPosData ("Overlay Hit Pos Data", Vector) = (0,0,0,0)
		_OverlayHitStartTime ("Overlay Hit Start Time", Float) = 0
		_OverlayTexture ("Overlay Texture", 2D) = "white" {}
		_CutOff ("CutOff", Float) = 0.5
		_Cull ("Cull Mode", Float) = 2
		_OverlayZTest ("ZTest", Float) = 4
		_OverlayPatternScrolling ("Pattern Scrolling", Vector) = (0,0,0,0)
		_OverlayPatternData ("Pattern Data", Vector) = (0,0,0,0)
	}
	//DummyShaderTextExporter
	SubShader{
		Tags { "RenderType"="Opaque" }
		LOD 200
		CGPROGRAM
#pragma surface surf Standard
#pragma target 3.0

		sampler2D _MainTex;
		fixed4 _Color;
		struct Input
		{
			float2 uv_MainTex;
		};
		
		void surf(Input IN, inout SurfaceOutputStandard o)
		{
			fixed4 c = tex2D(_MainTex, IN.uv_MainTex) * _Color;
			o.Albedo = c.rgb;
			o.Alpha = c.a;
		}
		ENDCG
	}
}