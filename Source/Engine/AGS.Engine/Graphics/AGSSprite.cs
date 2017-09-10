﻿using AGS.API;

namespace AGS.Engine
{
	public class AGSSprite : AGSComponent, ISprite
	{
		private readonly IHasImage _hasImage;
        private readonly ITranslate _translate;        
        private readonly IRotate _rotate;
        private readonly IMaskLoader _maskLoader;
        private readonly Resolver _resolver;
        private static readonly SizeF _emptySize = new SizeF(1f, 1f);

		public AGSSprite (Resolver resolver, IMaskLoader maskLoader)
		{
            _maskLoader = maskLoader;
            _resolver = resolver;
            OnScaleChanged = new AGSEvent();

            //todo: abstract it to the constructor
            _translate = new AGSTranslate();
            _hasImage = new AGSHasImage();
            _hasImage.Anchor = new PointF();                        
            _rotate = new AGSRotate();

            ScaleX = 1;
            ScaleY = 1;
            _hasImage.OnImageChanged.Subscribe(() => ScaleBy(ScaleX, ScaleY));
        }

        private AGSSprite(AGSSprite sprite) : this(sprite._resolver, sprite._maskLoader)
        {
            _translate.Location = sprite._translate.Location;
            _hasImage.Anchor = sprite._hasImage.Anchor;
            _hasImage.Image = sprite._hasImage.Image;
            _hasImage.Tint = sprite._hasImage.Tint;
            _hasImage.CustomRenderer = sprite._hasImage.CustomRenderer;
            _rotate.Angle = sprite._rotate.Angle;
            ScaleX = sprite.ScaleX;
            ScaleY = sprite.ScaleY;
            Width = sprite.Width;
            Height = sprite.Height;
        }

        #region ISprite implementation

        public void ResetBaseSize(float initialWidth, float initialHeight)
        {
        }

        public void ResetScale(float initialWidth, float initialHeight)
        {
        }

        public void ResetScale()
        {
            ScaleX = 1;
            ScaleY = 1;
            var image = _hasImage.Image;
            if (image != null)
            {
                Width = _hasImage.Image.Width;
                Height = _hasImage.Image.Height;
            }
            OnScaleChanged.Invoke();
        }

        public void ScaleBy(float scaleX, float scaleY)
        {
            ScaleX = scaleX;
            ScaleY = scaleY;
            var image = _hasImage.Image;
            if (image != null)
            {
                Width = _hasImage.Image.Width * ScaleX;
                Height = _hasImage.Image.Height * ScaleY;
            }
            OnScaleChanged.Invoke();
        }

        public void ScaleTo(float width, float height)
        {
            Width = width;
            Height = height;
            var image = _hasImage.Image;
            if (image != null)
            {
                ScaleX = Width / _hasImage.Image.Width;
                ScaleY = Height / _hasImage.Image.Height;
            }
            OnScaleChanged.Invoke();
        }

        public void FlipHorizontally()
        {
            ScaleBy(-ScaleX, ScaleY);
            _hasImage.Anchor = new PointF(-_hasImage.Anchor.X, _hasImage.Anchor.Y);
            OnScaleChanged.Invoke();
        }

        public void FlipVertically()
        {
            ScaleBy(ScaleX, -ScaleY);
            _hasImage.Anchor = new PointF(_hasImage.Anchor.X, -_hasImage.Anchor.Y);
            OnScaleChanged.Invoke();
        }

        public ISprite Clone()
		{
            return new AGSSprite(this);
		}

        public ILocation Location { get { return _translate.Location; } set { _translate.Location = value; } }

        public float X { get { return _translate.X; } set { _translate.X = value; } }

        public float Y { get { return _translate.Y; } set { _translate.Y = value; } }

        public float Z { get { return _translate.Z; } set { _translate.Z = value; } }

        public float Height { get; private set; }

        public float Width { get; private set; }

        public float ScaleX { get; private set; }

        public float ScaleY { get; private set; }

        public SizeF BaseSize 
        { 
            get 
            {
                var image = _hasImage.Image;
                if (image == null) return _emptySize;
                return new SizeF(image.Width, image.Height);
            } 
        }

        public float Angle { get { return _rotate.Angle; } set { _rotate.Angle = value; } }

        public PointF Anchor { get { return _hasImage.Anchor; } set { _hasImage.Anchor = value; } }

        public IImageRenderer CustomRenderer { get { return _hasImage.CustomRenderer; } set { _hasImage.CustomRenderer = value; } }

        public IImage Image { get { return _hasImage.Image; } set { _hasImage.Image = value; } }

        public IEvent OnImageChanged { get { return _hasImage.OnImageChanged; } }

        public byte Opacity { get { return _hasImage.Opacity; } set { _hasImage.Opacity = value; } }

        public Color Tint { get { return _hasImage.Tint; } set { _hasImage.Tint = value; } }

        public IEvent OnLocationChanged { get { return _translate.OnLocationChanged; } }
        public IEvent OnAngleChanged { get { return _rotate.OnAngleChanged; } }
        public IEvent OnAnchorChanged { get { return _hasImage.OnAnchorChanged; } }
        public IEvent OnTintChanged { get { return _hasImage.OnTintChanged; } }
        public IEvent OnScaleChanged { get; private set; }

        public IArea PixelPerfectHitTestArea { get; private set; }
        public void PixelPerfect(bool pixelPerfect)
        {
            IArea area = PixelPerfectHitTestArea;
            if (!pixelPerfect)
            {
                if (area == null) return;
                area.Enabled = false;
                return;
            }
            if (area != null)
            {
                area.Enabled = true;
                return;
            }

            string areaId = string.Format("Sprite_PixelPerfect_{0}", Image.ID);
            string maskId = string.Format("Mask_{0}", areaId);
            PixelPerfectHitTestArea = new AGSArea(areaId, _resolver) { Mask = _maskLoader.Load(maskId, _hasImage.Image.OriginalBitmap) };
            var debugDraw = PixelPerfectHitTestArea.Mask.DebugDraw;
            if (debugDraw != null) debugDraw.RemoveComponents<IPixelPerfectComponent>(); //Removing the pixel perfect from the debug draw mask, otherwise it disables the pixel perfect for the images which can be used by actual characters
            PixelPerfectHitTestArea.Enabled = true;
        }
        #endregion

        public override string ToString()
		{
            return _hasImage.ToString();
		}
	}
}

