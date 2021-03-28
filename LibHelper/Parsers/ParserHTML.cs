using System;
using System.Collections.Generic;
using System.Linq;

namespace LibHelper.Parsers
{
    /// <summary>
    /// Класс, который может разбирать HTML страницу
    /// </summary>
    public class ParserHTML
    {
        /// <summary>
        /// Получить таблицу с частотами слов
        /// </summary>
        /// <param name="source">Источник - строка</param>
        /// <param name="separators">Массив разделителей</param>
        /// <returns>Словарь в формате <Слово,Частота></returns>
        public static Dictionary<string, int> ParseWords(string source, params char[] separators)
        {
            if (string.IsNullOrEmpty(source))
                throw new Exception("Исходная строка пустая!");

            if (separators is null)
                throw new Exception("Не задан массив разделителей!");

            string[] words = source.Split(separators, StringSplitOptions.RemoveEmptyEntries);
            Dictionary<string, int> statistics = words
            .GroupBy(word => word)
            .ToDictionary(
                kvp => kvp.Key,
                kvp => kvp.Count());
            
            return statistics;
        }

        //HashSet<string> uniqueWords =
        //  new HashSet<string>(source.Split(separators, StringSplitOptions.RemoveEmptyEntries),
        //        StringComparer.OrdinalIgnoreCase);
    }
}
