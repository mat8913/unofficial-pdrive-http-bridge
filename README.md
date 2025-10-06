# unofficial-pdrive-http-bridge

An unofficial Proton Drive command-line client based off the SDK tech demo.

## Warnings

* The Proton devs strongly discourage the use of the SDK tech demo by 3rd party
  applications. I have used it anyway.
* I wrote this for myself and it has recieved very limited testing.
* **Keep backups. Data loss may occur due to unforeseen errors. You have been
  warned.**

## Features

* Supports 2FA
* Supports HTTP HEAD requests to get file/folder metadata without initiating a
  download. Metadata includes:
  * Last modified time
  * Content length
  * Content type
* Supports partial downloads (eg. seek support for media players)
* Supports parallel downloads
* Caches metadata to minimize API requests and supports event API to
  automatically update cache

## Build

If you use NixOS, you can just run `nix-build`. Otherwise, you will need to:

* Build https://github.com/ProtonDriveApps/dotnet-crypto and put the nuget
  package in your local nuget repo.
* Build the `0.2.10-mat8913` branch from my SDK demo fork at
  https://github.com/mat8913/proton-sdk-tech-demo/tree/0.2.10-mat8913 and put
  the nuget packages in your local nuget repo.
* Run `dotnet build` to build this

## Usage

When you first run `unofficial-pdrive-http-bridge`, it will generate a random
password and print it to the console. This password protects the http endpoints,
it is NOT your Proton account password.

To access the web UI, go to http://127.0.0.1:9000/ and log in with the password
generated above and any username (the username field is ignored).

Once you have access to the web UI, you can go to the login form to log in to
your Proton account. You only need to do this once.

The following command-line flags are supported:

* `--Hostname=<hostname>` - set which hostname the server should listen on
  (default: `127.0.0.1`)
* `--Port=<port>` - set which port the server should listen on (default: `9000`)
* `--ResetPassword=true` - generate a new web UI password and print it to the
  console
* `--ResetCache=true` - reset the cache of known files and folders
