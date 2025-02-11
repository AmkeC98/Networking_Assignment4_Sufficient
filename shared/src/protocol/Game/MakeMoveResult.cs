﻿namespace shared
{
    /**
     * Send from SERVER to all CLIENTS in response to a client's MakeMoveRequest
     */
    public class MakeMoveResult : ASerializable
    {
        public int whoMadeTheMove;
        public TicTacToeBoardData boardData;
        public bool isGameFinished;

        public override void Serialize(Packet pPacket)
        {
            pPacket.Write(whoMadeTheMove);
            pPacket.Write(boardData);
            pPacket.Write(isGameFinished);
        }

        public override void Deserialize(Packet pPacket)
        {
            whoMadeTheMove = pPacket.ReadInt();
            boardData = pPacket.Read<TicTacToeBoardData>();
            isGameFinished = pPacket.ReadBool();
        }
    }
}