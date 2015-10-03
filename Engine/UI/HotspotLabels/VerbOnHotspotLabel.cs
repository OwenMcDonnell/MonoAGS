﻿using System;
using AGS.API;
using System.Collections.Generic;
using System.Linq;

namespace AGS.Engine
{
	public class VerbOnHotspotLabel
	{
		private Func<string> _getMode;
		private IGameEvents _events;
		private ILabel _label;
		private IInput _input;
		private IGameState _state;
		private Dictionary<string, string> _verbFormats;

		public VerbOnHotspotLabel(Func<string> getMode, IGame game, ILabel label)
		{
			_getMode = getMode;
			_label = label;
			_events = game.Events;
			_input = game.Input;
			_state = game.State;

			_verbFormats = new Dictionary<string, string> (10)
			{
				{"Look", "Look on {0}"},
				{"Interact", "Interact with {0}"},
				{"Walk", "Walk to {0}"},
				{"Talk", "Talk to {0}"},
			};
		}

		public void AddVerb(string verb, string format)
		{
			_verbFormats[verb] = format;
		}

		public void Start()
		{
			_events.OnRepeatedlyExecute.Subscribe(onTick);
		}

		private void onTick(object sender, EventArgs args)
		{
			IPoint position = _input.MousePosition;
			IObject obj = _state.Player.Character.Room.GetObjectAt (position.X, position.Y);
			if (obj == null || obj.Hotspot == null) 
			{
				_label.Visible = false;
				return;
			}
			_label.Visible = true;
			if (_state.Player.Character.Inventory.ActiveItem != null)
			{
				IInventoryItem inventoryItem = _state.Player.Character.Inventory.Items.FirstOrDefault(
					                              i => i.Graphics == obj);
				if (inventoryItem != null)
				{
					inventoryItem = _state.Player.Character.Inventory.ActiveItem;
					_label.Text = string.Format("Use {0} on {1}", inventoryItem.Graphics.Hotspot ??
						inventoryItem.CursorGraphics.Hotspot ?? "Item", obj.Hotspot);
					return;
				}
			}
			
			_label.Text = getSentence(obj.Hotspot);

		}

		private string getSentence(string hotspot)
		{
			string format;
			if (_verbFormats.TryGetValue(_getMode(), out format))
			{
				return string.Format(format, hotspot);
			}
			return hotspot;
		}
	}
}

