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

type: <my-change-type>
version: <my-version or Unreleased>
reference: <my-ref>[Optional]
project: <my-project>[Required if MultiProject = true]
```
For changes which are not going to be immediately released you should use `Unreleased` as the version. Once you add a commit with a release version, the `Unreleased` commits directly preceding it will be grouped underneath the relevant release version.

For a multi project repository, include the `project: <my-project>` section as well.
### Existing repository
Write all future commit messages in the default format [listed above](#new-repository) or in the format defined in your custom settings file.

If you have changes that you want to include which are unparsable based on the defined format, you can write them into a fileSource and pass it into the generator on execution (see [optional parameters](#optional-parameters)).

The file should have one change per line in one of the following formats

`<version>|<change-type>|<summary>|<date in yyyy-MM-dd format>`

 or

`<reference>|<version>|<change-type>|<summary>|<date in yyyy-MM-dd>`

 or for multi project repositories

`<project>|<reference>|<version>|<change-type>|<summary>|<date in yyyy-MM-dd>`

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
|exclude| `Maintenance,Fixed` | Comma separated change types to exclude. Overrides value specified in custom settings file.
|minVersion| `0.1.0` | The minimum version of the changelog, will not include changes lower than this version. Overrides value specified in custom settings file.
|repository| `path\to\repository` | Path to repository root. Defaults to execution directory. Overrides value specified in custom settings file.
|fileSource| `someFileSource.txt` | Path to file source. Overrides value specified in custom settings file.

#### Custom App Settings
See `dotnet-gitchanges\appsettings.json` for the default settings file. Any settings not set in your custom app settings file will default to the values in the default file.

***Example Settings File***
```
{
    "Parsing": {
        "Version": {
            "SourceType" : "Message",
            "Pattern" : "version:(.*)[\n]?",
            "IsOptional" : false
        },
        "ChangeType": {
            "SourceType" : "Message",
            "Pattern" : "type:(.*)[\n]?",
            "IsOptional" : false
        },
        "Reference": {
            "SourceType" : "Message",
            "Pattern" : "reference:(.*)[\n]?",
            "IsOptional" : true
        },
    },
    "Template": "someCustomTemplate.mustache",
    "ChangeTypesToExclude": "Maintenance",
    "MinVersion": "0.1.0",
    "Repository": {
      "Path": ".",
      "OverrideSource": "someOverrideSource.txt"
    },
    "FileSource": "someHistorialChanges.txt",
    "MultiProject": false
}
```
***Descriptions***

|Name|Description|
|-------------|-------------------------|
| Parsing.Version.SourceType | Source type to extract Version from. Can be "Message" or "Tag". Defaults to "Message".
| Parsing.Version.Pattern | Regex pattern for extracting the Version from the source. Defaults to "version:(.*)[\n]?".
| Parsing.Version.IsOptional | Does the Version need to be present in the change? If false, non-matching changes will be skipped. If true, non-matching changes will default to the most recently parsed version. Defaults to False.
| Parsing.ChangeType.SourceType | Source type to extract Change Type from. Can be "Message" or "Tag". Defaults to "Message".
| Parsing.ChangeType.Pattern | Regex pattern for extracting the Change Type from the source. Defaults to "type:(.*)[\n]?".
| Parsing.ChangeType.IsOptional | Does the Change Type need to be present in the change? If false, non-matching changes will be skipped. If true, non-matching changes will default to an 'Uncategorized' change type. Defaults to False.
| Parsing.Reference.SourceType | Source type to extract Reference from. Can be "Message" or "Tag". Defaults to "Message".
| Parsing.Reference.Pattern | Regex pattern for extracting the Reference from the source. Defaults to "reference:(.*)[\n]?".
| Parsing.Reference.IsOptional | Does the Reference need to be present in the change? If false, non-matching changes will be skipped. If true, non-matching changes will default to an empty reference. Defaults to true.
| Parsing.Project.SourceType | Source type to extract Project from. Can be "Message" or "Tag". Defaults to "Message".
| Parsing.Project.Pattern | Regex pattern for extracting the Project from the source. Defaults to "project:(.*)[\n]?".
| Parsing.Project.IsOptional | Does the Project need to be present in the change? If false, non-matching changes will be skipped. If true, non-matching changes will default to a 'Global' project. Defaults to True.
| Template | Path to custom template file.
| MinVersion | The minimum version of the changelog, generation will exclude changes lower than this version.
| Repository.Path | Path to repository root.
| Repository.OverrideSource | Path to override source (see [Overriding repository changes](#overriding-repository-changes))
| FileSource | Path to file source (see [Existing repository](#existing-repository)).
| MultiProject | Specifies whether repository should be processed as a multi project repository.

#### Custom Template
Currently the only supported templating syntax supported is [Mustache](http://mustache.github.io/mustache.5.html). See `dotnet-gitchanges\KeepAChangelogTemplate.Mustache` for the default template file.

The schema available for you to use in your custom templates is detailed below.

+-- versions // Collection of versions\
----+-- version // Version value\
----+-- date // Version date value\
----+-- changeTypes // Collection of version change types\
----+---+-- changeType // Change Type value\
----+---+-- changes // Collection of Change Type changes\
----+---+---+-- reference // Change reference value\
----+---+---+-- summary // Change summary

#### Overriding Repository Changes
In some cases, you may decide that you want to override a commit which exists in the repository history without having to rewrite the history. In this case you can provide an override source file in a [custom settings file](#custom-app-settings).

The file should have one change per line in one of the following formats

`<commitId>|<version>|<change-type>|<summary>|<date in yyyy-MM-dd format>`

 or

`<commitId>|<reference>|<version>|<change-type>|<summary>|<date in yyyy-MM-dd>`

or for multi project repositories

`<commitId>|<project>|<reference>|<version>|<change-type>|<summary>|<date in yyyy-MM-dd>`

#### Multi Project Repository
In some cases, you may have multiple projects in the same repository which all need to have their own changelogs. This use case is now supported by passing in a custom settings file with the MultiProject flag set to true. A changelog per project will be created based on the project information specified in the commit messages.

#### Git helpers
See `git\.gitmessage` and `git\hooks\prepare-commit-msg` for examples of how to use git hooks and message templates to construct messages in the desired format, automatically populating metadata where possible.

## Contribution
* If you want to contribute to code feel free to open a pull request
* If you find any bugs or errors please create an issue

## License

This project is licensed under the MIT License
