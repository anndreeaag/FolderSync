using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading;

class FolderSynchronizer
{
    private static string sourceFolder;
    private static string replicaFolder;
    private static string logFilePath;
    private static int syncInterval;
    private static Timer syncTimer;

    static void Main(string[] args)
    {
        if (args.Length != 4)
        {
            Console.WriteLine("Usage: FolderSynchronizer.exe <sourceFolder> <replicaFolder> <syncInterval> <logFilePath>");
            return;
        }

        sourceFolder = args[0];
        replicaFolder = args[1];
        syncInterval = int.Parse(args[2]);
        logFilePath = args[3];

        Log("Starting Folder Synchronizer...");
        Log($"Source Folder: {sourceFolder}");
        Log($"Replica Folder: {replicaFolder}");
        Log($"Sync Interval: {syncInterval} seconds");
        Log($"Log File Path: {logFilePath}");

        syncTimer = new Timer(SynchronizeFolders, null, 0, syncInterval * 1000);

        Console.WriteLine("Press [Enter] to exit...");
        Console.ReadLine();
    }

    private static void SynchronizeFolders(object state)
    {
        try
        {
            Log("Synchronization started.");
            SyncDirectory(sourceFolder, replicaFolder);
            Log("Synchronization completed.");
        }
        catch (Exception ex)
        {
            Log($"Error: {ex.Message}");
        }
    }

    private static void SyncDirectory(string source, string target)
    {
        var sourceInfo = new DirectoryInfo(source);
        var targetInfo = new DirectoryInfo(target);

        if (!targetInfo.Exists)
        {
            targetInfo.Create();
            Log($"Created directory: {targetInfo.FullName}");
        }

        // Sync files
        foreach (var file in sourceInfo.GetFiles())
        {
            string targetFilePath = Path.Combine(target, file.Name);
            if (!File.Exists(targetFilePath) || !FilesAreEqual(file.FullName, targetFilePath))
            {
                file.CopyTo(targetFilePath, true);
                Log($"Copied file: {file.FullName} to {targetFilePath}");
            }
        }

        // Sync directories
        foreach (var directory in sourceInfo.GetDirectories())
        {
            string targetDirectoryPath = Path.Combine(target, directory.Name);
            SyncDirectory(directory.FullName, targetDirectoryPath);
        }

        // Remove extraneous files and directories in target
        foreach (var file in targetInfo.GetFiles())
        {
            string sourceFilePath = Path.Combine(source, file.Name);
            if (!File.Exists(sourceFilePath))
            {
                file.Delete();
                Log($"Deleted file: {file.FullName}");
            }
        }

        foreach (var directory in targetInfo.GetDirectories())
        {
            string sourceDirectoryPath = Path.Combine(source, directory.Name);
            if (!Directory.Exists(sourceDirectoryPath))
            {
                directory.Delete(true);
                Log($"Deleted directory: {directory.FullName}");
            }
        }
    }

    private static bool FilesAreEqual(string file1, string file2)
    {
        using (var md5 = MD5.Create())
        {
            using (var stream1 = File.OpenRead(file1))
            using (var stream2 = File.OpenRead(file2))
            {
                byte[] hash1 = md5.ComputeHash(stream1);
                byte[] hash2 = md5.ComputeHash(stream2);
                return BitConverter.ToString(hash1) == BitConverter.ToString(hash2);
            }
        }
    }

    private static void Log(string message)
    {
        string logMessage = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - {message}";
        Console.WriteLine(logMessage);
        File.AppendAllText(logFilePath, logMessage + Environment.NewLine);
    }
}
