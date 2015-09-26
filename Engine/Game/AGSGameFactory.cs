﻿using System;
using AGS.API;
using Autofac;
using System.Drawing;

namespace AGS.Engine
{
	public class AGSGameFactory : IGameFactory
	{
		private IContainer _resolver;
		private IGameState _gameState;

		public AGSGameFactory(IGraphicsFactory graphics, IGameState state, IContainer resolver)
		{
			Graphics = graphics;
			_resolver = resolver;
			_gameState = state;
		}

		#region IGameFactory implementation

		public int GetInt(string name, int defaultValue = 0)
		{
			throw new NotImplementedException();
		}

		public float GetFloat(string name, float defaultValue = 0f)
		{
			throw new NotImplementedException();
		}

		public string GetString(string name, string defaultValue = null)
		{
			throw new NotImplementedException();
		}

		public ILabel GetLabel(string text, float width, float height, float x, float y, ITextConfig config = null)
		{
			SizeF baseSize = new SizeF(width, height);
			TypedParameter typedParameter = new TypedParameter (typeof(SizeF), baseSize);
			ILabel label = _resolver.Resolve<ILabel>(typedParameter);
			label.Text = text;
			label.X = x;
			label.Y = y;
			label.Tint = Color.Transparent;
			label.TextConfig = config ?? new AGSTextConfig();
			_gameState.UI.Add(label);
			return label;
		}

		public IObject GetObject(string[] sayWhenLook = null, string[] sayWhenInteract = null)
		{
			IObject obj = _resolver.Resolve<IObject>();

			subscribeSentences(sayWhenLook, obj.Interactions.OnLook);
			subscribeSentences(sayWhenInteract, obj.Interactions.OnInteract);

			return obj;
		}

		public ICharacter GetCharacter(string[] sayWhenLook = null, string[] sayWhenInteract = null)
		{
			ICharacter character = _resolver.Resolve<ICharacter>();

			subscribeSentences(sayWhenLook, character.Interactions.OnLook);
			subscribeSentences(sayWhenInteract, character.Interactions.OnInteract);

			return character;
		}

		public IObject GetHotspot(string maskPath, string hotspot, string[] sayWhenLook = null, string[] sayWhenInteract = null)
		{
			Bitmap image = (Bitmap)Image.FromFile(maskPath); 
			return GetHotspot(image, hotspot, sayWhenLook, sayWhenInteract);
		}

		public IObject GetHotspot(Bitmap maskBitmap, string hotspot, 
			string[] sayWhenLook = null, string[] sayWhenInteract = null)
		{
			IMaskLoader maskLoader = _resolver.Resolve<IMaskLoader>();
			IMask mask = maskLoader.Load(maskBitmap, debugDrawColor: Color.White);
			mask.DebugDraw.PixelPerfect(true);
			mask.DebugDraw.Hotspot = hotspot;
			mask.DebugDraw.Opacity = 0;
			mask.DebugDraw.Z = mask.MinY;

			subscribeSentences(sayWhenLook, mask.DebugDraw.Interactions.OnLook);
			subscribeSentences(sayWhenInteract, mask.DebugDraw.Interactions.OnInteract);

			return mask.DebugDraw;
		}

		public IEdge GetEdge(float value = 0f)
		{
			IEdge edge = _resolver.Resolve<IEdge>();
			edge.Value = value;
			return edge;
		}

		public IRoom GetRoom(string id, float leftEdge = 0f, float rightEdge = 0f, float topEdge = 0f, float bottomEdge = 0f)
		{
			AGSEdges edges = new AGSEdges (GetEdge(leftEdge), GetEdge(rightEdge), GetEdge(topEdge), GetEdge(bottomEdge));
			TypedParameter edgeParam = new TypedParameter (typeof(IAGSEdges), edges);
			TypedParameter idParam = new TypedParameter (typeof(string), id);
			IRoom room = _resolver.Resolve<IRoom>(idParam, edgeParam);
			room.Viewport.Follower = _resolver.Resolve<IFollower>();
			IPlayer player = _resolver.Resolve<IPlayer>();
			room.Viewport.Follower.Target = () => player.Character;
			return room;
		}

		public void RegisterCustomData(ICustomSerializable customData)
		{
			throw new NotImplementedException();
		}

		public IGraphicsFactory Graphics { get; private set; }

		public ISoundFactory Sound
		{
			get
			{
				throw new NotImplementedException();
			}
		}

		#endregion

		private void subscribeSentences(string[] sentences, IEvent<ObjectEventArgs> e)
		{
			if (sentences == null || e == null) return;

			e.SubscribeToAsync(async (sender, args) =>
			{
				foreach (string sentence in sentences)
				{
					await _gameState.Player.Character.SayAsync(sentence);
				}
			});
		}
	}
}

