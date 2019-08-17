# SimpleAutoUpdate.NET
SimpleAutoUpdate.NET is a simple c# program that enables developers to add auto update capabilites without the need to implement a library. SimpleAutoUpdate.NET can download a zip archive containing the update files and replace the existing files with the update. Optionally it can stop and restart your application to perform the update.

# How it works
SimpleAutoUpdate.NET checks a given URL for an update manifest. The update manifest contains the version of the update and a download URL. (Optionally a sha256 checksum). If the update is newer than the current application version it downloads the update, stops your application, replaces the application files with the files from the zip archive an restarts your application

# Integrate SimpleAutoUpdate.NET in your application

## 1. Add the necessary code to your application
If you want to update your software you can simply invoke SimpleAutoUpdate.NET.exe and pass the necessary parameters:

```console
SimpleAutoUpdate.NET.exe [currentVersion] [updateManifestUrl] ([pathToMainProgram])
```

* \[currentVersion] = The current version nummber of your application. Format: X.X.X.X X=digits e.g. 1.0.0.0
* \[updateManifestUrl] = The URL to the update manifest
* \[pathToMainProgram] = The path to your main exe (to start/stop your application) (optional)

## 2. Pack your application together with the SimpleAutoUpdate.NET.exe

## 3. Upload your package, create and upload the update manifest
The update manifest is a xml-file with the following format:

```xml
<item>
    <version>X.X.X.X/version>
    <url>[URL]</url>
	  <sha256>[Hash]</sha256>
</item>
```

* version = The version of the update Format: X.X.X.X X=digits e.g. 1.0.0.0
* url = The http address where the updater can find the update .zip file
* sha256 = Hash of the .zip file (optional)

## 4. Deploy your applicationxml
