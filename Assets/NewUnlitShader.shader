Shader "Custom/UIRedGlowShader"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1) // UI.Image 的颜色

        _GlowColor ("Glow Color", Color) = (1,0,0,1) // 要匹配的颜色
        _ColorSensitivity ("Color Sensitivity", Range(0, 1)) = 0.5 // 颜色匹配灵敏度
        _GlowIntensity ("Glow Intensity", Float) = 2.0 // 发光强度
        _FlashSpeed ("Flash Speed", Float) = 2.0 // 闪烁速度
        _FlashMin ("Flash Min", Range(0, 1)) = 0.15 // 闪烁最低亮度比例

        // --- UI Masking Properties ---
        _StencilComp ("Stencil Comparison", Float) = 8
        _Stencil ("Stencil ID", Float) = 0
        _StencilOp ("Stencil Operation", Float) = 0
        _StencilWriteMask ("Stencil Write Mask", Float) = 255
        _StencilReadMask ("Stencil Read Mask", Float) = 255
        _ColorMask ("Color Mask", Float) = 15
        [Toggle(UNITY_UI_ALPHACLIP)] _UseUIAlphaClip ("Use Alpha Clip", Float) = 0
    }

    SubShader
    {
        Tags
        {
            "Queue"="Transparent"
            "RenderType"="Transparent"
            "IgnoreProjector"="True"
            "PreviewType"="Plane"
        }

        Stencil
        {
            Ref [_Stencil]
            Comp [_StencilComp]
            Pass [_StencilOp]
            ReadMask [_StencilReadMask]
            WriteMask [_StencilWriteMask]
        }

        Cull Off
        Lighting Off
        ZWrite Off
        ZTest [unity_GUIZTestMode]
        Blend SrcAlpha OneMinusSrcAlpha
        ColorMask [_ColorMask]

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"
            #include "UnityUI.cginc"

            #pragma multi_compile_local _ UNITY_UI_ALPHACLIP

            struct appdata_t
            {
                float4 vertex   : POSITION;
                float4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex   : SV_POSITION;
                fixed4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
                float4 worldPosition : TEXCOORD1;
            };
            
            sampler2D _MainTex;
            fixed4 _Color;
            fixed4 _GlowColor;
            float _ColorSensitivity;
            float _GlowIntensity;
            float _FlashSpeed;
            float _FlashMin;
            float4 _ClipRect;

            v2f vert(appdata_t v)
            {
                v2f o;
                o.worldPosition = v.vertex;
                o.vertex = UnityObjectToClipPos(o.worldPosition);
                o.texcoord = v.texcoord;
                o.color = v.color * _Color; // 与 Image 组件的 Color 相乘
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                // 从纹理采样
                fixed4 texColor = tex2D(_MainTex, i.texcoord);
                
                // 结合 Image 组件的 Tint Color
                fixed4 baseColor = texColor * i.color;

                // 1. 计算与目标发光颜色的距离
                float colorDistance = distance(baseColor.rgb, _GlowColor.rgb);
                
                // 2. 新增判断：确保红色分量显著高于绿色和蓝色分量
                //    saturate() 会将结果限制在 0-1 之间
                float redDominance = saturate(baseColor.r - max(baseColor.g, baseColor.b));

                // 3. 结合两个因子
                //    只有颜色距离近 (glowFromDistance > 0) 且红色占主导 (redDominance > 0) 时才发光
                float glowFromDistance = 1.0 - smoothstep(0.0, _ColorSensitivity, colorDistance);
                float finalGlowFactor = glowFromDistance * redDominance;
                
                // 时间驱动的脉冲闪烁
                float pulse = _FlashMin + (1.0 - _FlashMin) * (0.5 + 0.5 * sin(_Time.y * _FlashSpeed * 6.28318));
                
                // 计算最终的发光颜色
                fixed4 emission = baseColor * finalGlowFactor * _GlowIntensity * pulse;

                // 最终颜色 = 基础颜色 + 发光颜色
                fixed4 finalColor = baseColor + emission;
                finalColor.a = baseColor.a;

                // 应用 UI 遮罩
                finalColor.a *= UnityGet2DClipping(i.worldPosition.xy, _ClipRect);
                #ifdef UNITY_UI_ALPHACLIP
                clip(finalColor.a - 0.001);
                #endif

                return finalColor;
            }
            ENDCG
        }
    }
}
