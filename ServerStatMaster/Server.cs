using LibHelper.Log;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace ServerStatMaster
{
    
    public class Server
    {
        static TcpListener tcpListener; // сервер для прослушивания
        List<ClientObject> clients = new List<ClientObject>(); // все подключения

        /// <summary>
        /// HttpClient is intended to be instantiated once per application, rather than per-use
        /// </summary>
        public readonly HttpClient httpClient = new HttpClient();

        protected internal void AddConnection(ClientObject clientObject)
        {
            clients.Add(clientObject);
        }
        protected internal void RemoveConnection(string id)
        {
            // получаем по id закрытое подключение
            ClientObject client = clients.FirstOrDefault(c => c.Id == id);
            // и удаляем его из списка подключений
            if (!(client is null))
                clients.Remove(client);
        }

        /// <summary>
        /// Прослушиваем входящие подключения
        /// </summary>
        protected internal void Listen()
        {
            try
            {
                DLog.Ln("Прослушиваем входящие подключения...", level: 1);
                tcpListener = new TcpListener(IPAddress.Any, 8888);
                tcpListener.Start();

                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine(">> Сервер запущен. Ожидание подключений...");
                DLog.Ln("Сервер запущен. Ожидание подключений...", level: 1);
                Console.ResetColor();

                while (true)
                {
                    TcpClient tcpClient = tcpListener.AcceptTcpClient();
                    
                    DLog.Ln("Принят запрос на подключение", level: 1);                    
                    ClientObject clientObject = new ClientObject(tcpClient, this);
                    DLog.Ln("Принят запрос на подключение", level: 1);
                    Thread clientThread = new Thread(new ThreadStart(clientObject.Process));
                    clientThread.Start();
                }
            }
            catch (Exception ex)
            { 
                //Disconnect();            
                Console.WriteLine(ex.Message);

            }
            finally
            {
                Stop();
            }
        }

        // трансляция сообщения подключенным клиентам
        protected internal void BroadcastMessage(string message, string id)
        {
            byte[] data = Encoding.Unicode.GetBytes(message);
            for (int i = 0; i < clients.Count; i++)
            {
                if (clients[i].Id != id) // если id клиента не равно id отправляющего
                {
                    clients[i].Stream.Write(data, 0, data.Length); //передача данных
                }
            }
        }
        // отключение всех клиентов
        protected internal void Disconnect()
        {
            tcpListener.Stop(); //остановка сервера

            for (int i = 0; i < clients.Count; i++)
            {
                clients[i].Close(); //отключение клиента
            }
            Environment.Exit(0); //завершение процесса
        }         

        // Остановка сервера
        ~Server()
        {
            Stop();
        }

        private void Stop()
        {
            // Если "слушатель" был создан
            if (!(tcpListener is null))
            {
                // Остановим его
                tcpListener.Stop();
            }
        }
    }
}
