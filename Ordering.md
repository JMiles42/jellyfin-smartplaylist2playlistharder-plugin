# Here are all the ways to order playlists

Ordering playlists is done via a stack of Order objects, that primarily follow the property names of the Operand type.  
There are some that don't relate to properties, such as the random one, which can be used to basically shuffle the playlists order.

| Name\/Operand Property Name | Alias                                          | Object Type |
| --------------------------- | ---------------------------------------------- | ----------- |
| NoOrder                     |                                                | N\/A        |
| RandomOrder                 | Random, RNG, RND, DiceRoll                     | N\/A        |
| Name                        |                                                | string      |
| SortName                    |                                                | string      |
| ForcedSortName              |                                                | string      |
| Path                        |                                                | string      |
| PremiereDate                | ReleaseDate, Release Date                      | double      |
| ProductionYearNotNull       | ProductionYear, Year                           | int         |
| ParentIndexNumberNotNull    | ParentIndexNumber, ParentIndex                 | int         |
| OriginalTitle               |                                                | string      |
| MediaType                   |                                                | string      |
| Album                       |                                                | string      |
| FolderPath                  |                                                | string      |
| ContainingFolderPath        |                                                | string      |
| FileNameWithoutExtension    |                                                | string      |
| OfficialRating              |                                                | string      |
| Overview                    |                                                | string      |
| Container                   |                                                | string      |
| Tagline                     |                                                | string      |
| ActorsLength                |                                                | int         |
| ArtistsLength               |                                                | int         |
| ComposersLength             |                                                | int         |
| DirectorsLength             |                                                | int         |
| GuestStarsLength            |                                                | int         |
| ProducersLength             |                                                | int         |
| WritersLength               |                                                | int         |
| GenresLength                |                                                | int         |
| StudiosLength               |                                                | int         |
| TagsLength                  |                                                | int         |
| PathSegmentLength           |                                                | int         |
| Exists                      |                                                | bool        |
| HasSubtitles                |                                                | bool        |
| IsFavoriteOrLiked           |                                                | bool        |
| IsHorizontal                |                                                | bool        |
| IsPlayed                    |                                                | bool        |
| IsSquare                    |                                                | bool        |
| IsVertical                  |                                                | bool        |
| DateCreated                 |                                                | double      |
| DateLastRefreshed           |                                                | double      |
| DateLastSaved               |                                                | double      |
| DateModified                |                                                | double      |
| PlayedPercentage            |                                                | double      |
| LastPlayedDate              |                                                | double      |
| DaysSinceCreated            | DateSinceCreated                               | int         |
| DaysSinceLastModified       | DateSinceLastModified                          | int         |
| DaysSinceLastRefreshed      | DateSinceLastRefreshed                         | int         |
| DaysSinceLastSaved          | DateSinceLastSaved                             | int         |
| DaysSincePremiereDate       | DateSincePremiereDate                          | int         |
| PlayedCount                 |                                                | int         |
| AiredSeasonNumberNotNull    | AiredSeasonNumber, SeasonNumber, Season Number | int         |
| PlaybackPositionTicks       |                                                | long        |
| CollectionName              | BoxSet                                         | string      |
| SeasonName                  | Season                                         | string      |
| SeriesName                  | Series                                         | string      |
| AllText                     |                                                | string      |
| Height                      |                                                | int         |
| Width                       |                                                | int         |
| ParentName                  |                                                | string      |
| GrandparentName             |                                                | string      |
| CommunityRatingNotNull      | CommunityRating                                | float       |
| CriticRatingNotNull         | CriticRating                                   | float       |
| EndDateNotNull              | EndDate                                        | DateTime    |
| ChannelId                   |                                                | Guid        |
| Id                          |                                                | Guid        |
| LengthNotNull               | Length                                         | TimeSpan    |
| LengthSecondsNotNull        | LengthSeconds                                  | double      |
| LengthMinutesNotNull        | LengthMinutes                                  | double      |
| LengthHoursNotNull          | LengthHours                                    | double      |
| LengthTicksNotNull          | LengthTicks                                    | long        |
| RunTimeTicksNotNull         | RunTimeTicks                                   | long        |
