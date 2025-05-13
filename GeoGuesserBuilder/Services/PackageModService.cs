using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace GeoGuesserBuilder.Services;

public class PackageModService
{
    private const string TempDir = ".\\temp_mod_output";
    private const string ModTargetDir = ".\\temp_mod_output\\GeoGuesserMod";
    private const string ModSourceDir = ".\\Resources\\ModEngine-2.1.0.0-win64";
    private string[] DirectoriesToCopy = new string[2]
    {
        "modengine2",
        "geoguesser"
    };
    private string[] FilesToCopy = new string[4]
    {
        "config_geoguesser.toml",
        "launchmod_geoguesser.bat",
        "modengine2_launcher.exe",
        "README.txt"
    };

    private static void CopyDirectory(string sourceDir, string destDir, bool overwrite = true)
    {
        if (!Directory.Exists(destDir))
        {
            Directory.CreateDirectory(destDir);
        }

        foreach (var file in Directory.GetFiles(sourceDir))
        {
            var destFile = Path.Combine(destDir, Path.GetFileName(file));
            File.Copy(file, destFile, overwrite);
        }

        foreach (var subdir in Directory.GetDirectories(sourceDir))
        {
            var destSubDir = Path.Combine(destDir, Path.GetFileName(subdir));
            CopyDirectory(subdir, destSubDir, overwrite);
        }
    }

    private void PackageModFiles(string sourceFolderPath)
    {
        var dialog = new Microsoft.Win32.SaveFileDialog
        {
            Title = "Save Packaged Mod",
            Filter = "Zip Archive (*.zip)|*.zip",
            FileName = "GeoGuesserMod.zip"
        };

        if (dialog.ShowDialog() == true)
        {
            try
            {
                if (!Directory.Exists(sourceFolderPath))
                {
                    System.Windows.MessageBox.Show("Source folder not found.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                ZipFile.CreateFromDirectory(sourceFolderPath, dialog.FileName, CompressionLevel.Optimal, includeBaseDirectory: false);
                System.Windows.MessageBox.Show("Mod files packaged successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Failed to package mod files:\n{ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }

    public void PackageModFiles()
    {
        // initialize a temp dir for the mod files
        if (Directory.Exists(TempDir))
        {
            Directory.Delete(TempDir, true);
        }
        Directory.CreateDirectory(TempDir);
        Directory.CreateDirectory(ModTargetDir);

        // copy required files to mod dir
        foreach (string dirName in DirectoriesToCopy)
        {
            CopyDirectory($"{ModSourceDir}\\{dirName}", $"{ModTargetDir}\\{dirName}", false);
        }
        
        foreach (string fileName in FilesToCopy)
        {
            File.Copy($"{ModSourceDir}\\{fileName}", $"{ModTargetDir}\\{fileName}", false);
        }

        // zip mod, prompt user for output location
        PackageModFiles(ModTargetDir);
    }
}
