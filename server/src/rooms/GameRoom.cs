using shared;
using System;

namespace server
{
	/**
	 * This room runs a single Game (at a time). 
	 * 
	 * The 'Game' is very simple at the moment:
	 *	- all client moves are broadcasted to all clients
	 *	
	 * The game has no end yet (that is up to you), in other words:
	 * all players that are added to this room, stay in here indefinitely.
	 */
	class GameRoom : Room
	{
		public bool IsGameInPlay { get; private set; }

		//wraps the board to play on...
		private TicTacToeBoard _board = new TicTacToeBoard();

<<<<<<< HEAD
		private string player1Name;
		private string player2Name;

		private List<TcpMessageChannel> _currentPlayers = new List<TcpMessageChannel>();

        public GameRoom(TCPGameServer pOwner) : base(pOwner)
=======
		public GameRoom(TCPGameServer pOwner) : base(pOwner)
>>>>>>> parent of f742ad3 (Game shows correct player names)
		{ }

		public void StartGame(TcpMessageChannel pPlayer1, TcpMessageChannel pPlayer2)
		{
			if (IsGameInPlay)
			{
				throw new Exception("Programmer error duuuude.");
			}

<<<<<<< HEAD
			player1Name = _server.GetPlayerInfo(pPlayer1).playerName;
			player2Name = _server.GetPlayerInfo(pPlayer2).playerName;

            _currentPlayers.Add(pPlayer1);
            _currentPlayers.Add(pPlayer2);

            IsGameInPlay = true;
=======
			IsGameInPlay = true;
>>>>>>> parent of f742ad3 (Game shows correct player names)
			addMember(pPlayer1);
			addMember(pPlayer2);
		}

		protected override void addMember(TcpMessageChannel pMember)
		{
			base.addMember(pMember);

			//notify client he has joined a game room 
			RoomJoinedEvent roomJoinedEvent = new RoomJoinedEvent();
			roomJoinedEvent.room = RoomJoinedEvent.Room.GAME_ROOM;
			pMember.SendMessage(roomJoinedEvent);
<<<<<<< HEAD

			//Send names of players to everyone
			SendPlayerNames sendPlayerNames = new SendPlayerNames();
			sendPlayerNames.player1String = player1Name;
			sendPlayerNames.player2String = player2Name;

			sendToAll(sendPlayerNames);
        }
=======
		}
>>>>>>> parent of f742ad3 (Game shows correct player names)

		public override void Update()
		{
			//demo of how we can tell people have left the game...
			int oldMemberCount = memberCount;
			base.Update();
			int newMemberCount = memberCount;

			if (oldMemberCount != newMemberCount)
			{
				Log.LogInfo("People left the game...", this);
			}
		}

		protected override void handleNetworkMessage(ASerializable pMessage, TcpMessageChannel pSender)
		{
			if (pMessage is MakeMoveRequest)
			{
				handleMakeMoveRequest(pMessage as MakeMoveRequest, pSender);
			}
		}

		private void handleMakeMoveRequest(MakeMoveRequest pMessage, TcpMessageChannel pSender)
		{
			//we have two players, so index of sender is 0 or 1, which means playerID becomes 1 or 2
			int playerID = indexOfMember(pSender) + 1;
			//make the requested move (0-8) on the board for the player
			_board.MakeMove(pMessage.move, playerID);

			//and send the result of the boardstate back to all clients
			MakeMoveResult makeMoveResult = new MakeMoveResult();
			makeMoveResult.whoMadeTheMove = playerID;
			makeMoveResult.boardData = _board.GetBoardData();
			makeMoveResult.isGameFinished = IsGameFinished();

            sendToAll(makeMoveResult);

			//If the game was finished, handle this as well
			if (IsGameFinished())
			{
				//Mark winning player as winner in PlayerInfo
				_server.GetPlayerInfo(pSender).hasWonPreviousGame = true; 

				//Remove each member from Game Room and send back to Lobby Room
				for (int i = 0; i < _currentPlayers.Count; i++)
				{
					_server.GetGameRoom().removeMember(_currentPlayers[i]);
					_server.GetLobbyRoom().AddMember(_currentPlayers[i]);
				}

				//Clear the list
				_currentPlayers.Clear();

				//Reset the room to be available
				this.IsGameInPlay = false;
				//Reset the gameboard data
				this._board.GetBoardData().board = new int[9] { 0, 0, 0, 0, 0, 0, 0, 0, 0 };

                Log.LogInfo("Game is over, gameroom available: " + this.IsGameInPlay.ToString(), this);
                Log.LogInfo("New Board Data: " + this._board.GetBoardData().ToString(), this);
            }
		}

		private bool IsGameFinished()
		{
			if (_board.GetBoardData().WhoHasWon() == 1 || _board.GetBoardData().WhoHasWon() == 2)
			{
				return true;
			}
            else
			{
				return false;
			}
		}
	}
}