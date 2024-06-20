using System;
using System.Collections.Generic;
using ExitGames.Client.Photon;
using UnityEngine;

internal class LoadBalancingPeer : PhotonPeer
{
	private enum RoomOptionBit
	{
		CheckUserOnJoin = 1,
		DeleteCacheOnLeave = 2,
		SuppressRoomEvents = 4,
		PublishUserId = 8,
		DeleteNullProps = 0x10,
		BroadcastPropsChangeToAll = 0x20
	}

	private readonly Dictionary<byte, object> opParameters = new Dictionary<byte, object>();

	internal bool IsProtocolSecure => base.UsedProtocol == ConnectionProtocol.WebSocketSecure;

	public LoadBalancingPeer(ConnectionProtocol protocolType)
		: base(protocolType)
	{
	}

	public LoadBalancingPeer(IPhotonPeerListener listener, ConnectionProtocol protocolType)
		: this(protocolType)
	{
		base.Listener = listener;
	}

	public virtual bool OpGetRegions(string appId)
	{
		Dictionary<byte, object> dictionary = new Dictionary<byte, object>();
		dictionary[224] = appId;
		SendOptions sendOptions = default(SendOptions);
		sendOptions.Reliability = true;
		sendOptions.Channel = 0;
		sendOptions.Encrypt = true;
		SendOptions sendOptions2 = sendOptions;
		return SendOperation(220, dictionary, sendOptions2);
	}

	public virtual bool OpJoinLobby(TypedLobby lobby = null)
	{
		if ((int)DebugOut >= 3)
		{
			base.Listener.DebugReturn(DebugLevel.INFO, "OpJoinLobby()");
		}
		Dictionary<byte, object> dictionary = null;
		if (lobby != null && !lobby.IsDefault)
		{
			dictionary = new Dictionary<byte, object>();
			dictionary[213] = lobby.Name;
			dictionary[212] = (byte)lobby.Type;
		}
		return SendOperation(229, dictionary, SendOptions.SendReliable);
	}

	public virtual bool OpLeaveLobby()
	{
		if ((int)DebugOut >= 3)
		{
			base.Listener.DebugReturn(DebugLevel.INFO, "OpLeaveLobby()");
		}
		return SendOperation((byte)228, (Dictionary<byte, object>)null, SendOptions.SendReliable);
	}

	private void RoomOptionsToOpParameters(Dictionary<byte, object> op, RoomOptions roomOptions)
	{
		if (roomOptions == null)
		{
			roomOptions = new RoomOptions();
		}
		Hashtable hashtable = new Hashtable();
		hashtable[253] = roomOptions.IsOpen;
		hashtable[254] = roomOptions.IsVisible;
		hashtable[250] = ((roomOptions.CustomRoomPropertiesForLobby == null) ? new string[0] : roomOptions.CustomRoomPropertiesForLobby);
		hashtable.MergeStringKeys(roomOptions.CustomRoomProperties);
		if (roomOptions.MaxPlayers > 0)
		{
			hashtable[byte.MaxValue] = roomOptions.MaxPlayers;
		}
		op[248] = hashtable;
		int num = 0;
		op[241] = roomOptions.CleanupCacheOnLeave;
		if (roomOptions.CleanupCacheOnLeave)
		{
			num |= 2;
			hashtable[249] = true;
		}
		num |= 1;
		op[232] = true;
		if (roomOptions.PlayerTtl > 0 || roomOptions.PlayerTtl == -1)
		{
			op[235] = roomOptions.PlayerTtl;
		}
		if (roomOptions.EmptyRoomTtl > 0)
		{
			op[236] = roomOptions.EmptyRoomTtl;
		}
		if (roomOptions.SuppressRoomEvents)
		{
			num |= 4;
			op[237] = true;
		}
		if (roomOptions.Plugins != null)
		{
			op[204] = roomOptions.Plugins;
		}
		if (roomOptions.PublishUserId)
		{
			num |= 8;
			op[239] = true;
		}
		if (roomOptions.DeleteNullProperties)
		{
			num |= 0x10;
		}
		op[191] = num;
	}

	public virtual bool OpCreateRoom(EnterRoomParams opParams)
	{
		if ((int)DebugOut >= 3)
		{
			base.Listener.DebugReturn(DebugLevel.INFO, "OpCreateRoom()");
		}
		Dictionary<byte, object> dictionary = new Dictionary<byte, object>();
		if (!string.IsNullOrEmpty(opParams.RoomName))
		{
			dictionary[byte.MaxValue] = opParams.RoomName;
		}
		if (opParams.Lobby != null && !opParams.Lobby.IsDefault)
		{
			dictionary[213] = opParams.Lobby.Name;
			dictionary[212] = (byte)opParams.Lobby.Type;
		}
		if (opParams.ExpectedUsers != null && opParams.ExpectedUsers.Length != 0)
		{
			dictionary[238] = opParams.ExpectedUsers;
		}
		if (opParams.OnGameServer)
		{
			if (opParams.PlayerProperties != null && opParams.PlayerProperties.Count > 0)
			{
				dictionary[249] = opParams.PlayerProperties;
				dictionary[250] = true;
			}
			RoomOptionsToOpParameters(dictionary, opParams.RoomOptions);
		}
		return SendOperation(227, dictionary, SendOptions.SendReliable);
	}

	public virtual bool OpJoinRoom(EnterRoomParams opParams)
	{
		if ((int)DebugOut >= 3)
		{
			base.Listener.DebugReturn(DebugLevel.INFO, "OpJoinRoom()");
		}
		Dictionary<byte, object> dictionary = new Dictionary<byte, object>();
		if (!string.IsNullOrEmpty(opParams.RoomName))
		{
			dictionary[byte.MaxValue] = opParams.RoomName;
		}
		if (opParams.CreateIfNotExists)
		{
			dictionary[215] = (byte)1;
			if (opParams.Lobby != null && !opParams.Lobby.IsDefault)
			{
				dictionary[213] = opParams.Lobby.Name;
				dictionary[212] = (byte)opParams.Lobby.Type;
			}
		}
		if (opParams.RejoinOnly)
		{
			dictionary[215] = (byte)3;
		}
		if (opParams.ExpectedUsers != null && opParams.ExpectedUsers.Length != 0)
		{
			dictionary[238] = opParams.ExpectedUsers;
		}
		if (opParams.OnGameServer)
		{
			if (opParams.PlayerProperties != null && opParams.PlayerProperties.Count > 0)
			{
				dictionary[249] = opParams.PlayerProperties;
				dictionary[250] = true;
			}
			if (opParams.CreateIfNotExists)
			{
				RoomOptionsToOpParameters(dictionary, opParams.RoomOptions);
			}
		}
		return SendOperation(226, dictionary, SendOptions.SendReliable);
	}

	public virtual bool OpJoinRandomRoom(OpJoinRandomRoomParams opJoinRandomRoomParams)
	{
		if ((int)DebugOut >= 3)
		{
			base.Listener.DebugReturn(DebugLevel.INFO, "OpJoinRandomRoom()");
		}
		Hashtable hashtable = new Hashtable();
		hashtable.MergeStringKeys(opJoinRandomRoomParams.ExpectedCustomRoomProperties);
		if (opJoinRandomRoomParams.ExpectedMaxPlayers > 0)
		{
			hashtable[byte.MaxValue] = opJoinRandomRoomParams.ExpectedMaxPlayers;
		}
		Dictionary<byte, object> dictionary = new Dictionary<byte, object>();
		if (hashtable.Count > 0)
		{
			dictionary[248] = hashtable;
		}
		if (opJoinRandomRoomParams.MatchingType != 0)
		{
			dictionary[223] = (byte)opJoinRandomRoomParams.MatchingType;
		}
		if (opJoinRandomRoomParams.TypedLobby != null && !opJoinRandomRoomParams.TypedLobby.IsDefault)
		{
			dictionary[213] = opJoinRandomRoomParams.TypedLobby.Name;
			dictionary[212] = (byte)opJoinRandomRoomParams.TypedLobby.Type;
		}
		if (!string.IsNullOrEmpty(opJoinRandomRoomParams.SqlLobbyFilter))
		{
			dictionary[245] = opJoinRandomRoomParams.SqlLobbyFilter;
		}
		if (opJoinRandomRoomParams.ExpectedUsers != null && opJoinRandomRoomParams.ExpectedUsers.Length != 0)
		{
			dictionary[238] = opJoinRandomRoomParams.ExpectedUsers;
		}
		return SendOperation(225, dictionary, SendOptions.SendReliable);
	}

	public virtual bool OpLeaveRoom(bool becomeInactive)
	{
		Dictionary<byte, object> dictionary = null;
		if (becomeInactive)
		{
			dictionary = new Dictionary<byte, object>();
			dictionary[233] = becomeInactive;
		}
		return SendOperation(254, dictionary, SendOptions.SendReliable);
	}

	public virtual bool OpGetGameList(TypedLobby lobby, string queryData)
	{
		if ((int)DebugOut >= 3)
		{
			base.Listener.DebugReturn(DebugLevel.INFO, "OpGetGameList()");
		}
		if (lobby == null)
		{
			if ((int)DebugOut >= 3)
			{
				base.Listener.DebugReturn(DebugLevel.INFO, "OpGetGameList not sent. Lobby cannot be null.");
			}
			return false;
		}
		if (lobby.Type != LobbyType.SqlLobby)
		{
			if ((int)DebugOut >= 3)
			{
				base.Listener.DebugReturn(DebugLevel.INFO, "OpGetGameList not sent. LobbyType must be SqlLobby.");
			}
			return false;
		}
		if (lobby.IsDefault)
		{
			if ((int)DebugOut >= 3)
			{
				base.Listener.DebugReturn(DebugLevel.INFO, "OpGetGameList not sent. LobbyName must be not null and not empty.");
			}
			return false;
		}
		Dictionary<byte, object> dictionary = new Dictionary<byte, object>();
		dictionary[213] = lobby.Name;
		dictionary[212] = (byte)lobby.Type;
		dictionary[245] = queryData;
		return SendOperation(217, dictionary, SendOptions.SendReliable);
	}

	public virtual bool OpFindFriends(string[] friendsToFind, FindFriendsOptions options = null)
	{
		Dictionary<byte, object> dictionary = new Dictionary<byte, object>();
		if (friendsToFind != null && friendsToFind.Length != 0)
		{
			dictionary[1] = friendsToFind;
		}
		if (options != null)
		{
			dictionary[2] = options.ToIntFlags();
		}
		return SendOperation(222, dictionary, SendOptions.SendReliable);
	}

	public bool OpSetCustomPropertiesOfActor(int actorNr, Hashtable actorProperties)
	{
		return OpSetPropertiesOfActor(actorNr, actorProperties.StripToStringKeys());
	}

	protected internal bool OpSetPropertiesOfActor(int actorNr, Hashtable actorProperties, Hashtable expectedProperties = null, bool webForward = false)
	{
		if ((int)DebugOut >= 3)
		{
			base.Listener.DebugReturn(DebugLevel.INFO, "OpSetPropertiesOfActor()");
		}
		if (actorNr <= 0 || actorProperties == null)
		{
			if ((int)DebugOut >= 3)
			{
				base.Listener.DebugReturn(DebugLevel.INFO, "OpSetPropertiesOfActor not sent. ActorNr must be > 0 and actorProperties != null.");
			}
			return false;
		}
		Dictionary<byte, object> dictionary = new Dictionary<byte, object>();
		dictionary.Add(251, actorProperties);
		dictionary.Add(254, actorNr);
		dictionary.Add(250, true);
		if (expectedProperties != null && expectedProperties.Count != 0)
		{
			dictionary.Add(231, expectedProperties);
		}
		if (webForward)
		{
			dictionary[234] = true;
		}
		SendOptions sendOptions = default(SendOptions);
		sendOptions.Reliability = true;
		sendOptions.Channel = 0;
		sendOptions.Encrypt = false;
		SendOptions sendOptions2 = sendOptions;
		return SendOperation(252, dictionary, sendOptions2);
	}

	protected internal void OpSetPropertyOfRoom(byte propCode, object value)
	{
		Hashtable hashtable = new Hashtable();
		hashtable[propCode] = value;
		OpSetPropertiesOfRoom(hashtable);
	}

	[Obsolete("Use the other overload method")]
	public bool OpSetCustomPropertiesOfRoom(Hashtable gameProperties, bool broadcast, byte channelId)
	{
		return OpSetPropertiesOfRoom(gameProperties.StripToStringKeys());
	}

	public bool OpSetCustomPropertiesOfRoom(Hashtable gameProperties, Hashtable expectedProperties = null, bool webForward = false)
	{
		return OpSetPropertiesOfRoom(gameProperties.StripToStringKeys(), expectedProperties.StripToStringKeys(), webForward);
	}

	protected internal bool OpSetPropertiesOfRoom(Hashtable gameProperties, Hashtable expectedProperties = null, bool webForward = false)
	{
		if ((int)DebugOut >= 3)
		{
			base.Listener.DebugReturn(DebugLevel.INFO, "OpSetPropertiesOfRoom()");
		}
		Dictionary<byte, object> dictionary = new Dictionary<byte, object>();
		dictionary.Add(251, gameProperties);
		dictionary.Add(250, true);
		if (expectedProperties != null && expectedProperties.Count != 0)
		{
			dictionary.Add(231, expectedProperties);
		}
		if (webForward)
		{
			dictionary[234] = true;
		}
		SendOptions sendOptions = default(SendOptions);
		sendOptions.Reliability = true;
		sendOptions.Channel = 0;
		sendOptions.Encrypt = false;
		SendOptions sendOptions2 = sendOptions;
		return SendOperation(252, dictionary, sendOptions2);
	}

	public virtual bool OpAuthenticate(string appId, string appVersion, AuthenticationValues authValues, string regionCode, bool getLobbyStatistics)
	{
		if ((int)DebugOut >= 3)
		{
			base.Listener.DebugReturn(DebugLevel.INFO, "OpAuthenticate()");
		}
		Dictionary<byte, object> dictionary = new Dictionary<byte, object>();
		if (getLobbyStatistics)
		{
			dictionary[211] = true;
		}
		SendOptions sendOptions;
		SendOptions sendOptions2;
		if (authValues != null && authValues.Token != null)
		{
			dictionary[221] = authValues.Token;
			sendOptions = default(SendOptions);
			sendOptions.Reliability = true;
			sendOptions.Channel = 0;
			sendOptions.Encrypt = false;
			sendOptions2 = sendOptions;
			return SendOperation(230, dictionary, sendOptions2);
		}
		dictionary[220] = appVersion;
		dictionary[224] = appId;
		if (!string.IsNullOrEmpty(regionCode))
		{
			dictionary[210] = regionCode;
		}
		if (authValues != null)
		{
			if (!string.IsNullOrEmpty(authValues.UserId))
			{
				dictionary[225] = authValues.UserId;
			}
			if (authValues.AuthType != CustomAuthenticationType.None)
			{
				if (!IsProtocolSecure && !base.IsEncryptionAvailable)
				{
					base.Listener.DebugReturn(DebugLevel.ERROR, "OpAuthenticate() failed. When you want Custom Authentication encryption is mandatory.");
					return false;
				}
				dictionary[217] = (byte)authValues.AuthType;
				if (!string.IsNullOrEmpty(authValues.Token))
				{
					dictionary[221] = authValues.Token;
				}
				else
				{
					if (!string.IsNullOrEmpty(authValues.AuthGetParameters))
					{
						dictionary[216] = authValues.AuthGetParameters;
					}
					if (authValues.AuthPostData != null)
					{
						dictionary[214] = authValues.AuthPostData;
					}
				}
			}
		}
		sendOptions = default(SendOptions);
		sendOptions.Reliability = true;
		sendOptions.Channel = 0;
		sendOptions.Encrypt = base.IsEncryptionAvailable;
		sendOptions2 = sendOptions;
		bool num = SendOperation(230, dictionary, sendOptions2);
		if (!num)
		{
			base.Listener.DebugReturn(DebugLevel.ERROR, "Error calling OpAuthenticate! Did not work. Check log output, AuthValues and if you're connected.");
		}
		return num;
	}

	public virtual bool OpAuthenticateOnce(string appId, string appVersion, AuthenticationValues authValues, string regionCode, EncryptionMode encryptionMode, ConnectionProtocol expectedProtocol)
	{
		if ((int)DebugOut >= 3)
		{
			base.Listener.DebugReturn(DebugLevel.INFO, "OpAuthenticate()");
		}
		Dictionary<byte, object> dictionary = new Dictionary<byte, object>();
		SendOptions sendOptions;
		SendOptions sendOptions2;
		if (authValues != null && authValues.Token != null)
		{
			dictionary[221] = authValues.Token;
			sendOptions = default(SendOptions);
			sendOptions.Reliability = true;
			sendOptions.Channel = 0;
			sendOptions.Encrypt = false;
			sendOptions2 = sendOptions;
			return SendOperation(231, dictionary, sendOptions2);
		}
		if (encryptionMode == EncryptionMode.DatagramEncryption && expectedProtocol != 0)
		{
			Debug.LogWarning("Expected protocol set to UDP, due to encryption mode DatagramEncryption. Changing protocol in PhotonServerSettings from: " + PhotonNetwork.PhotonServerSettings.Protocol);
			PhotonNetwork.PhotonServerSettings.Protocol = ConnectionProtocol.Udp;
			expectedProtocol = ConnectionProtocol.Udp;
		}
		dictionary[195] = (byte)expectedProtocol;
		dictionary[193] = (byte)encryptionMode;
		dictionary[220] = appVersion;
		dictionary[224] = appId;
		if (!string.IsNullOrEmpty(regionCode))
		{
			dictionary[210] = regionCode;
		}
		if (authValues != null)
		{
			if (!string.IsNullOrEmpty(authValues.UserId))
			{
				dictionary[225] = authValues.UserId;
			}
			if (authValues.AuthType != CustomAuthenticationType.None)
			{
				dictionary[217] = (byte)authValues.AuthType;
				if (!string.IsNullOrEmpty(authValues.Token))
				{
					dictionary[221] = authValues.Token;
				}
				else
				{
					if (!string.IsNullOrEmpty(authValues.AuthGetParameters))
					{
						dictionary[216] = authValues.AuthGetParameters;
					}
					if (authValues.AuthPostData != null)
					{
						dictionary[214] = authValues.AuthPostData;
					}
				}
			}
		}
		sendOptions = default(SendOptions);
		sendOptions.Reliability = true;
		sendOptions.Channel = 0;
		sendOptions.Encrypt = base.IsEncryptionAvailable;
		sendOptions2 = sendOptions;
		return SendOperation(231, dictionary, sendOptions2);
	}

	public virtual bool OpChangeGroups(byte[] groupsToRemove, byte[] groupsToAdd)
	{
		if ((int)DebugOut >= 5)
		{
			base.Listener.DebugReturn(DebugLevel.ALL, "OpChangeGroups()");
		}
		Dictionary<byte, object> dictionary = new Dictionary<byte, object>();
		if (groupsToRemove != null)
		{
			dictionary[239] = groupsToRemove;
		}
		if (groupsToAdd != null)
		{
			dictionary[238] = groupsToAdd;
		}
		return SendOperation(248, dictionary, SendOptions.SendReliable);
	}

	public virtual bool OpRaiseEvent(byte eventCode, object customEventContent, bool sendReliable, RaiseEventOptions raiseEventOptions)
	{
		opParameters.Clear();
		opParameters[244] = eventCode;
		if (customEventContent != null)
		{
			opParameters[245] = customEventContent;
		}
		if (raiseEventOptions == null)
		{
			raiseEventOptions = RaiseEventOptions.Default;
		}
		else
		{
			if (raiseEventOptions.CachingOption != 0)
			{
				opParameters[247] = (byte)raiseEventOptions.CachingOption;
			}
			if (raiseEventOptions.Receivers != 0)
			{
				opParameters[246] = (byte)raiseEventOptions.Receivers;
			}
			if (raiseEventOptions.InterestGroup != 0)
			{
				opParameters[240] = raiseEventOptions.InterestGroup;
			}
			if (raiseEventOptions.TargetActors != null)
			{
				opParameters[252] = raiseEventOptions.TargetActors;
			}
			if (raiseEventOptions.ForwardToWebhook)
			{
				opParameters[234] = true;
			}
		}
		SendOptions sendOptions = default(SendOptions);
		sendOptions.Reliability = sendReliable;
		sendOptions.Channel = raiseEventOptions.SequenceChannel;
		sendOptions.Encrypt = raiseEventOptions.Encrypt;
		SendOptions sendOptions2 = sendOptions;
		return SendOperation(253, opParameters, sendOptions2);
	}

	public virtual bool OpSettings(bool receiveLobbyStats)
	{
		if ((int)DebugOut >= 5)
		{
			base.Listener.DebugReturn(DebugLevel.ALL, "OpSettings()");
		}
		opParameters.Clear();
		if (receiveLobbyStats)
		{
			opParameters[0] = receiveLobbyStats;
		}
		if (opParameters.Count == 0)
		{
			return true;
		}
		return SendOperation(218, opParameters, SendOptions.SendReliable);
	}
}
