using GameService.Util;
using GameCore;
using System;
using System.Collections.Generic;

namespace GameService.CharacterData {
	public class Character : IIdentifiable {
		private readonly int _id;
		private string _name = "";
		private string _description = "";
		private readonly AssetReference _avatar;
		private User _controller = null;
		private bool _destroyed = false;
		private CharacterToken _token = CharacterToken.NEUTRAL;
		private Extra _belong = null;

		private int _refreshPoint = 0;
		private int _fatePoint = 0;
		private int _physicsStress = 0;
		private int _physicsStressMax = 0;
		private bool _physicsInvincible = false;
		private int _mentalStress = 0;
		private int _mentalStressMax = 0;
		private bool _mentalInvincible = false;
		private readonly Dictionary<AbilityType, AbilityData> _abilities;
		private readonly IdentifiedObjectList<Aspect> _aspects;
		private readonly IdentifiedObjectList<Stunt> _stunts;
		private readonly IdentifiedObjectList<Extra> _extras;
		private readonly IdentifiedObjectList<Consequence> _consequences;

		public int ID { get { return _id; } }
		public string Name { get { return _name; } set { _name = value; } }
		public string Description { get { return _description; } set { _description = value; } }
		public AssetReference Avatar { get { return _avatar; } }
		public User Controller { get { return _controller; } set { _controller = value; } }
		public bool Destroyed { get { return _destroyed; } set { _destroyed = value; } }
		public CharacterToken Token { get { return _token; } set { _token = value; } }
		public Extra Belong { get { return _belong; } }

		public int RefreshPoint { get { return _refreshPoint; } set { _refreshPoint = value; } }
		public int FatePoint { get { return _fatePoint; } set { _fatePoint = value; } }
		public int PhysicsStress { get { return _physicsStress; } set { _physicsStress = value; } }
		public int PhysicsStressMax { get { return _physicsStressMax; } set { _physicsStressMax = value; } }
		public bool PhysicsInvincible { get { return _physicsInvincible; } set { _physicsInvincible = value; } }
		public int MentalStress { get { return _mentalStress; } set { _mentalStress = value; } }
		public int MentalStressMax { get { return _mentalStressMax; } set { _mentalStressMax = value; } }
		public bool MentalInvincible { get { return _mentalInvincible; } set { _mentalInvincible = value; } }
		public Dictionary<AbilityType, AbilityData> Abilities { get { return _abilities; } }
		public IdentifiedObjectList<Aspect> Aspects { get { return _aspects; } }
		public IdentifiedObjectList<Stunt> Stunts { get { return _stunts; } }
		public IdentifiedObjectList<Extra> Extras { get { return _extras; } }
		public IdentifiedObjectList<Consequence> Consequences { get { return _consequences; } }
		
		public Character(int id, AssetReference avatar) {
			_id = id;
			_avatar = avatar;
			_abilities = new Dictionary<AbilityType, AbilityData>();
			_aspects = new IdentifiedObjectList<Aspect>();
			_stunts = new IdentifiedObjectList<Stunt>();
			_extras = new IdentifiedObjectList<Extra>();
			_consequences = new IdentifiedObjectList<Consequence>();
		}
	}

	public sealed class CharacterManager {
		private static readonly CharacterManager _instance = new CharacterManager();
		public static CharacterManager Instance { get { return _instance; } }

		private readonly IdentifiedObjectList<Character> _characterPool = new IdentifiedObjectList<Character>();
		private readonly IdentifiedObjectList<Character> _temporaryCharacterPool = new IdentifiedObjectList<Character>();

		public IdentifiedObjectList<Character> CharacterPool { get { return _characterPool; } }
		public IdentifiedObjectList<Character> TemporaryCharacterPool { get { return _temporaryCharacterPool; } }

		private CharacterManager() { }

		public Character FindCharacter(int id) {
			Character character;
			if (_characterPool.TryGetValue(id, out character)) return character;
			if (_temporaryCharacterPool.TryGetValue(id, out character)) return character;
			return null;
		}

	}
}
