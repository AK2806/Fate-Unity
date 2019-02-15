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
		private bool _repeatingAnimation = false;
		private Vec2 _position = new Vec2(0.0f, 0.0f);
		private int _zIndex = 0;
		private float _angle = 0.0f;
		private Vec2 _scale = new Vec2(1.0f, 1.0f);
		private float _alpha = 1.0f;
		private Vec3 _tint = new Vec3(1.0f, 1.0f, 1.0f);
		private int _spriteIndex = 0;

		public int ID { get { return _id; } }
		public Character CharacterRef { get { return _characterRef; } }
		public Portrait Portrait { get { return _portrait; } }
		public bool RepeatingAnimation { get { return _repeatingAnimation; } set { _repeatingAnimation = value; } }
		public bool Interactable { get { return _interactable; } set { _interactable = value; } }
		public Vec2 Position { get { return _position; } set { _position = value; } }
		public int ZIndex { get { return _zIndex; } set { _zIndex = value ; } }
		public float Angle { get { return _angle; } set { _angle = value; } }
		public Vec2 Scale { get { return _scale; } set { _scale = value; } }
		public float Alpha { get { return _alpha; } set { _alpha = value; } }
		public Vec3 Tint { get { return _tint; } set { _tint = value; } }
		public int SpriteIndex { get { return _spriteIndex; } set { _spriteIndex = value; } }
		 
		public SceneObject(int id, Character character, Portrait portrait) {
			_id = id;
			_characterRef = character;
			_portrait = portrait;
		}

	}

	public sealed class SceneCamera {
		private Vec2 _position = new Vec2(0.0f, 0.0f);
		private float _zoom = 1.0f;
		private bool _repeatingAnimation;

		public Vec2 Position { get { return  _position; } set { _position = value; } }
		public float Zoom { get { return _zoom; } set { _zoom = value; } }
		public bool RepeatingAnimation { get { return _repeatingAnimation; } set { _repeatingAnimation = value; } }
	}
}