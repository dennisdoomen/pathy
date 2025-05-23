// <autogenerated />

using System;
using System.IO;

namespace Pathy;

#pragma warning disable

#if PATHY_PUBLIC
public static class ChainablePathExtensions
#else
internal static class ChainablePathExtensions
#endif
{
    /// <summary>
    /// Creates the directory represented by the specified <see cref="ChainablePath"/> and any necessary subdirectories.
    /// </summary>
    public static void CreateDirectoryRecursively(this ChainablePath path)
    {
        Directory.CreateDirectory(path.ToString());
    }

    /// <summary>
    /// Deletes the file or directory represented by the specified <see cref="ChainablePath"/>.
    /// If the path represents a directory, it is deleted recursively.
    /// </summary>
    public static void DeleteFileOrDirectory(this ChainablePath path)
    {
        if (path.IsFile)
        {
            File.Delete(path.ToString());
        }
        else if (path.IsDirectory)
        {
            Directory.Delete(path.ToString(), recursive: true);
        }
    }

    /// <summary>
    /// Moves a file or directory represented by the specified <see cref="ChainablePath"/> to a target directory, with an optional new name.
    /// </summary>
    /// <param name="sourcePath">The path of the file or directory to move.</param>
    /// <param name="destinationDirectory">The target directory where the file or directory will be moved.</param>
    /// <param name="newName">The new name for the file or directory. If null, the original name will be preserved.</param>
    /// <exception cref="ArgumentException">Thrown if <paramref name="newName"/> is provided but is an empty string.</exception>
    public static void MoveFileOrDirectory(this ChainablePath sourcePath, ChainablePath destinationDirectory,
        string newName = null)
    {
        if (newName is { Length: 0 })
        {
            throw new ArgumentException("Renaming requires a valid name", nameof(newName));
        }

        if (sourcePath.IsFile)
        {
            File.Move(sourcePath, Path.Combine(destinationDirectory.ToString(), newName ?? sourcePath.Name));
        }
        else
        {
            Directory.Move(sourcePath, Path.Combine(destinationDirectory.ToString(), newName ?? sourcePath.Name));
        }
    }
}
