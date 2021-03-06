﻿using System.Diagnostics;
using System.Threading.Tasks;
using AGS.API;

using Autofac;

namespace AGS.Engine
{
	public class AGSObjectFactory : IObjectFactory
	{
        private readonly Resolver _resolver;
		private readonly IGameState _gameState;
        private IMaskLoader _maskLoader; //We can't get mask loader in the constructor due to a circular dependency -> mask loader requires the game factory, and the game factory requires the object factory

        public AGSObjectFactory(Resolver resolver, IGameState gameState)
		{
			_resolver = resolver;
			_gameState = gameState;
		}

		public IObject GetObject(string id)
		{
            Debug.WriteLine("Getting object: " + id ?? "null");
			TypedParameter idParam = new TypedParameter (typeof(string), id);
			return _resolver.Container.Resolve<IObject>(idParam);
		}

        public IObject GetAdventureObject(string id, string[] sayWhenLook = null, string[] sayWhenInteract = null)
        {
            IObject obj = GetObject(id);
            IHotspotComponent hotspot = obj.AddComponent<IHotspotComponent>();
            subscribeSentences(sayWhenLook, hotspot.Interactions.OnInteract(AGSInteractions.LOOK));
            subscribeSentences(sayWhenInteract, hotspot.Interactions.OnInteract(AGSInteractions.INTERACT));
            return obj;
        }

        public ICharacter GetCharacter(string id, IOutfit outfit, string[] sayWhenLook = null, string[] sayWhenInteract = null)
		{
			ICharacter character = GetCharacter(id, outfit, _resolver.Container.Resolve<IAnimationComponent>());

            subscribeSentences(sayWhenLook, character.Interactions.OnInteract(AGSInteractions.LOOK));
            subscribeSentences(sayWhenInteract, character.Interactions.OnInteract(AGSInteractions.INTERACT));

			return character;
		}

		public ICharacter GetCharacter(string id, IOutfit outfit, IAnimationComponent container)
		{
			TypedParameter outfitParam = new TypedParameter (typeof(IOutfit), outfit);
			TypedParameter idParam = new TypedParameter (typeof(string), id);
			TypedParameter animationParam = new TypedParameter (typeof(IAnimationComponent), container);
			ICharacter character = _resolver.Container.Resolve<ICharacter>(outfitParam, idParam, animationParam);
			return character;
		}

		public IObject GetHotspot(string maskPath, string hotspot, string[] sayWhenLook = null, 
			string[] sayWhenInteract = null, string id = null)
		{
            _maskLoader = _maskLoader ?? _resolver.Container.Resolve<IMaskLoader>();
			IMask mask = _maskLoader.Load(maskPath, debugDrawColor:  Colors.White, id: id ?? hotspot);
            if (mask == null) return newAdventureObject(id ?? hotspot, _resolver);
			setMask (mask, hotspot, sayWhenLook, sayWhenInteract);
			return mask.DebugDraw;
		}

		public async Task<IObject> GetHotspotAsync(string maskPath, string hotspot, string [] sayWhenLook = null,
			string [] sayWhenInteract = null, string id = null)
		{
            _maskLoader = _maskLoader ?? _resolver.Container.Resolve<IMaskLoader>();
			IMask mask = await _maskLoader.LoadAsync(maskPath, debugDrawColor: Colors.White, id: id ?? hotspot);
            if (mask == null) return newAdventureObject(id ?? hotspot, _resolver);
			setMask (mask, hotspot, sayWhenLook, sayWhenInteract);
			return mask.DebugDraw;
		}

        private IObject newAdventureObject(string id, Resolver resolver)
        {
            IObject obj = new AGSObject(id, _resolver);
            obj.AddComponent<IHotspotComponent>();
            return obj;
        }

		private void setMask (IMask mask, string hotspot, string [] sayWhenLook = null,
			string [] sayWhenInteract = null)
		{
            mask.DebugDraw.IsPixelPerfect = true;
            mask.DebugDraw.Enabled = true;
			mask.DebugDraw.DisplayName = hotspot;
			mask.DebugDraw.Opacity = 0;
			mask.DebugDraw.Z = mask.MinY;

            IHotspotComponent hotobj = mask.DebugDraw.GetComponent<IHotspotComponent>();
            subscribeSentences (sayWhenLook, hotobj.Interactions.OnInteract(AGSInteractions.LOOK));
            subscribeSentences (sayWhenInteract, hotobj.Interactions.OnInteract(AGSInteractions.INTERACT));
		}

		private void subscribeSentences(string[] sentences, IEvent<ObjectEventArgs> e)
		{
			if (sentences == null || e == null) return;

			e.SubscribeToAsync(async (_) =>
			{
				foreach (string sentence in sentences)
				{
					await _gameState.Player.SayAsync(sentence);
				}
			});
		}
	}
}

