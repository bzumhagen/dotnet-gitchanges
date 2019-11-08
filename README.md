# Gitchanges CLI Tool

Simple CLI Tool for generating changelogs from git history.

This tool will parse your git log, and extract metadata to construct a changelog dynamically for you.

## Getting Started

Install Gitchanges CLI Tool with dotnet global tool

```
dotnet tool install dotnet-gitchanges --global
```

Write commit messages in the following format, or specify a custom format in a custom settings file.

```
<summary>

<body>

tag: <my-tag>
version: <my-version>
reference: <my-ref>[Optional]
```

## Usage
### Basic
Run the following command at the root of your repository
```
gitchanges
```
### Advanced
#### Optional Parameters
Run the previous command with any combination of the following optional parameters

|Name|Example Value|Description|
|-------------|-------------|-------------------------|
|settings| `someCustomAppSettings.json` | Path to custom settings file.
|template| `someCustomTemplate.mustache` | Path to custom template file. Overrides value specified in custom settings file.
|exclude| `Maintenance,Fixed` | Comma separated tags to exclude. Overrides value specified in custom settings file.
|minVersion| `0.1.0` | The minimum version of the changelog, will not include changes lower than this version. Overrides value specified in custom settings file.
|repository| `path\to\repository` | Path to repository root. Defaults to execution directory. Overrides value specified in custom settings file.

#### Custom App Settings
See `dotnet-gitchanges\appsettings.json` for the default settings file. Any settings not set in your custom app settings file will default to the values in the default file.

***Example Settings File***
```
{
    "Parsing": {
        "Reference": "reference:(.*)[\n]?",
        "Version": "version:(.*)[\n]?",
        "Tag": "tag:(.*)[\n]?"
    },
    "Template": "",
    "TagsToExclude": "",
    "MinVersion": "",
    "Repository": "."
}
```
***Descriptions***

|Name|Description|
|-------------|-------------------------|
| Parsing.Reference | Regex for extracting the reference from the commit message.
| Parsing.Version | Regex for extracting the version from the commit message.
| Parsing.Reference | Regex for extracting the tag from the commit message.
| Template | Path to custom template file.
| MinVersion | The minimum version of the changelog, will not include changes lower than this version.
| Repository | Path to repository root.

#### Custom Template
Currently the only supported templating syntax supported is [Mustache](http://mustache.github.io/mustache.5.html). See `dotnet-gitchanges\KeepAChangelogTemplate.Mustache` for the default template file.

The schema available for you to use in your custom templates is detailed below.

+-- versions // Collection of versions\
----+-- version // Version value\
----+-- date // Version date value\
----+-- tags // Collection of version tags\
----+---+-- tag // Tag value\
----+---+-- changes // Collection of tag changes\
----+---+---+-- reference // Change reference value\
----+---+---+-- summary // Change summary

#### Git helpers
See `git\.gitmessage` and `git\hooks\prepare-commit-msg` for examples of how to use git hooks and message templates to construct messages in the desired format, automatically populating metadata where possible.

## Contribution

* If you want to contribute to code feel free to open a pull request
* If you find any bugs or errors please create an issue

## License

This project is licensed under the MIT License
