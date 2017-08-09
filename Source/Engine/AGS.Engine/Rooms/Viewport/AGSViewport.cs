﻿using AGS.API;

namespace AGS.Engine
{
	public class AGSViewport : IViewport
	{
        private float _x, _y, _scaleX, _scaleY, _angle;

		public AGSViewport ()
		{
			_scaleX = 1f;
			_scaleY = 1f;
            OnPositionChanged = new AGSEvent<object>();
            OnScaleChanged = new AGSEvent<object>();
            OnAngleChanged = new AGSEvent<object>();
		}

		#region IViewport implementation

        public float X { get { return _x; } set { refreshValue(ref _x, value, OnPositionChanged);} }

        public float Y { get { return _y; } set { refreshValue(ref _y, value, OnPositionChanged);} }

        public float ScaleX { get { return _scaleX; } set { refreshValue(ref _scaleX, value, OnScaleChanged);} }

        public float ScaleY { get { return _scaleY; } set { refreshValue(ref _scaleY, value, OnScaleChanged);} }

        public float Angle { get { return _angle; } set { refreshValue(ref _angle, value, OnAngleChanged);} }

		public ICamera Camera { get; set; }

        public IEvent<object> OnPositionChanged { get; private set; }
        public IEvent<object> OnScaleChanged { get; private set; }
        public IEvent<object> OnAngleChanged { get; private set; }

		#endregion

        private void refreshValue(ref float currentValue, float newValue, IEvent<object> changeEvent)
        {
#pragma warning disable RECS0018 // Comparison of floating point numbers with equality operator
            if (currentValue == newValue) return;
#pragma warning restore RECS0018 // Comparison of floating point numbers with equality operator
            currentValue = newValue;
            changeEvent.FireEvent(null);
        }
	}
}
