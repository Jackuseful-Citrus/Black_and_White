Shader "Custom/UI_InvertCircle"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)

        // 圆心 / 半径 / 边缘柔化
        _Center ("Center (UV)", Vector) = (0.5, 0.5, 0, 0)
        _Radius ("Radius", Range(0, 1.5)) = 0.0
        _Feather ("Feather", Range(0, 0.5)) = 0.05

        // --- UI Stencil & ColorMask（照抄 UI-Default） ---
        _StencilComp ("Stencil Comparison", Float) = 8
        _Stencil ("Stencil ID", Float) = 0
        _StencilOp ("Stencil Operation", Float) = 0
        _StencilWriteMask ("Stencil Write Mask", Float) = 255
        _StencilReadMask ("Stencil Read Mask", Float) = 255
        _ColorMask ("Color Mask", Float) = 15
    }

    SubShader
    {
        Tags
        {
            "Queue"="Transparent"
            "IgnoreProjector"="True"
            "RenderType"="Transparent"
            "PreviewType"="Plane"
            "CanUseSpriteAtlas"="True"
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


        Blend OneMinusDstColor Zero

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            #include "UnityUI.cginc"

            struct appdata_t
            {
                float4 vertex   : POSITION;
                float4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex        : SV_POSITION;
                fixed4 color         : COLOR;
                float2 texcoord      : TEXCOORD0;
                float4 worldPosition : TEXCOORD1;
                float4 screenPos     : TEXCOORD2; // 屏幕坐标
            };

            fixed4 _Color;
            sampler2D _MainTex;
            float4 _MainTex_ST;

            float2 _Center;
            float  _Radius;
            float  _Feather;

            v2f vert(appdata_t IN)
            {
                v2f OUT;
                OUT.worldPosition = IN.vertex;
                OUT.vertex = UnityObjectToClipPos(OUT.worldPosition);
                OUT.texcoord = TRANSFORM_TEX(IN.texcoord, _MainTex);
                OUT.color = IN.color * _Color;
                OUT.screenPos = ComputeScreenPos(OUT.vertex); // 带 w 的屏幕坐标
                return OUT;
            }

            fixed4 frag(v2f IN) : SV_Target
            {
                float2 uvScreen = IN.screenPos.xy / IN.screenPos.w;

                // 把圆心移到原点
                float2 p = uvScreen - _Center;

                // 用屏幕宽高比例把坐标拉正
                // _ScreenParams.x = width, _ScreenParams.y = height
                float aspect = _ScreenParams.x / _ScreenParams.y; 
                p.x *= aspect;           // 横向乘以宽高比，让 "1 像素" 在 x/y 上等价

                // 现在这个 d 就是“真·圆形距离”了
                float d = length(p);

                // 圆形 mask（跟之前一样）
                float inner = _Radius;
                float outer = _Radius + _Feather;
                float circle = 1.0 - smoothstep(inner, outer, d);


                // 圆外：直接丢弃像素，留下面的背景不动
                if (circle <= 0.0)
                    clip(-1);

                // 如果你给这个 Image 用的是纯白贴图，这里就是固定白色
                fixed4 col = tex2D(_MainTex, IN.texcoord) * IN.color;

                // UI Mask 支持
                #ifdef UNITY_UI_CLIP_RECT
                col.a *= UnityGet2DClipping(IN.worldPosition.xy, _ClipRect);
                #endif

                #ifdef UNITY_UI_ALPHACLIP
                clip (col.a - 0.001);
                #endif

                return col; // 只在圆内绘制 → 触发反色混合
            }
            ENDCG
        }
    }
}
