using System;
using System.IO;
using System.Text;

namespace LibHelper.Log
{
    /// <summary>
    /// Класс для текстового журналирования из любой точки. Требует инициализации в основной программе.
    /// </summary>
    /// <remarks>
    /// Статический класс для текстового журналирования. Потокобезопасное исполнение. Работает и в основном коде и при
    /// использованиии внутри бибилиотек. Экземпляр журнала - один общий на все приложение.
    /// </remarks>
    /// <example>
    /// В инициализацию приложения добавить:
    /// <code>
    /// DLog.Open(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + @"\logs\", "TM");
    /// //что-то отправить в журнал
    /// DLog.Ln("Запус приложения...");
    /// </code>
    /// <para>Где:</para>
    /// <para><c>logs</c> - имя подкаталога, в который будут записываться журнал</para>
    /// <para><c>TM</c> - префикс имен журналов</para>
    /// </example>
    public static class DLog
    {
        //объект для текстового файла
        private static StreamWriter LogStreamWriter;
        //переменная для рассчета времени
        private static TimeSpan ts;
        //объект для блокирования при многопоточной записи
        private static object LogLocker = new object();
        //флаг инициализации
        private static bool isLogInitialized = false;
        //полное имя файла журнала
        public static string LogFullFileName { get; private set; } = "";


        /// <summary>
        /// Инициализация внутреннего логгера
        /// </summary>
        /// <param name="LogDir">Путь к каталогу журналов </param>
        /// <param name="FileNamePrefix">Префикс имени файла журнала </param>
        /// <returns></returns>
        public static bool Open(string LogDir, string FileNamePrefix)
        {

            isLogInitialized = false;
            bool _res = false;

            //корректировка слэшей
            LogDir += @"\";
            LogDir = LogDir.Replace(@"\\", @"\");

            //инициализация (создание) пути 
            if (Directory.Exists(LogDir))
            {
                _res = true;
            }
            else
            {
                try
                {
                    Directory.CreateDirectory(LogDir);
                    _res = true;
                }
                catch { }
            }
            if (!_res) { return false; }

            //создание имени и открытие файла
            _res = false;
            string logFilename = $"{FileNamePrefix}-{DateTime.Now.ToString("yyyyMMdd-HHmmss")}.log";
            LogFullFileName = LogDir + logFilename;
            try
            {
                //LogStreamWriter = new StreamWriter(LogFullFileName, false, Encoding.GetEncoding("windows-1251"));
                LogStreamWriter = new StreamWriter(LogFullFileName, false, Encoding.Default);
                LogStreamWriter.AutoFlush = true;
            }
            catch { }
            _res = (LogStreamWriter != null);
            if (!_res) { return false; }

            //для расчета времени выполнения
            ts = new TimeSpan(DateTime.Now.Ticks);

            isLogInitialized = true;

            Ln($" {DateTime.Now} > ({Encoding.Default.WebName}) LOG OPEN / ЖУРНАЛ ОТКРЫТ", noDateTime: true);

            return true;
        }

        /// <summary>
        /// Корректное закрытие внутреннего логгера
        /// </summary>
        public static void Close()
        {
            if (!isLogInitialized) return;
            isLogInitialized = false;
            try
            {
                LogStreamWriter?.Close();
            }
            finally
            {
                if (LogStreamWriter != null)
                {
                    LogStreamWriter.Dispose();
                }
            }

        }

        /// <summary>
        /// Вывести простое сообщение БЕЗ перевода строки
        /// </summary>
        /// <param name="logText"></param>
        public static void LnNN(string logText, bool noDateTime = false)
        {
            if (!isLogInitialized) return;
            //поточная блокировка блока
            //если будет глючить, читать сюда: https://docs.microsoft.com/ru-ru/dotnet/api/system.io.textwriter.synchronized?view=netframework-4.8

            lock (LogLocker)
            {
                try
                {
                    if (noDateTime)
                    {
                        LogStreamWriter.Write(logText);
                    }
                    else
                    {
                        LogStreamWriter.Write($"{DateTime.Now.ToString("HH-mm-ss.ff")}| {logText}");
                    }
                }
                catch { }
            }
        }

        /// <summary>
        /// Сообщение c переводом строки. Начинается со штампа времени.
        /// Для ускорения - указать noDateTime = true
        /// </summary>
        /// <param name="logText"></param>
        /// <param name="level">Уровень вложенности метода</param>
        public static void Ln(string logText, int level = 0, bool noDateTime = false)
        {
            if (!isLogInitialized) return;
            //поточная блокировка блока
            lock (LogLocker)
            {
                try
                {
                    if (level < 0)
                        throw new Exception($"Уровень вложенности 'level'={level} отрицательный!");

                    if (level > 0)
                        logText = RepeatStr("--", level) + " " + logText;

                    if (noDateTime)
                    {
                        LogStreamWriter.WriteLine(logText);
                    }
                    else
                    {
                        LogStreamWriter.WriteLine($"{DateTime.Now.ToString("HH-mm-ss.ff")}| {logText}");
                    }

                }
                catch { }
            }
        }

        private static string RepeatStr(string str, int count)
        {
            if (count < 0)
                throw new Exception($"'count' = {count} меньше нуля!");

            if (count == 0)
                return ""; //Умный RepeatStr

            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < count; i++)
            {
                sb.Append(str);
            }

            return sb.ToString();
        }
    }
}
