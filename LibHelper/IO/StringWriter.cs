using System.IO;
using System.Reflection;

namespace LibHelper.IO
{
    public class Writer
    {
        public static string WriteToHTMLFile(string fileName, string source)
        {
            string path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)
                + "\\html";

            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            string fullName = $"{path}\\{fileName}.html";

            using (StreamWriter htmlFile = new StreamWriter(fullName))
            {
                htmlFile.Write(source);
            }

            return fullName;
        }
    }
}
