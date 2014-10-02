//Based off of the original implementation of a circle button from https://github.com/markushi/android-circlebutton

using System;
using Android.Widget;
using Android.Graphics;
using Android.Animation;
using Android.Util;
using Android.Content;
using Java.Interop;

namespace EightBot.Droid.CircleButton
{
	public class CircleButton : ImageView
	{
		private const int PRESSED_COLOR_LIGHTUP = 255 / 25;
		private const int PRESSED_RING_ALPHA = 75;
		private const int DEFAULT_PRESSED_RING_WIDTH_DIP = 4;
		private const int ANIMATION_TIME_ID = Android.Resource.Integer.ConfigShortAnimTime;

		private int _centerY;
		private int _centerX;
		private int _outerRadius;
		private int _pressedRingRadius;

		private Paint _circlePaint;
		private Paint _focusPaint;

		private float _animationProgress;

		private int _pressedRingWidth;
		private int _defaultColor = Android.Graphics.Color.Black;
		private int _pressedColor;
		private ObjectAnimator _pressedAnimator;

		public override bool Pressed {
			get {
				return base.Pressed;
			}
			set {
				base.Pressed = value;

				if (_circlePaint != null)
					_circlePaint.Color = new Android.Graphics.Color(Pressed ? _pressedColor : _defaultColor);

				if (Pressed)
					ShowPressedRing ();
				else
					HidePressedRing ();
			}
		}

		public float AnimationProgress {
			[Export("getAnimationProgress")]
			get { return _animationProgress; } 
			[Export("setAnimationProgress")]
			set { _animationProgress = value; Invalidate (); }
		}

		public int Color {
			get { return _defaultColor; }
			set { 
				_defaultColor = value;
				_pressedColor = GetHighlightColor (value, PRESSED_COLOR_LIGHTUP);

				_circlePaint.Color = new Android.Graphics.Color (_defaultColor);
				_focusPaint.Color = new Android.Graphics.Color (_defaultColor);

				_focusPaint.Alpha = PRESSED_RING_ALPHA;

				Invalidate ();
			}
		}
			
		public CircleButton(Context context) : base(context)
		{
			Init(context, null);
		}

		public CircleButton(Context context, IAttributeSet attrs) : base(context, attrs)
		{
			Init(context, attrs);
		}

		public CircleButton(Context context, IAttributeSet attrs, int defStyle) : base(context, attrs, defStyle)
		{
			Init(context, attrs);
		}

		private void Init(Context context, IAttributeSet attrs) {
			Focusable = true;
			SetScaleType (ScaleType.CenterInside);
			Clickable = true;

			_circlePaint = new Paint (PaintFlags.AntiAlias);
			_circlePaint.SetStyle (Paint.Style.Fill);

			_focusPaint = new Paint (PaintFlags.AntiAlias);
			_focusPaint.SetStyle (Paint.Style.Stroke);

			_pressedRingWidth = (int)TypedValue.ApplyDimension (ComplexUnitType.Dip, DEFAULT_PRESSED_RING_WIDTH_DIP, Resources.DisplayMetrics);

			var color = Android.Graphics.Color.Black;
			if(attrs != null){
				var styledAttributes = context.ObtainStyledAttributes (attrs, Resource.Styleable.CircleButton);
				color = styledAttributes.GetColor (Resource.Styleable.CircleButton_cb_color, color);
				_pressedRingWidth = (int)styledAttributes.GetDimension (Resource.Styleable.CircleButton_cb_pressed_ring_width, _pressedRingWidth);
				styledAttributes.Recycle ();
			}

			Color = color;

			_focusPaint.StrokeWidth = _pressedRingWidth;

			_pressedAnimator = ObjectAnimator.OfFloat (this, "animationProgress", 0, 0);
			_pressedAnimator.SetDuration (Resources.GetInteger (ANIMATION_TIME_ID));
		}
			
		protected override void OnDraw (Canvas canvas)
		{
			canvas.DrawCircle (_centerX, _centerY, _pressedRingRadius + _animationProgress, _focusPaint);
			canvas.DrawCircle (_centerX, _centerY, _outerRadius - _pressedRingWidth, _circlePaint);

			base.OnDraw (canvas);
		}

		protected override void OnSizeChanged (int w, int h, int oldw, int oldh)
		{
			base.OnSizeChanged (w, h, oldw, oldh);

			_centerX = w / 2;
			_centerY = h / 2;

			_outerRadius = Math.Min (w, h) / 2;

			_pressedRingRadius = _outerRadius - _pressedRingWidth - _pressedRingWidth / 2;
		}

		private void HidePressedRing () {
			_pressedAnimator.SetFloatValues (_pressedRingWidth, 0f);
			_pressedAnimator.Start ();
		}

		private void ShowPressedRing () {
			_pressedAnimator.SetFloatValues (_animationProgress, _pressedRingWidth);

			_pressedAnimator.Start ();
		}

		private int GetHighlightColor(int color, int amount) {
			return Android.Graphics.Color.Argb(
				Math.Min(255, Android.Graphics.Color.GetAlphaComponent(color)), 
				Math.Min(255, Android.Graphics.Color.GetRedComponent(color) + amount),
				Math.Min(255, Android.Graphics.Color.GetGreenComponent(color) + amount), 
				Math.Min(255, Android.Graphics.Color.GetBlueComponent(color) + amount));
		}

	}
}

