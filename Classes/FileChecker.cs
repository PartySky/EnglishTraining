using System.IO;

namespace EnglishTraining
{
    public class FileChecker
    {
        public bool ChecIfkExist(string filePath) {
            if (File.Exists(filePath))
            {
                return true;
            }
            return false;
        }
    }
}
