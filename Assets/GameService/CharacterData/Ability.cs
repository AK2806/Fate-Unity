using GameService.Util;
using GameCore;
using System;
using System.Collections.Generic;

namespace GameService.CharacterData {
	public struct AbilityData {
		public string customName;
		public int level;
		public bool damageMental;
	}

	public sealed class AbilityType : IEquatable<AbilityType> {
		private static readonly Dictionary<int, AbilityType> _abilityTypes = new Dictionary<int, AbilityType>();
		public static Dictionary<int, AbilityType> AbilityTypes { get { return _abilityTypes; } }

		private readonly int _id;
		private readonly string _name;
		
		public int ID { get { return _id; } }
		public string Name { get { return _name; } }

		public AbilityType(int id, string name) {
			_id = id;
			_name = name;
		}

		public bool Equals(AbilityType other) {
			return other != null && _id == other._id;
		}

		public override bool Equals(object obj) {
			return Equals(obj as AbilityType);
		}

		public override int GetHashCode() {
			return _id.GetHashCode();
		}
	}
}
