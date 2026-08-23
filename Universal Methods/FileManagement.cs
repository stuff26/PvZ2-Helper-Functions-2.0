namespace UniversalMethods
{
    public static class FileManagement
    {
        /// <summary>
        /// Copy the contents of one folder to another and recursively copy over its nested files and folders
        /// </summary>
        /// <param name="oldFolder">Folder that will be copied from</param>
        /// <param name="newFolder">Folder that will be deleted and then copied to</param>
        public static void CopyFolder(string oldFolder, string newFolder)
        {
            if (Directory.Exists(newFolder))
                Directory.Delete(newFolder, true);
            else
                Directory.CreateDirectory(newFolder);

            var filesToCopy = Directory.GetFiles(oldFolder);
            foreach (var file in filesToCopy)
            {
                var baseFileName = Path.GetFileName(file);
                var newFilePath = Path.Join(newFolder, baseFileName);
                File.Copy(file, newFilePath);
            }

            var directoriesToCopy = Directory.GetDirectories(oldFolder);
            foreach (var dir in directoriesToCopy)
            {
                var baseDir = dir.Split("\\")[^1];
                var newNestedFolder = Path.Join(newFolder, baseDir);
                CopyFolder(dir, newNestedFolder);
            }
        }

        /// <summary>
        /// Checks if a folder is empty, containing no files or subdirectories
        /// </summary>
        /// <param name="path">Folder path to check</param>
        /// <returns>True if the folder exists and is empty, otherwise false</returns>
        public static bool IsFolderEmpty(string path)
        {
            if (!Directory.Exists(path)) return false;
            return !Directory.EnumerateFileSystemEntries(path).Any();
        }
        
        /// <summary>
        /// Creates a nested folder to avoid errors
        /// </summary>
        /// <param name="directory">Directory to create</param>
        public static void CreateNestedFolder(string directory)
        {
            if (Path.Exists(directory)) return;
            
            var parentFolder = Path.GetDirectoryName(directory);
            if (!Path.Exists(parentFolder))
            {
                CreateNestedFolder(parentFolder!);
            }
            Directory.CreateDirectory(directory);
        }

        public static void RenameFile(string file, string newName)
        {
            var parentDir = Path.GetDirectoryName(file);
            var newPath = Path.Join(parentDir, newName);
            File.Move(file, newPath);
        }
    }
}