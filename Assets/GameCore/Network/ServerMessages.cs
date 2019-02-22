using GameCore.Network.Stream;
using GameCore.StoryScene;
using GameCore.BattleScene;
using System.Diagnostics;
using GameCore.StoryScene.AnimationCommand;

namespace GameCore.Network.ServerMessages {
	public sealed class UserOnlineMessage : Message {
		public const int MESSAGE_TYPE = -1;
		public override int MessageType { get { return MESSAGE_TYPE; } }

		public RuntimeId playerID;

		public override void ReadFrom(IDataInputStream stream) {
			playerID.ReadFrom(stream);
		}

		public override void WriteTo(IDataOutputStream stream) {
			playerID.WriteTo(stream);
		}
	}

	public sealed class UserOfflineMessage : Message {
		public const int MESSAGE_TYPE = -2;
		public override int MessageType { get { return MESSAGE_TYPE; } }

		public RuntimeId playerID;

		public override void ReadFrom(IDataInputStream stream) {
			playerID.ReadFrom(stream);
		}

		public override void WriteTo(IDataOutputStream stream) {
			playerID.WriteTo(stream);
		}
	}
	
	public sealed class UserLoadingMessage : Message {
		public const int MESSAGE_TYPE = -3;
		public override int MessageType { get { return MESSAGE_TYPE; } }

		public RuntimeId playerID;
		public byte percent;

		public override void ReadFrom(IDataInputStream stream) {
			playerID.ReadFrom(stream);
			percent = stream.ReadByte();
		}

		public override void WriteTo(IDataOutputStream stream) {
			playerID.WriteTo(stream);
			stream.WriteByte(percent);
		}
	}
	
	public sealed class UserLoadCompleteMessage : Message {
		public const int MESSAGE_TYPE = -4;
		public override int MessageType { get { return MESSAGE_TYPE; } }

		public RuntimeId playerID;

		public override void ReadFrom(IDataInputStream stream) {
			playerID.ReadFrom(stream);
		}

		public override void WriteTo(IDataOutputStream stream) {
			playerID.WriteTo(stream);
		}
	}

	public sealed class SyncUsersInfoMessage : Message {
		public const int MESSAGE_TYPE = -5;
		public override int MessageType { get { return MESSAGE_TYPE; } }

		public RuntimeId clientUserID;
		public RuntimeId[] usersID;
		public UserInfo[] usersInfo;
		public RuntimeId[] charactersID;

		public override void ReadFrom(IDataInputStream stream) {
			clientUserID.ReadFrom(stream);
			int length = stream.ReadInt32();
			usersID = new RuntimeId[length];
			usersInfo = new UserInfo[length];
			charactersID = new RuntimeId[length];
			for (int i = 0; i < length; ++i) {
				usersID[i].ReadFrom(stream);
			}
			for (int i = 0; i < length; ++i) {
				usersInfo[i].ReadFrom(stream);
			}
			for (int i = 0; i < length; ++i) {
				charactersID[i].ReadFrom(stream);
			}
		}

		public override void WriteTo(IDataOutputStream stream) {
			Debug.Assert(usersID.Length == usersInfo.Length && usersID.Length == charactersID.Length);
			clientUserID.WriteTo(stream);
			stream.WriteInt32(usersID.Length);
			foreach (var id in usersID) {
				id.WriteTo(stream);
			}
			foreach (var info in usersInfo) {
				info.WriteTo(stream);
			}
			foreach (var characterID in charactersID) {
				characterID.WriteTo(stream);
			}
		}
	}

	public sealed class CampaignStartMessage : Message {
		public const int MESSAGE_TYPE = -6;
		public override int MessageType { get { return MESSAGE_TYPE; } }

		public override void ReadFrom(IDataInputStream stream) { }
		public override void WriteTo(IDataOutputStream stream) { }
	}

	/*
	public sealed class ResourceSynchronizationMessage : Message {
		public const int MESSAGE_TYPE = -2;
		public override int MessageType { get { return MESSAGE_TYPE; } }

		public string[] packs;
		public Guid[] guids;
		public short[] transmittalIDs;
		public byte[] belongPacksIndex;
		
		public override void ReadFrom(IDataInputStream stream) {
			int length = stream.ReadByte();
			packs = new string[length];
			for (int i = 0; i < length; ++i) {
				packs[i] = stream.ReadString();
			}
			length = stream.ReadInt16();
			guids = new Guid[length];
			transmittalIDs = new short[length];
			belongPacksIndex = new byte[length];
			for (int i = 0; i < length; ++i) {
				guids[i] = InputStreamHelper.ReadGuid(stream);
				transmittalIDs[i] = stream.ReadInt16();
				belongPacksIndex[i] = stream.ReadByte();
			}
		}

		public override void WriteTo(IDataOutputStream stream) {
			Debug.Assert(packs.Length <= 256 && guids.Length <= 65536 &&
				guids.Length == transmittalIDs.Length && guids.Length == belongPacksIndex.Length);
			stream.WriteByte((byte)packs.Length);
			foreach (var pack in packs) {
				stream.WriteString(pack);
			}
			stream.WriteInt16((short)guids.Length);
			foreach (Guid guid in guids) {
				OutputStreamHelper.WriteGuid(stream, guid);
			}
			foreach (var transmittalID in transmittalIDs) {
				stream.WriteInt16(transmittalID);
			}
			foreach (var belongPackIndex in belongPacksIndex) {
				stream.WriteByte(belongPackIndex);
			}
		}
	}
	*/

	public sealed class UserDecidingMessage : Message {
		public const int MESSAGE_TYPE = -6;
		public override int MessageType { get { return MESSAGE_TYPE; } }

		public string promptText;
		public string[] selections;

		public override void WriteTo(IDataOutputStream stream) {
			stream.WriteString(promptText);
			stream.WriteInt32(selections.Length);
			foreach (var selection in selections) {
				stream.WriteString(selection);
			}
		}

		public override void ReadFrom(IDataInputStream stream) {
			promptText = stream.ReadString();
			int length = stream.ReadInt32();
			selections = new string[length];
			for (int i = 0; i < length; ++i) {
				selections[i] = stream.ReadString();
			}
		}
	}

	public sealed class WaitUserDecisionMessage : Message {
		public const int MESSAGE_TYPE = -7;
		public override int MessageType { get { return MESSAGE_TYPE; } }

		public bool enabled;

		public override void WriteTo(IDataOutputStream stream) {
			stream.WriteBoolean(enabled);
		}

		public override void ReadFrom(IDataInputStream stream) {
			enabled = stream.ReadBoolean();
		}
	}

	public sealed class ChatMessage : Message {
		public const int MESSAGE_TYPE = -8;
		public override int MessageType { get { return MESSAGE_TYPE; } }

		public RuntimeId senderID;
		public bool isPrivate;
		public string text;

		public override void WriteTo(IDataOutputStream stream) {
			senderID.WriteTo(stream);
			stream.WriteBoolean(isPrivate);
			stream.WriteString(text);
		}

		public override void ReadFrom(IDataInputStream stream) {
			senderID.ReadFrom(stream);
			isPrivate = stream.ReadBoolean();
			text = stream.ReadString();
		}
	}

	public sealed class LoadAssetsMessage : Message {
		public const int MESSAGE_TYPE = -9;
		public override int MessageType { get { return MESSAGE_TYPE; } }

		public bool isFixed;
		public AssetReference[] assets;

		public override void ReadFrom(IDataInputStream stream) {
			isFixed = stream.ReadBoolean();
			int length = stream.ReadInt32();
			assets = new AssetReference[length];
			for (int i = 0; i < length; ++i) {
				assets[i].ReadFrom(stream);
			}
		}

		public override void WriteTo(IDataOutputStream stream) {
			stream.WriteBoolean(isFixed);
			stream.WriteInt32(assets.Length);
			foreach (var asset in assets) {
				asset.WriteTo(stream);
			}
		}
	}

	public sealed class ShowSceneMessage : Message {
		public const int MESSAGE_TYPE = -10;
		public override int MessageType { get { return MESSAGE_TYPE; } }

		public SceneType scene;

		public override void WriteTo(IDataOutputStream stream) {
			stream.WriteByte((byte)scene);
		}

		public override void ReadFrom(IDataInputStream stream) {
			scene = (SceneType)stream.ReadByte();
		}
	}

	public sealed class DisplayDicePointsMessage : Message {
		public const int MESSAGE_TYPE = -37;
		public override int MessageType { get { return MESSAGE_TYPE; } }

		public RuntimeId userID;
		public int[] dicePoints;

		public override void WriteTo(IDataOutputStream stream) {
			userID.WriteTo(stream);
			stream.WriteInt32(dicePoints.Length);
			foreach (var point in dicePoints) {
				stream.WriteInt32(point);
			}
		}

		public override void ReadFrom(IDataInputStream stream) {
			userID.ReadFrom(stream);
			int length = stream.ReadInt32();
			dicePoints = new int[length];
			for (int i = 0; i < length; ++i) {
				dicePoints[i] = stream.ReadInt32();
			}
		}
	}

	public sealed class PlayBGMMessage : Message {
		public const int MESSAGE_TYPE = -10;
		public override int MessageType { get { return MESSAGE_TYPE; } }

		public AssetReference bgm;

		public override void WriteTo(IDataOutputStream stream) {
			bgm.WriteTo(stream);
		}

		public override void ReadFrom(IDataInputStream stream) {
			bgm.ReadFrom(stream);
		}
	}

	public sealed class StopBGMMessage : Message {
		public const int MESSAGE_TYPE = -11;
		public override int MessageType { get { return MESSAGE_TYPE; } }

		public override void ReadFrom(IDataInputStream stream) { }
		public override void WriteTo(IDataOutputStream stream) { }
	}

	public sealed class PlaySEMessage : Message {
		public const int MESSAGE_TYPE = -12;
		public override int MessageType { get { return MESSAGE_TYPE; } }

		public AssetReference se;

		public override void WriteTo(IDataOutputStream stream) {
			se.WriteTo(stream);
		}

		public override void ReadFrom(IDataInputStream stream) {
			se.ReadFrom(stream);
		}
	}

	public sealed class CampaignDataMessage : Message {
		public const int MESSAGE_TYPE = -72;
		public override int MessageType { get { return MESSAGE_TYPE; } }

		public struct Node : IStreamable {
			public AssetReference thumbnail;
			public RuntimeId id;
			public string name;
			public string description;

			public void ReadFrom(IDataInputStream stream) {
				thumbnail.ReadFrom(stream);
				id.ReadFrom(stream);
				name = stream.ReadString();
				description = stream.ReadString();
			}

			public void WriteTo(IDataOutputStream stream) {
				thumbnail.WriteTo(stream);
				id.WriteTo(stream);
				stream.WriteString(name);
				stream.WriteString(description);
			}
		}

		public Node[] campaignNodes;
		public Node[][] pointcutShotNodes;

		public override void ReadFrom(IDataInputStream stream) {
			int length = stream.ReadInt32();
			campaignNodes = new Node[length];
			for (int i = 0; i < length; ++i) {
				campaignNodes[i].ReadFrom(stream);
			}
			pointcutShotNodes = new Node[length][];
			for (int i = 0; i < length; ++i) {
				int shotLength = stream.ReadInt32();
				pointcutShotNodes[i] = new Node[shotLength];
				for (int j = 0; j < shotLength; ++j) {
					pointcutShotNodes[i][j].ReadFrom(stream);
				}
			}
		}

		public override void WriteTo(IDataOutputStream stream) {
			Debug.Assert(campaignNodes.Length == pointcutShotNodes.Length);
			stream.WriteInt32(campaignNodes.Length);
			foreach (var campaignNode in campaignNodes) {
				campaignNode.WriteTo(stream);
			}
			foreach (var shotNodeList in pointcutShotNodes) {
				stream.WriteInt32(shotNodeList.Length);
				foreach (var shotNode in shotNodeList) {
					shotNode.WriteTo(stream);
				}
			}
		}
	}

	public sealed class CampaignNextActionInfoMessage : Message {
		public const int MESSAGE_TYPE = -72;
		public override int MessageType { get { return MESSAGE_TYPE; } }

		public bool hasNext;
		public Description description;

		public override void ReadFrom(IDataInputStream stream) {
			hasNext = stream.ReadBoolean();
			if (hasNext) {
				description.ReadFrom(stream);
			}
		}

		public override void WriteTo(IDataOutputStream stream) {
			stream.WriteBoolean(hasNext);
			if (hasNext) {
				description.WriteTo(stream);
			}
		}
	}

	public sealed class CheckerStartCheckMessage : Message {
		public const int MESSAGE_TYPE = -71;
		public override int MessageType { get { return MESSAGE_TYPE; } }

		public RuntimeId initiativeID;
		public RuntimeId initiativeAbilityTypeID;
		public CharacterAction action;
		public RuntimeId[] targetsID;

		public override void ReadFrom(IDataInputStream stream) {
			initiativeID.ReadFrom(stream);
			initiativeAbilityTypeID.ReadFrom(stream);
			action = (CharacterAction)stream.ReadByte();
			int length = stream.ReadInt32();
			targetsID = new RuntimeId[length];
			for (int i = 0; i < length; ++i) {
				targetsID[i].ReadFrom(stream);;
			}
		}

		public override void WriteTo(IDataOutputStream stream) {
			initiativeID.WriteTo(stream);
			initiativeAbilityTypeID.WriteTo(stream);
			stream.WriteByte((byte)action);
			stream.WriteInt32(targetsID.Length);
			foreach (var targetID in targetsID) {
				targetID.WriteTo(stream);
			}
		}
	}

	public sealed class CheckerCheckNextOneMessage : Message {
		public const int MESSAGE_TYPE = -72;
		public override int MessageType { get { return MESSAGE_TYPE; } }

		public RuntimeId nextOneID;

		public override void ReadFrom(IDataInputStream stream) {
			nextOneID.ReadFrom(stream);
		}

		public override void WriteTo(IDataOutputStream stream) {
			nextOneID.WriteTo(stream);
		}
	}

	public sealed class CheckerEndCheckMessage : Message {
		public const int MESSAGE_TYPE = -73;
		public override int MessageType { get { return MESSAGE_TYPE; } }

		public override void ReadFrom(IDataInputStream stream) { }
		public override void WriteTo(IDataOutputStream stream) { }
	}

	public sealed class CheckerCheckResultMessage : Message {
		public const int MESSAGE_TYPE = -75;
		public override int MessageType { get { return MESSAGE_TYPE; } }

		public CheckResult initiative;
		public CheckResult passive;
		public int delta;

		public override void WriteTo(IDataOutputStream stream) {
			stream.WriteByte((byte)initiative);
			stream.WriteByte((byte)passive);
			stream.WriteInt32(delta);
		}

		public override void ReadFrom(IDataInputStream stream) {
			initiative = (CheckResult)stream.ReadByte();
			passive = (CheckResult)stream.ReadByte();
			delta = stream.ReadInt32();
		}
	}

	public sealed class CheckerNotifyPassiveSelectActionMessage : Message {
		public const int MESSAGE_TYPE = -55;
		public override int MessageType { get { return MESSAGE_TYPE; } }

		public override void ReadFrom(IDataInputStream stream) { }
		public override void WriteTo(IDataOutputStream stream) { }
	}

	public sealed class CheckerNotifySelectAspectMessage : Message {
		public const int MESSAGE_TYPE = -56;
		public override int MessageType { get { return MESSAGE_TYPE; } }

		public bool isInitiative;

		public override void ReadFrom(IDataInputStream stream) {
			isInitiative = stream.ReadBoolean();
		}

		public override void WriteTo(IDataOutputStream stream) {
			stream.WriteBoolean(isInitiative);
		}
	}

	public sealed class CheckerCharacterActionResponseMessage : Message {
		public const int MESSAGE_TYPE = -40;
		public override int MessageType { get { return MESSAGE_TYPE; } }

		public bool isInitiative;
		public bool failure;
		public string failureDescription;

		public override void ReadFrom(IDataInputStream stream) {
			isInitiative = stream.ReadBoolean();
			failure = stream.ReadBoolean();
			failureDescription = stream.ReadString();
		}

		public override void WriteTo(IDataOutputStream stream) {
			stream.WriteBoolean(isInitiative);
			stream.WriteBoolean(failure);
			stream.WriteString(failureDescription);
		}
	}

	public sealed class CheckerSelectAspectResponseMessage : Message {
		public const int MESSAGE_TYPE = -42;
		public override int MessageType { get { return MESSAGE_TYPE; } }

		public bool selectionOver;
		public bool isInitiative;
		public bool failure;
		public string failureDescription;

		public override void ReadFrom(IDataInputStream stream) {
			selectionOver = stream.ReadBoolean();
			isInitiative = stream.ReadBoolean();
			failure = stream.ReadBoolean();
			failureDescription = stream.ReadString();
		}

		public override void WriteTo(IDataOutputStream stream) {
			stream.WriteBoolean(selectionOver);
			stream.WriteBoolean(isInitiative);
			stream.WriteBoolean(failure);
			stream.WriteString(failureDescription);
		}
	}

	public sealed class CheckerCanInitiativeUseSkillMessage : Message {
		public const int MESSAGE_TYPE = -66;
		public override int MessageType { get { return MESSAGE_TYPE; } }

		public bool result;

		public override void WriteTo(IDataOutputStream stream) {
			stream.WriteBoolean(result);
		}

		public override void ReadFrom(IDataInputStream stream) {
			result = stream.ReadBoolean();
		}
	}

	public sealed class CheckerCanInitiativeUseStuntMessage : Message {
		public const int MESSAGE_TYPE = -66;
		public override int MessageType { get { return MESSAGE_TYPE; } }

		public bool result;

		public override void WriteTo(IDataOutputStream stream) {
			stream.WriteBoolean(result);
		}

		public override void ReadFrom(IDataInputStream stream) {
			result = stream.ReadBoolean();
		}
	}

	public sealed class CheckerPassiveUsableActionListMessage : Message {
		public const int MESSAGE_TYPE = -67;
		public override int MessageType { get { return MESSAGE_TYPE; } }

		public RuntimeId[] abilityTypesID;
		public RuntimeId[] stuntsID;

		public override void WriteTo(IDataOutputStream stream) {
			stream.WriteInt32(abilityTypesID.Length);
			foreach (var abilityTypeID in abilityTypesID) {
				abilityTypeID.WriteTo(stream);
			}
			stream.WriteInt32(stuntsID.Length);
			foreach (var stuntID in stuntsID) {
				stuntID.WriteTo(stream);
			}
		}

		public override void ReadFrom(IDataInputStream stream) {
			int length = stream.ReadInt32();
			abilityTypesID = new RuntimeId[length];
			for (int i = 0; i < length; ++i) {
				abilityTypesID[i].ReadFrom(stream);
			}
			length = stream.ReadInt32();
			stuntsID = new RuntimeId[length];
			for (int i = 0; i < length; ++i) {
				stuntsID[i].ReadFrom(stream);
			}
		}
	}

	public sealed class CheckerStuntTargetValidityMessage : Message {
		public const int MESSAGE_TYPE = -76;
		public override int MessageType { get { return MESSAGE_TYPE; } }

		public bool result;

		public override void WriteTo(IDataOutputStream stream) {
			stream.WriteBoolean(result);
		}

		public override void ReadFrom(IDataInputStream stream) {
			result = stream.ReadBoolean();
		}
	}

	public sealed class CheckerUpdateSumPointMessage : Message {
		public const int MESSAGE_TYPE = -57;
		public override int MessageType { get { return MESSAGE_TYPE; } }

		public bool isInitiative;
		public int point;

		public override void ReadFrom(IDataInputStream stream) {
			isInitiative = stream.ReadBoolean();
			point = stream.ReadInt32();
		}

		public override void WriteTo(IDataOutputStream stream) {
			stream.WriteBoolean(isInitiative);
			stream.WriteInt32(point);
		}
	}

	public sealed class CheckerDisplayUsingSkillMessage : Message {
		public const int MESSAGE_TYPE = -58;
		public override int MessageType { get { return MESSAGE_TYPE; } }

		public bool isInitiative;
		public RuntimeId abilityTypeID;

		public override void ReadFrom(IDataInputStream stream) {
			isInitiative = stream.ReadBoolean();
			abilityTypeID.ReadFrom(stream);
		}

		public override void WriteTo(IDataOutputStream stream) {
			stream.WriteBoolean(isInitiative);
			abilityTypeID.WriteTo(stream);
		}
	}

	public sealed class CheckerDisplayUsingStuntMessage : Message {
		public const int MESSAGE_TYPE = -81;
		public override int MessageType { get { return MESSAGE_TYPE; } }

		public RuntimeId characterID;
		public RuntimeId stuntID;

		public override void WriteTo(IDataOutputStream stream) {
			characterID.WriteTo(stream);
			stuntID.WriteTo(stream);
		}

		public override void ReadFrom(IDataInputStream stream) {
			characterID.ReadFrom(stream);
			stuntID.ReadFrom(stream);
		}
	}

	public sealed class CheckerDisplayUsingAspectMessage : Message {
		public const int MESSAGE_TYPE = -59;
		public override int MessageType { get { return MESSAGE_TYPE; } }

		public bool isInitiative;
		public RuntimeId aspectOwnerID;
		public RuntimeId aspectID;

		public override void ReadFrom(IDataInputStream stream) {
			isInitiative = stream.ReadBoolean();
			aspectOwnerID.ReadFrom(stream);
			aspectID.ReadFrom(stream);
		}

		public override void WriteTo(IDataOutputStream stream) {
			stream.WriteBoolean(isInitiative);
			aspectOwnerID.WriteTo(stream);
			aspectID.WriteTo(stream);
		}
	}

	public sealed class SyncAbilityTypeListMessage : Message {
		public const int MESSAGE_TYPE = -33;
		public override int MessageType { get { return MESSAGE_TYPE; } }

		public struct TypeInfo : IStreamable {
			public RuntimeId id;
			public string name;

			public void ReadFrom(IDataInputStream stream) {
				id.ReadFrom(stream);
				name = stream.ReadString();
			}

			public void WriteTo(IDataOutputStream stream) {
				id.WriteTo(stream);
				stream.WriteString(name);
			}
		}

		public TypeInfo[] abilityTypes;

		public override void WriteTo(IDataOutputStream stream) {
			stream.WriteInt32(abilityTypes.Length);
			foreach (var abilityType in abilityTypes) {
				abilityType.WriteTo(stream);
			}
		}

		public override void ReadFrom(IDataInputStream stream) {
			int length = stream.ReadInt32();
			abilityTypes = new TypeInfo[length];
			for (int i = 0; i < length; ++i) {
				abilityTypes[i].ReadFrom(stream);
			}
		}
	}

	public sealed class CreateCharacterMessage : Message {
		public const int MESSAGE_TYPE = -251;
		public override int MessageType { get { return MESSAGE_TYPE; } }

		public RuntimeId id;
		public Description description;
		public AssetReference avatar;

		public override void ReadFrom(IDataInputStream stream) {
			id.ReadFrom(stream);
			description.ReadFrom(stream);
			avatar.ReadFrom(stream);
		}

		public override void WriteTo(IDataOutputStream stream) {
			id.WriteTo(stream);
			description.WriteTo(stream);
			avatar.WriteTo(stream);
		}
	}
	
	public sealed class CreateTemporaryCharacterMessage : Message {
		public const int MESSAGE_TYPE = -252;
		public override int MessageType { get { return MESSAGE_TYPE; } }

		public RuntimeId id;
		public Description description;
		public AssetReference avatar;

		public override void ReadFrom(IDataInputStream stream) {
			id.ReadFrom(stream);
			description.ReadFrom(stream);
			avatar.ReadFrom(stream);
		}

		public override void WriteTo(IDataOutputStream stream) {
			id.WriteTo(stream);
			description.WriteTo(stream);
			avatar.WriteTo(stream);
		}
	}
	
	public sealed class ClearTemporaryCharacterListMessage : Message {
		public const int MESSAGE_TYPE = -253;
		public override int MessageType { get { return MESSAGE_TYPE; } }
		
		public override void ReadFrom(IDataInputStream stream) { }
		public override void WriteTo(IDataOutputStream stream) { }
	}

	public sealed class UpdateCharacterBaseDataMessage : Message {
		public const int MESSAGE_TYPE = -254;
		public override int MessageType { get { return MESSAGE_TYPE; } }

		public RuntimeId characterID;
		public RuntimeId controllerID;
		public bool destroyed;
		public CharacterSituationToken situationToken;
		public CharacterClassToken classToken;
		public int fatePoint;
		public int refreshPoint;
		public int physicsStress;
		public int physicsStressMax;
		public bool physicsInvincible;
		public int mentalStress;
		public int mentalStressMax;
		public bool mentalInvincible;

		public override void ReadFrom(IDataInputStream stream) {
			characterID.ReadFrom(stream);
			controllerID.ReadFrom(stream);
			destroyed = stream.ReadBoolean();
			situationToken = (CharacterSituationToken)stream.ReadByte();
			fatePoint = stream.ReadInt32();
			refreshPoint = stream.ReadInt32();
			physicsStress = stream.ReadInt32();
			physicsStressMax = stream.ReadInt32();
			physicsInvincible = stream.ReadBoolean();
			mentalStress = stream.ReadInt32();
			mentalStressMax = stream.ReadInt32();
			mentalInvincible = stream.ReadBoolean();
		}

		public override void WriteTo(IDataOutputStream stream) {
			characterID.WriteTo(stream);
			controllerID.WriteTo(stream);
			stream.WriteBoolean(destroyed);
			stream.WriteByte((byte)situationToken);
			stream.WriteInt32(fatePoint);
			stream.WriteInt32(refreshPoint);
			stream.WriteInt32(physicsStress);
			stream.WriteInt32(physicsStressMax);
			stream.WriteBoolean(physicsInvincible);
			stream.WriteInt32(mentalStress);
			stream.WriteInt32(mentalStressMax);
			stream.WriteBoolean(mentalInvincible);
		}
	}

	public sealed class UpdateCharacterAbilityDataMessage : Message {
		public const int MESSAGE_TYPE = -295;
		public override int MessageType { get { return MESSAGE_TYPE; } }

		public RuntimeId characterID;
		public RuntimeId typeID;
		public string customName;
		public int level;
		public bool damageMental;

		public override void WriteTo(IDataOutputStream stream) {
			characterID.WriteTo(stream);
			typeID.WriteTo(stream);
			stream.WriteString(customName);
			stream.WriteInt32(level);
			stream.WriteBoolean(damageMental);
		}

		public override void ReadFrom(IDataInputStream stream) {
			characterID.ReadFrom(stream);
			typeID.ReadFrom(stream);
			customName = stream.ReadString();
			level = stream.ReadInt32();
			damageMental = stream.ReadBoolean();
		}
	}

	public sealed class CharacterAddAspectMessage : Message {
		public const int MESSAGE_TYPE = -276;
		public override int MessageType { get { return MESSAGE_TYPE; } }

		public RuntimeId characterID;
		public RuntimeId aspectID;
		public Description description;

		public override void WriteTo(IDataOutputStream stream) {
			characterID.WriteTo(stream);
			aspectID.WriteTo(stream);
			description.WriteTo(stream);
		}

		public override void ReadFrom(IDataInputStream stream) {
			characterID.ReadFrom(stream);
			aspectID.ReadFrom(stream);
			description.ReadFrom(stream);
		}
	}

	public sealed class CharacterRemoveAspectMessage : Message {
		public const int MESSAGE_TYPE = -277;
		public override int MessageType { get { return MESSAGE_TYPE; } }

		public RuntimeId characterID;
		public RuntimeId aspectID;

		public override void WriteTo(IDataOutputStream stream) {
			characterID.WriteTo(stream);
			aspectID.WriteTo(stream);
		}

		public override void ReadFrom(IDataInputStream stream) {
			characterID.ReadFrom(stream);
			aspectID.ReadFrom(stream);
		}
	}

	public sealed class CharacterUpdateAspectDataMessage : Message {
		public const int MESSAGE_TYPE = -278;
		public override int MessageType { get { return MESSAGE_TYPE; } }

		public RuntimeId characterID;
		public RuntimeId aspectID;
		public PersistenceType persistenceType;
		public RuntimeId benefiterID;
		public int benefitTimes;

		public override void WriteTo(IDataOutputStream stream) {
			characterID.WriteTo(stream);
			aspectID.WriteTo(stream);
			stream.WriteByte((byte)persistenceType);
			benefiterID.WriteTo(stream);
			stream.WriteInt32(benefitTimes);
		}

		public override void ReadFrom(IDataInputStream stream) {
			characterID.ReadFrom(stream);
			aspectID.ReadFrom(stream);
			persistenceType = (PersistenceType)stream.ReadByte();
			benefiterID.ReadFrom(stream);
			benefitTimes = stream.ReadInt32();
		}
	}

	public sealed class CharacterClearAspectListMessage : Message {
		public const int MESSAGE_TYPE = -279;
		public override int MessageType { get { return MESSAGE_TYPE; } }

		public RuntimeId characterID;

		public override void WriteTo(IDataOutputStream stream) {
			characterID.WriteTo(stream);
		}

		public override void ReadFrom(IDataInputStream stream) {
			characterID.ReadFrom(stream);
		}
	}

	public sealed class CharacterAddConsequenceMessage : Message {
		public const int MESSAGE_TYPE = -218;
		public override int MessageType { get { return MESSAGE_TYPE; } }

		public RuntimeId characterID;
		public RuntimeId consequenceID;
		public Description description;

		public override void WriteTo(IDataOutputStream stream) {
			characterID.WriteTo(stream);
			consequenceID.WriteTo(stream);
			description.WriteTo(stream);
		}

		public override void ReadFrom(IDataInputStream stream) {
			characterID.ReadFrom(stream);
			consequenceID.ReadFrom(stream);
			description.ReadFrom(stream);
		}
	}

	public sealed class CharacterRemoveConsequenceMessage : Message {
		public const int MESSAGE_TYPE = -228;
		public override int MessageType { get { return MESSAGE_TYPE; } }

		public RuntimeId characterID;
		public RuntimeId consequenceID;

		public override void WriteTo(IDataOutputStream stream) {
			characterID.WriteTo(stream);
			consequenceID.WriteTo(stream);
		}

		public override void ReadFrom(IDataInputStream stream) {
			characterID.ReadFrom(stream);
			consequenceID.ReadFrom(stream);
		}
	}

	public sealed class CharacterUpdateConsequenceMessage : Message {
		public const int MESSAGE_TYPE = -328;
		public override int MessageType { get { return MESSAGE_TYPE; } }

		public RuntimeId characterID;
		public RuntimeId consequenceID;
		public PersistenceType persistenceType;
		public RuntimeId benefiterID;
		public int benefitTimes;
		public int counteractLevel;
		public int actualDamage;
		public bool mental;

		public override void WriteTo(IDataOutputStream stream) {
			characterID.WriteTo(stream);
			consequenceID.WriteTo(stream);
			stream.WriteByte((byte)persistenceType);
			benefiterID.WriteTo(stream);
			stream.WriteInt32(benefitTimes);
			stream.WriteInt32(counteractLevel);
			stream.WriteInt32(actualDamage);
			stream.WriteBoolean(mental);
		}

		public override void ReadFrom(IDataInputStream stream) {
			characterID.ReadFrom(stream);
			consequenceID.ReadFrom(stream);
			persistenceType = (PersistenceType)stream.ReadByte();
			benefiterID.ReadFrom(stream);
			benefitTimes = stream.ReadInt32();
			counteractLevel = stream.ReadInt32();
			actualDamage = stream.ReadInt32();
			mental = stream.ReadBoolean();
		}
	}

	public sealed class CharacterClearConsequenceListMessage : Message {
		public const int MESSAGE_TYPE = -427;
		public override int MessageType { get { return MESSAGE_TYPE; } }

		public RuntimeId characterID;

		public override void WriteTo(IDataOutputStream stream) {
			characterID.WriteTo(stream);
		}

		public override void ReadFrom(IDataInputStream stream) {
			characterID.ReadFrom(stream);
		}
	}

	public sealed class CharacterAddStuntMessage : Message {
		public const int MESSAGE_TYPE = -530;
		public override int MessageType { get { return MESSAGE_TYPE; } }

		public RuntimeId characterID;
		public RuntimeId stuntID;
		public Description description;

		public override void WriteTo(IDataOutputStream stream) {
			characterID.WriteTo(stream);
			stuntID.WriteTo(stream);
			description.WriteTo(stream);
		}

		public override void ReadFrom(IDataInputStream stream) {
			characterID.ReadFrom(stream);
			stuntID.ReadFrom(stream);
			description.ReadFrom(stream);
		}
	}

	public sealed class CharacterRemoveStuntMessage : Message {
		public const int MESSAGE_TYPE = -630;
		public override int MessageType { get { return MESSAGE_TYPE; } }

		public RuntimeId characterID;
		public RuntimeId stuntID;

		public override void WriteTo(IDataOutputStream stream) {
			characterID.WriteTo(stream);
			stuntID.WriteTo(stream);
		}

		public override void ReadFrom(IDataInputStream stream) {
			characterID.ReadFrom(stream);
			stuntID.ReadFrom(stream);
		}
	}

	public sealed class CharacterUpdateStuntMessage : Message {
		public const int MESSAGE_TYPE = -730;
		public override int MessageType { get { return MESSAGE_TYPE; } }

		public RuntimeId characterID;
		public RuntimeId stuntID;

		public override void WriteTo(IDataOutputStream stream) {
			characterID.WriteTo(stream);
			stuntID.WriteTo(stream);
		}

		public override void ReadFrom(IDataInputStream stream) {
			characterID.ReadFrom(stream);
			stuntID.ReadFrom(stream);
		}
	}

	public sealed class CharacterClearStuntListMessage : Message {
		public const int MESSAGE_TYPE = -827;
		public override int MessageType { get { return MESSAGE_TYPE; } }

		public RuntimeId characterID;

		public override void WriteTo(IDataOutputStream stream) {
			characterID.WriteTo(stream);
		}

		public override void ReadFrom(IDataInputStream stream) {
			characterID.ReadFrom(stream);
		}
	}

	public sealed class CharacterAddExtraMessage : Message {
		public const int MESSAGE_TYPE = -931;
		public override int MessageType { get { return MESSAGE_TYPE; } }

		public RuntimeId characterID;
		public RuntimeId extraID;
		public Description description;
		public RuntimeId boundCharacterID;

		public override void WriteTo(IDataOutputStream stream) {
			characterID.WriteTo(stream);
			extraID.WriteTo(stream);
			description.WriteTo(stream);
			boundCharacterID.WriteTo(stream);
		}

		public override void ReadFrom(IDataInputStream stream) {
			characterID.ReadFrom(stream);
			extraID.ReadFrom(stream);
			description.ReadFrom(stream);
			boundCharacterID.ReadFrom(stream);
		}
	}

	public sealed class CharacterRemoveExtraMessage : Message {
		public const int MESSAGE_TYPE = -310;
		public override int MessageType { get { return MESSAGE_TYPE; } }

		public RuntimeId characterID;
		public RuntimeId extraID;

		public override void WriteTo(IDataOutputStream stream) {
			characterID.WriteTo(stream);
			extraID.WriteTo(stream);
		}

		public override void ReadFrom(IDataInputStream stream) {
			characterID.ReadFrom(stream);
			extraID.ReadFrom(stream);
		}
	}

	public sealed class CharacterUpdateExtraMessage : Message {
		public const int MESSAGE_TYPE = -311;
		public override int MessageType { get { return MESSAGE_TYPE; } }

		public RuntimeId characterID;
		public RuntimeId extraID;
		public bool isLongRangeWeapon;
		public bool isVehicle;

		public override void WriteTo(IDataOutputStream stream) {
			characterID.WriteTo(stream);
			extraID.WriteTo(stream);
			stream.WriteBoolean(isLongRangeWeapon);
			stream.WriteBoolean(isVehicle);
		}

		public override void ReadFrom(IDataInputStream stream) {
			characterID.ReadFrom(stream);
			extraID.ReadFrom(stream);
			isLongRangeWeapon = stream.ReadBoolean();
			isVehicle = stream.ReadBoolean();
		}
	}

	public sealed class CharacterClearExtraListMessage : Message {
		public const int MESSAGE_TYPE = -617;
		public override int MessageType { get { return MESSAGE_TYPE; } }

		public RuntimeId characterID;

		public override void WriteTo(IDataOutputStream stream) {
			characterID.WriteTo(stream);
		}

		public override void ReadFrom(IDataInputStream stream) {
			characterID.ReadFrom(stream);
		}
	}

	public sealed class DirectResistableSkillsListMessage : Message {
		public const int MESSAGE_TYPE = -32;
		public override int MessageType { get { return MESSAGE_TYPE; } }

		public RuntimeId[] abilityTypesID;

		public override void WriteTo(IDataOutputStream stream) {
			stream.WriteInt32(abilityTypesID.Length);
			foreach (var abilityTypeID in abilityTypesID) {
				abilityTypeID.WriteTo(stream);
			}
		}

		public override void ReadFrom(IDataInputStream stream) {
			int length = stream.ReadInt32();
			abilityTypesID = new RuntimeId[length];
			for (int i = 0; i < length; ++i) {
				abilityTypesID[i].ReadFrom(stream);
			}
		}
	}

	public sealed class DirectResistableStuntsListMessage : Message {
		public const int MESSAGE_TYPE = -79;
		public override int MessageType { get { return MESSAGE_TYPE; } }

		public RuntimeId characterID;
		public RuntimeId[] stuntsID;

		public override void WriteTo(IDataOutputStream stream) {
			characterID.WriteTo(stream);
			stream.WriteInt32(stuntsID.Length);
			foreach (var stuntID in stuntsID) {
				stuntID.WriteTo(stream);
			}
		}

		public override void ReadFrom(IDataInputStream stream) {
			characterID.ReadFrom(stream);
			int length = stream.ReadInt32();
			stuntsID = new RuntimeId[length];
			for (int i = 0; i < length; ++i) {
				stuntsID[i].ReadFrom(stream);
			}
		}
	}

	public sealed class StorySceneResetMessage : Message {
		public const int MESSAGE_TYPE = -2;
		public override int MessageType { get { return MESSAGE_TYPE; } }

		public override void ReadFrom(IDataInputStream stream) { }
		public override void WriteTo(IDataOutputStream stream) { }
	}

	public sealed class StorySceneCreateObjectMessage : Message {
		public const int MESSAGE_TYPE = -3;
		public override int MessageType { get { return MESSAGE_TYPE; } }

		public RuntimeId objID;
		public RuntimeId characterID;
		public Portrait portrait;

		public override void WriteTo(IDataOutputStream stream) {
			objID.WriteTo(stream);
			characterID.WriteTo(stream);
			portrait.WriteTo(stream);
		}

		public override void ReadFrom(IDataInputStream stream) {
			objID.ReadFrom(stream);
			characterID.ReadFrom(stream);
			portrait.ReadFrom(stream);
		}
	}

	public sealed class StorySceneDestroyObjectMessage : Message {
		public const int MESSAGE_TYPE = -4;
		public override int MessageType { get { return MESSAGE_TYPE; } }

		public RuntimeId objID;
		
		public override void WriteTo(IDataOutputStream stream) {
			objID.WriteTo(stream);
		}

		public override void ReadFrom(IDataInputStream stream) {
			objID.ReadFrom(stream);
		}
	}

	public sealed class StorySceneUpdateObjectDataMessage : Message {
		public const int MESSAGE_TYPE = -42;
		public override int MessageType { get { return MESSAGE_TYPE; } }

		public RuntimeId objID;
		public bool interactable;

		public override void WriteTo(IDataOutputStream stream) {
			objID.WriteTo(stream);
			stream.WriteBoolean(interactable);
		}

		public override void ReadFrom(IDataInputStream stream) {
			objID.ReadFrom(stream);
			interactable = stream.ReadBoolean();
		}
	}
	
	public sealed class StorySceneExecuteAnimCommandsMessage : Message {
		public const int MESSAGE_TYPE = -5;
		public override int MessageType { get { return MESSAGE_TYPE; } }

		public struct ObjectAnimation : IStreamable {
			public float repeatStartOffsetTime;
			public float repeatEndTime;
			public ObjectAnimCommand[] commands;
			public ObjectAnimCommand[] repeatCommands;

			public void ReadFrom(IDataInputStream stream) {
				repeatStartOffsetTime = stream.ReadSingle();
				repeatEndTime = stream.ReadSingle();
				int length = stream.ReadInt32();
				commands = new ObjectAnimCommand[length];
				for (int i = 0; i < length; ++i) {
					commands[i] = InputStreamHelper.ReadStorySceneObjectCommand(stream);
				}
				length = stream.ReadInt32();
				repeatCommands = new ObjectAnimCommand[length];
				for (int i = 0; i < length; ++i) {
					repeatCommands[i] = InputStreamHelper.ReadStorySceneObjectCommand(stream);
				}
			}

			public void WriteTo(IDataOutputStream stream) {
				stream.WriteSingle(repeatStartOffsetTime);
				stream.WriteSingle(repeatEndTime);
				stream.WriteInt32(commands.Length);
				foreach (var cmd in commands) {
					OutputStreamHelper.WriteStorySceneObjectCommand(stream, cmd);
				}
				stream.WriteInt32(repeatCommands.Length);
				foreach (var cmd in repeatCommands) {
					OutputStreamHelper.WriteStorySceneObjectCommand(stream, cmd);
				}
			}
		}

		public struct CameraAnimation : IStreamable {
			public float repeatStartOffsetTime;
			public float repeatEndTime;
			public CameraAnimCommand[] commands;
			public CameraAnimCommand[] repeatCommands;

			public void ReadFrom(IDataInputStream stream) {
				repeatStartOffsetTime = stream.ReadSingle();
				repeatEndTime = stream.ReadSingle();
				int length = stream.ReadInt32();
				commands = new CameraAnimCommand[length];
				for (int i = 0; i < length; ++i) {
					commands[i] = InputStreamHelper.ReadStorySceneCameraCommand(stream);
				}
				length = stream.ReadInt32();
				repeatCommands = new CameraAnimCommand[length];
				for (int i = 0; i < length; ++i) {
					repeatCommands[i] = InputStreamHelper.ReadStorySceneCameraCommand(stream);
				}
			}

			public void WriteTo(IDataOutputStream stream) {
				stream.WriteSingle(repeatStartOffsetTime);
				stream.WriteSingle(repeatEndTime);
				stream.WriteInt32(commands.Length);
				foreach (var cmd in commands) {
					OutputStreamHelper.WriteStorySceneCameraCommand(stream, cmd);
				}
				stream.WriteInt32(repeatCommands.Length);
				foreach (var cmd in repeatCommands) {
					OutputStreamHelper.WriteStorySceneCameraCommand(stream, cmd);
				}
			}
		}

		public RuntimeId[] objectsID;
		public ObjectAnimation[] objectsAnimation;
		public bool hasCameraAnimation;
		public CameraAnimation cameraAnimation;

		public override void WriteTo(IDataOutputStream stream) {
			Debug.Assert(objectsID.Length == objectsAnimation.Length);
			stream.WriteInt32(objectsID.Length);
			foreach (var objID in objectsID) {
				objID.WriteTo(stream);
			}
			foreach (var objAnim in objectsAnimation) {
				objAnim.WriteTo(stream);
			}
			stream.WriteBoolean(hasCameraAnimation);
			if (hasCameraAnimation) {
				cameraAnimation.WriteTo(stream);
			}
		}

		public override void ReadFrom(IDataInputStream stream) {
			int length = stream.ReadInt32();
			objectsID = new RuntimeId[length];
			for (int i = 0; i < length; ++i) {
				objectsID[i].ReadFrom(stream);
			}
			objectsAnimation = new ObjectAnimation[length];
			for (int i = 0; i < length; ++i) {
				objectsAnimation[i].ReadFrom(stream);
			}
			hasCameraAnimation = stream.ReadBoolean();
			if (hasCameraAnimation) {
				cameraAnimation.ReadFrom(stream);
			}
		}
	}

	public sealed class StorySceneCompleteAnimCommandsMessage : Message {
		public const int MESSAGE_TYPE = -51;
		public override int MessageType { get { return MESSAGE_TYPE; } }

		public override void ReadFrom(IDataInputStream stream) { }
		public override void WriteTo(IDataOutputStream stream) { }
	}

	public sealed class StorySceneEnterStoryModeMessage : Message {
		public const int MESSAGE_TYPE = -9;
		public override int MessageType { get { return MESSAGE_TYPE; } }

		public override void WriteTo(IDataOutputStream stream) { }
		public override void ReadFrom(IDataInputStream stream) { }
	}

	public sealed class StorySceneEnterTalkModeMessage : Message {
		public const int MESSAGE_TYPE = -91;
		public override int MessageType { get { return MESSAGE_TYPE; } }

		public RuntimeId talkCharacter1;
		public RuntimeId talkCharacter2;
		public bool chara1Start;

		public override void WriteTo(IDataOutputStream stream) {
			talkCharacter1.WriteTo(stream);
			talkCharacter2.WriteTo(stream);
			stream.WriteBoolean(chara1Start);
		}

		public override void ReadFrom(IDataInputStream stream) {
			talkCharacter1.ReadFrom(stream);
			talkCharacter2.ReadFrom(stream);
			chara1Start = stream.ReadBoolean();
		}
	}

	public sealed class StorySceneTurnTalkMessage : Message {
		public const int MESSAGE_TYPE = -96;
		public override int MessageType { get { return MESSAGE_TYPE; } }

		public override void WriteTo(IDataOutputStream stream) { }
		public override void ReadFrom(IDataInputStream stream) { }
	}

	public sealed class StorySceneEnterInvestigationModeMessage : Message {
		public const int MESSAGE_TYPE = -94;
		public override int MessageType { get { return MESSAGE_TYPE; } }

		public RuntimeId investigatingPlayer;

		public override void WriteTo(IDataOutputStream stream) {
			investigatingPlayer.WriteTo(stream);
		}

		public override void ReadFrom(IDataInputStream stream) {
			investigatingPlayer.ReadFrom(stream);
		}
	}
	
	public sealed class StorySceneShowSelectionsMessage : Message {
		public const int MESSAGE_TYPE = -92;
		public override int MessageType { get { return MESSAGE_TYPE; } }
		
		public string[] selections;

		public override void WriteTo(IDataOutputStream stream) {
			stream.WriteInt32(selections.Length);
			foreach (var selection in selections) {
				stream.WriteString(selection);
			}
		}

		public override void ReadFrom(IDataInputStream stream) {
			int length = stream.ReadInt32();
			selections = new string[length];
			for (int i = 0; i < length; ++i) {
				selections[i] = stream.ReadString();
			}
		}
	}

	public sealed class StorySceneHideSelectionsMessage : Message {
		public const int MESSAGE_TYPE = -93;
		public override int MessageType { get { return MESSAGE_TYPE; } }

		public override void WriteTo(IDataOutputStream stream) { }
		public override void ReadFrom(IDataInputStream stream) { }
	}

	public sealed class StorySceneShowSelectionVoterMessage : Message {
		public const int MESSAGE_TYPE = -95;
		public override int MessageType { get { return MESSAGE_TYPE; } }

		public int selectionIndex;
		public RuntimeId voterID;

		public override void WriteTo(IDataOutputStream stream) {
			stream.WriteInt32(selectionIndex);
			voterID.WriteTo(stream);
		}

		public override void ReadFrom(IDataInputStream stream) {
			selectionIndex = stream.ReadInt32();
			voterID.ReadFrom(stream);
		}
	}

	public sealed class StorySceneUpdateDialogMessage : Message {
		public const int MESSAGE_TYPE = -14;
		public override int MessageType { get { return MESSAGE_TYPE; } }

		public bool isPrivate;
		public RuntimeId portraitObjectID;
		public string text;

		public override void WriteTo(IDataOutputStream stream) {
			stream.WriteBoolean(isPrivate);
			portraitObjectID.WriteTo(stream);
			stream.WriteString(text);
		}

		public override void ReadFrom(IDataInputStream stream) {
			isPrivate = stream.ReadBoolean();
			portraitObjectID.ReadFrom(stream);
			text = stream.ReadString();
		}
	}
	
	public sealed class BattleScenePushGridObjectMessage : Message {
		public const int MESSAGE_TYPE = -48;
		public override int MessageType { get { return MESSAGE_TYPE; } }

		public GridObjectData objData;
		public GridSprite view;

		public override void WriteTo(IDataOutputStream stream) {
			objData.WriteTo(stream);
			view.WriteTo(stream);
		}

		public override void ReadFrom(IDataInputStream stream) {
			objData.ReadFrom(stream);
			view.ReadFrom(stream);
		}
	}

	public sealed class BattleSceneRemoveGridObjectMessage : Message {
		public const int MESSAGE_TYPE = -49;
		public override int MessageType { get { return MESSAGE_TYPE; } }

		public RuntimeId gridObjID;

		public override void WriteTo(IDataOutputStream stream) {
			gridObjID.WriteTo(stream);
		}

		public override void ReadFrom(IDataInputStream stream) {
			gridObjID.ReadFrom(stream);
		}
	}

	public sealed class BattleSceneAddLadderObjectMessage : Message {
		public const int MESSAGE_TYPE = -50;
		public override int MessageType { get { return MESSAGE_TYPE; } }

		public LadderObjectData objData;
		public GridSprite view;

		public override void WriteTo(IDataOutputStream stream) {
			objData.WriteTo(stream);
			view.WriteTo(stream);
		}

		public override void ReadFrom(IDataInputStream stream) {
			objData.ReadFrom(stream);
			view.ReadFrom(stream);
		}
	}

	public sealed class BattleSceneRemoveLadderObjectMessage : Message {
		public const int MESSAGE_TYPE = -51;
		public override int MessageType { get { return MESSAGE_TYPE; } }

		public RuntimeId ladderObjID;

		public override void WriteTo(IDataOutputStream stream) {
			ladderObjID.WriteTo(stream);
		}

		public override void ReadFrom(IDataInputStream stream) {
			ladderObjID.ReadFrom(stream);
		}
	}

	public sealed class BattleSceneResetMessage : Message {
		public const int MESSAGE_TYPE = -52;
		public override int MessageType { get { return MESSAGE_TYPE; } }

		public int rows;
		public int cols;

		public override void WriteTo(IDataOutputStream stream) {
			stream.WriteInt32(rows);
			stream.WriteInt32(cols);
		}

		public override void ReadFrom(IDataInputStream stream) {
			rows = stream.ReadInt32();
			cols = stream.ReadInt32();
		}
	}

	public sealed class BattleSceneGridDataMessage : Message {
		public const int MESSAGE_TYPE = -69;
		public override int MessageType { get { return MESSAGE_TYPE; } }

		public int row;
		public int col;
		public bool isMiddleLand;

		public override void ReadFrom(IDataInputStream stream) {
			row = stream.ReadInt32();
			col = stream.ReadInt32();
			isMiddleLand = stream.ReadBoolean();
		}

		public override void WriteTo(IDataOutputStream stream) {
			stream.WriteInt32(row);
			stream.WriteInt32(col);
			stream.WriteBoolean(isMiddleLand);
		}
	}

	public sealed class BattleSceneGridObjectDataMessage : Message {
		public const int MESSAGE_TYPE = -62;
		public override int MessageType { get { return MESSAGE_TYPE; } }

		public GridObjectData objData;

		public override void ReadFrom(IDataInputStream stream) {
			objData.ReadFrom(stream);
		}

		public override void WriteTo(IDataOutputStream stream) {
			objData.WriteTo(stream);
		}
	}

	public sealed class BattleSceneLadderObjectDataMessage : Message {
		public const int MESSAGE_TYPE = -63;
		public override int MessageType { get { return MESSAGE_TYPE; } }

		public LadderObjectData objData;

		public override void ReadFrom(IDataInputStream stream) {
			objData.ReadFrom(stream);
		}

		public override void WriteTo(IDataOutputStream stream) {
			objData.WriteTo(stream);
		}
	}

	public sealed class BattleSceneStartBattleMessage : Message {
		public const int MESSAGE_TYPE = -86;
		public override int MessageType { get { return MESSAGE_TYPE; } }

		public override void ReadFrom(IDataInputStream stream) { }
		public override void WriteTo(IDataOutputStream stream) { }
	}

	public sealed class BattleSceneUpdateTurnOrderMessage : Message {
		public const int MESSAGE_TYPE = -53;
		public override int MessageType { get { return MESSAGE_TYPE; } }

		public RuntimeId[] objsIDOrdered;

		public override void ReadFrom(IDataInputStream stream) {
			int length = stream.ReadInt32();
			objsIDOrdered = new RuntimeId[length];
			for (int i = 0; i < length; ++i) {
				objsIDOrdered[i].ReadFrom(stream);
			}
		}

		public override void WriteTo(IDataOutputStream stream) {
			stream.WriteInt32(objsIDOrdered.Length);
			foreach (var objID in objsIDOrdered) {
				objID.WriteTo(stream);
			}
		}
	}

	public sealed class BattleSceneNewTurnMessage : Message {
		public const int MESSAGE_TYPE = -54;
		public override int MessageType { get { return MESSAGE_TYPE; } }

		public RuntimeId objIDWhoseTurn;
		public bool canOperate;
		
		public override void ReadFrom(IDataInputStream stream) {
			objIDWhoseTurn.ReadFrom(stream);
			canOperate = stream.ReadBoolean();
		}

		public override void WriteTo(IDataOutputStream stream) {
			objIDWhoseTurn.WriteTo(stream);
			stream.WriteBoolean(canOperate);
		}
	}

	public sealed class BattleSceneMovePathInfoMessage : Message {
		public const int MESSAGE_TYPE = -60;
		public override int MessageType { get { return MESSAGE_TYPE; } }

		public struct ReachableGrid : IStreamable {
			public int prevPlaceIndex;
			public GridPos pos;
			public int leftMovePoint;

			public void ReadFrom(IDataInputStream stream) {
				prevPlaceIndex = stream.ReadInt32();
				pos.ReadFrom(stream);
				leftMovePoint = stream.ReadInt32();
			}

			public void WriteTo(IDataOutputStream stream) {
				stream.WriteInt32(prevPlaceIndex);
				pos.WriteTo(stream);
				stream.WriteInt32(leftMovePoint);
			}
		}

		public ReachableGrid[] pathInfo;

		public override void ReadFrom(IDataInputStream stream) {
			int length = stream.ReadInt32();
			pathInfo = new ReachableGrid[length];
			for (int i = 0; i < length; ++i) {
				pathInfo[i].ReadFrom(stream);
			}
		}

		public override void WriteTo(IDataOutputStream stream) {
			stream.WriteInt32(pathInfo.Length);
			foreach (var grid in pathInfo) {
				grid.WriteTo(stream);
			}
		}
	}

	public sealed class BattleSceneCanObjectTakeExtraMoveMessage : Message {
		public const int MESSAGE_TYPE = -68;
		public override int MessageType { get { return MESSAGE_TYPE; } }

		public bool result;

		public override void ReadFrom(IDataInputStream stream) {
			result = stream.ReadBoolean();
		}

		public override void WriteTo(IDataOutputStream stream) {
			stream.WriteBoolean(result);
		}
	}

	public sealed class BattleSceneDisplayObjectMovingMessage : Message {
		public const int MESSAGE_TYPE = -61;
		public override int MessageType { get { return MESSAGE_TYPE; } }

		public RuntimeId objID;
		public BattleMapDirection direction;
		public bool stairway;

		public override void ReadFrom(IDataInputStream stream) {
			objID.ReadFrom(stream);
			direction = (BattleMapDirection)stream.ReadByte();
			stairway = stream.ReadBoolean();
		}

		public override void WriteTo(IDataOutputStream stream) {
			objID.WriteTo(stream);
			stream.WriteByte((byte)direction);
			stream.WriteBoolean(stairway);
		}
	}

	public sealed class BattleSceneDisplayObjectTakeExtraMovePointMessage : Message {
		public const int MESSAGE_TYPE = -64;
		public override int MessageType { get { return MESSAGE_TYPE; } }

		public RuntimeId objID;
		public RuntimeId moveAbilityTypeID;
		public int newMovePoint;

		public override void ReadFrom(IDataInputStream stream) {
			objID.ReadFrom(stream);
			moveAbilityTypeID.ReadFrom(stream);
			newMovePoint = stream.ReadInt32();
		}

		public override void WriteTo(IDataOutputStream stream) {
			objID.WriteTo(stream);
			moveAbilityTypeID.WriteTo(stream);
			stream.WriteInt32(newMovePoint);
		}
	}

	public sealed class BattleSceneUpdateActionPointMessage : Message {
		public const int MESSAGE_TYPE = -65;
		public override int MessageType { get { return MESSAGE_TYPE; } }

		public RuntimeId objID;
		public int newActionPoint;

		public override void ReadFrom(IDataInputStream stream) {
			objID.ReadFrom(stream);
			newActionPoint = stream.ReadInt32();
		}

		public override void WriteTo(IDataOutputStream stream) {
			objID.WriteTo(stream);
			stream.WriteInt32(newActionPoint);
		}
	}

	public sealed class BattleSceneUpdateMovePointMessage : Message {
		public const int MESSAGE_TYPE = -70;
		public override int MessageType { get { return MESSAGE_TYPE; } }

		public RuntimeId objID;
		public int newMovePoint;

		public override void ReadFrom(IDataInputStream stream) {
			objID.ReadFrom(stream);
			newMovePoint = stream.ReadInt32();
		}

		public override void WriteTo(IDataOutputStream stream) {
			objID.WriteTo(stream);
			stream.WriteInt32(newMovePoint);
		}
	}

	public sealed class BattleSceneActionAffectableAreasMessage : Message {
		public const int MESSAGE_TYPE = -77;
		public override int MessageType { get { return MESSAGE_TYPE; } }

		public GridPos[] centers;
		public GridPos[][] areas;

		public override void ReadFrom(IDataInputStream stream) {
			int length = stream.ReadInt32();
			centers = new GridPos[length];
			for (int i = 0; i < length; ++i) {
				centers[i].ReadFrom(stream);
			}
			areas = new GridPos[length][];
			for (int i = 0; i < length; ++i) {
				int gridCount = stream.ReadInt32();
				areas[i] = new GridPos[gridCount];
				for (int j = 0; j < gridCount; ++j) {
					areas[i][j].ReadFrom(stream);
				}
			}
		}

		public override void WriteTo(IDataOutputStream stream) {
			Debug.Assert(centers.Length == areas.Length);
			stream.WriteInt32(centers.Length);
			foreach (var center in centers) {
				center.WriteTo(stream);
			}
			foreach (var area in areas) {
				stream.WriteInt32(area.Length);
				foreach (var grid in area) {
					grid.WriteTo(stream);
				}
			}
		}
	}
}
