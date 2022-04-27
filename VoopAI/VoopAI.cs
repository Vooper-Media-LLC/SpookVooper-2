using System.Text.Json;
using System.Reflection;
using Valour.Net;

namespace SV2.VoopAI;
class VoopAI
{
    public static bool prod;
    public static List<string> prefixes;

    public static async Task Main()
    {
        ValourConfig valourConfig;
        if (File.Exists("./SV2Config/ValourConfig.json"))
        {
            // If there is a config, read it
            valourConfig = await JsonSerializer.DeserializeAsync<ValourConfig>(File.OpenRead("./SV2Config/ValourConfig.json"));
        }
        else
        {
            // Otherwise create a config with default values and write it to the location
            valourConfig = new ValourConfig()
            {
                Email = "",
                BotPassword = ""
            };

            File.WriteAllText("./SV2Config/ValourConfig.json", JsonSerializer.Serialize(valourConfig));
            Console.WriteLine("Error: No DB config was found. Creating file...");
        }
        //if (prod) LoadSVIDNameCache();

        ValourNetClient.AddPrefix("/");

        await ValourNetClient.Start(valourConfig.Email,valourConfig.BotPassword);

        //OnMessageRecieved += MessageHandler;

        //await Task.Delay(-1);
    }
}