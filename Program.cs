//token 7997922332:AAF_Xri6xLs_DGu4kcmlJO5NEg_SX1PQ1bw
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Extensions.Options;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.Passport;

internal class Program
{
    private const string botToken = "7997922332:AAF_Xri6xLs_DGu4kcmlJO5NEg_SX1PQ1bw";
    private static TelegramBotClient botClient = new TelegramBotClient(botToken);
    private static ApContext context = new ApContext();
    private static bool addflag = false;
      private static bool allflag = false;

    private static void Main(string[] args)
    {
        botClient.StartReceiving(HandleUpdateAsync, HandleErrorAsync);
        Console.ReadLine();
    }

    private static async Task HandleErrorAsync(
        ITelegramBotClient client,
        Exception exception,
        HandleErrorSource source,
        CancellationToken token
    )
    {
        System.Console.WriteLine(exception.Message);
    }

    private static async Task HandleUpdateAsync(
        ITelegramBotClient client,
        Update update,
        CancellationToken token
    )
    {
        if (update.Type != UpdateType.Message)
        {
            return;
        }
        long chatId = update.Message.Chat.Id;
        string messageText = update.Message.Text.Trim();
        if (messageText.StartsWith("/add"))
        {
            await client.SendMessage(chatId, "Пожалуйста, введите заметку");
            addflag = true;
        }
        else if (addflag == true)
        {


            context.Notes.Add(new Note { Message = messageText });
            context.SaveChanges();
            await client.SendMessage(chatId, $"Заметка, {messageText} успешна сохранена");
            addflag = false;

        }
        if (messageText.StartsWith("/all"))
        {
            List<Note> note = context.Notes.ToList();
            for (int i = 0; i < note.Count; i++)
            {
                await client.SendMessage(chatId, note[i].Message);
            }
            allflag = false;
        }
        if (messageText.StartsWith("/delete"))
        {
            
            List<Note> note = context.Notes.ToList();
            int a = 0;
            for (int i = 0; i < note.Count; i++)
            {
                await client.SendMessage(chatId, i + " " + note[i].Message);
                a++;
            }
            //smth
            for (int i = 0; i < note.Count; i++)
            {

            }
        }
        
        
    }
}

public class Note
{
    public int Id { get; set; }
    public string Message { get; set; }
}

public class ApContext : DbContext
{
    public DbSet<Note> Notes { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        string dbPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Notes.db");
        optionsBuilder.UseSqlite($"Data Source={dbPath}");
    }
}
