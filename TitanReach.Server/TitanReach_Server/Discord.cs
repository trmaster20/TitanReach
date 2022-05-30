using Discord.Webhook;
using System.Diagnostics;
using System.Threading.Tasks;

namespace TitanReach_Server
{
    class Discord
    {

        public static void Message(string s)
        {
            if (!Server.Instance.UseDiscord)
                return;
            Task.Run(() =>
            {
                using (var client = new DiscordWebhookClient("https://discordapp.com/api/webhooks/734327014303072366/HML1oN5JImwCbUAASyo7ny8ek0wZ3KbK4AsN6MBxRxDZ3f3hmVdppXeUR_gJaX8s5E5L"))
                {
                    client.SendMessageAsync(text: s);
                }
            });//https://discord.com/api/webhooks/803527629948452904/tt4fN7E8yuxisXUr4X5Kp1vrNcQ4RpfJy9JmYQCykkx2sd06vcFIpPmJFr624Ge_Q6XC

        }

        public static void MessageDB(string s)
        {
            //if (!Server.Instance.UseDiscord)
            //  return;
              if (Server.SERVER_LOCAL)
                return;
            
            s = ("[World " + Server.SERVER_World + " " + Server.Instance.LocationFlag + "]: " + s).Trim();
            Task.Run(() =>
            {
                using (var client = new DiscordWebhookClient("https://discord.com/api/webhooks/815117258987798528/9w6eZm-7CQAkGZ7u8UGPlKYQgAWACtLdL9Kf0cfiz4WHGcjYG844wW-x1Wu3rXKgaMiF"))
                {
                    client.SendMessageAsync(text: s);
                }
            });

        }

        public static void SendFileDB(string path, string s) {
            //if (Server.SERVER_LOCAL)
           //    return;

            s = ("[World " + Server.SERVER_World + " " + Server.Instance.LocationFlag + "]: " + s).Trim();
            Task.Run(() => {
                using (var client = new DiscordWebhookClient("https://discord.com/api/webhooks/815117258987798528/9w6eZm-7CQAkGZ7u8UGPlKYQgAWACtLdL9Kf0cfiz4WHGcjYG844wW-x1Wu3rXKgaMiF")) {
                    client.SendFileAsync(filePath: path, text: s);
                }
            });
        }

        public static void Chat(string s)
        {
            if (!Server.Instance.UseDiscord)
                return;
            Task.Run(() =>
            {
                using (var client = new DiscordWebhookClient("https://discord.com/api/webhooks/870940049603178527/l2j1sirk6EwsvcOMBp0g2dmA2sbVXee1dOhMv7s2SftkaWqe0YIBlx_n4aVCzp4ZLJbE"))
                {
                    client.SendMessageAsync(text: s);
                }
            });

        }
    }
}
