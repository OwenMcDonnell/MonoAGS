﻿using System;
using AGS.Engine;
using AGS.API;

namespace DemoQuest
{
	//Shaders source taken from: https://github.com/mattdesl/lwjgl-basics/wiki/ShaderLesson3 & https://www.youtube.com/watch?v=qNM0k522R7o
	public static class Shaders
	{
		const string FRAGMENT_SHADER_GRAYSCALE = 
@"
#ifdef GL_ES
            precision mediump float;
#endif        
            uniform sampler2D uTexture;
#ifdef GL_ES
            varying vec4 vColor;
            varying vec2 vTexCoord;
#else
            varying vec4 gl_Color;
#endif

            void main()
            {
#ifndef GL_ES
                vec2 vTexCoord = gl_TexCoord[0].xy;
                vec4 vColor = gl_Color;
#endif
            	vec4 col = texture2D(uTexture, vTexCoord) * vColor;
            	float gray = dot(col.rgb, vec3(0.299, 0.587, 0.114));
            	gl_FragColor = vec4(gray, gray, gray, col.a);
            }";

		const string FRAGMENT_SHADER_SEPIA = 
@"
#ifdef GL_ES
            precision mediump float;
#endif        
            uniform sampler2D uTexture;
#ifdef GL_ES
            varying vec4 vColor;
            varying vec2 vTexCoord;
#else
            varying vec4 gl_Color;
#endif
            const vec3 SEPIA = vec3(1.2, 1.0, 0.8); 

            void main()
            {
#ifndef GL_ES
                vec2 vTexCoord = gl_TexCoord[0].xy;
                vec4 vColor = gl_Color;
#endif
            	vec4 col = texture2D(uTexture, vTexCoord) * vColor;
            	float gray = dot(col.rgb, vec3(0.299, 0.587, 0.114));
            	gl_FragColor = vec4(vec3(gray) * SEPIA, col.a);
            }";

		const string FRAGMENT_SHADER_SOFT_SEPIA = 
			@"
#ifdef GL_ES
            precision mediump float;
#endif        
            uniform sampler2D uTexture;
#ifdef GL_ES
            varying vec4 vColor;
            varying vec2 vTexCoord;
#else
            varying vec4 gl_Color;
#endif
            const vec3 SEPIA = vec3(1.2, 1.0, 0.8); 

            void main()
            {
#ifndef GL_ES
                vec2 vTexCoord = gl_TexCoord[0].xy;
                vec4 vColor = gl_Color;
#endif
            	vec4 col = texture2D(uTexture, vTexCoord);
            	float gray = dot(col.rgb, vec3(0.299, 0.587, 0.114));
            	vec3 sepiaColor = vec3(gray) * SEPIA;
            	col.rgb = mix(col.rgb, sepiaColor, 0.75);
            	gl_FragColor = col * vColor;
            }";

		const string FRAGMENT_SHADER_VIGNETTE = 
			@"
#ifdef GL_ES
            precision mediump float;
#endif        
            uniform sampler2D uTexture;
#ifdef GL_ES
            varying vec4 vColor;
            varying vec2 vTexCoord;
#else
            varying vec4 gl_Color;
#endif
            //The resolution needs to be set whenever the screen resizes
            uniform vec2 resolution;

            //RADIUS of our vignette, where 0.5 results in a circle fitting the screen
            const float RADIUS = 0.75;

            //softness of our vignette, between 0.0 and 1.0
            const float SOFTNESS = 0.45;

            void main()
            {
#ifndef GL_ES
                vec2 vTexCoord = gl_TexCoord[0].xy;
                vec4 vColor = gl_Color;
#endif
            	vec4 col = texture2D(uTexture, vTexCoord);

            	//determine center position
                vec2 position = (gl_FragCoord.xy / resolution.xy) - vec2(0.5);

                //determine the vector length of the center position
                float len = length(position);

                //use smoothstep to create a smooth vignette
                float vignette = smoothstep(RADIUS, RADIUS-SOFTNESS, len);

                //apply the vignette with 50% opacity
                col.rgb = mix(col.rgb, col.rgb * vignette, 0.5);

            	gl_FragColor = col * vColor;
            }
            ";
		const string FRAGMENT_SHADER_BLUR = 
			@"
#ifdef GL_ES
            precision mediump float;
#endif        
            uniform sampler2D uTexture;
#ifdef GL_ES
            varying vec4 vColor;
            varying vec2 vTexCoord;
#else
            varying vec4 gl_Color;
#endif

            void main()
            {
#ifndef GL_ES
                vec2 vTexCoord = gl_TexCoord[0].xy;
                vec4 vColor = gl_Color;
#endif
            	vec4 col = texture2D(uTexture, vTexCoord) * vColor;

            	int i, j;
            	vec4 sum = vec4(0);
            	for (i = -2; i <= 2; i++)
            		for (j = -2; j <= 2; j++)
            		{
            			vec2 offset = vec2(i, j) * 0.01;
            			vec4 currentCol = texture2D(uTexture, vTexCoord + offset);
            			sum += currentCol;
            		}
            	
            	gl_FragColor = (sum / vec4(25));	
            }";

        private static string getVertexShader()
        {
            return Hooks.GraphicsBackend.GetStandardVertexShader(); 
        }

		public static void SetStandardShader()
		{
			unbindVignetteShader();
            AGSGame.Shader = GLShader.FromText(getVertexShader(), Hooks.GraphicsBackend.GetStandardFragmentShader());
		}

		public static void SetGrayscaleShader()
		{
			unbindVignetteShader();
			AGSGame.Shader =  GLShader.FromText(getVertexShader(), FRAGMENT_SHADER_GRAYSCALE);
		}

		public static void SetSepiaShader()
		{
			unbindVignetteShader();
			AGSGame.Shader =  GLShader.FromText(getVertexShader(), FRAGMENT_SHADER_SEPIA);
		}

		public static void SetSoftSepiaShader()
		{
			unbindVignetteShader();
			AGSGame.Shader =  GLShader.FromText(getVertexShader(), FRAGMENT_SHADER_SOFT_SEPIA);
		}

		public static void SetBlurShader()
		{
			unbindVignetteShader();
			AGSGame.Game.State.Player.Shader = GLShader.FromText(getVertexShader(), FRAGMENT_SHADER_BLUR);
		}

		private static GLShader _vignetteShader;
		public static void SetVignetteShader()
		{
			_vignetteShader = GLShader.FromText(getVertexShader(), FRAGMENT_SHADER_VIGNETTE);
			AGSGame.Game.Events.OnBeforeRender.Subscribe(firstSetupVignette);
			AGSGame.Shader = _vignetteShader;
		}

		private static void firstSetupVignette(object sender, AGSEventArgs args)
		{
			setVignetteResolution();
			AGSGame.Game.Events.OnBeforeRender.Unsubscribe(firstSetupVignette);
			AGSGame.Game.Events.OnScreenResize.Subscribe(onVignetteShaderResize);
		}

		private static void onVignetteShaderResize(object sender, AGSEventArgs args)
		{
			setVignetteResolution();
		}

		private static void setVignetteResolution()
		{
            var resolution = AGSGame.Game.Settings.WindowSize;
			_vignetteShader.Compile();
			_vignetteShader.Bind();
			_vignetteShader.SetVariable("resolution", resolution.Width, resolution.Height);
		}

		private static void unbindVignetteShader()
		{
			var shader = _vignetteShader;
			if (shader == null) return;
			AGSGame.Game.Events.OnScreenResize.Unsubscribe(onVignetteShaderResize);
			_vignetteShader = null;
		}

		public static void SetShakeShader()
		{
			unbindVignetteShader();
			ShakeEffect effect = new ShakeEffect ();
			effect.RunBlocking(TimeSpan.FromSeconds(5));
		}

		public static void TurnOffShader()
		{
			unbindVignetteShader();
			AGSGame.Shader = null;
			AGSGame.Game.State.Player.Shader = null;
		}
	}
}

