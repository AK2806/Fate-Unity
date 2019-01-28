using System.Collections;
using System.Collections.Generic;
using GameCore.Network;
using GameCore;
using GameCore.StoryScene;
using GameService.Util;
using GameService.CharacterData;

namespace GameService.ServerProxy.DataComponent.StoryScene {
    public sealed class SceneObject : IIdentifiable {
		private readonly int _id;
		private readonly Character _characterRef;
		private readonly Portrait _portrait;
		private bool _interactable = false;

		public int ID { get { return _id; } }
		public Character CharacterRef { get { return _characterRef; } }
		public Portrait Portrait { get { return _portrait; } }
		public bool Interactable { get { return _interactable; } set { _interactable = value; } }

		public SceneObject(int id, Character character, Portrait portrait) {
			_id = id;
			_characterRef = character;
			_portrait = portrait;
		}

	}
}