using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Threading;
using LibHelper.IO;
using LibHelper.Log;
using LibHelper.Parsers;

namespace ClientStatMaster
{
    class Program
    {
        private const string host = "127.0.0.1"; //TODO Вынести в настройки или запрашивать у пользователя
        private const int port = 8888;//TODO Вынести в настройки или запрашивать у пользователя
        static TcpClient client;
        static NetworkStream stream;

        static void Main(string[] args)
        {
            //"Настройка локализации
            Thread.CurrentThread.CurrentUICulture = System.Globalization.CultureInfo.GetCultureInfo("ru-RU");//en-US//ru-RU
            Thread.CurrentThread.CurrentCulture = System.Globalization.CultureInfo.GetCultureInfo("ru-RU");

            //Подключаем журнал
            if (!DLog.Open(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + @"\logs\", "TM"))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                writeln(AppResource.LogErrorLoading);
                Console.ResetColor();

                Environment.Exit(0);
            }

            DrawHelloBlock();

            writeln(AppResource.QuestionToClient);

            DLog.Ln("Создание клиента");
            client = new TcpClient();
            DLog.Ln("Создание клиента..OK");
            try
            {
                DLog.Ln("Подключение клиента");
                client.Connect(host, port);
                DLog.Ln("Подключение клиента..OK");

                DLog.Ln("Подключение потока");
                stream = client.GetStream();
                DLog.Ln("Подключение потока..OK");

                while (true)
                {
                    SendRequest(); //Отправить запрос
                    
                    string response = GetResponse(); //Получить ответ

                    Processing(response); //Обработать ответ       
                }
            }
            catch (Exception ex)
            {
                DLog.Ln($"Ошибка: '{ex.Message}'");
                writeln(ex.Message);
            }
            finally
            {
                DLog.Ln("finally. Закрытие клиента TCP");
                if (!(client is null))
                    client.Close();
                DLog.Ln("finally. Закрытие клиента TCP..OK");
            }
        }

        private static void Processing(string response)
        {
            if (response.StartsWith("[501]"))
            {
                DLog.Ln("Неизвестный запрос..");
                writeln($">> Сервер:\n{response}");
            }
            else
            {
                DLog.Ln("Сохранение HTML-страницы..");
                string sGuid = Guid.NewGuid().ToString();
                string path = Writer.WriteToHTMLFile("file_"+ sGuid, response);
                DLog.Ln($"Сохранение HTML-страницы..OK: '{path}'");
                writeln($">> Скачан файл: '{path}'");

                writeln($">> Получение статистики...");
                Dictionary<string, int> table = 
                    ParserHTML.ParseWords(response, ' ', ',', '.', '!', '?','"', 
                                    ';', ':', '[', ']', '(', ')','\n', '\r', '\t',
                                    '<', '>', '|', '/', '\\', '{', '}', '=', '+');

                writeln(">> Количество уникальных слов:");
                foreach (var row in table)
                {
                    writeln($"{row.Key} - {row.Value}");
                }
            }
        }

        private static string GetResponse()
        {
            DLog.Ln("Получение ответа");
            byte[] data  = new byte[64]; // буфер для получаемых данных
            StringBuilder builder = new StringBuilder();
            do
            {
                int bytes = stream.Read(data, 0, data.Length);
                builder.Append(Encoding.Unicode.GetString(data, 0, bytes));
            }
            while (stream.DataAvailable);
            DLog.Ln("Получение ответа..OK");

            return builder.ToString();
        }

        private static void SendRequest()
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write("Введи адрес ресурса в формате 'http://[твой_ресурс]': ");
            Console.ResetColor();
            string uri = Console.ReadLine();

            DLog.Ln($"Пользователь ввел '{uri}'");
            writeln($">> Ты ввел: '{uri}'");

            byte[] data = Encoding.Unicode.GetBytes(uri);
            DLog.Ln("Отправка сообщения");
            stream.Write(data, 0, data.Length);
            DLog.Ln("Отправка сообщения..OK");
        }

        /// <summary>
        /// Отрисовка приветствия
        /// </summary>
        static void DrawHelloBlock()
        {
            DLog.Ln("Отрисовка приветствия", level: 1);

            int len = AppResource.HelloTitle.Length;
            string border = string.Join("", Enumerable.Repeat("=", len + 4));
            border = " " + border + " ";
            string spaces = string.Join("", Enumerable.Repeat(" ", len + 4));

            Console.ForegroundColor = ConsoleColor.Blue;
            writeln(border);
            writeln($"|{spaces}|");
            writeln($"|  {AppResource.HelloTitle}  |");
            writeln($"|{spaces}|");
            writeln(border);
            Console.ResetColor();

            DLog.Ln("Отрисовка приветствия..OK", level: 1);
        }

        /// <summary>
        /// Вспомогательный метод
        /// </summary>
        /// <param name="str"></param>
        static void writeln(string str)//, ConsoleColor color = ConsoleColor.White, bool resetColor = false)
        {
            Console.WriteLine(str);
        }

        //static async void SendMessageAsync(string uri)
        //{
        //    await Task.Run(() => SendMessage(uri));
        //}

        //// отправка сообщений
        //static void SendMessage(string uri)
        //{
        //    //Console.WriteLine("C>> Введите сообщение: ");

        //    while (true)
        //    {
        //        //string message = Console.ReadLine();
        //        byte[] data = Encoding.Unicode.GetBytes(uri);
        //        if (stream is null)
        //            Console.WriteLine(">> stream is null");
        //        stream.Write(data, 0, data.Length);
        //    }
        //}
        //// получение сообщений
        //static string ReceiveMessage()
        //{
        //    while (true)
        //    {
        //        try
        //        {
        //            byte[] data = new byte[64]; // буфер для получаемых данных
        //            StringBuilder builder = new StringBuilder();
        //            int bytes = 0;
        //            do
        //            {
        //                bytes = stream.Read(data, 0, data.Length);
        //                builder.Append(Encoding.Unicode.GetString(data, 0, bytes));
        //            }
        //            while (stream.DataAvailable);

        //            string message = builder.ToString();
        //            return message;
        //            //Console.WriteLine(message);//вывод сообщения
        //        }
        //        catch
        //        {
        //            Console.WriteLine(">> Подключение прервано!"); //соединение было прервано
        //            Console.ReadLine();
        //            Disconnect();
        //           // return "";
        //        }
        //    }
        //}

        ///// <summary>
        ///// Отключение
        ///// </summary>
        //static void Disconnect()
        //{
        //    DLog.Ln("Закрытие потока NetworkStream", level: 1);
        //    if (!(stream is null))
        //        stream.Close();
        //    DLog.Ln("Закрытие потока NetworkStream..OK", level: 1);

        //    DLog.Ln("Закрытие клиента TCP", level: 1);
        //    if (!(client is null))
        //        client.Close();
        //    DLog.Ln("Закрытие клиента TCP..OK", level: 1);

        //    DLog.Ln("ВЫХОД..", level: 1);
        //    Environment.Exit(0);
        //    DLog.Ln("ВЫХОД..OK", level: 1);
        //}
    }
}
