using LibHelper.Log;
using System;
using System.IO;
using System.Reflection;
using System.Threading;

namespace ServerStatMaster
{
    class Program
    {
        /// <summary>
        /// Объект 'Сервер'
        /// </summary>
        static Server server;

        /// <summary>
        /// Поток для прослушивания
        /// </summary>
        static Thread listenThread;

        static void Main(string[] args)
        {
            try
            {
                //Подключаем журнал
                if (!DLog.Open(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + @"\logs\", "TM"))
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Ошибка загрузки журнала");
                    Console.ResetColor();
                }

                DLog.Ln("Запуск сервера...");

                server = new Server();
                listenThread = new Thread(new ThreadStart(server.Listen));
                listenThread.Start(); //старт потока
            }
            catch (Exception ex)
            {
                DLog.Ln($"Ошибка: '{ex.Message}'");
                DLog.Ln("Отключение сервера...");
                server.Disconnect();
                DLog.Ln("Отключение сервера...OK");
                Console.WriteLine(ex.Message);
            }
        }
    }
}
