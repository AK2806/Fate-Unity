using GameCore.StoryScene.AnimationCommand;
using System;

namespace GameCore.Network.Stream {
	public interface IDataOutputStream {
		void WriteBoolean(Boolean val);
		void WriteString(String val);
		void WriteByte(Byte val);
		void WriteInt16(Int16 val);
		void WriteInt32(Int32 val);
		void WriteSingle(Single val);
	}

	public interface IDataInputStream {
		Boolean ReadBoolean();
		String ReadString();
		Byte ReadByte();
		Int16 ReadInt16();
		Int32 ReadInt32();
		Single ReadSingle();
	}

	public interface IStreamable {
		void WriteTo(IDataOutputStream stream);
		void ReadFrom(IDataInputStream stream);
	}
	
	public static class OutputStreamHelper {
		public static void WriteGuid(IDataOutputStream stream, Guid guid) {
			byte[] bs = guid.ToByteArray();
			stream.WriteByte((byte)bs.Length);
			foreach (var b in bs) {
				stream.WriteByte(b);
			}
		}
		
		public static void WriteStorySceneObjectCommand(IDataOutputStream stream, AnimCommand command) {
			stream.WriteBoolean(command.isCamera);
			stream.WriteBoolean(command.isEased);
			stream.WriteSingle(command.startTime);
			if (command.isCamera) {
				if (command.isEased) {
					var easeAttrCmd = (EaseCameraAttribute)command;
					stream.WriteByte((byte)easeAttrCmd.attrType);
					stream.WriteByte((byte)easeAttrCmd.esType);
					stream.WriteSingle(easeAttrCmd.duration);
					switch (easeAttrCmd.attrType) {
						case CameraAttrType.Position: {
								var posCmd = (EaseCameraPosition)command;
								posCmd.val.WriteTo(stream);
							}
							break;
						case CameraAttrType.Zoom: {
								var zoomCmd = (EaseCameraZoom)command;
								stream.WriteSingle(zoomCmd.val);
							}
							break;
					}
				} else {
					var setAttrCmd = (SetCameraAttribute)command;
					stream.WriteByte((byte)setAttrCmd.attrType);
					switch (setAttrCmd.attrType) {
						case CameraAttrType.Position: {
								var posCmd = (SetCameraPosition)command;
								posCmd.val.WriteTo(stream);
							}
							break;
						case CameraAttrType.Zoom: {
								var zoomCmd = (SetCameraZoom)command;
								stream.WriteSingle(zoomCmd.val);
							}
							break;
					}
				}
			} else {
				if (command.isEased) {
					var easeAttrCmd = (EaseObjectAttribute)command;
					stream.WriteByte((byte)easeAttrCmd.attrType);
					stream.WriteByte((byte)easeAttrCmd.esType);
					stream.WriteSingle(easeAttrCmd.duration);
					switch (easeAttrCmd.attrType) {
						case ObjectAttrType.Position: {
								var posCmd = (EaseObjectPosition)command;
								posCmd.val.WriteTo(stream);
							}
							break;
						case ObjectAttrType.Rotation: {
								var rotCmd = (EaseObjectRotation)command;
								stream.WriteSingle(rotCmd.angle);
							}
							break;
						case ObjectAttrType.Scale: {
								var scaleCmd = (EaseObjectScale)command;
								scaleCmd.val.WriteTo(stream);
							}
							break;
						case ObjectAttrType.Alpha: {
								var alphaCmd = (EaseObjectAlpha)command;
								stream.WriteSingle(alphaCmd.val);
							}
							break;
						case ObjectAttrType.Tint: {
								var tintCmd = (EaseObjectTint)command;
								tintCmd.color.WriteTo(stream);
							}
							break;
						case ObjectAttrType.Sprite: {
								var spriteCmd = (EaseObjectSprite)command;
								stream.WriteInt32(spriteCmd.spriteIndex);
							}
							break;
					}
				} else {
					var setAttrCmd = (SetObjectAttribute)command;
					stream.WriteByte((byte)setAttrCmd.attrType);
					switch (setAttrCmd.attrType) {
						case ObjectAttrType.Position: {
								var posCmd = (SetObjectPosition)command;
								posCmd.val.WriteTo(stream);
							}
							break;
						case ObjectAttrType.ZIndex: {
								var zCmd = (SetObjectZIndex)command;
								stream.WriteInt32(zCmd.val);
							}
							break;
						case ObjectAttrType.Rotation: {
								var rotCmd = (SetObjectRotation)command;
								stream.WriteSingle(rotCmd.angle);
							}
							break;
						case ObjectAttrType.Scale: {
								var scaleCmd = (SetObjectScale)command;
								scaleCmd.val.WriteTo(stream);
							}
							break;
						case ObjectAttrType.Alpha: {
								var alphaCmd = (SetObjectAlpha)command;
								stream.WriteSingle(alphaCmd.val);
							}
							break;
						case ObjectAttrType.Tint: {
								var tintCmd = (SetObjectTint)command;
								tintCmd.color.WriteTo(stream);
							}
							break;
						case ObjectAttrType.Sprite: {
								var spriteCmd = (SetObjectSprite)command;
								stream.WriteInt32(spriteCmd.spriteIndex);
							}
							break;
					}
				}
			}
		}
	}

	public static class InputStreamHelper {
		public static Guid ReadGuid(IDataInputStream stream) {
			int length = stream.ReadByte();
			byte[] bs = new byte[length];
			for (int i = 0; i < length; ++i) {
				bs[i] = stream.ReadByte();
			}
			return new Guid(bs);
		}

		public static AnimCommand ReadStorySceneObjectCommand(IDataInputStream stream) {
			AnimCommand ret;
			bool isCameraCmd = stream.ReadBoolean();
			bool isEasedCmd = stream.ReadBoolean();
			float startTime = stream.ReadSingle();
			if (isCameraCmd) {
				if (isEasedCmd) {
					CameraAttrType attrType = (CameraAttrType)stream.ReadByte();
					EaseType esType = (EaseType)stream.ReadByte();
					float duration = stream.ReadSingle();
					switch (attrType) {
						case CameraAttrType.Position: {
								var posCmd = new EaseCameraPosition();
								posCmd.val.ReadFrom(stream);
								ret = posCmd;
							}
							break;
						case CameraAttrType.Zoom: {
								var zoomCmd = new EaseCameraZoom();
								zoomCmd.val = stream.ReadSingle();
								ret = zoomCmd;
							}
							break;
						default:
							throw new NotImplementedException();
					}
					var easeAttrCmd = (EaseCameraAttribute)ret;
					easeAttrCmd.esType = esType;
					easeAttrCmd.duration = duration;
				} else {
					CameraAttrType attrType = (CameraAttrType)stream.ReadByte();
					switch (attrType) {
						case CameraAttrType.Position: {
								var posCmd = new SetCameraPosition();
								posCmd.val.ReadFrom(stream);
								ret = posCmd;
							}
							break;
						case CameraAttrType.Zoom: {
								var zoomCmd = new SetCameraZoom();
								zoomCmd.val = stream.ReadSingle();
								ret = zoomCmd;
							}
							break;
						default:
							throw new NotImplementedException();
					}
				}
			} else {
				if (isEasedCmd) {
					ObjectAttrType attrType = (ObjectAttrType)stream.ReadByte();
					EaseType esType = (EaseType)stream.ReadByte();
					float duration = stream.ReadSingle();
					switch (attrType) {
						case ObjectAttrType.Position: {
								var posCmd = new EaseObjectPosition();
								posCmd.val.ReadFrom(stream);
								ret = posCmd;
							}
							break;
						case ObjectAttrType.Rotation: {
								var rotCmd = new EaseObjectRotation();
								rotCmd.angle = stream.ReadSingle();
								ret = rotCmd;
							}
							break;
						case ObjectAttrType.Scale: {
								var scaleCmd = new EaseObjectScale();
								scaleCmd.val.ReadFrom(stream);
								ret = scaleCmd;
							}
							break;
						case ObjectAttrType.Alpha: {
								var alphaCmd = new EaseObjectAlpha();
								alphaCmd.val = stream.ReadSingle();
								ret = alphaCmd;
							}
							break;
						case ObjectAttrType.Tint: {
								var tintCmd = new EaseObjectTint();
								tintCmd.color.ReadFrom(stream);
								ret = tintCmd;
							}
							break;
						case ObjectAttrType.Sprite: {
								var spriteCmd = new EaseObjectSprite();
								spriteCmd.spriteIndex = stream.ReadInt32();
								ret = spriteCmd;
							}
							break;
						default:
							throw new NotImplementedException();
					}
					var easeAttrCmd = (EaseObjectAttribute)ret;
					easeAttrCmd.esType = esType;
					easeAttrCmd.duration = duration;
				} else {
					ObjectAttrType attrType = (ObjectAttrType)stream.ReadByte();
					switch (attrType) {
						case ObjectAttrType.Position: {
								var posCmd = new SetObjectPosition();
								posCmd.val.ReadFrom(stream);
								ret = posCmd;
							}
							break;
						case ObjectAttrType.ZIndex: {
								var zCmd = new SetObjectZIndex();
								zCmd.val = stream.ReadInt32();
								ret = zCmd;
							}
							break;
						case ObjectAttrType.Rotation: {
								var rotCmd = new SetObjectRotation();
								rotCmd.angle = stream.ReadSingle();
								ret = rotCmd;
							}
							break;
						case ObjectAttrType.Scale: {
								var scaleCmd = new SetObjectScale();
								scaleCmd.val.ReadFrom(stream);
								ret = scaleCmd;
							}
							break;
						case ObjectAttrType.Alpha: {
								var alphaCmd = new SetObjectAlpha();
								alphaCmd.val = stream.ReadSingle();
								ret = alphaCmd;
							}
							break;
						case ObjectAttrType.Tint: {
								var tintCmd = new SetObjectTint();
								tintCmd.color.ReadFrom(stream);
								ret = tintCmd;
							}
							break;
						case ObjectAttrType.Sprite: {
								var spriteCmd = new SetObjectSprite();
								spriteCmd.spriteIndex = stream.ReadInt32();
								ret = spriteCmd;
							}
							break;
						default:
							throw new NotImplementedException();
					}
				}
			}
			ret.startTime = startTime;
			return ret;
		}
	}
}
