Shader "HighlightPlus/Geometry/ComposeGlow" {
	Properties {
		_MainTex ("Texture", any) = "black" {}
		_Color ("Color", Vector) = (1,1,1,1)
		[HideInInspector] _Cull ("Cull Mode", Float) = 2
		[HideInInspector] _ZTest ("ZTest Mode", Float) = 0
		[HideInInspector] _Flip ("Flip", Vector) = (0,1,0,1)
		[HideInInspector] _BlendSrc ("Blend Src", Float) = 1
		[HideInInspector] _BlendDst ("Blend Dst", Float) = 1
		_Debug ("Debug Color", Vector) = (0,0,0,0)
		[HideInInspector] _GlowStencilComp ("Stencil Comp", Float) = 6
		_Padding ("Padding", Float) = 0
		[HideInInspector] _Pixelation ("Pixelation", Float) = 0
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