using Microsoft.Data.SqlClient;
using System;
using Telegram.Bot.Types.ReplyMarkups;
using Telegram.Bot.Types;
using Telegram.Bot;
using Lemur.Server.Data;
using Microsoft.EntityFrameworkCore;
using Lemur.Server.Models.ModelsTelegram;
using System.Text.RegularExpressions;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
using Telegram.Bot.Types.Enums;
using System.Threading;
using System.Net;

namespace Lemur.Server.Services.ServicesTelegram;

public class TelegramBotService
{
      private readonly string _botToken;
    private static IServiceScopeFactory _scopeFactory;
    private readonly ITelegramBotClient _botClient;
    private static readonly Random _random = new Random();

    public TelegramBotService(string botToken, IServiceScopeFactory scopeFactory, bool isProxy = false)
    {
        _botToken = botToken;
        _scopeFactory = scopeFactory;

        var proxy = new WebProxy("http://winproxy.server.lan:3128", true)
        {
            UseDefaultCredentials = false, // Imposta su true se il proxy richiede autenticazione
        };

        HttpClient httpClient = new HttpClient(new HttpClientHandler
        {
            Proxy = proxy,
            UseProxy = true,
        });


        if(isProxy)
        {
            _botClient = new TelegramBotClient(_botToken, httpClient);
        }
        else
        {
            _botClient = new TelegramBotClient(_botToken);
        }   


        InitializeCommands().Wait();
    }


    private async Task InitializeCommands()
    {

        using var scope = _scopeFactory.CreateScope();
        var _context = scope.ServiceProvider.GetRequiredService<LemurDbContext>();

        var operazioniDefault = await _context.Permissions
            .Where(op => op.IsDefault == true).ToListAsync();

        if (operazioniDefault == null || operazioniDefault.Count == 0)
        {
            return;
        }

        var commands = new List<BotCommand>();

        foreach (var operazione in operazioniDefault)
        {
            commands.Add(new BotCommand
            {
                Command = $"{operazione.Command}", // Assumi che 'Command' sia il campo del comando
                Description = operazione.Description ?? "" // Assumi che 'Description' sia il campo della descrizione
            });
        }

        await _botClient.SetMyCommands(commands);
    }


    public async Task ClearCommandsAsync()
    {
        await _botClient.DeleteMyCommands();
    }

    // Metodo per inviare messaggi al bot
    public async Task<bool> SendMessageAsync(long chatId, string message, string attachmentFilePath = null)
    {
        try
        {

            var msg = await _botClient.SendMessage(chatId, message, Telegram.Bot.Types.Enums.ParseMode.Html);

            if (msg == null)
            {
                return false; // Messaggio non inviato correttamente
            }


            if (!string.IsNullOrEmpty(attachmentFilePath) && System.IO.File.Exists(attachmentFilePath))
            {
                string fileExtension = Path.GetExtension(attachmentFilePath).ToLower();

                // Se è un file PDF
                if (fileExtension == ".pdf")
                {
                    using (var fileStream = new FileStream(attachmentFilePath, FileMode.Open))
                    {
                        await _botClient.SendDocument(chatId, Telegram.Bot.Types.InputFile.FromStream(fileStream, Path.GetFileName(attachmentFilePath)));
                    }
                    return true;
                }

                // Se è un'immagine (jpg, jpeg, png, gif, bmp)
                if (fileExtension == ".jpg" || fileExtension == ".jpeg" || fileExtension == ".png" || fileExtension == ".gif" || fileExtension == ".bmp")
                {
                    using (var fileStream = new FileStream(attachmentFilePath, FileMode.Open))
                    {
                        await _botClient.SendPhoto(chatId, Telegram.Bot.Types.InputFile.FromStream(fileStream, Path.GetFileName(attachmentFilePath)));
                    }
                    return true;
                }

                // Se è un file Word (doc, docx)
                if (fileExtension == ".doc" || fileExtension == ".docx")
                {
                    using (var fileStream = new FileStream(attachmentFilePath, FileMode.Open))
                    {
                        await _botClient.SendDocument(chatId, Telegram.Bot.Types.InputFile.FromStream(fileStream, Path.GetFileName(attachmentFilePath)));
                    }
                    return true;
                }

                // Se è un file Excel (xls, xlsx)
                if (fileExtension == ".xls" || fileExtension == ".xlsx")
                {
                    using (var fileStream = new FileStream(attachmentFilePath, FileMode.Open))
                    {
                        await _botClient.SendDocument(chatId, Telegram.Bot.Types.InputFile.FromStream(fileStream, Path.GetFileName(attachmentFilePath)));
                    }
                    return true;
                }

                // Se è un file di testo (txt)
                if (fileExtension == ".txt")
                {
                    using (var fileStream = new FileStream(attachmentFilePath, FileMode.Open))
                    {
                        await _botClient.SendDocument(chatId, Telegram.Bot.Types.InputFile.FromStream(fileStream, Path.GetFileName(attachmentFilePath)));
                    }
                    return true;
                }

                // Se è un file audio (mp3, wav, m4a)
                if (fileExtension == ".mp3" || fileExtension == ".wav" || fileExtension == ".m4a")
                {
                    using (var fileStream = new FileStream(attachmentFilePath, FileMode.Open))
                    {
                        await _botClient.SendAudio(chatId, Telegram.Bot.Types.InputFile.FromStream(fileStream, Path.GetFileName(attachmentFilePath)));
                    }
                    return true;
                }

                // Se è un file video (mp4, mov, avi)
                if (fileExtension == ".mp4" || fileExtension == ".mov" || fileExtension == ".avi")
                {
                    using (var fileStream = new FileStream(attachmentFilePath, FileMode.Open))
                    {
                        await _botClient.SendVideo(chatId, Telegram.Bot.Types.InputFile.FromStream(fileStream, Path.GetFileName(attachmentFilePath)));
                    }
                    return true;
                }


                if (fileExtension == ".zip" || fileExtension == ".rar")
                {
                    using (var fileStream = new FileStream(attachmentFilePath, FileMode.Open))
                    {
                        await _botClient.SendDocument(chatId, Telegram.Bot.Types.InputFile.FromStream(fileStream, Path.GetFileName(attachmentFilePath)));
                    }
                    return true;
                }

            }




            return true; // Solo il messaggio di testo è stato inviato
        }
        catch (Exception ex)
        {
            // Log dell'errore
            Console.WriteLine($"Errore nell'invio del messaggio o allegato: {ex.Message}");
            return false;
        }
    }


    // Metodo per elaborare gli aggiornamenti
    public async Task HandleUpdatesAsync()
    {
        var offset = 0;

        while (true)
        {
            var updates = await _botClient.GetUpdates(offset);

            foreach (var update in updates)
            {
                if (update.Message != null)
                {
                    await ProcessMessageAsync(update.Message);
                }

                if (update.CallbackQuery != null && update.CallbackQuery.Data != null)
                {
                    await ProcessMessageAsync(update.Message, update.CallbackQuery);
                }

                offset = update.Id + 1;
            }
        }
    }

    //Migliorato
    //public async Task HandleUpdatesAsync(CancellationToken cancellationToken)
    //{
    //    var offset = 0;

    //    while (!cancellationToken.IsCancellationRequested)
    //    {
    //        try
    //        {
    //            var updates = await _botClient.GetUpdatesAsync(offset, timeout: 10, cancellationToken: cancellationToken);

    //            foreach (var update in updates)
    //            {
    //                if (update.Message != null)
    //                {
    //                    await ProcessMessageAsync(update.Message);
    //                }
    //                if (update.CallbackQuery != null && update.CallbackQuery.Data != null)
    //                {
    //                    await ProcessMessageAsync(update.Message, update.CallbackQuery);
    //                }

    //                offset = update.Id + 1;
    //            }
    //        }
    //        catch (Exception ex)
    //        {
    //            Console.WriteLine($"Errore durante l'elaborazione degli aggiornamenti: {ex.Message}");
    //            await Task.Delay(2000, cancellationToken); // Aspetta 2 secondi prima di riprovare
    //        }
    //    }
    //}


    private async Task ProcessMessageAsync(Message message, CallbackQuery callbackQuery = null)
    {
        try
        {

            if (message == null && callbackQuery == null)
                return;

            if(message?.Sticker != null)
            {
                return;
            }

            using var scope = _scopeFactory.CreateScope();
            var _context = scope.ServiceProvider.GetRequiredService<LemurDbContext>();
            long chatId = message?.Chat?.Id ?? callbackQuery?.From?.Id ?? throw new InvalidOperationException("Both message and callbackQuery are null or do not contain a chat ID.");

            List<Permission> permessiUtente = new();
            var commands = new List<BotCommand>();
            var scopeBot = new BotCommandScopeChat { ChatId = chatId };

            //BLOCCO REPLYTESTO---------------------------------------------------------------------------------
            //Registrazione
            if (message?.ReplyToMessage != null && message?.ReplyToMessage?.Text == $"Ciao {message.From.FirstName}, sembra che tu non sia registrato. " +
             "Per procedere inserisci la tua password del gruppo:")
            {

                var passwordFromUtente = message?.Text?.Trim();

                //var checkedPasswordGruppo = await _context.Groups.Where(cr => cr.Password == passwordFromUtente).FirstOrDefaultAsync();

                var checkedPasswordGruppo = await _context.Groups
                        .Where(cr => cr.Password == passwordFromUtente)
                        .Select(gruppo => new
                        {
                            Gruppo = gruppo, // L'oggetto Gruppo completo
                            Permessi = gruppo.GroupPermissions.Select(gp => gp.Permission).ToList() // Lista dei permessi
                        })
                        .FirstOrDefaultAsync();

                if (checkedPasswordGruppo == null)
                {
                    var msgErrore = await _botClient.SendMessage(
                        message.Chat.Id,
                        $"{message.From.FirstName}, purtroppo la password inserita non corrisponde a nessun gruppo. " +
                        "Verifica la tua password e riprova. Se hai bisogno di assistenza, contatta il supporto. Scrivimi un qualsiasi messaggio per tentare nuovamente.");
                    return;
                }

                var gruppo = checkedPasswordGruppo.Gruppo; // Ottieni l'oggetto Gruppo
                var permessi = checkedPasswordGruppo.Permessi; // Ottieni la lista dei permessi

                //Controllo
                if (gruppo?.Id != null)
                {
                    try
                    {


                        UserTelegram userTelegram = new()
                        {
                            Username = message.From.FirstName,
                            GroupId = gruppo.Id,
                            TelegramId = message.From.Id,
                            TelegramIdChat = message.Chat.Id
                        };

                        _context.Update(userTelegram);

                        var changes = await _context.SaveChangesAsync();

                        if (changes > 0)
                        {

                            var msg = await _botClient.SendMessage(
                                 message.Chat.Id,
                                 $"{message.From.FirstName}, i tuoi dati sono stati salvati correttamente!");

                            foreach (var operazione in permessi)
                            {
                                commands.Add(new BotCommand
                                {
                                    Command = $"{operazione.Command}", // Assumi che 'Command' sia il campo del comando
                                    Description = operazione.Description ?? "" // Assumi che 'Description' sia il campo della descrizione
                                });
                            }

                            await _botClient.SetMyCommands(commands, scopeBot);
                            return;

                        }
                        else
                        {

                            await _botClient.SendMessage(
                                message.Chat.Id,
                                $"{message.From.FirstName}, nessuna modifica è stata applicata. Contatta il supporto.");
                            return;
                        }
                    }
                    catch (Exception ex)
                    {
                        // Gestione degli errori
                        await _botClient.SendMessage(
                            message.Chat.Id,
                            $"{message.From.FirstName}, si è verificato un errore durante l'aggiornamento dei dati: {ex.Message}. Contatta il supporto.");
                        return;
                    }
                }       
            }



            //"Per creare un nuovo gruppo, inserisci il nome e la password nel seguente formato:\n\n`nome_gruppo password`"
            if (message?.ReplyToMessage != null && message?.ReplyToMessage?.Text == $"Per creare un nuovo gruppo, inserisci il nome e la password nel seguente formato: " + "nome_gruppo password")
            {
                var newGruppo = message?.Text?.Trim();

                // Dividi la stringa usando lo spazio come delimitatore
                string[] parts = newGruppo.Split(' ');

                // Verifica se ci sono esattamente due parti
                if (parts.Length == 2)
                {
                    string nomeGruppo = parts[0];
                    string password = parts[1];

                    // Esegui controlli sulla validità di nomeGruppo e password
                    if (!string.IsNullOrWhiteSpace(nomeGruppo) && !string.IsNullOrWhiteSpace(password))
                    {

                        Models.ModelsTelegram.Group newGroup = new ()
                        {
                            Name = nomeGruppo,
                            Password = password,
                        };

                        _context.Groups.Add(newGroup);

                        var changes = await _context.SaveChangesAsync();

                        if (changes > 0)
                        {

                            var msg = await _botClient.SendMessage(
                                 message.Chat.Id,
                                 $"{message.From.FirstName}, i dati del gruppo sono stati salvati correttamente!");
                            return;

                        }
                        else
                        {
                            await _botClient.SendMessage(
                                message.Chat.Id,
                                $"{message.From.FirstName}, nessuna modifica è stata applicata. Contatta il supporto.");
                            return;
                        }
                    }
                    else
                    {
                        await _botClient.SendMessage(
                             chatId: chatId,
                             text: $"Errore: Il nome del gruppo e la password non possono essere vuoti."
                         );
                        return;
                    }
                }
                else
                {
                    await _botClient.SendMessage(
                         chatId: chatId,
                         text: $"Errore: Inserisci il nome del gruppo e la password separati da uno spazio."
                     );
                    return;
                }
            }


            //BLOCCO REPLY TESTO---------------------------------------------------------------------------------



            //BLOCCO CALLBACK (Click su un bottone)-----------------------------------------------------------------------

            if (callbackQuery != null)
            {

                //Caffe gratuito
                if (callbackQuery?.Message?.Text == "Se indovini qual è il tuo animale alter ego oggi, vinci un caffè gratis:")
                {

                    var todayStart = DateTime.UtcNow.Date;

                    var userFromDb = await _context.Users.Where(u => u.TelegramIdChat == chatId).FirstOrDefaultAsync();

                    var isUserHasPlayedToday = await _context.GameResults
                        .AnyAsync(g => g.UserId == userFromDb.Id && g.GameDate >= todayStart);

                    if (isUserHasPlayedToday) // Prevent playing multiple times a day
                    {
                        await _botClient.SendMessage(
                            chatId: callbackQuery.Message.Chat.Id,
                            text: "Hai già giocato oggi. Riprova domani!"
                        );
                        return;
                    }


                    var animaliFromDb = await _context.Animali.ToListAsync();
                    if (animaliFromDb.Count == 0) // Handle the case where there are no animals in the database
                    {
                        await _botClient.SendMessage(
                            chatId: callbackQuery.Message.Chat.Id,
                            text: "Non ci sono animali disponibili per il gioco."
                        );
                        return;
                    }

                    var chosenAnimal = callbackQuery?.Data;
                    var randomAnimal = animaliFromDb[_random.Next(animaliFromDb.Count)];

                    if (chosenAnimal == randomAnimal.Nome)
                    {
                        await _botClient.SendMessage(
                            chatId: callbackQuery.Message.Chat.Id,
                            text: $"Complimenti, {randomAnimal.Nome}! Hai vinto un caffè gratuito oggi!"
                        );
                        // ID dello sticker che vuoi inviare
                        string stickerId = "CAACAgIAAxkBAAP-Z6Dz_KgisuecW-8ln8U8RDtlTNwAAhMAA8A2TxOqs4f3fzjKpTYE"; // Sostituisci con l'ID del tuo sticker

                        // Invia lo sticker
                        await _botClient.SendSticker(
                            chatId: callbackQuery.Message.Chat.Id,
                            sticker: stickerId
                        );

                        if (userFromDb?.Id!= null)
                        {
                            // Create the GameResult *before* sending the message
                            var gameResult = new GameResult
                            {
                                UserId = (int)userFromDb.Id,
                                ChosenAnimal = chosenAnimal,
                                RandomAnimal = randomAnimal.Nome,
                                IsWin = chosenAnimal == randomAnimal.Nome // Set IsWin based on the comparison
                            };
                            _context.GameResults.Add(gameResult);
                            await _context.SaveChangesAsync();
                        }

                        return;
                    }
                    else
                    {
                        await _botClient.SendMessage(
                            chatId: callbackQuery.Message.Chat.Id,
                            text: $"Mi dispiace, oggi sei {randomAnimal.Nome} e il caffè gratuito è annullato."
                        );
                        // ID dello sticker che vuoi inviare
                        string stickerId = "CAACAgIAAxkBAAP8Z6DzkCHoQ9gS7y1aPb0SBpH9TfAAAgEAA8A2TxMYLnMwqz8tUTYE"; // Sostituisci con l'ID del tuo sticker

                        // Invia lo sticker
                        await _botClient.SendSticker(
                            chatId: callbackQuery.Message.Chat.Id,
                            sticker: stickerId
                        );

                        if (userFromDb?.Id != null)
                        {
                            // Create the GameResult *before* sending the message
                            var gameResult = new GameResult
                            {
                                UserId = (int)userFromDb.Id,
                                ChosenAnimal = chosenAnimal,
                                RandomAnimal = randomAnimal.Nome,
                                IsWin = chosenAnimal == randomAnimal.Nome // Set IsWin based on the comparison
                            };
                            _context.GameResults.Add(gameResult);
                            await _context.SaveChangesAsync();
                        }
                        return;
                        
                    }
                }
            }


            //BLOCCO CALLBACK (Click su un bottone)-----------------------------------------------------------------------





            //Controllo se registrato
            var checkedUser = await _context.Users.Where(ut => ut.TelegramIdChat == chatId)
               .Include(gr => gr.Group)
               .ThenInclude(gp => gp.GroupPermissions)
               .ThenInclude(perm => perm.Permission)
               .FirstOrDefaultAsync();

            if (checkedUser == null)
            {
                // Cancella i comandi del bot per questa chat (opzionale, ma utile per evitare conflitti)
                await _botClient.DeleteMyCommands(scopeBot);

                // Invia il messaggio di registrazione
                var regMsg = await _botClient.SendMessage(
                    chatId: message.Chat.Id,
                    text: $"Ciao {message.From.FirstName}, sembra che tu non sia registrato. " +
                          "Per procedere inserisci la tua password del gruppo:",
                    replyMarkup: new ForceReplyMarkup { Selective = true }
                );
                return;
            }


            //proseguire con i messaggi  NON RISPOSTE
            await _botClient.DeleteMyCommands(scopeBot);

            // Query unificata per ottenere i permessi del gruppo e i permessi predefiniti
            permessiUtente = await _context.Users
                .Where(u => u.Id == checkedUser.Id)
                .SelectMany(u => u.Group.GroupPermissions
                    .Select(gp => gp.Permission)
                    .Union(_context.Permissions.Where(p => p.IsDefault)) // Aggiungi i permessi predefiniti
                )
                .Distinct() // Rimuovi eventuali duplicati
                .ToListAsync();

            foreach (var operazione in permessiUtente)
            {
                commands.Add(new BotCommand
                {
                    Command = $"{operazione.Command}", // Assumi che 'Command' sia il campo del comando
                    Description = operazione.Description ?? "" // Assumi che 'Description' sia il campo della descrizione
                });
            }

            await _botClient.SetMyCommands(commands, scopeBot);


            //BLOCCO MESSAGGI DAL COMMANDI

            var messageText = message?.Text.ToLower();

            // Controllo delle operazioni autorizzate
            if (!permessiUtente.Any(op => string.Equals(op.Command, messageText, StringComparison.OrdinalIgnoreCase)))
            {
                await _botClient.SendMessage(
                    chatId: chatId,
                    text: $"Il comando '{messageText}' non è autorizzato per il tuo gruppo. Contatta l'assistenza."
                );
                return;
            }

            var operazioneTrovata = permessiUtente
                .FirstOrDefault(op => string.Equals(op.Command, messageText, StringComparison.OrdinalIgnoreCase));


            //La risposta dal DB se il campo Response esiste ( es: mio info)
            if (operazioneTrovata != null && !string.IsNullOrEmpty(operazioneTrovata.Response))
            {
                await _botClient.SendMessage(
                         chatId: message.Chat.Id,
                         text: $"{operazioneTrovata.Response}"
                     );
                return;
            }


        switch (operazioneTrovata?.Command.ToLower())
        {

                case "/add_group":

                    await _botClient.SendMessage(
                     chatId: chatId,
                     text: "Per creare un nuovo gruppo, inserisci il nome e la password nel seguente formato: " + "nome_gruppo password",
                     replyMarkup: new ForceReplyMarkup { Selective = true }
                 );
                break;

                case "/caffe_gratuito":

                    // Recupera tutti gli animali dal database
                    var animaliFromDB = await _context.Animali.ToListAsync();

                    // Crea una lista di pulsanti inline
                    var inlineKeyboardButtons = new List<InlineKeyboardButton[]>();

                    foreach (var animale in animaliFromDB)
                    {
                        // Aggiungi un pulsante per ogni animale
                        inlineKeyboardButtons.Add(new[]
                        {
                                InlineKeyboardButton.WithCallbackData(animale.Nome, animale.Descrizione.ToString())
                        });
                    }

                    // Crea la tastiera inline con i pulsanti
                    var inlineKeyboard = new InlineKeyboardMarkup(inlineKeyboardButtons);


                    await _botClient.SendMessage(
                    chatId: chatId,
                    text: "Se indovini qual è il tuo animale alter ego oggi, vinci un caffè gratis:",
                    replyMarkup: inlineKeyboard
                );

                break;

            default:
                await _botClient.SendMessage(
                    chatId: chatId,
                    text: "Comando non riconosciuto."
                );
                break;
        }




        }
        catch (Exception ex)
        {
            Console.WriteLine($"Errore durante l'elaborazione del messaggio: {ex.Message}");

            if (message != null)
            {
                var chatId = message.Chat.Id;
                var msg = await _botClient.SendMessage(chatId, $"Si è verificato un errore durante l'elaborazione della richiesta. Riprova più tardi.\n {ex.Message}");
            }
        }
    }




    //
    public async Task StartPeriodicTaskAsync(CancellationToken cancellationToken)
    {
        //while (!cancellationToken.IsCancellationRequested)
        //{
        //    try
        //    {
        //        using var scope = _scopeFactory.CreateScope();
        //        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        //        // Recupero gli ultimi 10 record non ancora ricevuti dall'operatore
        //        var latestRecords = await context.MessaggioTelegram
        //            .Where(x => x.TEG_ChatID == 772617342 && !x.IsMsgRicevutoFromOperatore && !x.IsMsgFromUtente)
        //            .OrderByDescending(r => r.DataOraInvio)
        //            .Take(10)
        //            .ToListAsync(cancellationToken);

        //        if (latestRecords.Any())
        //        {
        //            var updatedMessages = new List<MessaggioTelegram>(); // Lista per le modifiche

        //            foreach (var msg in latestRecords)
        //            {
        //                string allegato = null;

        //                if (msg.Allegati != null)
        //                {
        //                    allegato = msg.Allegati;
        //                }

        //                string operatore = msg.NomeOperatore ?? "test_operatore";

        //                string finalMessage = $"<u><b>Operatore: {operatore}</b></u>\n\n{msg.Testo}";

        //                bool inSended = await SendMessageAsync(772617342, $"{finalMessage}", allegato);

        //                if (inSended)
        //                {
        //                    msg.IsMsgRicevutoFromOperatore = true;
        //                    updatedMessages.Add(msg);
        //                }
        //            }

        //            // Salvare tutte le modifiche in un'unica operazione
        //            if (updatedMessages.Any())
        //            {
        //                context.UpdateRange(updatedMessages);
        //                await context.SaveChangesAsync(cancellationToken);
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        // Log dell'errore
        //        Console.WriteLine($"Errore: {ex.Message}");
        //    }

        //    // Aspetta un secondo prima del prossimo ciclo
        //    await Task.Delay(1000, cancellationToken);
        //}
    }

    // Metodo per la gestione degli allegati
    private async Task GestisciAllegato(string? attachmentFilePath, long chatId)
    {
        if (!string.IsNullOrEmpty(attachmentFilePath) && File.Exists(attachmentFilePath))
        {
            string fileExtension = Path.GetExtension(attachmentFilePath).ToLower();

            // Determina quale tipo di file inviare in base all'estensione
            switch (fileExtension)
            {
                case ".pdf":
                case ".doc":
                case ".docx":
                case ".xls":
                case ".xlsx":
                case ".txt":
                case ".zip":
                case ".rar":
                    await InviaDocumento(attachmentFilePath, chatId);
                    break;
                case ".jpg":
                case ".jpeg":
                case ".png":
                case ".gif":
                case ".bmp":
                    await InviaFoto(attachmentFilePath, chatId);
                    break;
                case ".mp3":
                case ".wav":
                case ".m4a":
                    await InviaAudio(attachmentFilePath, chatId);
                    break;
                case ".mp4":
                case ".mov":
                case ".avi":
                    await InviaVideo(attachmentFilePath, chatId);
                    break;
                default:
                    await _botClient.SendMessage(
                        chatId: chatId,
                        text: "Formato di file non supportato."
                    );
                    break;
            }
        }
        else
        {
            await _botClient.SendMessage(
                chatId: chatId,
                text: "Il file allegato non è stato trovato o non è valido."
            );
        }
    }

    // Metodo per inviare un documento
    private async Task InviaDocumento(string attachmentFilePath, long chatId)
    {
        using (var fileStream = new FileStream(attachmentFilePath, FileMode.Open))
        {
            await _botClient.SendDocument(chatId, Telegram.Bot.Types.InputFile.FromStream(fileStream, Path.GetFileName(attachmentFilePath)));
        }
    }

    // Metodo per inviare una foto
    private async Task InviaFoto(string attachmentFilePath, long chatId)
    {
        using (var fileStream = new FileStream(attachmentFilePath, FileMode.Open))
        {
            await _botClient.SendPhoto(chatId, Telegram.Bot.Types.InputFile.FromStream(fileStream, Path.GetFileName(attachmentFilePath)));
        }
    }

    // Metodo per inviare un audio
    private async Task InviaAudio(string attachmentFilePath, long chatId)
    {
        using (var fileStream = new FileStream(attachmentFilePath, FileMode.Open))
        {
            await _botClient.SendAudio(chatId, Telegram.Bot.Types.InputFile.FromStream(fileStream, Path.GetFileName(attachmentFilePath)));
        }
    }

    // Metodo per inviare un video
    private async Task InviaVideo(string attachmentFilePath, long chatId)
    {
        using (var fileStream = new FileStream(attachmentFilePath, FileMode.Open))
        {
            await _botClient.SendVideo(chatId, Telegram.Bot.Types.InputFile.FromStream(fileStream, Path.GetFileName(attachmentFilePath)));
        }
    }






}