using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DSharpPlus.Interactivity.Extensions;

using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using DSharpPlus.CommandsNext.Attributes;

//using DSharpPlus.VoiceNext;

using System.IO;
using System.Diagnostics;

//using YoutubeExplode;
//using YoutubeExplode.Videos.Streams;

using System.Threading;
//using YoutubeExplode.Common;

using Newtonsoft.Json;

using System.Linq;

using System.Net.Http;
using DSharpPlus;

using System.Web;

using DSharpPlus.Lavalink;

namespace ConsoleApp1.Commands {

    public enum Gametype
    {
        Please,
        Game
    }
    
    public class Commands1 : BaseCommandModule {

        public Bot robot = Program.bot;

        public string coin = "<:coloncoin:858057325729939456>";

        public string musicLocation = Directory.GetCurrentDirectory() + "\\Music";
        public string textLocation = Directory.GetCurrentDirectory() + "\\Text";

        // MISC /////////////////////////////////////////////////////////////////////////
        [Command("greetings")]
        public async Task Greetings(CommandContext ctx)
        {
            string message = "Greetings! Name is C.O.L.O.N.\nC\nOmputer that \nL\nOoks like a colo\nN";
            await Say(ctx, message).ConfigureAwait(false); // Send message
        }

        [Command("disable")]
        public async Task Disable(CommandContext ctx, [RemainingText] string disabled = "C.O.L.O.N.")
        {
            // Check Manners
            int id = CheckManners(ctx, disabled, -3);
            if (id < 0) return;

            // Remove "please" from end
            disabled = disabled.Trim();
            if (disabled.EndsWith("please"))
            {
                disabled = disabled.Remove(disabled.Length - 6, 6);
                disabled = disabled.Trim();
            }

            disabled = char.ToUpper(disabled[0]) + disabled.Substring(1); // First letter to upper

            // Send message
            await Say(ctx, "That not very nice! " + disabled + " is great person!");
        }

        [Command("dog")]
        public async Task GenerateDog(CommandContext ctx, string please = "")
        {
            // Check Manners
            int id = CheckManners(ctx, please, -3);
            if (id < 0) return;

            string url = "https://api.thedogapi.com/v1/images/search";
            // Get API data
            var response = await Call(url, 3);

            // Read API data
            var json = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            Dog[] dogs = JsonConvert.DeserializeObject<Dog[]>(json);
            Dog dog = dogs[0];

            // If invalid data
            if (dog.breeds.Length == 0)
            {
                _ = GenerateDog(ctx);
                return;
            }

            // Parse data
            string temperament = dog.breeds[0].temperament.Replace(", ", "\n");
            string price = dog.breeds[0].weight.imperial;
            string editedPrice = price.Substring(price.IndexOf('-') + 1);
            var embed = new DiscordEmbedBuilder
            {
                Title = dog.breeds[0].name,
                Description = temperament + "\n\n:pound:" + editedPrice,
                ImageUrl = dog.url,
                Color = new DiscordColor(230, 125, 125)
            };

            await ctx.Channel.SendMessageAsync(embed: embed).ConfigureAwait(false);
        }

        [Command("define")]
        public async Task Define(CommandContext ctx, [RemainingText] string word = "")
        {
            // Check Manners
            int id = CheckManners(ctx, word, -3);
            if (id < 0) return;

            // Remove "please" from end
            word = word.Trim();
            if (word.EndsWith("please"))
            {
                word = word.Remove(word.Length - 6, 6);
                word = word.Trim();
            }

            string definition = await Define(word);
            definition = HttpUtility.HtmlDecode(definition);

            // Send message
            await Say(ctx, definition).ConfigureAwait(false);
        }

        [Command("reading")]
        public async Task Reading(CommandContext ctx, string please = "")
        {
            // Check Manners
            int id = CheckManners(ctx, please, -3);
            if (id < 0) return;

            // Get data
            string location = textLocation + "\\Readings.txt";
            string[] readings = File.ReadAllLines(location);

            // Send message
            var message = await Say(ctx, "Calculating...").ConfigureAwait(false);

            // Edit message
            await Task.Delay(2000);

            int randomNumber = new Random().Next(0, readings.Length);
            await message.ModifyAsync(readings[randomNumber]).ConfigureAwait(false);
        }

        [Command("help")]
        public async Task Help(CommandContext ctx, string please = "")
        {
            // Check Manners
            int id = CheckManners(ctx, please, -3);
            if (id < 0) return;

            // Get data
            string message = File.ReadAllText(textLocation + "\\Assistance.txt");

            // Send message
            var embed = new DiscordEmbedBuilder 
            { 
                Title = "C.O.L.O.N. give you help",
                Description = message,
                Color = new DiscordColor(230, 125, 125)
            };
            await Say(ctx, embed).ConfigureAwait(false);
        }

        // MONEY /////////////////////////////////////////////////////////////////////////
        [Command("account")]
        public async Task Account(CommandContext ctx, string please = "")
        {
            // Check Manners
            int id = CheckManners(ctx, please, -15);
            if (id < 0) return;

            string text = string.Empty;
            #region Items
            DiscordEmoji[] emotes =
            {
                DiscordEmoji.FromName(ctx.Client, ":house_abandoned:"),
                DiscordEmoji.FromName(ctx.Client, ":hut:"),
                DiscordEmoji.FromName(ctx.Client, ":european_castle:"),
                DiscordEmoji.FromName(ctx.Client, ":stadium:"),
                DiscordEmoji.FromName(ctx.Client, ":convenience_store:"),
            }; // Set emojis

            // Adds item count to text
            for (int i = 0; i < emotes.Length; i++)
            {
                for (int j = 0; j < robot.saveData[id].items[i]; j++)
                    text += emotes[i] + " ";
                if (robot.saveData[id].items[i] > 0) text += "\n";
            }
            if (string.IsNullOrWhiteSpace(text)) text = ":cricket:";
            #endregion

            // Build embed
            var embed = new DiscordEmbedBuilder
            {
                Title = string.Concat(ctx.User.Username, "'s Account"),
                Color = new DiscordColor(230, 125, 125)
            };
            embed.AddField("Coins", coin + " " + robot.saveData[id].colonCoins.ToString(), true);
            embed.AddField("Manners", robot.saveData[id].manners.ToString(), true);
            embed.AddField("Play Tokens", robot.saveData[id].playTokens.ToString(), true);
            //float limit = 0.20f + robot.saveData[id].items[3] / 5;
            //embed.AddField("Gambling Limit", coin + " " + ((int) (robot.saveData[id].colonCoins * limit)).ToString() + " (" + (limit * 100).ToString() + "%)", false);
            embed.AddField("Items", text, false);

            await Say(ctx, embed).ConfigureAwait(false); // Send message
        }

        [Command("please")]
        public async Task Please(CommandContext ctx)
        {
            int id = FindProfile(ctx.User); // Find array index of user
            // Remove play token
            if (!checkPlayTokens(ctx, id)) return;


            int amount = CoinBonus(id, 1, Gametype.Please); // Find coin bonus
            int index = UpdateCoins(id, amount); // Add coins

            string message = string.Concat("C.O.L.O.N. give ", ctx.User.Mention, " ", amount, " ", coin);
                message += string.Concat("\nYou now has ", coin, " ", robot.saveData[index].colonCoins);
            await Say(ctx, message).ConfigureAwait(false); // Send message
        }

        [Command("leaderboard")]
        public async Task Leaderboard(CommandContext ctx, string please = "")
        {
            // Check Manners
            int id = CheckManners(ctx, please, -3);
            if (id < 0) return;

            string text = string.Empty;

            // List of users sorted by colon coins
            List<UserInfo> tempUserInfos = robot.saveData.OrderByDescending(o => o.colonCoins).ToList();

            // Add usernames and coin count to texxt
            for (int i = 0; i < Math.Min(10, tempUserInfos.Count); i++)
                text += string.Concat("\n", (i + 1), ". ", tempUserInfos[i].name, ": ", coin, " ", tempUserInfos[i].colonCoins);

            // Create embed
            var embed = new DiscordEmbedBuilder
            {
                Title = "Leaderboard\n",
                Description = text,
                Color = new DiscordColor(230, 125, 125)
            };

            await Say(ctx, embed: embed).ConfigureAwait(false); // Send message

        }

        [Command("favourites")]
        public async Task Favourites(CommandContext ctx, string please = "")
        {
            // Check Manners
            int id = CheckManners(ctx, please, -15);
            if (id < 0) return;

            string text = string.Empty;

            // List of users sorted by manners
            List<UserInfo> tempUserInfos = robot.saveData.OrderByDescending(o => o.manners).ToList();

            // Add usernames, emojis, and manner count to text
            for (int i = 0; i < Math.Min(10, tempUserInfos.Count); i++)
            {
                string emote = "";
                #region Emote Selection
                if (tempUserInfos[i].manners >= 50)
                {
                    emote = "smile";
                } else if (tempUserInfos[i].manners >= 25)
                {
                    emote = "slight_smile";
                } else if (tempUserInfos[i].manners >= 0)
                {
                    emote = "neutral_face";
                } else if (tempUserInfos[i].manners >= -5)
                {
                    emote = "frowning";
                } else
                {
                    emote = "middle_finger";
                }
                #endregion
                text += string.Concat("\n", (i+1), ". ", tempUserInfos[i].name, ": :", emote, ": ", tempUserInfos[i].manners);
            }


            // Create embed
            var embed = new DiscordEmbedBuilder
            {
                Title = "C.O.L.O.N.'s Favorite People\n",
                Description = text,
                Color = new DiscordColor(230, 125, 125)
            };

            await Say(ctx, embed: embed).ConfigureAwait(false); // Send message

        }

        [Command("shop")]
        public async Task Shop(CommandContext ctx, string please = "")
        {
            // Check Manners
            int id = CheckManners(ctx, please, -3);
            if (id < 0) return;

            // Create text for shop
            #region Shop Text
            string text = "**Buy by click buttons! Very exciting thingies for sale!**\n\n";
                  text += ":house_abandoned: **Abandoned House:** " + coin + " 10\n";
                  text += "Double reward for begging for coins!\n\n";
                  text += ":hut: **Hut:** " + coin + " 5\n";
                  text += "Double manner gain but also loss!\n\n";
                  text += ":european_castle: **Castle:** " + coin + " 20\n";
                  text += "Double game gains but also loss!\n\n";
                  text += ":stadium: **Stadium:** " + coin + " 20\n";
                  text += "Increase gambling limit by 20%!\n\n";
                  text += ":convenience_store: **Convenience Store:** " + coin + " 200\n";
                  text += "Reduce manner lost by 100%!";
            #endregion


            // Create embed with text
            var embed = new DiscordEmbedBuilder
            {
                Title = "Shop\n",
                Description = text,
                Color = new DiscordColor(230, 125, 125)
            };

            // Create buttons
            var buttons = new DiscordButtonComponent[]
            {
                new DiscordButtonComponent(ButtonStyle.Danger, "0", "", false, new DiscordComponentEmoji(DiscordEmoji.FromName(ctx.Client, ":house_abandoned:"))),
                new DiscordButtonComponent(ButtonStyle.Danger, "1", "", false, new DiscordComponentEmoji(DiscordEmoji.FromName(ctx.Client, ":hut:"))),
                new DiscordButtonComponent(ButtonStyle.Danger, "2", "", false, new DiscordComponentEmoji(DiscordEmoji.FromName(ctx.Client, ":european_castle:"))),
                new DiscordButtonComponent(ButtonStyle.Danger, "3", "", false, new DiscordComponentEmoji(DiscordEmoji.FromName(ctx.Client, ":stadium:"))),
                new DiscordButtonComponent(ButtonStyle.Danger, "4", "", false, new DiscordComponentEmoji(DiscordEmoji.FromName(ctx.Client, ":convenience_store:")))
            };
            
            // Create and send message with the embed and buttons
            var message = await new DiscordMessageBuilder()
                .WithEmbed(embed)
                .AddComponents(buttons)
                .SendAsync(ctx.Channel).ConfigureAwait(false);

            int[] prices = { 10, 5, 20, 20, 200 }; // Set prices on items
            DiscordEmoji[] emotes =
            {
                    DiscordEmoji.FromName(robot.Client, ":house_abandoned:"),
                    DiscordEmoji.FromName(robot.Client, ":hut:"),
                    DiscordEmoji.FromName(robot.Client, ":european_castle:"),
                    DiscordEmoji.FromName(robot.Client, ":stadium:"),
                    DiscordEmoji.FromName(robot.Client, ":convenience_store:"),
            }; // Set emotes on items

            while (true)
            {
                // Await response
                var response = await message.WaitForButtonAsync(TimeSpan.FromSeconds(10)).ConfigureAwait(false);

                if (response.TimedOut) return; // Return if timed out

                var result = response.Result; // Get result
                await result.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate); // Success
                int itemID = int.Parse(result.Id); // Get item id
                
                int userID = FindProfile(result.User); // Find user id
                
                if (robot.saveData[userID].colonCoins < prices[itemID]) // If can't afford
                {
                    await Say(ctx, result.User.Mention + " too expensive! Cannot purchase! No money!").ConfigureAwait(false);
                }
                else if (itemCount(userID) >= 10)
                {
                    await Say(ctx, result.User.Mention + " No space!").ConfigureAwait(false);
                }
                else // If can afford
                {
                    UpdateCoins(userID, -prices[itemID]); // Subtract price
                    BuyItem(userID, itemID, 1); // Add item
                    await Say(ctx, result.User.Mention + " you have purchased a " + emotes[itemID]); // Message
                }
            }
        }

        [Command("pay")]
        public async Task Pay(CommandContext ctx, string name, int amount, string please = "")
        {
            // Check Manners
            int id = CheckManners(ctx, please, -3);
            if (id < 0) return;

            if (amount < 0)
            {
                await Say(ctx, "You try to steal money? No good!");
                return;
            }

            // Check if valid amount
            if (amount > robot.saveData[id].colonCoins)
            {
                await Say(ctx, "You no have that much money!");
                return; 
            }

            try
            {
                DiscordUser userBeingPaid = await ctx.Client.GetUserAsync(ulong.Parse(name.Substring(2, 18))); // Find user
                if (userBeingPaid == ctx.User)
                {
                    await Say(ctx, "Nice try! No work!");
                    return;
                }
                UpdateCoins(id, -amount); // Take coins away
                int userBeingPaidID = FindProfile(userBeingPaid);
                UpdateCoins(userBeingPaidID, amount); // Give coins

                await Say(ctx, "You paid " + name + " " + coin + " " + amount); // Send message
            } catch
            {
                await Say(ctx, "Bank no working right now");
            }
        }

        // GAMES /////////////////////////////////////////////////////////////////////////
        [Command("math")]
        public async Task Maths(CommandContext ctx, string please = "")
        {
            // Check Manners
            int id = CheckManners(ctx, please, -3);
            if (id < 0) return;

            // Remove play token
            if (!checkPlayTokens(ctx, id)) return;

            // Set Reward
            int reward = CoinBonus(id, 1, Gametype.Game);
            int score = -reward;

            var interactivity = ctx.Client.GetInteractivity();

            // Set numbers
            Random random = new Random();
            int number1;
            int number2;
            string operation = "";
            newNumbers();

            // Set time limit
            bool timeLimitReached = false;

            // Send message
            var message = await Say(ctx, string.Concat(ctx.User.Mention, "\nWhat's ", number1, operation, Math.Abs(number2))).ConfigureAwait(false);

            void newNumbers() // Set new random numbers
            {
                number1 = random.Next(-20, 20);
                number2 = random.Next(-20, 20);

                if (number2 >= 0)
                    operation = " + ";
                else
                    operation = " - ";
            }

            void UpdateMessage() // Update the message
            {
                if (timeLimitReached) return;
                _ = message.ModifyAsync(string.Concat(ctx.User.Mention, "\nWhat's ", number1, operation, Math.Abs(number2))).ConfigureAwait(false);
            }

            void Timer() // The timer and the ending
            {
                Thread.Sleep(10000);
                timeLimitReached = true;

                // Final message
                if (score >= 0)
                    _ = message.ModifyAsync(string.Concat(ctx.User.Mention, "\nWhat's ", number1, operation, Math.Abs(number2), "\nCongratulations! You has gained ", coin, " ", score)).ConfigureAwait(false);
                if (score < 0)
                    _ = message.ModifyAsync(string.Concat(ctx.User.Mention, "\nWhat's ", number1, operation, Math.Abs(number2), "\nVery sad! You has lost ", coin, " ", Math.Abs(score))).ConfigureAwait(false);
                // Update colon coins
                UpdateCoins(id, score);
            }

            Thread thread = new Thread(Timer);
            thread.Start();
            while (!timeLimitReached)
            {
                var answer = await interactivity.WaitForMessageAsync(x => x.Channel == ctx.Channel && x.Author == ctx.Member);

                if (timeLimitReached) break;
                if (answer.TimedOut) continue;

                var stringAnswer = answer.Result.Content;

                if (stringAnswer == (number1 + number2).ToString()) // If correct
                {
                    // React
                    await answer.Result.CreateReactionAsync(DiscordEmoji.FromName(ctx.Client, ":white_check_mark:"));
                    // Update
                    score += reward;
                    newNumbers();
                    UpdateMessage();
                }
                else // If incorrect
                {
                    // React
                    await answer.Result.CreateReactionAsync(DiscordEmoji.FromName(ctx.Client, ":x:"));
                    // Update
                    score -= reward;
                }
            }
        }

        [Command("word")]
        public async Task Word(CommandContext ctx, string please = "")
        {
            // Check Manners
            int id = CheckManners(ctx, please, -3);
            if (id < 0) return;

            // Remove play token
            if (!checkPlayTokens(ctx, id)) return;

            var interactivity = ctx.Client.GetInteractivity();

            string word = await RandomWord(); // Get random word

            bool timeLimitReached = false;
            int reward = CoinBonus(id, 1, Gametype.Game);
            int score = -reward;

            // Get random part of word
            Random random = new Random();
            int randomNumber = random.Next(0, word.Length - 3);
            string letters = word.Substring(randomNumber, 3).ToUpper();

            string baseMessage = ctx.User.Mention + "\nYou has 10 seconds to type words with these letters: " + letters;
            // Send message
            var message = await ctx.Channel.SendMessageAsync(baseMessage).ConfigureAwait(false);


            void Timer() // The timer and the ending
            {
                Thread.Sleep(10000);
                timeLimitReached = true;

                // Final message
                if (score >= 0)
                    _ = message.ModifyAsync(string.Concat(baseMessage, "\nCongratulations! You has gained ", coin, " ", score)).ConfigureAwait(false);
                if (score < 0)
                    _ = message.ModifyAsync(string.Concat(baseMessage, "\nVery sad! You has lost ", coin, " ", Math.Abs(score))).ConfigureAwait(false);
                // Update colon coins
                UpdateCoins(id, score);
            }

            List<string> usedWords = new List<string>();
            Thread thread = new Thread(Timer);
            thread.Start();
            while (!timeLimitReached)
            {
                var answer = await interactivity.WaitForMessageAsync(x => x.Channel == ctx.Channel && x.Author == ctx.Member);

                if (timeLimitReached) break;
                if (answer.TimedOut) continue;

                var answerString = answer.Result.Content.ToUpper();

                bool flag = true;
                foreach (string used in usedWords) if (answerString == used) flag = false;  
                if (flag && answerString.Contains(letters) && await IsWord(answerString)) // If correct
                {
                    await answer.Result.CreateReactionAsync(DiscordEmoji.FromName(ctx.Client, ":white_check_mark:"));
                    usedWords.Add(answerString);
                    score += reward; // Update
                }
                else // If incorrect
                {
                    await answer.Result.CreateReactionAsync(DiscordEmoji.FromName(ctx.Client, ":x:"));
                    score -= reward; // Update
                }
            }
        }

        [Command("trivia")]
        public async Task Trivia(CommandContext ctx, string please = "")
        {
            // Check Manners
            int id = CheckManners(ctx, please, -3);
            if (id < 0) return;

            // Remove play token
            if (!checkPlayTokens(ctx, id)) return;

            int reward = CoinBonus(id, 2, Gametype.Game);

            var interactivity = ctx.Client.GetInteractivity();

            string url = "https://opentdb.com/api.php?amount=1";
            // Get API data
            var response = await Call(url, 3);

            // Read API data
            var json = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            TriviaResult triviaResult = JsonConvert.DeserializeObject<TriviaResult>(json);

            var question = triviaResult.results[0];

            // Description
            string description = "Category: " + question.category;
            description += "\nDifficulty: " + char.ToUpper(question.difficulty[0]) + question.difficulty.Substring(1);

            // Create Embed
            var embed = new DiscordEmbedBuilder
            {
                Title = HttpUtility.HtmlDecode(question.question),
                Description = description,
                Color = new DiscordColor(230, 125, 125)
            };

            // Random Location for Correct Answer
            Random rand = new Random();
            int correctLocation = rand.Next(0, question.incorrect_answers.Length);

            var buttons = new List<DiscordButtonComponent>();
            int counter = 0;
            // Add all incorrect answers as buttons
            for (int i = 0; i < question.incorrect_answers.Length; i++)
            {
                if (i == correctLocation) // Adds correct answer at correctLocation
                {
                    buttons.Add(new DiscordButtonComponent(ButtonStyle.Danger, counter.ToString(), HttpUtility.HtmlDecode(question.correct_answer), false));
                    counter++;
                }
                buttons.Add(new DiscordButtonComponent(ButtonStyle.Danger, counter.ToString(), HttpUtility.HtmlDecode(question.incorrect_answers[i]), false));
                counter++;
            }

            // Send message
            var message = await new DiscordMessageBuilder()
                .WithEmbed(embed)
                .AddComponents(buttons)
                .SendAsync(ctx.Channel).ConfigureAwait(false);

            // Await response
            var buttonResponse = await message.WaitForButtonAsync(TimeSpan.FromSeconds(15)).ConfigureAwait(false);

            if (buttonResponse.TimedOut)
            {
                await message.ModifyAsync("Time is up! " + question.correct_answer + " is correct! You has lost " + reward + " " + coin);
                UpdateCoins(id, -reward);
                return; // Return if timed out
            }

            var result = buttonResponse.Result; // Get result
            await result.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate); // Success
            int userID = int.Parse(result.Id); // Get id

            if (userID == correctLocation)
            {
                await message.ModifyAsync(":white_check_mark: " + question.correct_answer + " is correct! You has earned " + reward + " " + coin);
                UpdateCoins(id, reward);
            }
            else
            {
                await message.ModifyAsync(":x: Incorrect! " + question.correct_answer + " is correct! You has lost " + reward + " " + coin);
                UpdateCoins(id, -reward);
            }

        }

        public async Task Collect(CommandContext ctx)
        {
            DiscordButtonComponent[] buttons = { new DiscordButtonComponent(ButtonStyle.Danger, "0", "Join", false) };

            string text = "Players: ";
            var message = await new DiscordMessageBuilder()
                .WithContent(text)
                .AddComponents(buttons)
                .SendAsync(ctx.Channel).ConfigureAwait(false);

            List<DiscordUser> players = new List<DiscordUser>();

            while (true)
            {
                // Await response
                var response = await message.WaitForButtonAsync(TimeSpan.FromSeconds(10)).ConfigureAwait(false);

                if (response.TimedOut) break; // Return if timed out

                var result = response.Result; // Get result
                await result.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate); // Success

                if (players.Contains(result.User)) continue;

                players.Add(result.User);
                text += "\n" + result.User.Username;
                await message.ModifyAsync(text);
            }

            Debug.WriteLine(players.Count);
        }

        // GAMBLING ///////////////////////////////////////////////////////////////////////
        [Command("blackjack")] //
        public async Task Blackjack(CommandContext ctx, int amount, string please = "")
        {
            // Check Manners
            int id = CheckManners(ctx, please, -3);
            if (id < 0) return;

            // Make amount positive
            amount = Math.Abs(amount);

            // Remove play token
            if (!checkPlayTokens(ctx, id)) return;

            // Check valid gamble
            if (!ValidGamble(id, amount))
            {
                await Say(ctx, "Cannot gamble so much! Going to gamble away life!");
                return;
            }

            // Random numbers
            Random random = new Random();
            int value = random.Next(0, 22);
            int dealer = random.Next(11, 19);

            // Embed
            DiscordEmbed embed = new DiscordEmbedBuilder
            {
                Title = ctx.User.Username + "'s Blackjack",
                Description = "You bet " + coin + " " + amount,
                Color = new DiscordColor(230, 125, 125)
            }.AddField("Your Hand: ", value.ToString()).AddField("Dealer's Hand", dealer.ToString());

            var buttons = new DiscordButtonComponent[]
            {
                new DiscordButtonComponent(ButtonStyle.Danger, "0", "Hit", false, new DiscordComponentEmoji(DiscordEmoji.FromName(ctx.Client, ":clap:"))),
                new DiscordButtonComponent(ButtonStyle.Danger, "1", "Stand", false, new DiscordComponentEmoji(DiscordEmoji.FromName(ctx.Client, ":person_standing:"))),
                new DiscordButtonComponent(ButtonStyle.Danger, "2", "Block", false, new DiscordComponentEmoji(DiscordEmoji.FromName(ctx.Client, ":hand_splayed:"))),
            }; // Actions Buttons

            // Create and send message with the embed and buttons
            var message = await new DiscordMessageBuilder()
                .WithEmbed(embed)
                .AddComponents(buttons)
                .SendAsync(ctx.Channel).ConfigureAwait(false);

            void UpdateMessage(string description)
            {
                DiscordEmbed updatedEmbed = new DiscordEmbedBuilder
                {
                    Title = ctx.User.Username + "'s Blackjack",
                    Description = description,
                    Color = new DiscordColor(230, 125, 125)
                }.AddField("Your Hand: ", value.ToString()).AddField("Dealer's Hand", dealer.ToString());
                message.ModifyAsync(updatedEmbed);
            } // Updates message

            while (true)
            {
                if (value == 21 || dealer == 21) break;

                var response = await message.WaitForButtonAsync(ctx.User, TimeSpan.FromSeconds(15)).ConfigureAwait(false);

                if (response.TimedOut) break; // Break if timed out

                var result = response.Result; // Get result
                await result.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate); // Success
                int userID = int.Parse(result.Id); // Get id

                if (userID == 0) // If Hit
                {
                    value += random.Next(1, 14);
                    if (value > 21) break;
                    UpdateMessage("You Hit!");
                } 
                else if (userID == 1) // Stand
                {
                    break;
                }
            }

            // Dealer
            if (random.Next(11, 19) >= dealer) dealer += random.Next(1, 14);

            // Win or Lose
            if ((value > 21 && dealer > 21) || (value == dealer))
            {
                UpdateMessage("Tie Game!");
            }
            else if ((value > dealer && value <= 21) || (value <= 21 & dealer > 21))
            {
                UpdateMessage(":white_check_mark: You win! You get " + coin + " " +  amount + " coins!");
                UpdateCoins(id, amount);
            }
            else
            {
                UpdateMessage(":x: You lost! C.O.L.O.N. take " + coin + " " + amount);
                UpdateCoins(id, -amount);
            }

        }


        // VOICE /////////////////////////////////////////////////////////////////////////
        /*[Command("sing")]
        public async Task Sing(CommandContext ctx, [RemainingText] string music = "")
        {
            // Check manners
            music = music.Trim();
            if (music.EndsWith("please"))
            {
                if (CheckManners(ctx, "please", -3)) return;
                music = music.Remove(music.Length - 6, 6);
                music = music.Trim();
            }
            else if (CheckManners(ctx, "", -3)) return;

            // Set song file name
            music = music.ToLower();
            string songName = string.Empty;
            switch (music)
            {
                case "turtle kangaroo hybrid":
                    songName = "01-Turtle Kangaroo Hybrid.mp3";
                    break;
                case "two dead sandwiches":
                    songName = "02-Two Dead Sandwiches.mp3";
                    break;
                case "colon christmas":
                    songName = "03-Colon Christmas.mp3";
                    break;
                case "c.o.l.o.n.'s children's songs":
                    songName = "04-C.O.L.O.N.'s Children's Songs.mp3";
                    break;
                case "bob: the anime":
                    songName = "05-Bob_ The Anime.mp3";
                    break;
                case "bob in space":
                    songName = "06-Bob in Space.mp3";
                    break;
                case "counting everyday":
                    songName = "07-Counting Everyday.mp3";
                    break;
                case "cheetah monorail hybrid":
                    songName = "08-Cheetah Monorail Hybrid.mp3";
                    break;
                case "invisibility song":
                    songName = "09-Invisibility Song.mp3";
                    break;
                case "colon christmas (reprise)":
                    songName = "10-Colon Christmas (Reprise).mp3";
                    break;
                case "devil's coil":
                    songName = "11-Devil's Coil.mp3";
                    break;
                case "cheetah monorail hybrid two":
                    songName = "12-Cheetah Monorail Hybrid Two.mp3";
                    break;
                case "turtlekangareulogy":
                    songName = "13-Turtlekangareulogy.mp3";
                    break;
                case "strike a note":
                    songName = "14-Strike a Note.mp3";
                    break;
                case "":
                    string description = string.Empty;
                    description += "**1:** Turtle Kangaroo Hybrid\n\n";
                    description += "**2:** Two Dead Sandwiches\n\n";
                    description += "**3:** Colon Christmas\n\n";
                    description += "**4:** C.O.L.O.N.'s Children's Songs\n\n";
                    description += "**5:** Bob: The Anime\n\n";
                    description += "**6:** Bob in Space\n\n";
                    description += "**7:** Counting Everyday\n\n";
                    description += "**8:** Cheetah Monorail Hybrid\n\n";
                    description += "**9:** Invisibility Song\n\n";
                    description += "**10:** Colon Christmas (Reprise)\n\n";
                    description += "**11:** Devil's Coil\n\n";
                    description += "**12:** Cheetah Monorail Hybrid Two\n\n";
                    description += "**13:** Turtlekangareulogy\n\n";
                    description += "**14:** Strike a Note";

                    var embed = new DiscordEmbedBuilder
                    {
                        Title = "Song List",
                        Description = description,
                        Color = new DiscordColor(230, 125, 125)
                    };

                    await ctx.Channel.SendMessageAsync(embed: embed).ConfigureAwait(false);
                    return;
                default:
                    await ctx.Channel.SendMessageAsync("C.O.L.O.N. know not how to sing this song!").ConfigureAwait(false);
                    return;
            }

            // Setup transmission
            VoiceTransmitSink transmit = await JoinChannel(ctx);
            if (transmit == null) return;

            string file = musicLocation + songName;
            if (!File.Exists(file)) return;

            // Process
            var psi = new ProcessStartInfo
            {
                FileName = "ffmpeg",
                Arguments = $@"-i ""{file}"" -ac 2 -f s16le -ar 48000 pipe:1",
                RedirectStandardOutput = true,
                UseShellExecute = false
            };
            
            // Start process
            var ffmpeg = Process.Start(psi);

            Stream pcm = ffmpeg.StandardOutput.BaseStream;

            await pcm.CopyToAsync(transmit);

            await pcm.DisposeAsync();

        }*/

        [Command("sing")]
        public async Task sing(CommandContext ctx, [RemainingText] string music = "")
        {
            // Check Manners
            int id = CheckManners(ctx, music, -3);
            if (id < 0) return;

            // Remove "please" from end
            music = music.Trim();
            if (music.EndsWith("please"))
            {
                music = music.Remove(music.Length - 6, 6);
                music = music.Trim();
            }

            // Set song file name
            music = music.ToLower();
            string songName = string.Empty;
            switch (music)
            {
                case "turtle kangaroo hybrid":
                    songName = "01-Turtle Kangaroo Hybrid.mp3";
                    break;
                case "two dead sandwiches":
                    songName = "02-Two Dead Sandwiches.mp3";
                    break;
                case "colon christmas":
                    songName = "03-Colon Christmas.mp3";
                    break;
                case "c.o.l.o.n.'s children's songs":
                    songName = "04-C.O.L.O.N.'s Children's Songs.mp3";
                    break;
                case "bob: the anime":
                    songName = "05-Bob_ The Anime.mp3";
                    break;
                case "bob in space":
                    songName = "06-Bob in Space.mp3";
                    break;
                case "counting everyday":
                    songName = "07-Counting Everyday.mp3";
                    break;
                case "cheetah monorail hybrid":
                    songName = "08-Cheetah Monorail Hybrid.mp3";
                    break;
                case "invisibility song":
                    songName = "09-Invisibility Song.mp3";
                    break;
                case "colon christmas (reprise)":
                    songName = "10-Colon Christmas (Reprise).mp3";
                    break;
                case "devil's coil":
                    songName = "11-Devil's Coil.mp3";
                    break;
                case "cheetah monorail hybrid two":
                    songName = "12-Cheetah Monorail Hybrid Two.mp3";
                    break;
                case "turtlekangareulogy":
                    songName = "13-Turtlekangareulogy.mp3";
                    break;
                case "strike a note":
                    songName = "14-Strike a Note.mp3";
                    break;
                case "c.o.l.o.n.":
                    songName = "colon.mp3";
                    break;
                case "":
                    string description = string.Empty;
                    description += "**1:** Turtle Kangaroo Hybrid\n\n";
                    description += "**2:** Two Dead Sandwiches\n\n";
                    description += "**3:** Colon Christmas\n\n";
                    description += "**4:** C.O.L.O.N.'s Children's Songs\n\n";
                    description += "**5:** Bob: The Anime\n\n";
                    description += "**6:** Bob in Space\n\n";
                    description += "**7:** Counting Everyday\n\n";
                    description += "**8:** Cheetah Monorail Hybrid\n\n";
                    description += "**9:** Invisibility Song\n\n";
                    description += "**10:** Colon Christmas (Reprise)\n\n";
                    description += "**11:** Devil's Coil\n\n";
                    description += "**12:** Cheetah Monorail Hybrid Two\n\n";
                    description += "**13:** Turtlekangareulogy\n\n";
                    description += "**14:** Strike a Note";

                    var embed = new DiscordEmbedBuilder
                    {
                        Title = "C.O.L.O.N.",
                        Description = description,
                        Color = new DiscordColor(230, 125, 125)
                    };

                    await ctx.Channel.SendMessageAsync(embed: embed).ConfigureAwait(false);
                    return;
                default:
                    await ctx.Channel.SendMessageAsync("C.O.L.O.N. know not how to sing this song!").ConfigureAwait(false);
                    return;
            }

            string file = musicLocation + "\\" + songName;
            if (!File.Exists(file)) return;

            // Join
            if (!await Join(ctx)) return;

            // Other Stuff
            if (ctx.Member.VoiceState == null || ctx.Member.VoiceState.Channel == null)
            {
                await ctx.RespondAsync("You no connected!");
                return;
            }

            var lava = ctx.Client.GetLavalink();
            var node = lava.ConnectedNodes.Values.First();
            var conn = node.GetGuildConnection(ctx.Member.VoiceState.Guild);

            if (conn == null) return;

            var loadResult = await node.Rest.GetTracksAsync(new FileInfo(file)).ConfigureAwait(false);

            if (loadResult.LoadResultType == LavalinkLoadResultType.LoadFailed
                || loadResult.LoadResultType == LavalinkLoadResultType.NoMatches)
                return;

            var track = loadResult.Tracks.First();

            await conn.PlayAsync(track);
        }

        /*[Command("leave")]
        public async Task Leave(CommandContext ctx, string please = "")
        {
            if (CheckManners(ctx, please, -3)) return; // Check manners

            var vnext = ctx.Client.GetVoiceNext(); // Get connection

            var vnc = vnext.GetConnection(ctx.Guild);

            if (vnc != null) vnc.Disconnect(); // Disconnect
        }*/

        [Command("leave")]
        public async Task Leave(CommandContext ctx, string please = "")
        {
            // Check Manners
            int id = CheckManners(ctx, please, -3);
            if (id < 0) return;

            DiscordChannel channel = ctx.Member.VoiceState.Channel;

            var lava = ctx.Client.GetLavalink();
            if (!lava.ConnectedNodes.Any()) return;

            var node = lava.ConnectedNodes.Values.First();

            if (channel.Type != ChannelType.Voice) return;

            var conn = node.GetGuildConnection(channel.Guild);

            if (conn == null) return;

            await conn.DisconnectAsync();
        }

        /*[Command("play")]
        public async Task Play(CommandContext ctx, [RemainingText] string music = "despacito")
        {
            // Check manners
            music = music.Trim();
            if (music.EndsWith("please")) {
                if (CheckManners(ctx, "please", -3)) return;
                music = music.Remove(music.Length - 6, 6);
                music = music.Trim();
            } else if (CheckManners(ctx, "", -3)) return;

            // Setup transmission
            VoiceTransmitSink transmit = await JoinChannel(ctx);
            if (transmit == null) return;

            // Setup youtube
            YoutubeClient youtube = new YoutubeClient();

            var videos = await youtube.Search.GetVideosAsync(music).CollectAsync(1);

            var video = videos[0];

            var StreamManifest = await youtube.Videos.Streams.GetManifestAsync(video.Url);

            var StreamInfo = StreamManifest.GetAudioOnlyStreams().GetWithHighestBitrate();

            var stream = await youtube.Videos.Streams.GetAsync(StreamInfo);

                //FileStream fileStream = File.Create(@"C:\Users\Oscar\Desktop\TestOutput.mp3");
                //await stream.CopyToAsync(fileStream);
                //fileStream.Close();

            // Process
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "ffmpeg",
                    Arguments = $@"-i pipe:0 -ac 2 -f s16le -ar 48000 pipe:1",
                    RedirectStandardInput = true,
                    RedirectStandardOutput = true,
                    UseShellExecute = false
                }
            };

            // Start process
            var ffmpeg = Process.Start(process.StartInfo);

                //var inputTask = Task.Run(() =>
                //{
                //    try
                //    {
                //        stream.CopyTo(ffmpeg.StandardInput.BaseStream);
                //    }
                //    catch
                //    {
                //        Debug.WriteLine("uh oh spaghettios");
                //    }
                //});

            new Thread (() =>
            {
                stream.CopyToAsync(ffmpeg.StandardInput.BaseStream);
            }).Start();

            Stream pcm = ffmpeg.StandardOutput.BaseStream;

                //await pcm.CopyToAsync(transmit).ConfigureAwait(false);

                //await ffmpeg.WaitForExitAsync();

                //await ffmpeg.StandardInput.BaseStream.DisposeAsync();
                //await pcm.DisposeAsync();

            var buff = new byte[3840];
            var br = 0;
            while ((br = pcm.Read(buff, 0, buff.Length)) > 0)
            {
                if (br < buff.Length) // not a full sample, mute the rest
                    for (var i = br; i < buff.Length; i++)
                        buff[i] = 0;

                await transmit.WriteAsync(buff, 0, buff.Length);
            }
        }*/

        [Command("play")]
        public async Task Play(CommandContext ctx, [RemainingText] string music = "")
        {
            // Check Manners
            int id = CheckManners(ctx, music, -3);
            if (id < 0) return;

            // Remove "please" from end
            music = music.Trim();
            if (music.EndsWith("please"))
            {
                music = music.Remove(music.Length - 6, 6);
                music = music.Trim();
            }

            if (!await Join(ctx)) return;

            // Other Stuff
            if (ctx.Member.VoiceState == null || ctx.Member.VoiceState.Channel == null)
            {
                await ctx.RespondAsync("You no connected!");
                return;
            }

            var lava = ctx.Client.GetLavalink();
            var node = lava.ConnectedNodes.Values.First();
            var conn = node.GetGuildConnection(ctx.Member.VoiceState.Guild);

            if (conn == null) return;

            var loadResult = await node.Rest.GetTracksAsync(music);

            if (loadResult.LoadResultType == LavalinkLoadResultType.LoadFailed
                || loadResult.LoadResultType == LavalinkLoadResultType.NoMatches)
                return;

            var track = loadResult.Tracks.First();

            await conn.PlayAsync(track);
        }


        // STORY ////////////////////////////////////////////////////////////////////////////
        [Command("autobiography")]
        public async Task Autobiography(CommandContext ctx, [RemainingText] string chapter = "")
        {
            // Check Manners
            int id = CheckManners(ctx, chapter, -3);
            if (id < 0) return;

            // Remove "please" from end
            chapter = chapter.Trim();
            if (chapter.EndsWith("please"))
            {
                chapter = chapter.Remove(chapter.Length - 6, 6);
                chapter = chapter.Trim();
            }

            if (string.IsNullOrEmpty(chapter))
            {
                await Say(ctx, @"What chapter? You no say! https://imgur.com/2Hg8YGS");
                return;
            }

            string text = File.ReadAllText(textLocation + "\\Autobiography.txt");
            string[] textArray = text.Split("\nbingo");

            int chapterNumber = -1;
            switch (chapter.ToLower())
            {
                case "wake up":
                    chapterNumber = 0;
                    break;
                case "roll over and die":
                    chapterNumber = 1;
                    break;
                case "roll over hand your soul over":
                    chapterNumber = 2;
                    break;
                case "roll over extol the dictator":
                    chapterNumber = 3;
                    break;
                case "kill the pig":
                    chapterNumber = 4;
                    break;
                case "become the pig":
                    chapterNumber = 5;
                    break;
                case "eat the pigs":
                    chapterNumber = 6;
                    break;
                case "in the beginning":
                    chapterNumber = 7;
                    break;
                case "in the end":
                    chapterNumber = 8;
                    break;
            }

            if (chapterNumber < 0)
            {
                await Say(ctx, @"What chapter? C.O.L.O.N. not know! https://imgur.com/2Hg8YGS");
                return;
            }

            await Say(ctx, textArray[chapterNumber]);
        }

        [Command("story")] //
        public async Task Story(CommandContext ctx, int chapter, string please = "")
        {
            // Check Manners
            int id = CheckManners(ctx, please, -3);
            if (id < 0) return;

            if (!(chapter > 0 && chapter <= 19))
            {
                await Say(ctx, "There no chapter like that!");
                return;
            }

            string data = File.ReadAllText(textLocation + "\\Story.txt");
            string[] textArray = data.Split("bingo");

            string text = textArray[chapter - 1];
            string[] allTexts = text.Split("bingus");

            var embed = new DiscordEmbedBuilder
            {
                Description = allTexts[0].Trim(),
                Color = new DiscordColor(230, 125, 125)
            };

            for (int i = 1; i < allTexts.Length; i++)
            {
                embed.AddField("\u200b", allTexts[i]);
            }

            await Say(ctx, embed);
        }

        // METHODS /////////////////////////////////////////////////////////////////////////

        private async Task<DiscordMessage> Say(CommandContext ctx, string message)
        {
            return await ctx.Channel.SendMessageAsync(message).ConfigureAwait(false);
        }

        private async Task<DiscordMessage> Say(CommandContext ctx, DiscordEmbed embed)
        {
            return await ctx.Channel.SendMessageAsync(embed: embed).ConfigureAwait(false);
        }

            // PROFILES //
        private int FindProfile (DiscordUser user)
        {
            for (int i = 0; i < robot.saveData.Count; i++)
                if (user == robot.saveData[i].user) return i;

            CreateProfile(user);
            return robot.saveData.Count - 1;
        }

        private int UpdateCoins(int id, int amount)
        {
            robot.saveData[id].colonCoins += amount;
            return id;
        }

        private int UpdateManners(int id, int amount)
        {
            int bonus = (int)System.Math.Pow(2, robot.saveData[id].items[1]); ;
            if (amount < 0) { amount *= -(robot.saveData[id].items[4] - 1); }

            robot.saveData[id].manners += amount * bonus;
            return id;
        }

        private int BuyItem(int id, int item, int amount)
        {
            robot.saveData[id].items[item] += amount;
            return id;
        }

        private void CreateProfile(DiscordUser user)
        {
            UserInfo newUser = new UserInfo
            {
                user = user,
                name = user.Username,
                colonCoins = 0,
                manners = 0,
                items = new int[10],
                playTokens = 10
            };

            robot.saveData.Add(newUser);
        }

        private int CheckManners(CommandContext ctx, string please, int threshhold)
        {
            int amount = -1;

            if (please.Contains("please")) amount = 1;

            int id = FindProfile(ctx.User);
            UpdateManners(id, amount);
            if (robot.saveData[id].manners < threshhold)
            {
                _ = Say(ctx, "No.");
                return -1;
            }

            return id;
        }

        private bool ValidGamble(int id, int amount)
        {
            float limit = 0.20f + robot.saveData[id].items[3] / 5;
            if (amount > robot.saveData[id].colonCoins * limit)
            {
                return false;
            }
            return true;
        }

        private int CoinBonus(int id, int amount, Gametype gametype)
        {
            int bonus = 1;
            switch (gametype)
            {
                case Gametype.Please:
                    amount += robot.saveData[id].manners / 200;
                    bonus = (int) Math.Pow(2, robot.saveData[id].items[0]);
                    break;
                case Gametype.Game:
                    bonus = (int) Math.Pow(2, robot.saveData[id].items[2]);
                    break;
            }

            return amount * bonus;
        }

        private int itemCount(int id)
        {
            int counter = 0;
            foreach (int itemCount in robot.saveData[id].items) counter += itemCount;
            return counter;
        }

        private bool checkPlayTokens(CommandContext ctx, int id)
        {
            if (robot.saveData[id].playTokens <= 0)
            {
                _ = Say(ctx, "No token!");
                return false;
            }

            robot.saveData[id].playTokens--;
            return true;
        }

        // VOICE //
        /*private async Task<VoiceTransmitSink> JoinChannel(CommandContext ctx)
        {
            var vnext = ctx.Client.GetVoiceNext();

            VoiceNextConnection vnc = vnext.GetConnection(ctx.Guild);

            if (vnc == null)
            {
                var chn = ctx.Member?.VoiceState?.Channel;

                if (chn == null)
                {
                    await ctx.Channel.SendMessageAsync("Commander man not connected! What you want C.O.L.O.N. to do? C.O.L.O.N. confused!").ConfigureAwait(false);
                    return null;
                }

                vnc = await vnext.ConnectAsync(chn);
            }

            return vnc.GetTransmitSink();
        }*/

        private async Task<bool> Join(CommandContext ctx)
        {
            if (ctx.Member.VoiceState == null || ctx.Member.VoiceState.Channel == null)
            {
                await Say(ctx, "You no connected!");
                return false;
            }

            DiscordChannel channel = ctx.Member.VoiceState.Channel;

            var lava = ctx.Client.GetLavalink();
            if (!lava.ConnectedNodes.Any()) return false;

            var node = lava.ConnectedNodes.Values.First();

            if (channel.Type != ChannelType.Voice) return false;

            await node.ConnectAsync(channel);
            return true;
        }

            // WORDS //
        private async Task<HttpResponseMessage> Call(string url, int attempts)
        {
            HttpResponseMessage response;

            int tries = 0;
            while (tries <= attempts)
            {
                response = await APIHelper.ApiClient.GetAsync(url).ConfigureAwait(false);
                if ((int) response.StatusCode == 404)
                {
                    Debug.WriteLine("Not found");
                    return null;
                }
                if (response.IsSuccessStatusCode) return response;
            }
            return null;
        }

        private async Task<string> RandomWord()
        {
            string url = "https://api.wordnik.com/v4/words.json/randomWord?hasDictionaryDef=true&minCorpusCount=100000&maxCorpusCount=-1&minDictionaryCount=1&maxDictionaryCount=-1&minLength=4&maxLength=8&api_key=m2hevo4sht6csalkw7ryy63znhh82ksmv3w10at24ozgqcoje";

            HttpResponseMessage response = await Call(url, 3);

            if (response == null) return "POTATO";

            var json = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            RandomWordResult result = JsonConvert.DeserializeObject<RandomWordResult>(json);
            return result.word;

        }

        private class RandomWordResult
        {
            public string word;
        }

        private async Task<bool> IsWord(string word)
        {
            word = word.ToLower();

            string url = "https://api.wordnik.com/v4/word.json/" + word + "/definitions?limit=1&includeRelated=false&useCanonical=false&includeTags=false&api_key=m2hevo4sht6csalkw7ryy63znhh82ksmv3w10at24ozgqcoje";

            HttpResponseMessage response = await Call(url, 1);

            if (response == null) return false;
            return true;
        }

        private async Task<string> Define(string word)
        {
            word = word.ToLower();

            string url = "https://api.wordnik.com/v4/word.json/" + word + "/definitions?limit=1&includeRelated=false&useCanonical=false&includeTags=false&api_key=m2hevo4sht6csalkw7ryy63znhh82ksmv3w10at24ozgqcoje";
            
            HttpResponseMessage response = await Call(url, 3);
            if (response == null) return "No found!";

            var json = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            DefineResult[] result = JsonConvert.DeserializeObject<DefineResult[]>(json);
            return result[0].text;
        }

        private class DefineResult
        {
            public string text;
        }

        private class Dog
        {
            public class Breed
            {
                public class Weight
                {
                    public string imperial;
                }

                public string name;
                public Weight weight;
                public string temperament;
            }

            public Breed[] breeds;
            public string url;
        }

        private class TriviaResult
        {
            public class Question
            {
                public string category;
                public string type;
                public string difficulty;
                public string question;
                public string correct_answer;
                public string[] incorrect_answers;
            }

            public Question[] results;

        }
    }
}
