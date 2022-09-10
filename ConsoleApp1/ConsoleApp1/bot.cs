using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

using System.Threading.Tasks;

using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Extensions;

using DSharpPlus.EventArgs;

using Newtonsoft.Json;

using ConsoleApp1.Commands;
using DSharpPlus.Entities;

using DSharpPlus.Net;
using DSharpPlus.Lavalink;



namespace ConsoleApp1
{
    [Serializable]
    public class UserInfo
    {
        public DiscordUser user;
        public string name;
        public long colonCoins;
        public int manners;
        public int[] items;
        public int playTokens;
    }

    public class Bot 
    {
        public DiscordClient Client { get; private set; }

        public InteractivityExtension Interactivity { get; private set; }

        public CommandsNextExtension Commands { get; private set; }

        //public VoiceNextExtension voice { get; private set; }

        public async Task RunAsync() // Start Bot
        {
            var json = string.Empty;

            using(var fs = File.OpenRead(Directory.GetCurrentDirectory() + "\\Jasons\\config.json"))
            using(var sr = new StreamReader(fs, new UTF8Encoding(false)))
                json = await sr.ReadToEndAsync().ConfigureAwait(false);

            var configJson = JsonConvert.DeserializeObject<ConfigJson>(json);

            var config = new DiscordConfiguration
            {
                Token = configJson.Token,
                TokenType = TokenType.Bot,
                AutoReconnect = true
                //LogLevel = LogLevel.Debug,
                //UseInternalLogHandler = true
            };

            Client = new DiscordClient(config);

            Client.Ready += OnClientReady;

            Client.UseInteractivity(new InteractivityConfiguration()
            {
                Timeout = TimeSpan.FromSeconds(15)
            });

            var commandsConfig = new CommandsNextConfiguration
            {
                StringPrefixes = new string[] { configJson.Prefix },
                EnableDms = false,
                EnableMentionPrefix = true,
                IgnoreExtraArguments = true,
                EnableDefaultHelp = false
            };

            Commands = Client.UseCommandsNext(commandsConfig);

            Commands.RegisterCommands<Commands1>();

            //voice = Client.UseVoiceNext();

            var endpoint = new ConnectionEndpoint
            {
                Hostname = "127.0.0.1", // From your server configuration.
                Port = 3001 // From your server configuration
            };

            var lavalinkConfig = new LavalinkConfiguration
            {
                Password = "youshallnotpass", // From your server configuration.
                RestEndpoint = endpoint,
                SocketEndpoint = endpoint
            };

            var lavalink = Client.UseLavalink();


            await Client.ConnectAsync();

            Load();

            PeriodicSave().GetAwaiter();
            ResetTokens().GetAwaiter();

            await lavalink.ConnectAsync(lavalinkConfig);

            await Task.Delay(-1);
        }

        private Task OnClientReady(object sender, ReadyEventArgs e) // When Bot is Started
        {
            return Task.CompletedTask;
        }

        public List<UserInfo> saveData = new List<UserInfo>();

        private void Load()
        {
            string path = Directory.GetCurrentDirectory() + "\\Jasons\\Save.json";
            string data = File.ReadAllText(path);
            
            saveData = JsonConvert.DeserializeObject<List<UserInfo>>(data);
        }

        public void Save()
        {
            string path = Directory.GetCurrentDirectory() + "\\Jasons\\Save.json";
            string data = JsonConvert.SerializeObject(saveData, Formatting.Indented);
            File.WriteAllText(path, data);
            Console.WriteLine("Saved");
        }

        private async Task PeriodicSave()
        {
            while (true)
            {
                await Task.Delay(600000);
                Save();
            }
        }

        private async Task ResetTokens()
        {
            void Reset()
            {
                foreach(var userInfo in saveData) userInfo.playTokens = 10;
                Save();
                Console.WriteLine("Play Tokens Reset");
            }

            TimeSpan timeUntilMidnight = DateTime.Today.AddDays(1) - DateTime.Now;

            await Task.Delay(timeUntilMidnight);
            Reset();

            while (true)
            {
                await Task.Delay(TimeSpan.FromDays(1));
                Reset();
            }
        }

    }   
}
