using GameCore.Network.Stream;
using System;

namespace GameCore {
	public enum SceneType {
		STORY, BATTLE
	}
	
	public enum CharacterAction {
		CREATE_ASPECT = 1,
		ATTACK = 2,
		HINDER = 4
	}

	public enum CharacterSituationToken {
		PLAYER,
		FRIENDLY,
		NEUTRAL,
		NEUTRAL_HOSTILE,
		HOSTILE
	}

	public enum CharacterClassToken {
		CREATURE,
		ITEM,
		ENVIRONMENT
	}

	public enum CheckResult {
		FAIL = 0,
		TIE = 1,
		SUCCEED = 2,
		SUCCEED_WITH_STYLE = 3
	}

	public enum PersistenceType {
		Fixed = 0,
		Common = 1,
		Temporary = 2
	}

	public struct Vec2 : IStreamable {
		public float X, Y;

		public Vec2(float x, float y) {
			X = x; Y = y;
		}

		public void ReadFrom(IDataInputStream stream) {
			X = stream.ReadSingle();
			Y = stream.ReadSingle();
		}

		public void WriteTo(IDataOutputStream stream) {
			stream.WriteSingle(X);
			stream.WriteSingle(Y);
		}
	}

	public struct Vec3 : IStreamable {
		public float X, Y, Z;

		public Vec3(float x, float y, float z) {
			X = x; Y = y; Z = z;
		}

		public void ReadFrom(IDataInputStream stream) {
			X = stream.ReadSingle();
			Y = stream.ReadSingle();
			Z = stream.ReadSingle();
		}

		public void WriteTo(IDataOutputStream stream) {
			stream.WriteSingle(X);
			stream.WriteSingle(Y);
			stream.WriteSingle(Z);
		}
	}

	public struct Vec4 : IStreamable {
		public float X, Y, Z, W;

		public Vec4(float x, float y, float z, float w) {
			X = x; Y = y; Z = z; W = w;
		}

		public void ReadFrom(IDataInputStream stream) {
			X = stream.ReadSingle();
			Y = stream.ReadSingle();
			Z = stream.ReadSingle();
			W = stream.ReadSingle();
		}

		public void WriteTo(IDataOutputStream stream) {
			stream.WriteSingle(X);
			stream.WriteSingle(Y);
			stream.WriteSingle(Z);
			stream.WriteSingle(W);
		}
	}

	public struct Range : IStreamable {
		public bool lowOpen;
		public float low;

		public bool highOpen;
		public float high;

		public Range(float greaterEqual, float less) {
			lowOpen = false;
			highOpen = true;
			low = greaterEqual;
			high = less;
		}

		public bool InRange(float num) {
			bool ret = true;
			ret &= num > low || (!lowOpen && num == low);
			ret &= num < high || (!highOpen && num == high);
			return ret;
		}

		public bool OutOfRange(float num) {
			return !this.InRange(num);
		}

		public void ReadFrom(IDataInputStream stream) {
			lowOpen = stream.ReadBoolean();
			low = stream.ReadSingle();
			highOpen = stream.ReadBoolean();
			high = stream.ReadSingle();
		}

		public void WriteTo(IDataOutputStream stream) {
			stream.WriteBoolean(lowOpen);
			stream.WriteSingle(low);
			stream.WriteBoolean(highOpen);
			stream.WriteSingle(high);
		}
	}

	public struct Description : IStreamable {
		public string name;
		public string text;

		public void ReadFrom(IDataInputStream stream) {
			name = stream.ReadString();
			text = stream.ReadString();
		}

		public void WriteTo(IDataOutputStream stream) {
			stream.WriteString(name);
			stream.WriteString(text);
		}
	}

	public struct AssetReference : IStreamable {
		public bool isExternal;
		public int uid;
		public Guid guid;

		public void ReadFrom(IDataInputStream stream) {
			isExternal = stream.ReadBoolean();
			if (isExternal) {
				guid = InputStreamHelper.ReadGuid(stream);
			} else {
				uid = stream.ReadInt32();
			}
		}

		public void WriteTo(IDataOutputStream stream) {
			stream.WriteBoolean(isExternal);
			if (isExternal) {
				OutputStreamHelper.WriteGuid(stream, guid);
			} else {
				stream.WriteInt32(uid);
			}
		}
	}
	
	public struct RuntimeId : IStreamable {
		public int value;

		public void ReadFrom(IDataInputStream stream) {
			value = stream.ReadInt32();
		}

		public void WriteTo(IDataOutputStream stream) {
			stream.WriteInt32(value);
		}
	}

	public struct UserInfo : IStreamable {
		public bool isDM;
		public string id;
		public string name;
		public AssetReference avatar;

		public void ReadFrom(IDataInputStream stream) {
			isDM = stream.ReadBoolean();
			id = stream.ReadString();
			name = stream.ReadString();
			avatar.ReadFrom(stream);
		}

		public void WriteTo(IDataOutputStream stream) {
			stream.WriteBoolean(isDM);
			stream.WriteString(id);
			stream.WriteString(name);
			avatar.WriteTo(stream);
		}
	}

	namespace StoryScene {
		public struct Portrait : IStreamable {
			public AssetReference[] sprites;

			public void ReadFrom(IDataInputStream stream) {
				int length = stream.ReadInt32();
				sprites = new AssetReference[length];
				for (int i = 0; i < length; ++i) {
					sprites[i].ReadFrom(stream);
				}
			}

			public void WriteTo(IDataOutputStream stream) {
				stream.WriteInt32(sprites.Length);
				foreach (var sprite in sprites) {
					sprite.WriteTo(stream);
				}
			}
		}

		namespace AnimationCommand {
			public enum ObjectAttrType {
				Position, ZIndex, Rotation, Scale, Alpha, Tint, Sprite
			}

			public enum CameraAttrType {
				Position, Zoom
			}
			
			public enum EaseType {
				In = 1, Out = 2, InOut = 3,

				Linear = 4,
				Quad = 8, QuadIn = 9, QuadOut = 10, QuadInOut = 11,
				Cubic = 12, CubicIn = 13, CubicOut = 14, CubicInOut = 15,
				Quart = 16, QuartIn = 17, QuartOut = 18, QuartInOut = 19,
				Quint = 20, QuintIn = 21, QuintOut = 22, QuintInOut = 23,
				Sine = 24, SineIn = 25, SineOut = 26, SineInOut = 27,
				Expo = 28, ExpoIn = 29, ExpoOut = 30, ExpoInOut = 31,
				Circ = 32, CircIn = 33, CircOut = 34, CircInOut = 35,
				Back = 36, BackIn = 37, BackOut = 38, BackInOut = 39,
				Elastic = 40, ElasticIn = 41, ElasticOut = 42, ElasticInOut = 43,
				Bounce = 44, BounceIn = 45, BounceOut = 46, BounceInOut = 47
			}
			
			public class ObjectAnimCommand {
				public readonly bool isEased;
				public float timeOffset = 0.0f;
				protected ObjectAnimCommand(bool ease) { isEased = ease; }
			}

			public class CameraAnimCommand {
				public readonly bool isEased;
				public float timeOffset = 0.0f;
				protected CameraAnimCommand(bool ease) { isEased = ease; }
			}

			public class SetObjectAttribute : ObjectAnimCommand {
				public readonly ObjectAttrType attrType;
				protected SetObjectAttribute(ObjectAttrType t) : base(false) { attrType = t; }
			}

			public sealed class SetObjectPosition : SetObjectAttribute {
				public Vec2 val = new Vec2(0.0f, 0.0f);
				public SetObjectPosition() : base(ObjectAttrType.Position) { }
			}

			public sealed class SetObjectZIndex : SetObjectAttribute {
				public int val = 0;
				public SetObjectZIndex() : base(ObjectAttrType.ZIndex) { }
			}

			public sealed class SetObjectRotation : SetObjectAttribute {
				public float angle = 0.0f;
				public SetObjectRotation() : base(ObjectAttrType.Rotation) { }
			}

			public sealed class SetObjectScale : SetObjectAttribute {
				public Vec2 val = new Vec2(1.0f, 1.0f);
				public SetObjectScale() : base(ObjectAttrType.Scale) { }
			}

			public sealed class SetObjectAlpha : SetObjectAttribute {
				public float val = 1.0f;
				public SetObjectAlpha() : base(ObjectAttrType.Alpha) { }
			}

			public sealed class SetObjectTint : SetObjectAttribute {
				public Vec3 color = new Vec3(1.0f, 1.0f, 1.0f);
				public SetObjectTint() : base(ObjectAttrType.Tint) { }
			}

			public sealed class SetObjectSprite : SetObjectAttribute {
				public int spriteIndex = 0;
				public SetObjectSprite() : base(ObjectAttrType.Sprite) { }
			}

			public class EaseObjectAttribute : ObjectAnimCommand {
				public readonly ObjectAttrType attrType;
				public EaseType esType = EaseType.Linear;
				public float duration = 1.0f;
				protected EaseObjectAttribute(ObjectAttrType t) : base(true) { attrType = t; }
			}

			public sealed class EaseObjectPosition : EaseObjectAttribute {
				public Vec2 val;
				public EaseObjectPosition() : base(ObjectAttrType.Position) { }
			}

			public sealed class EaseObjectRotation : EaseObjectAttribute {
				public float angle = 0.0f;
				public EaseObjectRotation() : base(ObjectAttrType.Rotation) { }
			}

			public sealed class EaseObjectScale : EaseObjectAttribute {
				public Vec2 val = new Vec2(1.0f, 1.0f);
				public EaseObjectScale() : base(ObjectAttrType.Scale) { }
			}

			public sealed class EaseObjectAlpha : EaseObjectAttribute {
				public float val = 1.0f;
				public EaseObjectAlpha() : base(ObjectAttrType.Alpha) { }
			}

			public sealed class EaseObjectTint : EaseObjectAttribute {
				public Vec3 color = new Vec3(1.0f, 1.0f, 1.0f);
				public EaseObjectTint() : base(ObjectAttrType.Tint) { }
			}

			public sealed class EaseObjectSprite : EaseObjectAttribute {
				public int spriteIndex = 0;
				public EaseObjectSprite() : base(ObjectAttrType.Sprite) { }
			}

			public class SetCameraAttribute : CameraAnimCommand {
				public readonly CameraAttrType attrType;
				protected SetCameraAttribute(CameraAttrType t) : base(false) { attrType = t; }
			}

			public sealed class SetCameraPosition : SetCameraAttribute {
				public Vec2 val;
				public SetCameraPosition() : base(CameraAttrType.Position) { }
			}

			public sealed class SetCameraZoom : SetCameraAttribute {
				public float val;
				public SetCameraZoom() : base(CameraAttrType.Zoom) { }
			}

			public class EaseCameraAttribute : CameraAnimCommand {
				public readonly CameraAttrType attrType;
				public EaseType esType = EaseType.Linear;
				public float duration = 1.0f;
				protected EaseCameraAttribute(CameraAttrType t) : base(true) { attrType = t; }
			}

			public sealed class EaseCameraPosition : EaseCameraAttribute {
				public Vec2 val;
				public EaseCameraPosition() : base(CameraAttrType.Position) { }
			}

			public sealed class EaseCameraZoom : EaseCameraAttribute {
				public float val;
				public EaseCameraZoom() : base(CameraAttrType.Zoom) { }
			}
		}
	}

	namespace BattleScene {
		public enum BattleMapDirection {
			POSITIVE_ROW = 1,
			POSITIVE_COL = 2,
			NEGATIVE_ROW = 4,
			NEGATIVE_COL = 8
		}

		public struct SkillBattleMapProperty {
			public static readonly SkillBattleMapProperty INIT = new SkillBattleMapProperty() {
				actionPointCost = 1,
				useRange = new Range() { lowOpen = false, low = 0, highOpen = false, high = 0 },
				affectRange = new Range { lowOpen = false, low = 0, highOpen = false, high = 0 },
				islinearUse = false,
				islinearAffect = false,
				linearAffectDirection = BattleMapDirection.POSITIVE_ROW | BattleMapDirection.POSITIVE_COL | BattleMapDirection.NEGATIVE_ROW | BattleMapDirection.NEGATIVE_COL,
				linearUseDirection = BattleMapDirection.POSITIVE_ROW | BattleMapDirection.POSITIVE_COL | BattleMapDirection.NEGATIVE_ROW | BattleMapDirection.NEGATIVE_COL
			};

			public int actionPointCost;
			public Range useRange;
			public bool islinearUse;
			public BattleMapDirection linearUseDirection;
			public Range affectRange;
			public bool islinearAffect;
			public BattleMapDirection linearAffectDirection;
		}

		public struct GridPos : IStreamable {
			public int row;
			public int col;
			public bool highland;
			
			public void ReadFrom(IDataInputStream stream) {
				row = stream.ReadInt32();
				col = stream.ReadInt32();
				highland = stream.ReadBoolean();
			}

			public void WriteTo(IDataOutputStream stream) {
				stream.WriteInt32(row);
				stream.WriteInt32(col);
				stream.WriteBoolean(highland);
			}
		}

		public struct GridSprite : IStreamable {
			public void ReadFrom(IDataInputStream stream) {

			}

			public void WriteTo(IDataOutputStream stream) {

			}
		}

		public struct GridObjectData : IStreamable {
			public struct ActableObjectData : IStreamable {
				public int actionPoint;
				public int actionPointMax;
				public bool movable;
				public int movePoint;

				public void ReadFrom(IDataInputStream stream) {
					actionPoint = stream.ReadInt32();
					actionPointMax = stream.ReadInt32();
					movable = stream.ReadBoolean();
					movePoint = stream.ReadInt32();
				}

				public void WriteTo(IDataOutputStream stream) {
					stream.WriteInt32(actionPoint);
					stream.WriteInt32(actionPointMax);
					stream.WriteBoolean(movable);
					stream.WriteInt32(movePoint);
				}
			}

			public RuntimeId id;
			public int row;
			public int col;
			public bool obstacle;
			public bool highland;
			public int stagnate;
			public BattleMapDirection direction;
			public bool actable;
			public ActableObjectData actableObjData;

			public void ReadFrom(IDataInputStream stream) {
				id.ReadFrom(stream);
				row = stream.ReadInt32();
				col = stream.ReadInt32();
				obstacle = stream.ReadBoolean();
				highland = stream.ReadBoolean();
				stagnate = stream.ReadInt32();
				direction = (BattleMapDirection)stream.ReadByte();
				actable = stream.ReadBoolean();
				if (actable) {
					actableObjData.ReadFrom(stream);
				}
			}

			public void WriteTo(IDataOutputStream stream) {
				id.WriteTo(stream);
				stream.WriteInt32(row);
				stream.WriteInt32(col);
				stream.WriteBoolean(obstacle);
				stream.WriteBoolean(highland);
				stream.WriteInt32(stagnate);
				stream.WriteByte((byte)direction);
				stream.WriteBoolean(actable);
				if (actable) {
					actableObjData.WriteTo(stream);
				}
			}
		}

		public struct LadderObjectData : IStreamable {
			public RuntimeId id;
			public int row;
			public int col;
			public int stagnate;
			public BattleMapDirection direction;

			public void ReadFrom(IDataInputStream stream) {
				id.ReadFrom(stream);
				row = stream.ReadInt32();
				col = stream.ReadInt32();
				stagnate = stream.ReadInt32();
				direction = (BattleMapDirection)stream.ReadByte();
			}

			public void WriteTo(IDataOutputStream stream) {
				id.WriteTo(stream);
				stream.WriteInt32(row);
				stream.WriteInt32(col);
				stream.WriteInt32(stagnate);
				stream.WriteByte((byte)direction);
			}
		}
	}
}
