# Smart Playlist 2 Playlist Harder Plugin for Jellyfin

This is a more recent forked & upgraded version of [ankenyr](https://github.com/ankenyr/jellyfin-smartplaylist-plugin) plugin, which hadn't had updates in quite a while, and I found it a quite useful plugin and wanted to improve it.


## Overview

This plugin allows you to setup a series of rules to auto generate and update playlists within Jellyfin.
With the current implimentation these are limited to an individual user.

The json format currently in use, may not be the final version while I continue to work on this forked version.
Currently I am trying to keep backwards compatability in mind. For example with the ordering, instead of changing the base OrderBy to an array I added the nested ThenBy field, to allow old ones to work.
If I do make breaking changes, for my own sake as well, I will try to allow the older version to be compatable as well, but no guarantees. 

## How to Install

To use this plugin download the DLL and place it in your plugin directory. Once launched you should find in your data directory a folder called "smartplaylist". Your JSON files describing a playlist go in here.

## Configuration

To create a new playlist, create a json file in this directory having a format such as the following.

```json
{
  "Name": "CGP Grey",
  "FileName": "cgp_grey",
  "User": "rob",
  "ExpressionSets": [
    {
      "Expressions": [
        {
          "MemberName": "Directors",
          "Operator": "Contains",
          "TargetValue": "CGP Grey"
        },
        {
          "MemberName": "IsPlayed",
          "Operator": "Equal",
          "TargetValue": "False"
        }
      ]
    },
    {
      "Expressions": [
        {
          "MemberName": "Directors",
          "Operator": "Contains",
          "TargetValue": "Nerdwriter1"
        },
        {
          "MemberName": "IsPlayed",
          "Operator": "Equal",
          "TargetValue": "False"
        }
      ]
    }
  ],
  "Order": {
    "Name": "ReleaseDate",
    "Ascending": true,
    "ThenBy":[
      {
        "Name": "OriginalTitle",
        "Ascending": true
      }
    ]
  }
}
```

- Id: This field is created after the playlist is first rendered. Do not fill this in yourself.
- Name: Name of the playlist as it will appear in Jellyfin
- FileName: The actual filename. If you create a file named cgpgrey_playlist.json then this should be cgpgrey_playlist
- User: Name of the user for the playlist
- ExpressionSets: This is a list of Expressions. Each expression is OR'ed together.
- Expressions: This is the meat of the plugin. Expressions are a list of maps containing MemberName, Operator, and TargetValue. I am working on a list of all valid things. Currently there are three sets of expressions:

  - Universal LINQ expression operators: [This link](https://docs.microsoft.com/en-us/dotnet/api/system.linq.expressions.expressiontype?redirectedfrom=MSDN&view=net-6.0) is a list of all valid operators within expression trees but only a subset are valid for a specific operand.
  - String operators: Equals, StartsWith, EndsWith, Contains.
  - Regex operators: MatchRegex, NotMatchRegex.
  - StringListContainsSubstring: This is basically a string contains, but searches all entries in a list, such as Directors, Genres or Tags.

- MemberName: This is a reference to the properties in [Operand](https://github.com/ankenyr/jellyfin-smartplaylist-plugin/blob/master/Jellyfin.Plugin.SmartPlaylist/QueryEngine/Operand.cs "Operand"). You set this string to one of the property names to reference what you wish to filter on.
- Operator: An operation used to compare the TargetValue to the property of each piece of media. The above example would match anything with the director set as CGP Grey with a Premiere Date less than 2020/07/01
- TargetValue: The value to be compared to. Most things are converted into strings, booleans, or numbers. A date in the above example is converted to seconds since epoch.
- InvertResult: This allows you to invert any Expression, reguardless of it's type, name or operator.
- StringComparison: Only used specifically on string comparisons, this can allow you to ignore case, when using a value such as OrdinalIgnoreCase [Microsoft Docs](https://learn.microsoft.com/en-us/dotnet/api/system.stringcomparer.ordinalignorecase?view=net-6.0)


- Order: Provides the type of sorting you wish for the playlist. The following are a list of valid sorting methods so far.
- These generally match the property name in the BaseItem object, if it accepts muliple names, they will  be split by |
  - Album
  - ChannelId
  - Container
  - DateLastRefreshed
  - DateLastSaved
  - DateModified
  - EndDate
  - ForcedSortName
  - Height
  - Id
  - MediaType
  - Name
  - NoOrder
  - OriginalTitle
  - Overview
  - Path
  - ProductionYear|Year
  - Release Date|ReleaseDate|PremiereDate
  - SortName
  - Tagline
  - Width
  - RandomOrder|Random|RNG|DiceRoll;

## Future work
- Add Top X to limit playlist size (✅ Implimented)
- Add Library filter to only allow specific libraries in the Playlist
- Add AND clause to existing ORs, eg any current caluse, AND a new one.
- Add Dynamic Playlist Generater
  - This should allow you to setup rules to generate many playlists automatically.
  - eg, you want a playlist of the the last 5 days of any {Director}s videos
    this would generate a playlist for every {Director} that has a video uploaded in the past 5 days.

-Optimize playlist generators by pre grouping libaray query modes, eg user and item types, so it's not retriving the whole library for every playlist.

-Playlist refresh changes
  - Add IsReadonly (✅ Implimented)
  - Playlist specifc refresh rate (Unsure)
- More tests
  - An aim to have tests to test comparing expressions with actual JellyFin library documents.
  - More parsing and saving checks
- Add in more properties to be matched against. Please file a feature request if you have ideas.
- Document all operators that are valid for each property.
- Explore creating custom property types with custom operators.
- Once Jellyfin allows for custom web pages beyond the configuration page, explore how to allow configuration from the web interface rather than JSON files.
- CICD pipeline with automatic updates to latest JellyFin realease if there arn't breaking changes.

## Credits

Original plugin this fork was based off of [ankenyr](https://github.com/ankenyr/jellyfin-smartplaylist-plugin)
Rule engine was inspired by [this](https://stackoverflow.com/questions/6488034/how-to-implement-a-rule-engine "this") post in Stack Overflow.
Initially wanted to convert [ppankiewicz's plugin](https://github.com/ppankiewicz/Emby.SmartPlaylist.Plugin "ppankiewicz's plugin") but found it to be too incompatible and difficult to work with. I did take some bits of code mostly around interfacing with the filesystem.
