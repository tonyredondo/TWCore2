using System;
using System.Threading.Tasks;
using TWCore.Bot;
using TWCore.Bot.Slack;
using TWCore.Serialization;
using TWCore.Services;

// ReSharper disable UnusedMember.Global

namespace TWCore.Tests
{
    /// <inheritdoc />
    public class SlackBotTest : ContainerParameterServiceAsync
    {
        public SlackBotTest() : base("slackbot", "Slack Bot Test")
        {
        }

        protected override async Task OnHandlerAsync(ParameterHandlerInfo info)
        {
            Core.DebugMode = true;
            Core.Log.Warning("Slack Bot Test");

            Core.Log.InfoBasic("Please enter the Bot Token:");
            var token = Console.ReadLine();

            var slackTransport = new SlackBotTransport(token);
            var botEngine = new BotEngine(slackTransport);
            
            botEngine.Commands.Add(msg => msg.StartsWith("Hello"), (bot, message) =>
            {
                Core.Log.Warning(message.SerializeToJson());

                bot.SendTextMessageAsync(message.Chat, "Message Received");
                
                return true;
            });

            botEngine.Commands.Add(msg => msg.StartsWith("Hola"), (bot, message) =>
            {
                Core.Log.Warning(message.SerializeToJson());

                bot.SendTextMessageAsync(message.Chat, "Hola que tal?");

                return true;
            });


            botEngine.Commands.Add(msg => msg.StartsWith(":track"), (bot, message) =>
            {
                Core.Log.Warning(message.SerializeToJson());

                bot.TrackChat(message.Chat);
                bot.SendTextMessageAsync(message.Chat, "Chat tracked.");

                return true;
            });

            botEngine.Commands.Add(msg => msg.StartsWith(":untrack"), (bot, message) =>
            {
                Core.Log.Warning(message.SerializeToJson());

                bot.UnTrackChat(message.Chat);
                bot.SendTextMessageAsync(message.Chat, "Chat untracked.");

                return true;
            });

            botEngine.Commands.Add(msg => msg.StartsWith(":hello"), (bot, message) =>
            {
                Core.Log.Warning(message.SerializeToJson());

                bot.SendTextMessageToTrackedChatsAsync("Hola todos.");

                return true;
            });

            Core.Log.Warning("Connecting...");
            await botEngine.StartListenerAsync().ConfigureAwait(false);

            Core.Log.Warning("Connected.");

            Console.ReadLine();
        }
    }
}