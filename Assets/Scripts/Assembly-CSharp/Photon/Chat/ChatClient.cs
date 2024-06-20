using System;
using System.Collections.Generic;
using ExitGames.Client.Photon;

namespace Photon.Chat
{
	public class ChatClient : IPhotonPeerListener
	{
		private const int FriendRequestListMax = 1024;

		public const int DefaultMaxSubscribers = 100;

		private string chatRegion = "EU";

		public int MessageLimit;

		public readonly Dictionary<string, ChatChannel> PublicChannels;

		public readonly Dictionary<string, ChatChannel> PrivateChannels;

		private readonly HashSet<string> PublicChannelsUnsubscribing;

		private readonly IChatClientListener listener;

		public ChatPeer chatPeer;

		private const string ChatAppName = "chat";

		private bool didAuthenticate;

		private int? statusToSetWhenConnected;

		private object messageToSetWhenConnected;

		private int msDeltaForServiceCalls = 50;

		private int msTimestampOfLastServiceCall;

		public string NameServerAddress { get; private set; }

		public string FrontendAddress { get; private set; }

		public string ChatRegion
		{
			get
			{
				return chatRegion;
			}
			set
			{
				chatRegion = value;
			}
		}

		public ChatState State { get; private set; }

		public ChatDisconnectCause DisconnectedCause { get; private set; }

		public bool CanChat
		{
			get
			{
				if (State == ChatState.ConnectedToFrontEnd)
				{
					return HasPeer;
				}
				return false;
			}
		}

		private bool HasPeer => chatPeer != null;

		public string AppVersion { get; private set; }

		public string AppId { get; private set; }

		public AuthenticationValues AuthValues { get; set; }

		public string UserId
		{
			get
			{
				if (AuthValues == null)
				{
					return null;
				}
				return AuthValues.UserId;
			}
			private set
			{
				if (AuthValues == null)
				{
					AuthValues = new AuthenticationValues();
				}
				AuthValues.UserId = value;
			}
		}

		public bool UseBackgroundWorkerForSending { get; set; }

		public ConnectionProtocol TransportProtocol
		{
			get
			{
				return chatPeer.TransportProtocol;
			}
			set
			{
				if (chatPeer == null || chatPeer.PeerState != 0)
				{
					listener.DebugReturn(DebugLevel.WARNING, "Can't set TransportProtocol. Disconnect first! " + ((chatPeer != null) ? ("PeerState: " + chatPeer.PeerState) : "The chatPeer is null."));
				}
				else
				{
					chatPeer.TransportProtocol = value;
				}
			}
		}

		public Dictionary<ConnectionProtocol, Type> SocketImplementationConfig => chatPeer.SocketImplementationConfig;

		public DebugLevel DebugOut
		{
			get
			{
				return chatPeer.DebugOut;
			}
			set
			{
				chatPeer.DebugOut = value;
			}
		}

		public bool CanChatInChannel(string channelName)
		{
			if (CanChat && PublicChannels.ContainsKey(channelName))
			{
				return !PublicChannelsUnsubscribing.Contains(channelName);
			}
			return false;
		}

		public ChatClient(IChatClientListener listener, ConnectionProtocol protocol = ConnectionProtocol.Udp)
		{
			this.listener = listener;
			State = ChatState.Uninitialized;
			chatPeer = new ChatPeer(this, protocol);
			chatPeer.SerializationProtocolType = SerializationProtocol.GpBinaryV18;
			PublicChannels = new Dictionary<string, ChatChannel>();
			PrivateChannels = new Dictionary<string, ChatChannel>();
			PublicChannelsUnsubscribing = new HashSet<string>();
		}

		public bool Connect(string appId, string appVersion, AuthenticationValues authValues)
		{
			chatPeer.TimePingInterval = 3000;
			DisconnectedCause = ChatDisconnectCause.None;
			AuthValues = authValues;
			AppId = appId;
			AppVersion = appVersion;
			didAuthenticate = false;
			chatPeer.QuickResendAttempts = 2;
			chatPeer.SentCountAllowance = 7;
			PublicChannels.Clear();
			PrivateChannels.Clear();
			PublicChannelsUnsubscribing.Clear();
			NameServerAddress = chatPeer.NameServerAddress;
			bool num = chatPeer.Connect();
			if (num)
			{
				State = ChatState.ConnectingToNameServer;
			}
			if (UseBackgroundWorkerForSending)
			{
				SupportClass.StartBackgroundCalls(SendOutgoingInBackground, msDeltaForServiceCalls, "ChatClient Service Thread");
			}
			return num;
		}

		public bool ConnectAndSetStatus(string appId, string appVersion, AuthenticationValues authValues, int status = 2, object message = null)
		{
			statusToSetWhenConnected = status;
			messageToSetWhenConnected = message;
			return Connect(appId, appVersion, authValues);
		}

		public void Service()
		{
			while (HasPeer && chatPeer.DispatchIncomingCommands())
			{
			}
			if (!UseBackgroundWorkerForSending && (Environment.TickCount - msTimestampOfLastServiceCall > msDeltaForServiceCalls || msTimestampOfLastServiceCall == 0))
			{
				msTimestampOfLastServiceCall = Environment.TickCount;
				while (HasPeer && chatPeer.SendOutgoingCommands())
				{
				}
			}
		}

		private bool SendOutgoingInBackground()
		{
			while (HasPeer && chatPeer.SendOutgoingCommands())
			{
			}
			return State != ChatState.Disconnected;
		}

		[Obsolete("Better use UseBackgroundWorkerForSending and Service().")]
		public void SendAcksOnly()
		{
			if (HasPeer)
			{
				chatPeer.SendAcksOnly();
			}
		}

		public void Disconnect()
		{
			if (HasPeer && chatPeer.PeerState != 0)
			{
				chatPeer.Disconnect();
			}
		}

		public void StopThread()
		{
			if (HasPeer)
			{
				chatPeer.StopThread();
			}
		}

		public bool Subscribe(string[] channels)
		{
			return Subscribe(channels, 0);
		}

		public bool Subscribe(string[] channels, int[] lastMsgIds)
		{
			if (!CanChat)
			{
				if ((int)DebugOut >= 1)
				{
					listener.DebugReturn(DebugLevel.ERROR, "Subscribe called while not connected to front end server.");
				}
				return false;
			}
			if (channels == null || channels.Length == 0)
			{
				if ((int)DebugOut >= 2)
				{
					listener.DebugReturn(DebugLevel.WARNING, "Subscribe can't be called for empty or null channels-list.");
				}
				return false;
			}
			for (int i = 0; i < channels.Length; i++)
			{
				if (string.IsNullOrEmpty(channels[i]))
				{
					if ((int)DebugOut >= 1)
					{
						listener.DebugReturn(DebugLevel.ERROR, $"Subscribe can't be called with a null or empty channel name at index {i}.");
					}
					return false;
				}
			}
			if (lastMsgIds == null || lastMsgIds.Length != channels.Length)
			{
				if ((int)DebugOut >= 1)
				{
					listener.DebugReturn(DebugLevel.ERROR, "Subscribe can't be called when \"lastMsgIds\" array is null or does not have the same length as \"channels\" array.");
				}
				return false;
			}
			Dictionary<byte, object> operationParameters = new Dictionary<byte, object>
			{
				{ 0, channels },
				{ 9, lastMsgIds },
				{ 14, -1 }
			};
			return chatPeer.SendOperation(0, operationParameters, SendOptions.SendReliable);
		}

		public bool Subscribe(string[] channels, int messagesFromHistory)
		{
			if (!CanChat)
			{
				if ((int)DebugOut >= 1)
				{
					listener.DebugReturn(DebugLevel.ERROR, "Subscribe called while not connected to front end server.");
				}
				return false;
			}
			if (channels == null || channels.Length == 0)
			{
				if ((int)DebugOut >= 2)
				{
					listener.DebugReturn(DebugLevel.WARNING, "Subscribe can't be called for empty or null channels-list.");
				}
				return false;
			}
			return SendChannelOperation(channels, 0, messagesFromHistory);
		}

		public bool Unsubscribe(string[] channels)
		{
			if (!CanChat)
			{
				if ((int)DebugOut >= 1)
				{
					listener.DebugReturn(DebugLevel.ERROR, "Unsubscribe called while not connected to front end server.");
				}
				return false;
			}
			if (channels == null || channels.Length == 0)
			{
				if ((int)DebugOut >= 2)
				{
					listener.DebugReturn(DebugLevel.WARNING, "Unsubscribe can't be called for empty or null channels-list.");
				}
				return false;
			}
			foreach (string item in channels)
			{
				PublicChannelsUnsubscribing.Add(item);
			}
			return SendChannelOperation(channels, 1, 0);
		}

		public bool PublishMessage(string channelName, object message, bool forwardAsWebhook = false)
		{
			return publishMessage(channelName, message, reliable: true, forwardAsWebhook);
		}

		internal bool PublishMessageUnreliable(string channelName, object message, bool forwardAsWebhook = false)
		{
			return publishMessage(channelName, message, reliable: false, forwardAsWebhook);
		}

		private bool publishMessage(string channelName, object message, bool reliable, bool forwardAsWebhook = false)
		{
			if (!CanChat)
			{
				if ((int)DebugOut >= 1)
				{
					listener.DebugReturn(DebugLevel.ERROR, "PublishMessage called while not connected to front end server.");
				}
				return false;
			}
			if (string.IsNullOrEmpty(channelName) || message == null)
			{
				if ((int)DebugOut >= 2)
				{
					listener.DebugReturn(DebugLevel.WARNING, "PublishMessage parameters must be non-null and not empty.");
				}
				return false;
			}
			Dictionary<byte, object> dictionary = new Dictionary<byte, object>
			{
				{ 1, channelName },
				{ 3, message }
			};
			if (forwardAsWebhook)
			{
				dictionary.Add(21, (byte)1);
			}
			return chatPeer.SendOperation(2, dictionary, new SendOptions
			{
				Reliability = reliable
			});
		}

		public bool SendPrivateMessage(string target, object message, bool forwardAsWebhook = false)
		{
			return SendPrivateMessage(target, message, encrypt: false, forwardAsWebhook);
		}

		public bool SendPrivateMessage(string target, object message, bool encrypt, bool forwardAsWebhook)
		{
			return sendPrivateMessage(target, message, encrypt, reliable: true, forwardAsWebhook);
		}

		internal bool SendPrivateMessageUnreliable(string target, object message, bool encrypt, bool forwardAsWebhook = false)
		{
			return sendPrivateMessage(target, message, encrypt, reliable: false, forwardAsWebhook);
		}

		private bool sendPrivateMessage(string target, object message, bool encrypt, bool reliable, bool forwardAsWebhook = false)
		{
			if (!CanChat)
			{
				if ((int)DebugOut >= 1)
				{
					listener.DebugReturn(DebugLevel.ERROR, "SendPrivateMessage called while not connected to front end server.");
				}
				return false;
			}
			if (string.IsNullOrEmpty(target) || message == null)
			{
				if ((int)DebugOut >= 2)
				{
					listener.DebugReturn(DebugLevel.WARNING, "SendPrivateMessage parameters must be non-null and not empty.");
				}
				return false;
			}
			Dictionary<byte, object> dictionary = new Dictionary<byte, object>
			{
				{ 225, target },
				{ 3, message }
			};
			if (forwardAsWebhook)
			{
				dictionary.Add(21, (byte)1);
			}
			return chatPeer.SendOperation(3, dictionary, new SendOptions
			{
				Reliability = reliable,
				Encrypt = encrypt
			});
		}

		private bool SetOnlineStatus(int status, object message, bool skipMessage)
		{
			if (!CanChat)
			{
				if ((int)DebugOut >= 1)
				{
					listener.DebugReturn(DebugLevel.ERROR, "SetOnlineStatus called while not connected to front end server.");
				}
				return false;
			}
			Dictionary<byte, object> dictionary = new Dictionary<byte, object> { { 10, status } };
			if (skipMessage)
			{
				dictionary[12] = true;
			}
			else
			{
				dictionary[3] = message;
			}
			return chatPeer.SendOperation(5, dictionary, SendOptions.SendReliable);
		}

		public bool SetOnlineStatus(int status)
		{
			return SetOnlineStatus(status, null, skipMessage: true);
		}

		public bool SetOnlineStatus(int status, object message)
		{
			return SetOnlineStatus(status, message, skipMessage: false);
		}

		public bool AddFriends(string[] friends)
		{
			if (!CanChat)
			{
				if ((int)DebugOut >= 1)
				{
					listener.DebugReturn(DebugLevel.ERROR, "AddFriends called while not connected to front end server.");
				}
				return false;
			}
			if (friends == null || friends.Length == 0)
			{
				if ((int)DebugOut >= 2)
				{
					listener.DebugReturn(DebugLevel.WARNING, "AddFriends can't be called for empty or null list.");
				}
				return false;
			}
			if (friends.Length > 1024)
			{
				if ((int)DebugOut >= 2)
				{
					listener.DebugReturn(DebugLevel.WARNING, "AddFriends max list size exceeded: " + friends.Length + " > " + 1024);
				}
				return false;
			}
			Dictionary<byte, object> operationParameters = new Dictionary<byte, object> { { 11, friends } };
			return chatPeer.SendOperation(6, operationParameters, SendOptions.SendReliable);
		}

		public bool RemoveFriends(string[] friends)
		{
			if (!CanChat)
			{
				if ((int)DebugOut >= 1)
				{
					listener.DebugReturn(DebugLevel.ERROR, "RemoveFriends called while not connected to front end server.");
				}
				return false;
			}
			if (friends == null || friends.Length == 0)
			{
				if ((int)DebugOut >= 2)
				{
					listener.DebugReturn(DebugLevel.WARNING, "RemoveFriends can't be called for empty or null list.");
				}
				return false;
			}
			if (friends.Length > 1024)
			{
				if ((int)DebugOut >= 2)
				{
					listener.DebugReturn(DebugLevel.WARNING, "RemoveFriends max list size exceeded: " + friends.Length + " > " + 1024);
				}
				return false;
			}
			Dictionary<byte, object> operationParameters = new Dictionary<byte, object> { { 11, friends } };
			return chatPeer.SendOperation(7, operationParameters, SendOptions.SendReliable);
		}

		public string GetPrivateChannelNameByUser(string userName)
		{
			return $"{UserId}:{userName}";
		}

		public bool TryGetChannel(string channelName, bool isPrivate, out ChatChannel channel)
		{
			if (!isPrivate)
			{
				return PublicChannels.TryGetValue(channelName, out channel);
			}
			return PrivateChannels.TryGetValue(channelName, out channel);
		}

		public bool TryGetChannel(string channelName, out ChatChannel channel)
		{
			if (PublicChannels.TryGetValue(channelName, out channel))
			{
				return true;
			}
			return PrivateChannels.TryGetValue(channelName, out channel);
		}

		public bool TryGetPrivateChannelByUser(string userId, out ChatChannel channel)
		{
			channel = null;
			if (string.IsNullOrEmpty(userId))
			{
				return false;
			}
			string privateChannelNameByUser = GetPrivateChannelNameByUser(userId);
			return TryGetChannel(privateChannelNameByUser, isPrivate: true, out channel);
		}

		void IPhotonPeerListener.DebugReturn(DebugLevel level, string message)
		{
			listener.DebugReturn(level, message);
		}

		void IPhotonPeerListener.OnEvent(EventData eventData)
		{
			switch (eventData.Code)
			{
			case 0:
				HandleChatMessagesEvent(eventData);
				break;
			case 2:
				HandlePrivateMessageEvent(eventData);
				break;
			case 4:
				HandleStatusUpdate(eventData);
				break;
			case 5:
				HandleSubscribeEvent(eventData);
				break;
			case 6:
				HandleUnsubscribeEvent(eventData);
				break;
			case 8:
				HandleUserSubscribedEvent(eventData);
				break;
			case 9:
				HandleUserUnsubscribedEvent(eventData);
				break;
			case 1:
			case 3:
			case 7:
				break;
			}
		}

		void IPhotonPeerListener.OnOperationResponse(OperationResponse operationResponse)
		{
			byte operationCode = operationResponse.OperationCode;
			if ((uint)operationCode > 3u && operationCode == 230)
			{
				HandleAuthResponse(operationResponse);
			}
			else if (operationResponse.ReturnCode != 0 && (int)DebugOut >= 1)
			{
				if (operationResponse.ReturnCode == -2)
				{
					listener.DebugReturn(DebugLevel.ERROR, $"Chat Operation {operationResponse.OperationCode} unknown on server. Check your AppId and make sure it's for a Chat application.");
				}
				else
				{
					listener.DebugReturn(DebugLevel.ERROR, $"Chat Operation {operationResponse.OperationCode} failed (Code: {operationResponse.ReturnCode}). Debug Message: {operationResponse.DebugMessage}");
				}
			}
		}

		void IPhotonPeerListener.OnStatusChanged(StatusCode statusCode)
		{
			switch (statusCode)
			{
			case StatusCode.Connect:
				if (!chatPeer.IsProtocolSecure)
				{
					chatPeer.EstablishEncryption();
				}
				else if (!didAuthenticate)
				{
					didAuthenticate = chatPeer.AuthenticateOnNameServer(AppId, AppVersion, chatRegion, AuthValues);
					if (!didAuthenticate && (int)DebugOut >= 1)
					{
						((IPhotonPeerListener)this).DebugReturn(DebugLevel.ERROR, "Error calling OpAuthenticate! Did not work. Check log output, AuthValues and if you're connected. State: " + State);
					}
				}
				if (State == ChatState.ConnectingToNameServer)
				{
					State = ChatState.ConnectedToNameServer;
					listener.OnChatStateChange(State);
				}
				else if (State == ChatState.ConnectingToFrontEnd)
				{
					AuthenticateOnFrontEnd();
				}
				break;
			case StatusCode.EncryptionEstablished:
				if (!didAuthenticate)
				{
					didAuthenticate = chatPeer.AuthenticateOnNameServer(AppId, AppVersion, chatRegion, AuthValues);
					if (!didAuthenticate && (int)DebugOut >= 1)
					{
						((IPhotonPeerListener)this).DebugReturn(DebugLevel.ERROR, "Error calling OpAuthenticate! Did not work. Check log output, AuthValues and if you're connected. State: " + State);
					}
				}
				break;
			case StatusCode.EncryptionFailedToEstablish:
				State = ChatState.Disconnecting;
				chatPeer.Disconnect();
				break;
			case StatusCode.Disconnect:
				if (State == ChatState.Authenticated)
				{
					ConnectToFrontEnd();
					break;
				}
				State = ChatState.Disconnected;
				listener.OnChatStateChange(ChatState.Disconnected);
				listener.OnDisconnected();
				break;
			}
		}

		private bool SendChannelOperation(string[] channels, byte operation, int historyLength)
		{
			Dictionary<byte, object> dictionary = new Dictionary<byte, object> { { 0, channels } };
			if (historyLength != 0)
			{
				dictionary.Add(14, historyLength);
			}
			return chatPeer.SendOperation(operation, dictionary, SendOptions.SendReliable);
		}

		private void HandlePrivateMessageEvent(EventData eventData)
		{
			object message = eventData.Parameters[3];
			string text = (string)eventData.Parameters[5];
			int msgId = (int)eventData.Parameters[8];
			string privateChannelNameByUser;
			if (UserId != null && UserId.Equals(text))
			{
				string userName = (string)eventData.Parameters[225];
				privateChannelNameByUser = GetPrivateChannelNameByUser(userName);
			}
			else
			{
				privateChannelNameByUser = GetPrivateChannelNameByUser(text);
			}
			if (!PrivateChannels.TryGetValue(privateChannelNameByUser, out var value))
			{
				value = new ChatChannel(privateChannelNameByUser);
				value.IsPrivate = true;
				value.MessageLimit = MessageLimit;
				PrivateChannels.Add(value.Name, value);
			}
			value.Add(text, message, msgId);
			listener.OnPrivateMessage(text, message, privateChannelNameByUser);
		}

		private void HandleChatMessagesEvent(EventData eventData)
		{
			object[] messages = (object[])eventData.Parameters[2];
			string[] senders = (string[])eventData.Parameters[4];
			string text = (string)eventData.Parameters[1];
			int lastMsgId = (int)eventData.Parameters[8];
			if (!PublicChannels.TryGetValue(text, out var value))
			{
				if ((int)DebugOut >= 2)
				{
					listener.DebugReturn(DebugLevel.WARNING, "Channel " + text + " for incoming message event not found.");
				}
			}
			else
			{
				value.Add(senders, messages, lastMsgId);
				listener.OnGetMessages(text, senders, messages);
			}
		}

		private void HandleSubscribeEvent(EventData eventData)
		{
			string[] array = (string[])eventData.Parameters[0];
			bool[] array2 = (bool[])eventData.Parameters[15];
			if (eventData.Parameters.TryGetValue(22, out var value))
			{
				Dictionary<object, object> newProperties = value as Dictionary<object, object>;
				if (array.Length == 1)
				{
					if (array2[0])
					{
						string text = array[0];
						if (PublicChannels.TryGetValue(text, out var value2))
						{
							value2.Subscribers.Clear();
							value2.ClearProperties();
						}
						else
						{
							value2 = new ChatChannel(text);
							value2.MessageLimit = MessageLimit;
							PublicChannels.Add(value2.Name, value2);
						}
						value2.ReadProperties(newProperties);
						if (value2.PublishSubscribers)
						{
							value2.Subscribers.Add(UserId);
							if (eventData.Parameters.TryGetValue(23, out value))
							{
								string[] users = value as string[];
								value2.AddSubscribers(users);
							}
						}
					}
					listener.OnSubscribed(array, array2);
					return;
				}
				listener.DebugReturn(DebugLevel.ERROR, "Unexpected: Subscribe event for multiple channels with channels properties returned. Ignoring properties.");
			}
			for (int i = 0; i < array.Length; i++)
			{
				if (array2[i])
				{
					string text2 = array[i];
					if (!PublicChannels.TryGetValue(text2, out var value3))
					{
						value3 = new ChatChannel(text2);
						value3.MessageLimit = MessageLimit;
						PublicChannels.Add(value3.Name, value3);
					}
				}
			}
			listener.OnSubscribed(array, array2);
		}

		private void HandleUnsubscribeEvent(EventData eventData)
		{
			string[] array = (string[])eventData[0];
			foreach (string text in array)
			{
				PublicChannels.Remove(text);
				PublicChannelsUnsubscribing.Remove(text);
			}
			listener.OnUnsubscribed(array);
		}

		private void HandleAuthResponse(OperationResponse operationResponse)
		{
			if ((int)DebugOut >= 3)
			{
				listener.DebugReturn(DebugLevel.INFO, operationResponse.ToStringFull() + " on: " + chatPeer.NameServerAddress);
			}
			if (operationResponse.ReturnCode == 0)
			{
				if (State == ChatState.ConnectedToNameServer)
				{
					State = ChatState.Authenticated;
					listener.OnChatStateChange(State);
					if (operationResponse.Parameters.ContainsKey(221))
					{
						if (AuthValues == null)
						{
							AuthValues = new AuthenticationValues();
						}
						AuthValues.Token = operationResponse[221] as string;
						FrontendAddress = (string)operationResponse[230];
						chatPeer.Disconnect();
					}
					else if ((int)DebugOut >= 1)
					{
						listener.DebugReturn(DebugLevel.ERROR, "No secret in authentication response.");
					}
					if (operationResponse.Parameters.ContainsKey(225))
					{
						string text = operationResponse.Parameters[225] as string;
						if (!string.IsNullOrEmpty(text))
						{
							UserId = text;
							listener.DebugReturn(DebugLevel.INFO, $"Received your UserID from server. Updating local value to: {UserId}");
						}
					}
				}
				else if (State == ChatState.ConnectingToFrontEnd)
				{
					State = ChatState.ConnectedToFrontEnd;
					listener.OnChatStateChange(State);
					listener.OnConnected();
					if (statusToSetWhenConnected.HasValue)
					{
						SetOnlineStatus(statusToSetWhenConnected.Value, messageToSetWhenConnected);
						statusToSetWhenConnected = null;
					}
				}
			}
			else
			{
				switch (operationResponse.ReturnCode)
				{
				case short.MaxValue:
					DisconnectedCause = ChatDisconnectCause.InvalidAuthentication;
					break;
				case 32755:
					DisconnectedCause = ChatDisconnectCause.CustomAuthenticationFailed;
					break;
				case 32756:
					DisconnectedCause = ChatDisconnectCause.InvalidRegion;
					break;
				case 32757:
					DisconnectedCause = ChatDisconnectCause.MaxCcuReached;
					break;
				case -3:
					DisconnectedCause = ChatDisconnectCause.OperationNotAllowedInCurrentState;
					break;
				}
				if ((int)DebugOut >= 1)
				{
					listener.DebugReturn(DebugLevel.ERROR, "Authentication request error: " + operationResponse.ReturnCode + ". Disconnecting.");
				}
				State = ChatState.Disconnecting;
				chatPeer.Disconnect();
			}
		}

		private void HandleStatusUpdate(EventData eventData)
		{
			string user = (string)eventData.Parameters[5];
			int status = (int)eventData.Parameters[10];
			object message = null;
			bool flag = eventData.Parameters.ContainsKey(3);
			if (flag)
			{
				message = eventData.Parameters[3];
			}
			listener.OnStatusUpdate(user, status, flag, message);
		}

		private void ConnectToFrontEnd()
		{
			State = ChatState.ConnectingToFrontEnd;
			if ((int)DebugOut >= 3)
			{
				listener.DebugReturn(DebugLevel.INFO, "Connecting to frontend " + FrontendAddress);
			}
			chatPeer.Connect(FrontendAddress, "chat");
		}

		private bool AuthenticateOnFrontEnd()
		{
			if (AuthValues != null)
			{
				if (string.IsNullOrEmpty(AuthValues.Token))
				{
					if ((int)DebugOut >= 1)
					{
						listener.DebugReturn(DebugLevel.ERROR, "Can't authenticate on front end server. Secret is not set");
					}
					return false;
				}
				Dictionary<byte, object> operationParameters = new Dictionary<byte, object> { { 221, AuthValues.Token } };
				return chatPeer.SendOperation(230, operationParameters, SendOptions.SendReliable);
			}
			if ((int)DebugOut >= 1)
			{
				listener.DebugReturn(DebugLevel.ERROR, "Can't authenticate on front end server. Authentication Values are not set");
			}
			return false;
		}

		private void HandleUserUnsubscribedEvent(EventData eventData)
		{
			string text = eventData.Parameters[1] as string;
			string text2 = eventData.Parameters[225] as string;
			if (PublicChannels.TryGetValue(text, out var value))
			{
				if (!value.PublishSubscribers && (int)DebugOut >= 2)
				{
					listener.DebugReturn(DebugLevel.WARNING, $"Channel \"{text}\" for incoming UserUnsubscribed (\"{text2}\") event does not have PublishSubscribers enabled.");
				}
				if (!value.Subscribers.Remove(text2) && (int)DebugOut >= 2)
				{
					listener.DebugReturn(DebugLevel.WARNING, $"Channel \"{text}\" does not contain unsubscribed user \"{text2}\".");
				}
			}
			else if ((int)DebugOut >= 2)
			{
				listener.DebugReturn(DebugLevel.WARNING, $"Channel \"{text}\" not found for incoming UserUnsubscribed (\"{text2}\") event.");
			}
			listener.OnUserUnsubscribed(text, text2);
		}

		private void HandleUserSubscribedEvent(EventData eventData)
		{
			string text = eventData.Parameters[1] as string;
			string text2 = eventData.Parameters[225] as string;
			if (PublicChannels.TryGetValue(text, out var value))
			{
				if (!value.PublishSubscribers && (int)DebugOut >= 2)
				{
					listener.DebugReturn(DebugLevel.WARNING, $"Channel \"{text}\" for incoming UserSubscribed (\"{text2}\") event does not have PublishSubscribers enabled.");
				}
				if (!value.Subscribers.Add(text2))
				{
					if ((int)DebugOut >= 2)
					{
						listener.DebugReturn(DebugLevel.WARNING, $"Channel \"{text}\" already contains newly subscribed user \"{text2}\".");
					}
				}
				else if (value.MaxSubscribers > 0 && value.Subscribers.Count > value.MaxSubscribers && (int)DebugOut >= 2)
				{
					listener.DebugReturn(DebugLevel.WARNING, $"Channel \"{text}\"'s MaxSubscribers exceeded. count={value.Subscribers.Count} > MaxSubscribers={value.MaxSubscribers}.");
				}
			}
			else if ((int)DebugOut >= 2)
			{
				listener.DebugReturn(DebugLevel.WARNING, $"Channel \"{text}\" not found for incoming UserSubscribed (\"{text2}\") event.");
			}
			listener.OnUserSubscribed(text, text2);
		}

		public bool Subscribe(string channel, int lastMsgId = 0, int messagesFromHistory = -1, ChannelCreationOptions creationOptions = null)
		{
			if (creationOptions == null)
			{
				creationOptions = ChannelCreationOptions.Default;
			}
			int maxSubscribers = creationOptions.MaxSubscribers;
			bool publishSubscribers = creationOptions.PublishSubscribers;
			if (maxSubscribers < 0)
			{
				if ((int)DebugOut >= 1)
				{
					listener.DebugReturn(DebugLevel.ERROR, "Cannot set MaxSubscribers < 0.");
				}
				return false;
			}
			if (lastMsgId < 0)
			{
				if ((int)DebugOut >= 1)
				{
					listener.DebugReturn(DebugLevel.ERROR, "lastMsgId cannot be < 0.");
				}
				return false;
			}
			if (messagesFromHistory < -1)
			{
				if ((int)DebugOut >= 2)
				{
					listener.DebugReturn(DebugLevel.WARNING, "messagesFromHistory < -1, setting it to -1");
				}
				messagesFromHistory = -1;
			}
			if (lastMsgId > 0 && messagesFromHistory == 0)
			{
				if ((int)DebugOut >= 2)
				{
					listener.DebugReturn(DebugLevel.WARNING, "lastMsgId will be ignored because messagesFromHistory == 0");
				}
				lastMsgId = 0;
			}
			Dictionary<object, object> dictionary = null;
			if (publishSubscribers)
			{
				if (maxSubscribers > 100)
				{
					if ((int)DebugOut >= 1)
					{
						listener.DebugReturn(DebugLevel.ERROR, $"Cannot set MaxSubscribers > {100} when PublishSubscribers == true.");
					}
					return false;
				}
				dictionary = new Dictionary<object, object>();
				dictionary[(byte)254] = true;
			}
			if (maxSubscribers > 0)
			{
				if (dictionary == null)
				{
					dictionary = new Dictionary<object, object>();
				}
				dictionary[byte.MaxValue] = maxSubscribers;
			}
			Dictionary<byte, object> dictionary2 = new Dictionary<byte, object>();
			dictionary2.Add(0, new string[1] { channel });
			Dictionary<byte, object> dictionary3 = dictionary2;
			if (messagesFromHistory != 0)
			{
				dictionary3.Add(14, messagesFromHistory);
			}
			if (lastMsgId > 0)
			{
				dictionary3.Add(9, new int[1] { lastMsgId });
			}
			if (dictionary != null && dictionary.Count > 0)
			{
				dictionary3.Add(22, dictionary);
			}
			return chatPeer.SendOperation(0, dictionary3, SendOptions.SendReliable);
		}
	}
}
