﻿using System;
using AGS.API;
using OpenTK.Graphics.OpenGL;
using System.Drawing;
using System.Drawing.Imaging;

namespace AGS.Engine.Desktop
{
	public class DesktopBitmap : IBitmap
	{
		private Bitmap _bitmap;
		private IBitmapTextDraw _textDraw;

		public DesktopBitmap(Bitmap bitmap)
		{
			_bitmap = bitmap;
			_textDraw = new DesktopBitmapTextDraw (_bitmap);
		}

		#region IBitmap implementation

		public void Clear()
		{
			_bitmap.Clear();
		}

		public AGS.API.Color GetPixel(int x, int y)
		{
			return _bitmap.GetPixel(x, y).Convert();
		}

		public void MakeTransparent(AGS.API.Color color)
		{
			_bitmap.MakeTransparent(color.Convert());
		}

		public void LoadTexture(int? textureToBind)
		{
			BitmapData data = _bitmap.LockBits(new System.Drawing.Rectangle(0, 0, Width, Height),
				ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

			if (textureToBind != null) 
				GL.BindTexture(TextureTarget.Texture2D, textureToBind.Value);
			
			GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, Width, Height, 0,
				OpenTK.Graphics.OpenGL.PixelFormat.Bgra, PixelType.UnsignedByte, data.Scan0);
			_bitmap.UnlockBits(data);
		}

		public IBitmap ApplyArea(IArea area)
		{
			//todo: performance can be improved by only creating a bitmap the size of the area, and not the entire background.
			//This will require to change the rendering as well to offset the location
			byte zero = (byte)0;
			Bitmap output = new Bitmap(Width, Height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
			using (FastBitmap inBmp = new FastBitmap (_bitmap, ImageLockMode.ReadOnly))
			{
				using (FastBitmap outBmp = new FastBitmap (output, ImageLockMode.WriteOnly, true))
				{
					for (int y = 0; y < Height; y++)
					{
						int bitmapY = Height - y - 1;
						for (int x = 0; x < Width; x++)
						{
							System.Drawing.Color color = inBmp.GetPixel(x, bitmapY);
							byte alpha = area.IsInArea(new AGS.API.PointF(x, y)) ? color.A : zero;
							outBmp.SetPixel(x, bitmapY, System.Drawing.Color.FromArgb(alpha, color));
						}
					}
				}
			}

			return new DesktopBitmap(output);
		}

		public IBitmap Crop(AGS.API.Rectangle cropRect)
		{
			return new DesktopBitmap(_bitmap.Clone (cropRect.Convert(), _bitmap.PixelFormat)); //todo: improve performance by using FastBitmap
		}

		public IMask CreateMask(IGameFactory factory, string path, bool transparentMeansMasked = false, 
			AGS.API.Color? debugDrawColor = null, string saveMaskToFile = null, string id = null)
		{
			Bitmap debugMask = null;
			FastBitmap debugMaskFast = null;
			if (saveMaskToFile != null || debugDrawColor != null)
			{
				debugMask = new Bitmap (Width, Height);
				debugMaskFast = new FastBitmap (debugMask, ImageLockMode.WriteOnly, true);
			}

			bool[][] mask = new bool[Width][];
			System.Drawing.Color drawColor = debugDrawColor != null ? debugDrawColor.Value.Convert() : System.Drawing.Color.Black;

			using (FastBitmap bitmapData = new FastBitmap (_bitmap, ImageLockMode.ReadOnly))
			{
				for (int x = 0; x < Width; x++)
				{
					for (int y = 0; y < Height; y++)
					{
						var pixelColor = bitmapData.GetPixel(x, y);

						bool masked = pixelColor.A == 255;
						if (transparentMeansMasked)
							masked = !masked;

						if (mask[x] == null)
							mask[x] = new bool[Height];
						mask[x][Height - y - 1] = masked;

						if (debugMask != null)
						{
							debugMaskFast.SetPixel(x, y, masked ? drawColor : System.Drawing.Color.Transparent);
						}
					}
				}
			}

			if (debugMask != null)
				debugMaskFast.Dispose();

			//Save the duplicate
			if (saveMaskToFile != null)
				debugMask.Save (saveMaskToFile);

			IObject debugDraw = null;
			if (debugDrawColor != null)
			{
				debugDraw = factory.Object.GetObject(id ?? path ?? "Mask Drawable");
				debugDraw.Image = factory.Graphics.LoadImage(new DesktopBitmap(debugMask), null, path);
				debugDraw.Anchor = new AGS.API.PointF ();
			}

			return new AGSMask (mask, debugDraw);
		}

		public IBitmapTextDraw GetTextDraw()
		{
			return _textDraw;
		}

		public int Width { get { return _bitmap.Width; } }

		public int Height { get { return _bitmap.Height; } }

		#endregion
	}
}
