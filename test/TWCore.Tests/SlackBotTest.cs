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
            
            botEngine.Commands.Add(msg => msg.StartsWith("Hello"), async (bot, message) =>
            {
                Core.Log.Warning(message.SerializeToJson());

                await bot.SendTextMessageAsync(message.Chat, "Message Received").ConfigureAwait(false);
                
                return true;
            });

            botEngine.Commands.Add(msg => msg.StartsWith("Hola"), async (bot, message) =>
            {
                Core.Log.Warning(message.SerializeToJson());

                await bot.SendTextMessageAsync(message.Chat, "Hola que tal?").ConfigureAwait(false);

                return true;
            });


            botEngine.Commands.Add(msg => msg.StartsWith(":track"), async (bot, message) =>
            {
                Core.Log.Warning(message.SerializeToJson());

                bot.TrackChat(message.Chat);
                await bot.SendTextMessageAsync(message.Chat, "Chat tracked.").ConfigureAwait(false);

                return true;
            });

            botEngine.Commands.Add(msg => msg.StartsWith(":untrack"), async (bot, message) =>
            {
                Core.Log.Warning(message.SerializeToJson());

                bot.UnTrackChat(message.Chat);
                await bot.SendTextMessageAsync(message.Chat, "Chat untracked.").ConfigureAwait(false);

                return true;
            });

            botEngine.Commands.Add(msg => msg.StartsWith(":hello"), async (bot, message) =>
            {
                Core.Log.Warning(message.SerializeToJson());

                await bot.SendTextMessageToTrackedChatsAsync("Hola todos.").ConfigureAwait(false);

                return true;
            });

            Core.Log.Warning("Connecting...");
            await botEngine.StartListenerAsync().ConfigureAwait(false);

            Core.Log.Warning("Connected.");

            Console.ReadLine();
        }
    }
}