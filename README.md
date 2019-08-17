# SimpleAutoUpdate.NET
SimpleAutoUpdate.NET is a simple c# program that enables developers to add auto update capabilites without the need to implement a library. SimpleAutoUpdate.NET can download a zip archive containing the update files and replace the existing files with the update. Optionally it can stop and restart your application to perform the update.

# How it works
SimpleAutoUpdate.NET checks a given URL for an update manifest. The update manifest contains the version of the update and a download URL. (Optionally a sha256 checksum). If the update is newer than the current application version it downloads the update, stops your application, replaces the application files with the files from the zip archive an restarts your application

# Integrate SimpleAutoUpdate.NET in your application
