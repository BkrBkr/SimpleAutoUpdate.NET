# SimpleAutoUpdate.NET
SimpleAutoUpdate.NET is a simple C# program that allows developers to add automatic update functions to their software without having to implement a library. SimpleAutoUpdate.NET can download a zip archive with the update files and replace the existing files with the update. Optionally, it can stop and restart your application to perform the update.

# How it works
SimpleAutoUpdate.NET checks a given URL for an update manifest. The update manifest contains the version of the update and a download URL. (Optional a sha256 checksum). If the update is newer than the current application version, SimpleAutoUpdate.NET downloads the update, stops your application, replaces the application files with the files from the zip archive, and restarts your application.

# Integrate SimpleAutoUpdate.NET in your application

## 1. Add the necessary code to your application
If you want to update your software, you can simply call SimpleAutoUpdate.NET.exe and pass the necessary parameters:

```console
SimpleAutoUpdate.NET.exe [currentVersion] [updateManifestUrl] ([pathToMainProgram])
```

* \[currentVersion] = The current version nummber of your application. Format: X.X.X.X X=digits e.g. 1.0.0.0
* \[updateManifestUrl] = The URL to the update manifest
* \[pathToMainProgram] = The path to your main exe (to start/stop your application) (optional)

For a code snippet refer to the "[Example c# code to invoke the update](#example-c-code-to-invoke-the-update)"-section below.

## 2. Pack your application together with the SimpleAutoUpdate.NET.exe

## 3. Upload your package, create and upload the update manifest
The update manifest is a xml-file with the following format:

```xml
<?xml version="1.0" encoding="UTF-8"?>
<item>
    <version>X.X.X.X/version>
    <url>[URL]</url>
    <sha256>[Hash]</sha256>
</item>
```

* version = The version of the update. Format: X.X.X.X X=digits e.g. 1.0.0.0
* url = The http address where the updater can find the update .zip file
* sha256 = Hash of the .zip file (optional)

## 4. Deploy your application

# Examples

## Example update manifest
```xml
<?xml version="1.0" encoding="UTF-8"?>
<item>
    <version>1.0.0.11</version>
    <url>https://github.com/BkrBkr/SpectrePatcher/releases/download/1.0.0.11/SpectrePatcher.zip</url>
    <sha256>09f3fb7df275aff5bfd14ffdc2afbc0ada24e307ef5fbcb2995dcf999423fac9</sha256>
</item>
```

## Example c# code to invoke the update

```csharp
  private void autoUpdate()
        {
            string exePath = System.Reflection.Assembly.GetEntryAssembly().Location;
            string workingDir = new System.IO.FileInfo(exePath).Directory.FullName;
            Version version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
            System.Diagnostics.Process p = new System.Diagnostics.Process();
            string updateExe = System.IO.Path.Combine(workingDir, "SimpleAutoUpdate.NET.exe");

            p.StartInfo.FileName = updateExe;
            p.StartInfo.Arguments = string.Format(" \"{0}\" \"{1}\" \"{2}\" ", version.ToString(), "https://raw.githubusercontent.com/BkrBkr/SpectrePatcher/master/update.xml", exePath);
            p.Start();
            p.WaitForExit();

            if (System.IO.File.Exists(System.IO.Path.Combine(workingDir, "SimpleAutoUpdate.NET.exe.update")))
            {
                System.IO.File.Delete(updateExe);
                System.IO.File.Move(System.IO.Path.Combine(workingDir, "SimpleAutoUpdate.NET.exe.update"), updateExe);
            }
        }
```
