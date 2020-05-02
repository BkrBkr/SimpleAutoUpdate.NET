using NDesk.Options;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
/*
 * SimpleAutoUpdate.NET - SimpleAutoUpdate.NET is a simple c# program that enables developers to add auto update capabilites without the need to implement a library
 * Copyright(C) 2019 Björn Kremer
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.

 
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program.If not, see<https://www.gnu.org/licenses/>.
 */
namespace SimpleAutoUpdate
{
    class Program
    {
        static void Main(string[] args)
        {
            new Program().run(args);


        }

        /**
         * Send error messages to error output and exit
         */
        private void reportError(string error)
        {
            Console.Error.WriteLine(error);
            Environment.Exit(-1);
        }
        /**
         * Main function: controls the application flow 
         */
        private void run(string[] args)
        {
            try
            {
              
                Boolean restartNeeded = false;

                string currentVersionString = "";
                string updateManifestURL ="";
                string downloadServerPrefix = "";
                string pathToMainProgram ="";
                bool hasMainProg = false;
                OptionSet p = new OptionSet() {
                    { "currentVersion=", "currentVersion",
                    v => currentVersionString=v },
                    { "updateManifestUrl=", "updateManifestUrl",
                    v => updateManifestURL=v },
                    { "downloadServerPrefix=", "downloadServerPrefix",
                    v => downloadServerPrefix=v },
                    { "pathToMainProgram=", "pathToMainProgram",
                    v => {hasMainProg=true; pathToMainProgram=v; } }
                };

                try
                {
                    p.Parse(args);
                }
                catch (OptionException)
                {
                    reportError("Invalid parameters");
                    return;
                }
                if (string.IsNullOrEmpty(currentVersionString))
                {
                    reportError("Parameter error:no current version");
                    return;
                }
                if (string.IsNullOrEmpty(currentVersionString))
                {
                    reportError("Parameter error:no update manifest URL");
                    return;
                }
                
                if (string.IsNullOrEmpty(downloadServerPrefix))
                {
                    reportError("Parameter error:no downloadServerPrefix");
                    return;
                }
                FileInfo mainProgram = null;
                if (hasMainProg)
                {
                    
                    if (string.IsNullOrEmpty(pathToMainProgram) || !File.Exists(pathToMainProgram))
                    {

                        reportError("Parameter error: invalid path to main program");
                        return;
                    }
                    mainProgram = new FileInfo(pathToMainProgram);
                }


                Version currentVersion = new Version(currentVersionString);
                if (currentVersion is null)
                {
                    reportError("Input parameter invalid: invalid release version");
                    return;
                }

                UpdateInformations updateInformation = GetUpdateInformations(updateManifestURL);
                if (updateInformation == null)
                {
                    reportError("No update informations found");
                    return;
                }
                if (!updateInformation.Url.AbsoluteUri.StartsWith(downloadServerPrefix, true, System.Globalization.CultureInfo.InvariantCulture))
                {
                    reportError("Untrusted URL");
                    return;
                }

                if (currentVersion < updateInformation.Version)
                {
                    if (mainProgram != null)
                    {
                        WaitForExit(mainProgram);
                        restartNeeded = true;
                    }
                    FileInfo updateZipPackage = DownloadPackage(updateInformation.Url);
                    try
                    {

                        if (!string.IsNullOrEmpty(updateInformation.Checksum) && !ValidateCheckSum(updateInformation.Checksum, updateZipPackage))
                        {
                            reportError("Invalid Checksum");
                            return;
                        }
                        Unzip(updateZipPackage, new FileInfo(System.Reflection.Assembly.GetEntryAssembly().Location).Directory);
                    }
                    finally
                    {
                        if (updateZipPackage.Exists)
                            updateZipPackage.Delete();
                    }
                }


                if (mainProgram != null && restartNeeded)
                {
                    Process.Start(mainProgram.FullName);
                }
            }
            catch (Exception ex)
            {
                reportError(ex.Message);
            }

        }

        /*
         * Wait for the given program to exit. Terminates the program if it doesn't. 
         */
        private void WaitForExit(FileInfo mainProgram)
        {
            Process[] processes = Process.GetProcessesByName(Path.GetFileNameWithoutExtension(mainProgram.Name));
            if (processes.Length > 0)
            {
                System.Threading.Thread.Sleep(1000);
                processes[0].CloseMainWindow();
                System.Threading.Thread.Sleep(3000);

                processes = Process.GetProcessesByName(Path.GetFileNameWithoutExtension(mainProgram.Name));
                if (processes.Length > 0)
                {
                    processes[0].Kill();
                    System.Threading.Thread.Sleep(3000);
                }
            }

        }

        /*
         *  Download and parse the update manifest 
         */
        private UpdateInformations GetUpdateInformations(string updateManifestURL)
        {
            XmlDocument updateInformation = new XmlDocument();
            updateInformation.Load(updateManifestURL);
            XmlElement root = updateInformation.DocumentElement;
            if (root is null)
            {
                reportError("Invalid update manifest: no xml content");
                return null;
            }

            XmlNode releaseVersionNode = root.SelectSingleNode("version");
            if (releaseVersionNode is null)
            {
                reportError("Invalid update manifest: no release version");
                return null;
            }

            string releaseVersionString = releaseVersionNode.InnerText;
            if (String.IsNullOrEmpty(releaseVersionString))
            {
                reportError("Invalid update manifest: no release version");
                return null;
            }

            Version releaseVersion = new Version(releaseVersionString);
            if (releaseVersion is null)
            {
                reportError("Invalid update manifest: invalid release version");
                return null;
            }

            XmlNode dowloadUrlNode = root.SelectSingleNode("url");
            if (dowloadUrlNode is null)
            {
                reportError("Invalid update manifest: no package url");
                return null;
            }

            string downloadUrl = dowloadUrlNode.InnerText;
            if (String.IsNullOrEmpty(releaseVersionString))
            {
                reportError("Invalid update manifest: no package url");
                return null;
            }

            String checkSum = "";
            XmlNode sha256 = root.SelectSingleNode("sha256");
            if (sha256 != null)
            {
                checkSum = sha256.InnerText;
            }

            return new UpdateInformations(releaseVersion, new Uri(downloadUrl), checkSum);
        }

        private class UpdateInformations
        {
            public Uri Url { get; }
            public Version Version { get; }
            public string Checksum { get; }
            public UpdateInformations(Version version, Uri url, string checksum)
            {
                this.Version = version;
                this.Url = url;
                this.Checksum = checksum;
            }

        }


        /*
         * Download the update zip
         */
        private FileInfo DownloadPackage(Uri url)
        {
            String updateZipPackage = Path.GetTempFileName();
            using (WebClient client = new WebClient())
            {

                client.DownloadFile(url, updateZipPackage);
            }
            return new FileInfo(updateZipPackage);
        }

        /*
         * Unzip the update zip and replace the existing files 
         */
        private void Unzip(FileInfo source, DirectoryInfo dest)
        {
            string thisExeName = new FileInfo(System.Reflection.Assembly.GetEntryAssembly().Location).Name;
            using (ZipArchive archive = ZipFile.OpenRead(source.FullName))
            {
                foreach (ZipArchiveEntry entry in archive.Entries)
                {
                    if (entry.Name.Equals(thisExeName)||entry.Name.Equals("NDesk.Options.dll"))
                        entry.ExtractToFile(Path.Combine(dest.FullName, entry.FullName + ".update"), true);
                    else
                        entry.ExtractToFile(Path.Combine(dest.FullName, entry.FullName), true);
                }
            }
        }

        /*
         * Validate the checksum of the update zip 
         */
        private bool ValidateCheckSum(string refCheckSum, FileInfo file)
        {
            string checkSum = null;
            using (System.IO.FileStream fileStream = System.IO.File.OpenRead(file.FullName))
            {
                using (System.Security.Cryptography.SHA256 sha = System.Security.Cryptography.SHA256.Create())
                {
                    checkSum = BitConverter.ToString(sha.ComputeHash(fileStream)).Replace("-", "");
                }
            }
            return refCheckSum.ToLowerInvariant().Equals(checkSum.ToLowerInvariant());
        }
    }

}
