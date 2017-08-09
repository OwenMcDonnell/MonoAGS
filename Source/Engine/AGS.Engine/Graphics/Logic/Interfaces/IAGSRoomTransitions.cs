﻿using System;
using AGS.API;

namespace AGS.Engine
{
	public interface IAGSRoomTransitions : IRoomTransitions
	{
		new RoomTransitionState State { get; set; }

		IEvent<object> OnStateChanged { get; }
	}
}
