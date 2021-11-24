using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Windows.Forms;

namespace e621_ReBot_v2.Modules
{
    public static class Module_UpdaterUpdater
    {
        public static void UpdateTheUpdater()
        {
            if (File.Exists("e621 ReBot Updater.exe"))
            {
                int CurrentVersion = int.Parse(FileVersionInfo.GetVersionInfo("e621 ReBot Updater.exe").FileVersion.Replace(".", ""));
                if (CurrentVersion < 1004)
                {
                    using (MemoryStream MemoryStreamTemp = new MemoryStream(Properties.Resources.e621_ReBot_Updater))
                    {
                        using (ZipArchive UpdateZip = new ZipArchive(MemoryStreamTemp))
                        {
                            foreach (ZipArchiveEntry entry in UpdateZip.Entries)
                            {
                                    entry.ExtractToFile(Path.Combine("./", entry.FullName), true);
                            }
                        }
                    }
                    MessageBox.Show("Updated the updater to latest version.", "e621 ReBot", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
        }
    }
}
