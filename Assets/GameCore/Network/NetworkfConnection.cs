using GameCore.Network.Stream;
using System;
using System.Collections.Generic;
using System.Threading;
using Bit = Networkf.BitHelper;

namespace GameCore.Network {
	public sealed class NetworkfMessage : Networkf.Message {
		private readonly Message _innerMessage;

		public Message InnerMessage { get { return _innerMessage; } }

		public static Networkf.Message ParseMessage(byte[] buf, ref int i) {
			int type = ReadMessageType(buf, ref i);
			var stream = new BitDataInputStream(buf, i);
			var message = Message.New(type);
			message.ReadFrom(stream);
			i = stream.i;
			return new NetworkfMessage(message);
		}

		public NetworkfMessage(Message message) :
			base(message.MessageType) {
			_innerMessage = message;
		}

		public override void WriteTo(byte[] buf, ref int i) {
			var stream = new BitDataOutputStream(buf, i);
			_innerMessage.WriteTo(stream);
			i = stream.i;
		}
	}
	
	public sealed class NetworkfConnection : Connection {
		private volatile Networkf.NetworkService _service = null;
		private readonly Queue<Message> _sendingMsgCache = new Queue<Message>();
		private readonly Queue<Message> _receivedMsgCache = new Queue<Message>();
		private readonly Queue<Message> _localMsgCache = new Queue<Message>();
		private readonly byte[] _exchange = new byte[4096];

		public NetworkfConnection() { // main thread
			new Thread(SendCachedMessage).Start();
		}

		public void ApplyService(Networkf.NetworkService service) { // listener thread
			service.parseMessage = NetworkfMessage.ParseMessage;
			service.OnMessageReceived += OnMessageReceived;
			service.OnServiceTeardown += OnServiceTeardown;
			_service = service;
		}

		public override bool Available() { // multi-threads
			return _service != null;
		}
		
		protected override void Send(Message message) { // main thread
			lock (_sendingMsgCache) {
				var copy = Message.New(message.MessageType);
				var read = new BitDataInputStream(_exchange);
				var write = new BitDataOutputStream(_exchange);
				message.WriteTo(write);
				copy.ReadFrom(read);
				_sendingMsgCache.Enqueue(copy);
				Monitor.Pulse(_sendingMsgCache);
			}
		}

		protected override Message[] TryReceive() { // main thread
			Message[] ret = null;
			lock (_receivedMsgCache) {
				if (_receivedMsgCache.Count > 0) {
					ret = _receivedMsgCache.ToArray();
					_receivedMsgCache.Clear();
				}
			}
			return ret;
		}

		protected override Message[] FetchLocalMessages() {
			Message[] ret = null;
			lock (_localMsgCache) {
				if (_localMsgCache.Count > 0) {
					ret = _localMsgCache.ToArray();
					_localMsgCache.Clear();
				}
			}
			return ret;
		}

		private void SendCachedMessage() { // sending thread
			while (true) {
				Message message;
				lock (_sendingMsgCache) {
					while (_sendingMsgCache.Count <= 0) Monitor.Wait(_sendingMsgCache);
					message = _sendingMsgCache.Dequeue();
				}
				try {
					var service = _service;
					if (service != null && Available()) {
						int sendingResult = service.SendMessage(new NetworkfMessage(message));
						if (sendingResult == -1) {
							service.TeardownService();
						}
					}
				} catch (Exception e) {
					Logger.WriteLine(e.Message);
				}
			}
		}

		private void OnMessageReceived(int id, Networkf.Message message) { // receiving thread
			lock (_receivedMsgCache) {
				_receivedMsgCache.Enqueue(((NetworkfMessage)message).InnerMessage);
			}
		}

		private void OnServiceTeardown() { // multi-threads
			var offlineMsg = new ClientMessages.ClientOfflineMessage();
			var service = _service;
			_service = null;
			lock (_receivedMsgCache) {
				_receivedMsgCache.Enqueue(offlineMsg);
			}
			service.parseMessage = null;
			service.OnMessageReceived -= OnMessageReceived;
			service.OnServiceTeardown -= OnServiceTeardown;
		}
	}
	
	public sealed class BitDataOutputStream : IDataOutputStream {
		public readonly byte[] bytes;
		public int start, i;

		public BitDataOutputStream(byte[] bytes, int i = 0) {
			this.bytes = bytes;
			this.start = this.i = i;
		}

		public void WriteBoolean(bool val) {
			Bit.WriteUInt8(bytes, ref i, (byte)(val ? 1 : 0));
		}

		public void WriteByte(byte val) {
			Bit.WriteUInt8(bytes, ref i, val);
		}

		public void WriteInt16(short val) {
			Bit.WriteInt16(bytes, ref i, val);
		}
		
		public void WriteInt32(int val) {
			Bit.WriteInt32(bytes, ref i, val);
		}

		public void WriteSingle(float val) {
			Bit.WriteSingle(bytes, ref i, val);
		}

		public void WriteString(string val) {
			Bit.WriteUInt16(bytes, ref i, (ushort)Bit.GetStringByteCount(val));
			Bit.WriteString(bytes, ref i, val);
		}
	}

	public sealed class BitDataInputStream : IDataInputStream {
		public readonly byte[] bytes;
		public int start, i;

		public BitDataInputStream(byte[] bytes, int i = 0) {
			this.bytes = bytes;
			this.start = this.i = i;
		}

		public bool ReadBoolean() {
			byte val = Bit.ReadUInt8(bytes, ref i);
			return val != 0;
		}

		public byte ReadByte() {
			return Bit.ReadUInt8(bytes, ref i);
		}

		public short ReadInt16() {
			return Bit.ReadInt16(bytes, ref i);
		}

		public int ReadInt32() {
			return Bit.ReadInt32(bytes, ref i);
		}

		public float ReadSingle() {
			return Bit.ReadSingle(bytes, ref i);
		}

		public string ReadString() {
			int byteCount = Bit.ReadUInt16(bytes, ref i);
			return Bit.ReadString(bytes, ref i, byteCount);
		}
	}

	public sealed class NSInitializer {
		private sealed class ServerRequireVerifyMessage : Networkf.Message {
			public const int KType = 0;
			public ServerRequireVerifyMessage() : base(KType) { }
		}

		private sealed class ClientVerifyMessage : Networkf.Message {
			public const int KType = 1;

			public byte[] verificationCode;

			public ClientVerifyMessage() : base(KType) { }

			public ClientVerifyMessage(byte[] buf, ref int i) : base(KType) {
				int byteCount = Bit.ReadInt32(buf, ref i);
				verificationCode = new byte[byteCount];
				for (int j = 0; j < byteCount; ++j) {
					verificationCode[j] = Bit.ReadUInt8(buf, ref i);
				}
			}

			public override void WriteTo(byte[] buf, ref int i) {
				Bit.WriteInt32(buf, ref i, verificationCode.Length);
				foreach (var b in verificationCode) {
					Bit.WriteUInt8(buf, ref i, b);
				}
			}
		}

		private sealed class ServerVerifyResultMessage : Networkf.Message {
			public const int KType = 2;

			public bool result;

			public ServerVerifyResultMessage() : base(KType) { }

			public ServerVerifyResultMessage(byte[] buf, ref int i) : base(KType) {
				byte val = Bit.ReadUInt8(buf, ref i);
				result = val != 0;
			}

			public override void WriteTo(byte[] buf, ref int i) {
				Bit.WriteUInt8(buf, ref i, (byte)(result ? 1 : 0));
			}
		}
		
		private static Networkf.Message ParseInitMessage(byte[] buf, ref int i) {
			var type = Networkf.Message.ReadMessageType(buf, ref i);
			switch (type) {
				case ServerRequireVerifyMessage.KType:
					return new ServerRequireVerifyMessage();
				case ClientVerifyMessage.KType:
					return new ClientVerifyMessage(buf, ref i);
				case ServerVerifyResultMessage.KType:
					return new ServerVerifyResultMessage(buf, ref i);
				default:
					throw new NotImplementedException();
			}
		}

		private readonly Networkf.NetworkService _service;
		private readonly object mtx = new object();
		private readonly int _retryCount;
		private readonly int _retryDelay;

		private bool _hasReceivedServerVerificationRequirement = false;
		private bool _hasReceivedServerVerificationResult = false;
		private bool _verificationResult = false;
		private byte[] _verificationCode = null;

		public NSInitializer(Networkf.NetworkService service, int retryCount = 10, int retryDelay = 1000) {
			_service = service;
			service.parseMessage = ParseInitMessage;
			service.OnMessageReceived += OnRawMessageReceived;
			_retryCount = retryCount;
			_retryDelay = retryDelay;
		}
		
		public void OnRawMessageReceived(int id, Networkf.Message message) {
			lock (mtx) {
				switch (message.type) {
					case ServerRequireVerifyMessage.KType:
						_hasReceivedServerVerificationRequirement = true;
						break;
					case ClientVerifyMessage.KType: {
							var clientMsg = (ClientVerifyMessage)message;
							_verificationCode = clientMsg.verificationCode;
						}
						break;
					case ServerVerifyResultMessage.KType: {
							var serverMsg = (ServerVerifyResultMessage)message;
							_verificationResult = serverMsg.result;
							_hasReceivedServerVerificationResult = true;
						}
						break;
					default:
						throw new NotImplementedException();
				}
				Monitor.Pulse(mtx);
			}
		}

		public bool ClientInit(byte[] verificationCode, NetworkfConnection applyTo) {
			Logger.WriteLine("waiting server requirement...");
			lock (mtx) {
				while (!_hasReceivedServerVerificationRequirement) Monitor.Wait(mtx);
			}
			Logger.WriteLine("server requirement received");
			var verifyMsg = new ClientVerifyMessage();
			verifyMsg.verificationCode = verificationCode;
			_service.SendMessage(verifyMsg);
			Logger.WriteLine("waiting verification result...");
			lock (mtx) {
				while (!_hasReceivedServerVerificationResult) Monitor.Wait(mtx);
			}
			Logger.WriteLine("verification result received");
			if (_verificationResult) {
				_service.parseMessage = null;
				_service.OnMessageReceived -= OnRawMessageReceived;
				applyTo.ApplyService(_service);
				return true;
			} else return false;
		}

		public byte[] ServerRequireClientVerify() {
			var serMsg = new ServerRequireVerifyMessage();
			int tryCount = 0;
			Logger.WriteLine("waiting client verify...");
			lock (mtx) {
				while (_verificationCode == null && tryCount < _retryCount) {
					_service.SendMessage(serMsg);
					++tryCount;
					Monitor.Wait(mtx, _retryDelay);
				}
			}
			if (_verificationCode == null) {
				Logger.WriteLine("broken client");
				_service.parseMessage = null;
				_service.OnMessageReceived -= OnRawMessageReceived;
				_service.TeardownService();
				return null;
			}
			Logger.WriteLine("client verification received");
			return _verificationCode;
		}

		public void ServerApplyConnection(NetworkfConnection applyTo) {
			if (applyTo != null) {
				_service.parseMessage = null;
				_service.OnMessageReceived -= OnRawMessageReceived;
				applyTo.ApplyService(_service);
				var serMsg = new ServerVerifyResultMessage();
				serMsg.result = true;
				_service.SendMessage(serMsg);
				Logger.WriteLine("verification result has sent");
			} else {
				var serMsg = new ServerVerifyResultMessage();
				serMsg.result = true;
				_service.SendMessage(serMsg);
				Logger.WriteLine("verification result has sent");
				_service.parseMessage = null;
				_service.OnMessageReceived -= OnRawMessageReceived;
				_service.TeardownService();
			}
		}
	}
}
