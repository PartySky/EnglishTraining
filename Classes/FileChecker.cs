using System.IO;

namespace EnglishTraining
{
    public class FileChecker
    {
        public bool CheckIfExist(string filePath) {
            if (File.Exists(filePath))
            {
                return true;
            }
            return false;
        }
    }
}
