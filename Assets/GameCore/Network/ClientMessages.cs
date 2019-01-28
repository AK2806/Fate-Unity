using GameCore.BattleScene;
using GameCore.Network.Stream;
using GameCore.Proxy;

namespace GameCore.Network.ClientMessages {
	public sealed class ClientOnlineMessage : Message {
		public const int MESSAGE_TYPE = 1;
		public override int MessageType { get { return MESSAGE_TYPE; } }

		public override void ReadFrom(IDataInputStream stream) { }
		public override void WriteTo(IDataOutputStream stream) { }
	}

	public sealed class ClientOfflineMessage : Message {
		public const int MESSAGE_TYPE = 2;
		public override int MessageType { get { return MESSAGE_TYPE; } }

		public override void ReadFrom(IDataInputStream stream) { }
		public override void WriteTo(IDataOutputStream stream) { }
	}
	
	public sealed class ClientLoadingMessage : Message {
		public const int MESSAGE_TYPE = 4;
		public override int MessageType { get { return MESSAGE_TYPE; } }

		public byte percent;

		public override void ReadFrom(IDataInputStream stream) {
			percent = stream.ReadByte();
		}

		public override void WriteTo(IDataOutputStream stream) {
			stream.WriteByte(percent);
		}
	}

	public sealed class ClientLoadCompleteMessage : Message {
		public const int MESSAGE_TYPE = 5;
		public override int MessageType { get { return MESSAGE_TYPE; } }

		public override void ReadFrom(IDataInputStream stream) { }
		public override void WriteTo(IDataOutputStream stream) { }
	}
	
	public sealed class UserDecisionMessage : Message {
		public const int MESSAGE_TYPE = 6;
		public override int MessageType { get { return MESSAGE_TYPE; } }

		public int selectionIndex;

		public override void WriteTo(IDataOutputStream stream) {
			stream.WriteInt32(selectionIndex);
		}

		public override void ReadFrom(IDataInputStream stream) {
			selectionIndex = stream.ReadInt32();
		}
	}

	public sealed class DMStartCampaignMessage : Message {
		public const int MESSAGE_TYPE = 7;
		public override int MessageType { get { return MESSAGE_TYPE; } }

		public override void WriteTo(IDataOutputStream stream) { }
		public override void ReadFrom(IDataInputStream stream) { }
	}

	public sealed class BroadcastChatMessage : Message {
		public const int MESSAGE_TYPE = 8;
		public override int MessageType { get { return MESSAGE_TYPE; } }

		public string text;

		public override void WriteTo(IDataOutputStream stream) {
			stream.WriteString(text);
		}

		public override void ReadFrom(IDataInputStream stream) {
			text = stream.ReadString();
		}
	}

	public sealed class PrivateChatMessage : Message {
		public const int MESSAGE_TYPE = 9;
		public override int MessageType { get { return MESSAGE_TYPE; } }

		public Identification receiverID;
		public string text;

		public override void WriteTo(IDataOutputStream stream) {
			receiverID.WriteTo(stream);
			stream.WriteString(text);
		}

		public override void ReadFrom(IDataInputStream stream) {
			receiverID.ReadFrom(stream);
			text = stream.ReadString();
		}
	}
	
	public sealed class CampaignDoNextActionMessage : Message {
		public const int MESSAGE_TYPE = 784;
		public override int MessageType { get { return MESSAGE_TYPE; } }

		public override void ReadFrom(IDataInputStream stream) { }
		public override void WriteTo(IDataOutputStream stream) { }
	}

	public sealed class CampaignJumpToMessage : Message {
		public const int MESSAGE_TYPE = 7484;
		public override int MessageType { get { return MESSAGE_TYPE; } }

		public Identification campaignID;

		public override void ReadFrom(IDataInputStream stream) {
			campaignID.ReadFrom(stream);
		}

		public override void WriteTo(IDataOutputStream stream) {
			campaignID.WriteTo(stream);
		}
	}

	public sealed class CampaignPlayShotMessage : Message {
		public const int MESSAGE_TYPE = 7884;
		public override int MessageType { get { return MESSAGE_TYPE; } }

		public Identification shotID;

		public override void ReadFrom(IDataInputStream stream) {
			shotID.ReadFrom(stream);
		}

		public override void WriteTo(IDataOutputStream stream) {
			shotID.WriteTo(stream);
		}
	}
	
	public sealed class CheckerPassiveSkillSelectedMessage : Message {
		public const int MESSAGE_TYPE = 5;
		public override int MessageType { get { return MESSAGE_TYPE; } }

		public Identification abilityTypeID;

		public override void WriteTo(IDataOutputStream stream) {
			abilityTypeID.WriteTo(stream);
		}

		public override void ReadFrom(IDataInputStream stream) {
			abilityTypeID.ReadFrom(stream);
		}
	}

	public sealed class CheckerPassiveStuntSelectedMessage : Message {
		public const int MESSAGE_TYPE = 7;
		public override int MessageType { get { return MESSAGE_TYPE; } }

		public Identification stuntID;

		public override void WriteTo(IDataOutputStream stream) {
			stuntID.WriteTo(stream);
		}

		public override void ReadFrom(IDataInputStream stream) {
			stuntID.ReadFrom(stream);
		}
	}

	public sealed class CheckerAspectSelectedMessage : Message {
		public const int MESSAGE_TYPE = 6;
		public override int MessageType { get { return MESSAGE_TYPE; } }

		public Identification aspectOwnerID;
		public Identification aspectID;
		public bool isConsequence;
		public bool reroll;

		public override void WriteTo(IDataOutputStream stream) {
			aspectOwnerID.WriteTo(stream);
			aspectID.WriteTo(stream);
			stream.WriteBoolean(isConsequence);
			stream.WriteBoolean(reroll);
		}

		public override void ReadFrom(IDataInputStream stream) {
			aspectOwnerID.ReadFrom(stream);
			aspectID.ReadFrom(stream);
			isConsequence = stream.ReadBoolean();
			reroll = stream.ReadBoolean();
		}
	}

	public sealed class CheckerAspectSelectionOverMessage : Message {
		public const int MESSAGE_TYPE = 18;
		public override int MessageType { get { return MESSAGE_TYPE; } }

		public override void WriteTo(IDataOutputStream stream) { }
		public override void ReadFrom(IDataInputStream stream) { }
	}

	public sealed class CheckerSetSkipSelectAspectMessage : Message {
		public const int MESSAGE_TYPE = 17;
		public override int MessageType { get { return MESSAGE_TYPE; } }

		public bool val;

		public override void WriteTo(IDataOutputStream stream) {
			stream.WriteBoolean(val);
		}

		public override void ReadFrom(IDataInputStream stream) {
			val = stream.ReadBoolean();
		}
	}

	public sealed class CheckerGetInitiativeCanUseSkillMessage : Message {
		public const int MESSAGE_TYPE = 26;
		public override int MessageType { get { return MESSAGE_TYPE; } }

		public Identification initiativeID;
		public Identification abilityTypeID;
		public CharacterAction action;

		public override void ReadFrom(IDataInputStream stream) {
			initiativeID.ReadFrom(stream);
			abilityTypeID.ReadFrom(stream);
			action = (CharacterAction)stream.ReadByte();
		}

		public override void WriteTo(IDataOutputStream stream) {
			initiativeID.WriteTo(stream);
			abilityTypeID.WriteTo(stream);
			stream.WriteByte((byte)action);
		}
	}

	public sealed class CheckerGetInitiativeCanUseStuntMessage : Message {
		public const int MESSAGE_TYPE = 39;
		public override int MessageType { get { return MESSAGE_TYPE; } }

		public Identification initiativeID;
		public Identification stuntID;
		public CharacterAction action;

		public override void ReadFrom(IDataInputStream stream) {
			initiativeID.ReadFrom(stream);
			stuntID.ReadFrom(stream);
			action = (CharacterAction)stream.ReadByte();
		}

		public override void WriteTo(IDataOutputStream stream) {
			initiativeID.WriteTo(stream);
			initiativeID.WriteTo(stream);
			stuntID.WriteTo(stream);
			stream.WriteByte((byte)action);
		}
	}

	public sealed class CheckerGetPassiveUsableActionListMessage : Message {
		public const int MESSAGE_TYPE = 27;
		public override int MessageType { get { return MESSAGE_TYPE; } }

		public override void ReadFrom(IDataInputStream stream) { }
		public override void WriteTo(IDataOutputStream stream) { }
	}

	public sealed class CheckerGetStuntTargetValidityMessage : Message {
		public const int MESSAGE_TYPE = 31;
		public override int MessageType { get { return MESSAGE_TYPE; } }

		public Identification initiativeID;
		public Identification targetID;
		public Identification stuntID;
		public CharacterAction action;

		public override void ReadFrom(IDataInputStream stream) {
			initiativeID.ReadFrom(stream);
			targetID.ReadFrom(stream);
			stuntID.ReadFrom(stream);
			action = (CharacterAction)stream.ReadByte();
		}

		public override void WriteTo(IDataOutputStream stream) {
			initiativeID.WriteTo(stream);
			targetID.WriteTo(stream);
			stuntID.WriteTo(stream);
			stream.WriteByte((byte)action);
		}
	}
	
	public sealed class GetDirectResistableSkillsMessage : Message {
		public const int MESSAGE_TYPE = 14;
		public override int MessageType { get { return MESSAGE_TYPE; } }

		public Identification initiativeAbilityTypeID;
		public CharacterAction actionType;

		public override void ReadFrom(IDataInputStream stream) {
			initiativeAbilityTypeID.ReadFrom(stream);
			actionType = (CharacterAction)stream.ReadByte();
		}

		public override void WriteTo(IDataOutputStream stream) {
			initiativeAbilityTypeID.WriteTo(stream);
			stream.WriteByte((byte)actionType);
		}
	}

	public sealed class GetDirectResistableStuntsMessage : Message {
		public const int MESSAGE_TYPE = 34;
		public override int MessageType { get { return MESSAGE_TYPE; } }

		public Identification initiativeAbilityTypeID;
		public Identification passiveCharacterID;
		public CharacterAction actionType;

		public override void ReadFrom(IDataInputStream stream) {
			initiativeAbilityTypeID.ReadFrom(stream);
			passiveCharacterID.ReadFrom(stream);
			actionType = (CharacterAction)stream.ReadByte();
		}

		public override void WriteTo(IDataOutputStream stream) {
			initiativeAbilityTypeID.WriteTo(stream);
			passiveCharacterID.WriteTo(stream);
			stream.WriteByte((byte)actionType);
		}
	}
	
	public sealed class StorySceneDMAddPlayerToStage : Message {
		public const int MESSAGE_TYPE = 77;
		public override int MessageType { get { return MESSAGE_TYPE; } }

		public Identification playerID;

		public override void WriteTo(IDataOutputStream stream) {
			playerID.WriteTo(stream);
		}

		public override void ReadFrom(IDataInputStream stream) {
			playerID.ReadFrom(stream);
		}
	}

	public sealed class StorySceneDMRemovePlayerFromStage : Message {
		public const int MESSAGE_TYPE = 88;
		public override int MessageType { get { return MESSAGE_TYPE; } }

		public Identification playerID;

		public override void WriteTo(IDataOutputStream stream) {
			playerID.WriteTo(stream);
		}

		public override void ReadFrom(IDataInputStream stream) {
			playerID.ReadFrom(stream);
		}
	}

	public sealed class StorySceneInvestigateObjectMessage : Message {
		public const int MESSAGE_TYPE = 331;
		public override int MessageType { get { return MESSAGE_TYPE; } }

		public Identification objID;

		public override void WriteTo(IDataOutputStream stream) {
			objID.WriteTo(stream);
		}

		public override void ReadFrom(IDataInputStream stream) {
			objID.ReadFrom(stream);
		}
	}

	public sealed class StorySceneSelectSelectionMessage : Message {
		public const int MESSAGE_TYPE = 354;
		public override int MessageType { get { return MESSAGE_TYPE; } }

		public int index;

		public override void WriteTo(IDataOutputStream stream) {
			stream.WriteInt32(index);
		}

		public override void ReadFrom(IDataInputStream stream) {
			index = stream.ReadInt32();
		}
	}

	public sealed class StorySceneObjectDoActionMessage : Message {
		public const int MESSAGE_TYPE = 21;
		public override int MessageType { get { return MESSAGE_TYPE; } }

		public Identification objID;
		public Identification abilityTypeOrStuntID;
		public bool isStunt;
		public CharacterAction action;
		public Identification[] targetsID;

		public override void ReadFrom(IDataInputStream stream) {
			objID.ReadFrom(stream);
			abilityTypeOrStuntID.ReadFrom(stream);
			isStunt = stream.ReadBoolean();
			action = (CharacterAction)stream.ReadByte();
			int length = stream.ReadInt32();
			targetsID = new Identification[length];
			for (int i = 0; i < length; ++i) {
				targetsID[i].ReadFrom(stream);;
			}
		}

		public override void WriteTo(IDataOutputStream stream) {
			objID.WriteTo(stream);
			abilityTypeOrStuntID.WriteTo(stream);
			stream.WriteBoolean(isStunt);
			stream.WriteByte((byte)action);
			stream.WriteInt32(targetsID.Length);
			foreach (var target in targetsID) {
				target.WriteTo(stream);
			}
		}
	}

	public sealed class StorySceneObjectUseStuntDirectlyMessage : Message {
		public const int MESSAGE_TYPE = 22;
		public override int MessageType { get { return MESSAGE_TYPE; } }

		public Identification objID;
		public Identification stuntID;
		public Identification[] targetsID;

		public override void ReadFrom(IDataInputStream stream) {
			objID.ReadFrom(stream);
			stuntID.ReadFrom(stream);
			int length = stream.ReadInt32();
			targetsID = new Identification[length];
			for (int i = 0; i < length; ++i) {
				targetsID[i].ReadFrom(stream);
			}
		}

		public override void WriteTo(IDataOutputStream stream) {
			objID.WriteTo(stream);
			stuntID.WriteTo(stream);
			stream.WriteInt32(targetsID.Length);
			foreach (var target in targetsID) {
				target.WriteTo(stream);
			}
		}
	}

	public sealed class StorySceneDialogSendTextMessage : Message {
		public const int MESSAGE_TYPE = 378;
		public override int MessageType { get { return MESSAGE_TYPE; } }

		public string text;

		public override void WriteTo(IDataOutputStream stream) {
			stream.WriteString(text);
		}

		public override void ReadFrom(IDataInputStream stream) {
			text = stream.ReadString();
		}
	}

	public sealed class StorySceneDMDialogSendPrivateTextMessage : Message {
		public const int MESSAGE_TYPE = 366;
		public override int MessageType { get { return MESSAGE_TYPE; } }

		public Identification[] players;
		public string text;

		public override void WriteTo(IDataOutputStream stream) {
			stream.WriteInt32(players.Length);
			foreach (var player in players) {
				player.WriteTo(stream);
			}
			stream.WriteString(text);
		}

		public override void ReadFrom(IDataInputStream stream) {
			int length = stream.ReadInt32();
			players = new Identification[length];
			for (int i = 0; i < length; ++i) {
				players[i].ReadFrom(stream);
			}
			text = stream.ReadString();
		}
	}
	
	public sealed class BattleSceneGetGridObjectDataMessage : Message {
		public const int MESSAGE_TYPE = 24;
		public override int MessageType { get { return MESSAGE_TYPE; } }

		public Identification objID;

		public override void ReadFrom(IDataInputStream stream) {
			objID.ReadFrom(stream);
		}

		public override void WriteTo(IDataOutputStream stream) {
			objID.WriteTo(stream);
		}
	}

	public sealed class BattleSceneGetLadderObjectDataMessage : Message {
		public const int MESSAGE_TYPE = 25;
		public override int MessageType { get { return MESSAGE_TYPE; } }

		public Identification objID;

		public override void ReadFrom(IDataInputStream stream) {
			objID.ReadFrom(stream);
		}

		public override void WriteTo(IDataOutputStream stream) {
			objID.WriteTo(stream);
		}
	}

	public sealed class BattleSceneGetObjectCanExtraMoveMessage : Message {
		public const int MESSAGE_TYPE = 28;
		public override int MessageType { get { return MESSAGE_TYPE; } }

		public override void ReadFrom(IDataInputStream stream) { }
		public override void WriteTo(IDataOutputStream stream) { }
	}

	public sealed class BattleSceneObjectTakeExtraMovePointMessage : Message {
		public const int MESSAGE_TYPE = 23;
		public override int MessageType { get { return MESSAGE_TYPE; } }

		public override void ReadFrom(IDataInputStream stream) { }
		public override void WriteTo(IDataOutputStream stream) { }
	}

	public sealed class BattleSceneGetMovePathInfoMessage : Message {
		public const int MESSAGE_TYPE = 19;
		public override int MessageType { get { return MESSAGE_TYPE; } }

		public override void WriteTo(IDataOutputStream stream) { }
		public override void ReadFrom(IDataInputStream stream) { }
	}

	public sealed class BattleSceneActableObjectMoveMessage : Message {
		public const int MESSAGE_TYPE = 20;
		public override int MessageType { get { return MESSAGE_TYPE; } }

		public GridPos dst;

		public override void WriteTo(IDataOutputStream stream) {
			dst.WriteTo(stream);
		}

		public override void ReadFrom(IDataInputStream stream) {
			dst.ReadFrom(stream);
		}
	}

	public sealed class BattleSceneActableObjectDoActionMessage : Message {
		public const int MESSAGE_TYPE = 21;
		public override int MessageType { get { return MESSAGE_TYPE; } }

		public Identification abilityTypeOrStuntID;
		public bool isStunt;
		public CharacterAction action;
		public GridPos dstCenter;
		public Identification[] targetsID;

		public override void ReadFrom(IDataInputStream stream) {
			abilityTypeOrStuntID.ReadFrom(stream);
			isStunt = stream.ReadBoolean();
			action = (CharacterAction)stream.ReadByte();
			dstCenter.ReadFrom(stream);
			int length = stream.ReadInt32();
			targetsID = new Identification[length];
			for (int i = 0; i < length; ++i) {
				targetsID[i].ReadFrom(stream);
			}
		}

		public override void WriteTo(IDataOutputStream stream) {
			abilityTypeOrStuntID.WriteTo(stream);
			stream.WriteBoolean(isStunt);
			stream.WriteByte((byte)action);
			dstCenter.WriteTo(stream);
			stream.WriteInt32(targetsID.Length);
			foreach (var target in targetsID) {
				target.WriteTo(stream);
			}
		}
	}

	public sealed class BattleSceneActableObjectUseStuntDirectlyMessage : Message {
		public const int MESSAGE_TYPE = 22;
		public override int MessageType { get { return MESSAGE_TYPE; } }

		public Identification stuntID;
		public GridPos dstCenter;
		public Identification[] targetsID;

		public override void ReadFrom(IDataInputStream stream) {
			stuntID.ReadFrom(stream);
			dstCenter.ReadFrom(stream);
			int length = stream.ReadInt32();
			targetsID = new Identification[length];
			for (int i = 0; i < length; ++i) {
				targetsID[i].ReadFrom(stream);
			}
		}

		public override void WriteTo(IDataOutputStream stream) {
			stuntID.WriteTo(stream);
			dstCenter.WriteTo(stream);
			stream.WriteInt32(targetsID.Length);
			foreach (var target in targetsID) {
				target.WriteTo(stream);
			}
		}
	}

	public sealed class BattleSceneGetActionAffectableAreasMessage : Message {
		public const int MESSAGE_TYPE = 32;
		public override int MessageType { get { return MESSAGE_TYPE; } }

		public Identification abilityTypeOrStuntID;
		public bool isStunt;

		public override void ReadFrom(IDataInputStream stream) {
			abilityTypeOrStuntID.ReadFrom(stream);
			isStunt = stream.ReadBoolean();
		}

		public override void WriteTo(IDataOutputStream stream) {
			abilityTypeOrStuntID.WriteTo(stream);
			stream.WriteBoolean(isStunt);
		}
	}

	public sealed class BattleSceneTurnOverMessage : Message {
		public const int MESSAGE_TYPE = 29;
		public override int MessageType { get { return MESSAGE_TYPE; } }

		public override void ReadFrom(IDataInputStream stream) { }
		public override void WriteTo(IDataOutputStream stream) { }
	}
}
