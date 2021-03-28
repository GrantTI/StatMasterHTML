using LibHelper.Log;
using System;
using System.Net.Http;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace ServerStatMaster
{
    public class ClientObject
    {
        /// <summary>
        /// Получить клиента HttpClient
        /// </summary>
        private HttpClient HttpClient 
        {
            get
            {
                DLog.Ln("Получение клиента httpClient", level: 3);
                if (server is null)
                {
                    DLog.Ln($"ОШИБКА: Объект сервера не найден! ('server is null')", level: 3);
                    throw new NullReferenceException("Сервер не найден!\n'server is null'");
                }
                if (server.httpClient is null)
                {
                    DLog.Ln($"ОШИБКА: Объект клиента http не найден! ('httpClient is null')", level: 3);
                    throw new NullReferenceException("Отсутствует возможность отправить запрос на ресурс!\n'httpClient is null'");
                }
                return server.httpClient;
            }
        }
        protected internal string Id { get; private set; }
        protected internal NetworkStream Stream { get; private set; }
        //string userName;
        TcpClient client;
        readonly Server server; // объект сервера

        public ClientObject(TcpClient tcpClient, Server serverObject)
        {
            DLog.Ln("Инициализация клиента TCP...", level: 2);
            
            Id = Guid.NewGuid().ToString();
            client = tcpClient;
            server = serverObject;
            serverObject.AddConnection(this);

            DLog.Ln("Инициализация клиента TCP...OK", level: 2);
        }

        /// <summary>
        /// Запуск слушателя клиента
        /// </summary>
        public void Process()
        {
            DLog.Ln("Запуск слушателя клиента", level: 2);
            NetworkStream stream = null;
            try
            {
                DLog.Ln("Получение потока", level: 2);
                stream = client.GetStream();
                byte[] data = new byte[64]; // буфер для получаемых данных
                while (true)
                {
                    DLog.Ln("Получение сообщения..", level: 2);
                    StringBuilder builder = new StringBuilder();
                    int bytes = 0;
                    do
                    {
                        bytes = stream.Read(data, 0, data.Length);
                        builder.Append(Encoding.Unicode.GetString(data, 0, bytes));
                    }
                    while (stream.DataAvailable);
                    DLog.Ln("Получение сообщения..OK", level: 2);

                    string message = builder.ToString();

                    Console.WriteLine(message);

                    DLog.Ln("Проверка запроса http://[]", level: 2);
                    if (message.StartsWith("http://")) //TODO - корректно проверить маску запросу
                                                       //например, с помощью регулярных выражений
                    {
                        DLog.Ln("Запрос http распознан", level: 2);
                        var ts = GetHTTPAsync(message);
                        data = Encoding.Unicode.GetBytes(ts.Result);
                        DLog.Ln("Данные получены", level: 2);
                    }
                    else
                    {
                        DLog.Ln("НЕВЕРНЫЙ формат http-запроса", level: 2);
                        data = Encoding.Unicode.GetBytes($"[501] Спасибо за Ваше обращение: '{message}'!" +
                            $" Мы с Вами обязательно свяжемся." +
                            $"\nМогу ли я Вам еще чем-то помочь?");
                    }

                    DLog.Ln("Отправляем ответное сообщение..", level: 2);
                    stream.Write(data, 0, data.Length);
                    DLog.Ln("Отправляем ответное сообщение....OK", level: 2);                                     
                }
            }
            catch (Exception ex)
            {
                DLog.Ln($"ОШИБКА: {ex.Message}", level: 2);
                Console.WriteLine(ex.Message);
            }
            finally
            {
                DLog.Ln($"finally", level: 2);
                // в случае выхода из цикла закрываем ресурсы
                //server.RemoveConnection(this.Id);
                Close();
            }
        }      

        /// <summary>
        /// Закрытие подключения
        /// </summary>
        protected internal void Close()
        {
            DLog.Ln("Закрытие потока NetworkStream", level: 3);
            if (!(Stream is null))
                Stream.Close();
            DLog.Ln("Закрытие потока NetworkStream..OK", level: 3);

            DLog.Ln("Закрытие клиента TCP", level: 3);
            if (!(client is null))
                client.Close();
            DLog.Ln("Закрытие клиента TCP..OK", level: 3);
        }

        async Task<string> GetHTTPAsync(string uri)
        {
            //var headers = HttpClient.DefaultRequestHeaders;

            //string header = "ie";
            //// пытаемся добавить header
            //if (!headers.UserAgent.TryParseAdd(header))
            //{
            //    throw new Exception(">> Invalid header value: " + header);
            //}

            Uri requestUri = new Uri(uri);
            HttpResponseMessage httpResponse = new HttpResponseMessage();
            string httpResponseBody = "";
            try
            {
                DLog.Ln("Отправление http-запроса..", level: 3);                
                httpResponse = await HttpClient.GetAsync(requestUri);
                DLog.Ln("Отправление http-запроса..OK", level: 3);

                DLog.Ln("Получение http-ответа..", level: 3);
                httpResponse.EnsureSuccessStatusCode();
                DLog.Ln("Получение http-ответа..OK", level: 3);

                DLog.Ln("Считывание тела http-ответа..", level: 3);
                httpResponseBody = await httpResponse.Content.ReadAsStringAsync();
                DLog.Ln("Считывание тела http-ответа..OK", level: 3);
                return httpResponseBody;
            }
            catch (Exception ex)
            {
                DLog.Ln($"Ошибка: '{ex.Message}'", level: 3);

                httpResponseBody = ">> Error: " + ex.HResult.ToString("X") + " Message: " + ex.Message;
                return httpResponseBody;
            }
        }
    }
}
