namespace shared
{
    /**
     * Send from SERVER to CLIENT
     */
    public class SendGameEndResult : ASerializable
    {
        public int winnerID;

        public override void Serialize(Packet pPacket)
        {
            pPacket.Write(winnerID);
        }

        public override void Deserialize(Packet pPacket)
        {
            winnerID = pPacket.ReadInt();
        }
    }
}