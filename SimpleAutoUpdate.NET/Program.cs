﻿using System;
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
        private void reportError(string error)
        {
            Console.Error.WriteLine(error);
            Environment.Exit(-1);
        }

        private void run(string[] args)
        {
            System.Net.ServicePointManager.SecurityProtocol = System.Net.SecurityProtocolType.Tls | System.Net.SecurityProtocolType.Tls11 | System.Net.SecurityProtocolType.Tls12 | System.Net.SecurityProtocolType.Ssl3;

            if (args.Count() < 2)
            {
                Console.WriteLine("Usage: SimpleAutoUpdate.exe [currentVersion] [updateManifestUrl] ([pathToMainProgram])");
                Environment.Exit(-2);
            }
            string currentVersionString = args[0];
            if (string.IsNullOrEmpty(currentVersionString))
            {
                reportError("Parameter error:no current version");
                return;
            }
            string updateManifestURL = args[1];
            if (string.IsNullOrEmpty(currentVersionString))
            {
                reportError("Parameter error:no update manifest URL");
                return;
            }
            FileInfo mainProgram = null;
            if (args.Count() > 2)
            {
                string pathToMainProgram = args[2];

                if (string.IsNullOrEmpty(pathToMainProgram) || !File.Exists(pathToMainProgram))
                {

                    reportError("Parameter error: invalid path to main program");
                    return;
                }
                mainProgram = new FileInfo(pathToMainProgram);
            }


            if (mainProgram != null)
            {
                WaitForExit(mainProgram);
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
            if (currentVersion < updateInformation.Version)
            {
                string updateZipPackage = DownloadPackage(updateInformation.Url);
                Unzip(updateZipPackage, new FileInfo(System.Reflection.Assembly.GetEntryAssembly().Location).Directory.FullName);
            }


            if (mainProgram != null)
            {
                Process.Start(mainProgram.FullName);
            }
        }

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

            return new UpdateInformations(releaseVersion, downloadUrl);
        }

        private class UpdateInformations
        {
            public string Url { get; }
            public Version Version { get; }
            public UpdateInformations(Version version, String url)
            {
                this.Version = version;
                this.Url = url;
            }

        }



        private string DownloadPackage(string url)
        {
            String updateZipPackage = Path.GetTempFileName();
            using (WebClient client = new WebClient())
            {

                client.DownloadFile(url, updateZipPackage);
            }
            return updateZipPackage;
        }

        private void Unzip(String source, String dest)
        {
            using (ZipArchive archive = ZipFile.OpenRead(source))
            {
                foreach (ZipArchiveEntry entry in archive.Entries)
                {
                    entry.ExtractToFile(Path.Combine(dest, entry.FullName), true);
                }
            }
        }
    }
}
