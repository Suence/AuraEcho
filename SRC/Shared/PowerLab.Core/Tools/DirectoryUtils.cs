using System.IO;

namespace PowerLab.Core.Tools;

/// <summary>
/// 目录操作工具类
/// </summary>
public static class DirectoryUtils
{
    public static void SafeMoveDirectory(string sourceDir, string destinationDir)
    {
        if (Path.GetPathRoot(sourceDir) == Path.GetPathRoot(destinationDir))
        {
            // 同一卷，直接移动
            Directory.Move(sourceDir, destinationDir);
            return;
        }

        // 跨卷，递归复制再删除
        CopyDirectory(sourceDir, destinationDir);
        Directory.Delete(sourceDir, recursive: true);
    }

    private static void CopyDirectory(string sourceDir, string destinationDir)
    {
        Directory.CreateDirectory(destinationDir);

        // 复制所有文件
        foreach (string filePath in Directory.GetFiles(sourceDir))
        {
            string fileName = Path.GetFileName(filePath);
            string destFile = Path.Combine(destinationDir, fileName);
            File.Copy(filePath, destFile, overwrite: true);
        }

        // 递归复制所有子目录
        foreach (string subDir in Directory.GetDirectories(sourceDir))
        {
            string dirName = Path.GetFileName(subDir);
            string destSubDir = Path.Combine(destinationDir, dirName);
            CopyDirectory(subDir, destSubDir);
        }
    }
}
