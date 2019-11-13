# Gitchanges CLI Tool
![Nuget](https://img.shields.io/nuget/v/dotnet-gitchanges)
[![Build Status](https://travis-ci.com/bzumhagen/dotnet-gitchanges.svg?branch=master)](https://travis-ci.com/bzumhagen/dotnet-gitchanges)

Simple CLI Tool for generating changelogs from git history.

This tool will parse your git log, and extract metadata to construct a changelog dynamically for you.

## Getting Started

Install Gitchanges CLI Tool with dotnet global tool

```
dotnet tool install dotnet-gitchanges --global
```

### New repository
Write commit messages in the following format, or specify a custom format in a custom settings file.

```
<summary>

<body>[Optional]

tag: <my-tag>
version: <my-version or Unreleased>
reference: <my-ref>[Optional]
```
For changes which are not going to be immediately released you should use `Unreleased` as the version. Once you add a commit with a release version, the `Unreleased` commits directly preceding it will be grouped underneath the relevant release version.

### Existing repository
Write all future commit messages in the format [listed above](#new-repository).

If you have existing changes that you want to include, you can write them into a fileSource and pass it into the generator on execution (see [optional parameters](#optional-parameters)).

The file should have one change per line in one of the following formats

`<version>|<tag>|<summary>|<date in yyyy-MM-dd format>`

 or

`<reference>|<version>|<tag>|<summary>|<date in yyyy-MM-dd>`

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
|fileSource| `someFileSource.txt` | Path to file source. Overrides value specified in custom settings file.

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
    "Template": "someCustomTemplate.mustache",
    "TagsToExclude": "Maintenance",
    "MinVersion": "0.1.0",
    "Repository": {
      "Path": ".",
      "OverrideSource": "someOverrideSource.txt"
    },
    "FileSource": "someHistorialChanges.txt"
}
```
***Descriptions***

|Name|Description|
|-------------|-------------------------|
| Parsing.Reference | Regex for extracting the reference from the commit message.
| Parsing.Version | Regex for extracting the version from the commit message.
| Parsing.Reference | Regex for extracting the tag from the commit message.
| Template | Path to custom template file.
| MinVersion | The minimum version of the changelog, generation will exclude changes lower than this version.
| Repository.Path | Path to repository root.
| Repository.OverrideSource | Path to override source (see [Overriding repository changes](#overriding-repository-changes))
| FileSource | Path to file source (see [Existing repository](#existing-repository)).

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

#### Overriding Repository Changes
In some cases, you may decide that you want to override a commit which exists in the repository history without having to rewrite the history. In this case you can provide an override source file in a [custom settings file](#custom-app-settings).

The file should have one change per line in one of the following formats

`<commitId>|<version>|<tag>|<summary>|<date in yyyy-MM-dd format>`

 or

`<commitId>|<reference>|<version>|<tag>|<summary>|<date in yyyy-MM-dd>`

#### Git helpers
See `git\.gitmessage` and `git\hooks\prepare-commit-msg` for examples of how to use git hooks and message templates to construct messages in the desired format, automatically populating metadata where possible.

## Contribution

* If you want to contribute to code feel free to open a pull request
* If you find any bugs or errors please create an issue

## License

This project is licensed under the MIT License
