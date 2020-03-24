# NetControl4BioMed

## Table of contents

* [Introduction](#introduction)
* [Download](#download)
* [Usage](#usage)

## Introduction

Welcome to the NetControl4BioMed repository!

This is a C# / .Net Core web application which aims to solve the target network controllability problem. The application is cross-platform, and can be run or hosted on all modern operating systems (Windows, MacOS, Linux).

## Download

The repository consists of a Visual Studio 2019 project. You can download it to run or build the application yourself. You need to have [.NET Core 3.1](https://dotnet.microsoft.com/download/dotnet-core/3.1) installed in order to run it, or the corresponding SDK in order to also be able to build it.

## Usage

Once downloaded, there are several steps to undergo in order to be able to run the application.

First of all, an SQL Server to connect to (with an empty database named `NetControl4BioMed` already created) is required. For example, you can download and install the latest free [SQL Server Express](https://www.microsoft.com/en-us/sql-server/sql-server-downloads).

Secondly, several configuration options for the application need to be defined in the `appsettings.Development.json` or `appsettings.Production.json` configuration files (based on the current running environment, development by default). The model for all of the required configuration options can be found in the `appsettings.Production.json` file.

If any of these options is not defined, then the application will encounter an error and fail to start. It is important to note that most of these option only need to have a non-empty value in order for the application to start. Actually valid credentials are only needed if trying to use the corresponding function or service (for example, the Google Client details are only needed for authenticating using a Google account; if this function is not needed, then you can simply write a dummy text as both `ClientId` and `ClientSecret`).

Finally, if some other application might use `localhost` on the predefined port, then you can configure the address of the local server by adding the `"Urls": "http://localhost:XXXX;https://localhost:XXXY"` configuration option to the `appsettings.Development.json` configuration file.

For further details about the implementation or functionality of the application, you can check the `About` page within it.
