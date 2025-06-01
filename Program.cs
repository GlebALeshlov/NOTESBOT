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
    private static int a = 0;
    private static bool deleteflag = false;
    private static bool editFlag = false;
    private static int noteToEditIndex = -1;
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
     CancellationToken token)
    {
        if (update.Type != UpdateType.Message)
        {
            return;
        }

        long chatId = update.Message.Chat.Id;
        string messageText = update.Message.Text.Trim();

        if (messageText.StartsWith("/help"))
        {
            string helpText = @"Функционал бота :
/add - добавить новую заметку
/all - показать все заметки
/delete - удалить заметку
/edit - редактировать заметку
/help - показать это сообщение";

            await client.SendMessage(chatId, helpText);
            return;


            if (messageText.StartsWith("/add"))
            {
                await client.SendMessage(chatId, "Пожалуйста, введите заметку");
                addflag = true;
                deleteflag = false;
                editFlag = false;
            }
            else if (addflag)
            {
                context.Notes.Add(new Note { Message = messageText });
                context.SaveChanges();
                await client.SendMessage(chatId, $"Заметка \"{messageText}\" успешно сохранена");
                addflag = false;
            }

            if (messageText.StartsWith("/all"))
            {
                List<Note> notes = context.Notes.OrderBy(n => n.Id).ToList();

                if (notes.Count == 0)
                {
                    await client.SendMessage(chatId, "У вас пока нет заметок");
                }
                else
                {
                    foreach (var note in notes)
                    {
                        await client.SendMessage(chatId, $"{note.Message}");
                    }
                }
            }


            if (messageText.StartsWith("/delete"))
            {
                List<Note> notes = context.Notes.OrderBy(n => n.Id).ToList();

                if (notes.Count == 0)
                {
                    await client.SendMessage(chatId, "У вас пока нет заметок для удаления");
                    return;
                }

                string response = "Выберите номер заметки для удаления:\n";
                for (int i = 0; i < notes.Count; i++)
                {
                    response += $"{i + 1}. {notes[i].Message}\n";
                }

                await client.SendMessage(chatId, response);
                deleteflag = true;
                addflag = false;
                editFlag = false;
            }
            else if (deleteflag)
            {
                List<Note> notes = context.Notes.OrderBy(n => n.Id).ToList();

                if (int.TryParse(messageText, out int noteNumber) &&
                    noteNumber > 0 && noteNumber <= notes.Count)
                {
                    var noteToDelete = notes[noteNumber - 1];
                    await client.SendMessage(chatId, $"Заметка \"{noteToDelete.Message}\" успешно удалена!");
                    context.Notes.Remove(noteToDelete);
                    context.SaveChanges();
                }
                else
                {
                    await client.SendMessage(chatId, "Неверный номер заметки");
                }

                deleteflag = false;
            }

            if (messageText.StartsWith("/edit"))
            {
                List<Note> notes = context.Notes.OrderBy(n => n.Id).ToList();

                if (notes.Count == 0)
                {
                    await client.SendMessage(chatId, "У вас пока нет заметок для редактирования");
                    return;
                }

                string response = "Введите номер заметки, которую вы хотите отредактировать:\n";
                for (int i = 0; i < notes.Count; i++)
                {
                    response += $"{i + 1}. {notes[i].Message}\n";
                }

                await client.SendMessage(chatId, response);
                editFlag = true;
                addflag = false;
                deleteflag = false;
            }
            else if (editFlag && noteToEditIndex == -1)
            {
                List<Note> notes = context.Notes.OrderBy(n => n.Id).ToList();

                if (int.TryParse(messageText, out int noteNumber) &&
                    noteNumber > 0 && noteNumber <= notes.Count)
                {
                    noteToEditIndex = noteNumber - 1;
                    await client.SendMessage(chatId, $"Введите новый текст для заметки с номером {noteNumber}");
                }
                else
                {
                    await client.SendMessage(chatId, "Неверный номер заметки");
                    editFlag = false;
                }
            }
            else if (editFlag && noteToEditIndex != -1)
            {
                List<Note> notes = context.Notes.OrderBy(n => n.Id).ToList();

                if (noteToEditIndex >= 0 && noteToEditIndex < notes.Count)
                {
                    var noteToEdit = notes[noteToEditIndex];
                    string oldMessage = noteToEdit.Message;
                    noteToEdit.Message = messageText;
                    context.SaveChanges();

                    await client.SendMessage(chatId, $"Заметка изменена:\nБыло: {oldMessage}\nСтало: {messageText}");
                }

                editFlag = false;
                noteToEditIndex = -1;
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
