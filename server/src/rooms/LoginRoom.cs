﻿using shared;

namespace server
{
	/**
	 * The LoginRoom is the first room clients 'enter' until the client identifies himself with a PlayerJoinRequest. 
	 * If the client sends the wrong type of request, it will be kicked.
	 *
	 * A connected client that never sends anything will be stuck in here for life,
	 * unless the client disconnects (that will be detected in due time).
	 */ 
	class LoginRoom : SimpleRoom
	{
		//arbitrary max amount just to demo the concept
		private const int MAX_MEMBERS = 50;

		public LoginRoom(TCPGameServer pOwner) : base(pOwner)
		{ }

		protected override void addMember(TcpMessageChannel pMember)
		{
			base.addMember(pMember);

			//notify the client that (s)he is now in the login room, clients can wait for that before doing anything else
			RoomJoinedEvent roomJoinedEvent = new RoomJoinedEvent();
			roomJoinedEvent.room = RoomJoinedEvent.Room.LOGIN_ROOM;
			pMember.SendMessage(roomJoinedEvent);
		}

		protected override void handleNetworkMessage(ASerializable pMessage, TcpMessageChannel pSender)
		{
			if (pMessage is PlayerJoinRequest)
			{
				handlePlayerJoinRequest(pMessage as PlayerJoinRequest, pSender);
			}
			else //if member sends something else than a PlayerJoinRequest
			{
				Log.LogInfo("Declining client, auth request not understood", this);

				//don't provide info back to the member on what it is we expect, just close and remove
				removeAndCloseMember(pSender);
			}
		}

		/**
		 * Tell the client he is accepted and move the client to the lobby room.
		 */
		private void handlePlayerJoinRequest(PlayerJoinRequest pMessage, TcpMessageChannel pSender)
		{
			Log.LogInfo("Handling player join request, please hold...", this);

            //Check if sent name already exists
            bool isNameInUse;

            if (_server.GetPlayerInfo((playerInfo) => playerInfo.playerName == pMessage.name).Count > 0)
			{
				isNameInUse = true;
            }
            else
            {
                Log.LogInfo("Before adding PlayerInfo" + _server.GetPlayerInfo((playerInfo) => playerInfo.playerName == pMessage.name).Count, this);
                _server.GetPlayerInfo(pSender);
                _server.GetPlayerInfo(pSender).playerName = pMessage.name;
				_server.GetPlayerInfo(pSender).hasWonPreviousGame = false;
                Log.LogInfo("After adding PlayerInfo" + _server.GetPlayerInfo((playerInfo) => playerInfo.playerName == pMessage.name).Count, this);

                isNameInUse = false;
            }

            //If name doesn't exist yet, move to next state
            if (isNameInUse == false)
            {
				//Send response to client so Unity can go to next client-side state (login to lobby)
                PlayerJoinResponse playerJoinResponse = new PlayerJoinResponse();
                playerJoinResponse.result = PlayerJoinResponse.RequestResult.ACCEPTED;
                pSender.SendMessage(playerJoinResponse);

                //Server-side also go to next server-side state (login to lobby)
                removeMember(pSender);
                _server.GetLobbyRoom().AddMember(pSender);
            }
            //Else if name already exists, try again
            else
            {
                PlayerJoinResponse playerJoinResponse = new PlayerJoinResponse();
                playerJoinResponse.result = PlayerJoinResponse.RequestResult.DECLINED;
                pSender.SendMessage(playerJoinResponse);
            }
		}
	}
}