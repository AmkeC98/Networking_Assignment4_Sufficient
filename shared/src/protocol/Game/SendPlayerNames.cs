namespace shared
{
    /**
     * Send from SERVER to CLIENT to share the names of the players in game
     */
    public class SendPlayerNames : ASerializable
    {
        public string player1String;
        public string player2String;

        public override void Serialize(Packet pPacket)
        {
            pPacket.Write(player1String);
            pPacket.Write(player2String);
        }

        public override void Deserialize(Packet pPacket)
        {
            player1String = pPacket.ReadString();
            player2String = pPacket.ReadString();
        }
    }
}