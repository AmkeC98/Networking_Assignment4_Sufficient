using shared;
using System.Collections.Generic;

namespace server
{
	/**
	 * The LobbyRoom is a little bit more extensive than the LoginRoom.
	 * In this room clients change their 'ready status'.
	 * If enough people are ready, they are automatically moved to the GameRoom to play a Game (assuming a game is not already in play).
	 */ 
	class LobbyRoom : SimpleRoom
	{
		//this list keeps tracks of which players are ready to play a game, this is a subset of the people in this room
		private List<TcpMessageChannel> _readyMembers = new List<TcpMessageChannel>();

		public LobbyRoom(TCPGameServer pOwner) : base(pOwner)
		{ }

		protected override void addMember(TcpMessageChannel pMember)
		{
			base.addMember(pMember);

			string winningMessage = "";
			if (_server.GetPlayerInfo(pMember).hasWonPreviousGame == true)
			{
				winningMessage = " , our winner, ";

            }

			//tell the member it has joined the lobby
			RoomJoinedEvent roomJoinedEvent = new RoomJoinedEvent();
			roomJoinedEvent.room = RoomJoinedEvent.Room.LOBBY_ROOM;
			pMember.SendMessage(roomJoinedEvent);

			//print some info in the lobby (can be made more applicable to the current member that joined)
			ChatMessage simpleMessage = new ChatMessage();
			simpleMessage.message = _server.GetPlayerInfo(pMember).playerName + winningMessage + " has joined the lobby! Say hello to them.";
			sendToAll(simpleMessage);

			//send information to all clients that the lobby count has changed
			sendLobbyUpdateCount();
		}

		/**
		 * Override removeMember so that our ready count and lobby count is updated (and sent to all clients)
		 * anytime we remove a member.
		 */
		protected override void removeMember(TcpMessageChannel pMember)
		{
			base.removeMember(pMember);
			_readyMembers.Remove(pMember);

			sendLobbyUpdateCount();
		}

		protected override void handleNetworkMessage(ASerializable pMessage, TcpMessageChannel pSender)
		{
			if (pMessage is ChangeReadyStatusRequest)
			{
				handleReadyNotification(pMessage as ChangeReadyStatusRequest, pSender);
			}
			else if (pMessage is ChatMessage)
			{
				handleChatMessages(pMessage as ChatMessage, pSender);
            }
		}

		private void handleReadyNotification(ChangeReadyStatusRequest pReadyNotification, TcpMessageChannel pSender)
		{
			//if the given client was not marked as ready yet, mark the client as ready
			if (pReadyNotification.ready)
			{
				if (!_readyMembers.Contains(pSender))
				{
					_readyMembers.Add(pSender);
				}
			}
			else //if the client is no longer ready, unmark it as ready
			{
				_readyMembers.Remove(pSender);
			}

			//do we have enough people for a game and is there no game running yet?
			if (_readyMembers.Count >= 2 /*&& !_server.GetGameRoom().IsGameInPlay*/)
			{
                Log.LogInfo("Member Count is 2 or more, starting to check if room is available", this);

                bool isGameroomAvailable = false;
				GameRoom availableGameroom = new GameRoom(_server);

                //Check for next available game room
                foreach (GameRoom gameRoom in _server.GetGameRooms())
				{
					if (gameRoom.IsGameInPlay == false)
					{
                        Log.LogInfo("Found an available existing GameRoom, choosing this one", this);

                        isGameroomAvailable = true;
						availableGameroom = gameRoom;
                        break;
                    }
                }

				if (isGameroomAvailable == false)
				{
                    Log.LogInfo("Didn't find an available existing GameRoom, creating a new one and adding to the list", this);

                    _server.GetGameRooms().Add(availableGameroom);
				}

                TcpMessageChannel player1 = _readyMembers[0];
                TcpMessageChannel player2 = _readyMembers[1];
                removeMember(player1);
                removeMember(player2);
                availableGameroom.StartGame(player1, player2);

                //Reset the win-state of the players when entering a game
                _server.GetPlayerInfo(player1).hasWonPreviousGame = false;
                _server.GetPlayerInfo(player2).hasWonPreviousGame = false;
            }

            //(un)ready-ing / starting a game changes the lobby/ready count so send out an update
            //to all clients still in the lobby
            sendLobbyUpdateCount();
		}

		private void handleChatMessages(ChatMessage pMessage, TcpMessageChannel pSender)
		{
            Log.LogInfo("Handling incoming chat message, received: ", this);
            Log.LogInfo(pMessage.message, this);

			//Make sure the correct sender's name is next to the message
			string userString = _server.GetPlayerInfo(pSender).playerName + " typed: \n";
			pMessage.message = userString + pMessage.message;

            sendToAll(pMessage);

        }

		private void sendLobbyUpdateCount()
		{
			LobbyInfoUpdate lobbyInfoMessage = new LobbyInfoUpdate();
			lobbyInfoMessage.memberCount = memberCount;
			lobbyInfoMessage.readyCount = _readyMembers.Count;
			sendToAll(lobbyInfoMessage);
		}
	}
}