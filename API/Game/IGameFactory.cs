﻿using System;
using System.Drawing;

namespace AGS.API
{
	public interface IGameFactory
	{
		IGraphicsFactory Graphics { get; }
		ISoundFactory Sound { get; }

		int GetInt(string name, int defaultValue = 0);
		float GetFloat(string name, float defaultValue = 0f);
		string GetString(string name, string defaultValue = null);

		ILabel GetLabel(string text, float width, float height, float x, float y, ITextConfig config = null);
		IObject GetObject(string[] sayWhenLook = null, string[] sayWhenInteract = null);
		ICharacter GetCharacter(string[] sayWhenLook = null, string[] sayWhenInteract = null);
		IObject GetHotspot(string maskPath, string hotspot, string[] sayWhenLook = null, string[] sayWhenInteract = null);
		IObject GetHotspot(Bitmap maskBitmap, string hotspot, string[] sayWhenLook = null, string[] sayWhenInteract = null);

		IEdge GetEdge(float value = 0f);
		IRoom GetRoom(string id, float leftEdge = 0f, float rightEdge = 0f, float bottomEdge = 0f, float topEdge = 0f);

		void RegisterCustomData(ICustomSerializable customData);
	}
}

