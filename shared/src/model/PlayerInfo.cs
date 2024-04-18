namespace shared
{
    /**
     * Empty placeholder class for the PlayerInfo object which is being tracked for each client by the server.
     * Add any data you want to store for the player here and make it extend ASerializable.
     */
    public class PlayerInfo : ASerializable
    {
        public string playerName;

        public override void Serialize(Packet pPacket)
        {
            pPacket.Write(playerName);
        }

        public override void Deserialize(Packet pPacket)
        {
            playerName = pPacket.ReadString();
        }
    }
}