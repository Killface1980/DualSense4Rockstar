using System.Net;
using Newtonsoft.Json;

namespace Shared
{
    public static class Triggers
    {
        public static IPAddress localhost = new IPAddress(new byte[4] { 127, 0, 0, 1 });

        public static string PacketToJson(Packet packet)
        {
            return JsonConvert.SerializeObject(packet);
        }

        public static Packet JsonToPacket(string json)
        {
            return JsonConvert.DeserializeObject<Packet>(json);
        }
    }
}
