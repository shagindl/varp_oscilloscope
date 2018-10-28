﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.UI;

namespace VARP.OSC
{
	[RequireComponent(typeof(RawImage))]
	public class OscGrid : MonoBehaviour
	{
		public Color bgColor;
		public Color gridColor;
		private OscSettings oscSettings;
		private RawImage screenImage;			//< The image to draw osciloscope
		private Texture2D screenTexture; 		//< The texture used for screenImage
		private Color[] clearColors; 			//< The colors to clear screenTexture
		private bool redraw;
		
		// ============================================================================================
		// Initialization
		// ============================================================================================

		/// <summary>Use this for initialization</summary>
		/// <param name="divisionsX">Divisions of X axis</para>
		/// <param name="diviaionsY">Divisions of Y axis</para>
		/// <param name="subdivisions">Subdivisions</para>
		public void Initialize(OscSettings oscSettings)
		{
			this.oscSettings = oscSettings;
			screenImage = GetComponent<RawImage>();
			drawGrid = oscSettings.drawGrid;
			drawRullerX = oscSettings.drawRullerX;
			drawRullerX = oscSettings.drawRullerY;
			redraw = true;
		}

		void Update()
		{
			if (redraw)
			{
				redraw = false;
				Clear(bgColor);
				PlotGrid(gridColor);
				Apply();
			}
		}

		private bool drawGrid;
		public bool DrawGrid
		{
			get { return drawGrid; }
			set { drawGrid = value; redraw = true; }
		}
		
		private bool drawRullerX;
		public bool DrawRullerX
		{
			get { return drawRullerX; }
			set { drawRullerX = value; redraw = true; }
		}
		
		private bool drawRullerY;
		public bool DrawRullerY
		{
			get { return drawRullerY; }
			set { drawRullerY = value; redraw = true; }
		}
		
		// ============================================================================================
		// Clear screen
		// ============================================================================================

		/// <summary>Clear screen by vlack color</summary>
		/// <param name="color">Grid color</para>
		public void Clear(Color color)
		{
			var w = (int) oscSettings.textureSize.x;
			var h = (int) oscSettings.textureSize.y;
			if (screenTexture == null)
			{
				CreateTexture(w, h, color);
			}
			else if (screenTexture.width != w || screenTexture.height != h)
			{
				Destroy(screenTexture);
				CreateTexture(w, h, color);
			}

			screenTexture.SetPixels(clearColors, 0);
		}

		// Create new texture and the buffer of colors to clear it
		private void CreateTexture(int w, int h, Color color)
		{
			clearColors = new Color[w * h];
			for (var i = 0; i < clearColors.Length; i++)
				clearColors[i] = color;
			screenTexture = new Texture2D(w, h, TextureFormat.RGBA32, false, false);
			screenTexture.filterMode = FilterMode.Bilinear;
			screenTexture.wrapMode = TextureWrapMode.Clamp;
		}

		// ============================================================================================
		// Display texture
		// ============================================================================================

		/// <summary>Apply texture to screen image</summary>
		public void Apply()
		{
			screenTexture.Apply();
			screenImage.texture = screenTexture;
			screenImage.color = Color.white;
		}

		// ============================================================================================
		// Draw grid
		// ============================================================================================

		/// <summary>
		/// Draw oscilloscope grid
		/// </summary>
		/// <param name="color"></param>
		public void PlotGrid(Color color)
		{
			var w = oscSettings.textureSize.x;
			var h = oscSettings.textureSize.y;
			var xcenter = oscSettings.textureCenter.x;
			var ycenter = oscSettings.textureCenter.y;
			var pxDiv = oscSettings.pixelsPerDivision;
			var pxSub = oscSettings.pixelsPerSubdivision;

			if (drawGrid)
			{
				// -- draw main grid 
				for (var x = xcenter; x<w; x+=pxDiv)
					PlotDotedLineVertical(screenTexture, x, ycenter, pxSub, color);
				for (var x = xcenter - pxDiv; x>=0; x-=pxDiv)
					PlotDotedLineVertical(screenTexture, x, ycenter, pxSub, color);
				for (var y = ycenter; y<h; y+=pxDiv)
					PlotDotedLineHorizontal(screenTexture, xcenter, y, pxSub, color);
				for (var y = ycenter - pxDiv; y>=0; y-=pxDiv)
					PlotDotedLineHorizontal(screenTexture, xcenter, y, pxSub, color);
			}

			if (drawRullerX)
			{
				// -- draw horizontal ruller bar in center
				PlotDotedLineHorizontal(screenTexture, xcenter, ycenter + 2, pxSub, color);
				PlotDotedLineHorizontal(screenTexture, xcenter, ycenter + 1, pxSub, color);
				PlotDotedLineHorizontal(screenTexture, xcenter, ycenter - 1, pxSub, color);
				PlotDotedLineHorizontal(screenTexture, xcenter, ycenter - 2, pxSub, color);
			}

			if (drawRullerY)
			{
				// -- draw verticals ruller bar in center
				PlotDotedLineVertical(screenTexture, xcenter + 2, ycenter, pxSub, color);
				PlotDotedLineVertical(screenTexture, xcenter + 1, ycenter, pxSub, color);
				PlotDotedLineVertical(screenTexture, xcenter - 1, ycenter, pxSub, color);
				PlotDotedLineVertical(screenTexture, xcenter - 2, ycenter, pxSub, color);
			}

			// -- draw frame arounf
			OscUtils.PlotRectangle(screenTexture, 0, 0, w - 1, h - 1, color);

		}
		
		// ============================================================================================
		// Draw dotted lines
		// ============================================================================================
		
		/// <summary>
		/// Draw horizontaly multiple dots as the ruller's divisions. Argumens are in pixel Units.
		/// </summary>
		/// <param name="color"></param>
		/// <param name="x">Line position</param>
		/// <param name="y">Line position</param>
		/// <param name="step">Step between divisions</param>
		public static void PlotDotedLineHorizontal(Texture2D texture, int x, int y, int step, Color color)
		{
			var w = texture.width;
			var ix = x;
			while (ix < w)
			{
				texture.SetPixel(ix, y, color);
				ix += step;
			}

			ix = x;
			while (ix >= 0)
			{
				texture.SetPixel(ix, y, color);
				ix -= step;
			}
		}

		/// <summary>
		/// Draw verticaly multiple dots as the ruller's divisions 
		/// </summary>
		/// <param name="color"></param>
		/// <param name="x">Line position</param>
		/// <param name="y">Line position</param>
		/// <param name="step">Step between divisions</param>
		public static void PlotDotedLineVertical(Texture2D texture, int x, int y, int step, Color color)
		{
			var h = texture.height;
			var iy = y;
			while (iy < h)
			{
				texture.SetPixel(x, iy, color);
				iy += step;
			}
			iy = y;
			while (iy >= 0)
			{
				texture.SetPixel(x, iy, color);
				iy -= step;
			}
		}


	}
}
