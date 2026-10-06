## What are badges in SkillTrie
The best way to describe a badge is by using the definition of it in the code

```csharp
[Table(nameof(Badge))]
public class Badge
{
    [Key]
    public string Identifier { get; set; }
    
    public string? Name { get; set; }
    
    public string? Description { get; set; }
    
    public string? Category { get; set; }
    
    public string? Stamp { get; set; }

    public int ProgressNeeded { get; set; }

    [NotMapped]
    public List<GameEvent> TriggerConditions { get; set; } = [];

    [IgnoreDataMember]
    public BadgeState BadgeState { get; set; }
}
```

Each badge has a unique identifier, which is also the primary key of the Badge table. </br>
Then each badge has a name, description and stamp, which are utility columns for visualising a badge for the user. This will be discussed in a the next section. </br>
The Catrgory column is for now still unused, but can be useful in the future for grouping badges if there are a lot.  </br>
Each badge also has a column called ProgressNeeded which is an integer, a user needs a progress equal or higher than this number in order to achieve this badge. This user progress is stored in a many to many table. This number as well as the semantics of the user progress is different for each badge. A badge for a user having created an account can set its ProgressNeeded to 1. A user having a progress of 0 means they have not accomplished this badge, whereas if their progress is 1 they have completed its badge, which makes it a boolean badge. A badge for answering 100 items correctly will set this number to 100. If a user has a progress on 40 on this badge, it would mean they have already 40 items answered correctly. </br>
Finally, each badge has a list of trigger conditions from which it gets activated. Everywhere in the code a `GameEvent` can be invoked by using the method `IGameEventManager.TriggerGameEvent(GameEventData data)`. `GameEventData` is a wrapper class which contains which `GameEvent` has been triggered and which user triggered it. This list contains from which game events this badge should be triggerd, in order to calculate the new progress of the user on this badge.
Each badge lastly also has a one-to-one mapping to a `BadgeState` entry, which is defined the following way.
```csharp
[Table(nameof(BadgeState))]
public class BadgeState
{
    [Key]
    public string BadgeIdentifier { get; init; }
    
    public DateTime? OpenFrom { get; set; }
    
    public DateTime? OpenUntil { get; set; }
    
    public string? FlagKey { get; set; }
    
    public string? FlagVariant { get; set; }

    public BadgePhase Phase { get; set; } = BadgePhase.Staging;
    
    public bool Excluded { get; set; }
    
    [IgnoreDataMember]
    [ForeignKey(nameof(BadgeIdentifier))]
    public Badge Badge { get; set; } = null!;
}
```

These properties do not define the badge, but do have influence on the logic of the application and are therefore defined in a separate table. OpenFrom and OpenUntil indicate from when to when a badge can be made progression on. <br/> 
The FlagKey and FlagVariant are columns to control feature flagging on badges. This will be discussed in a later section. </br> 
The phase of the badge has two (used) members, Published and Staging. Staging badges can still be updated by the admin in the badge settings in the admin site. They are not visible to the users yet. Published badges cannot be edited by admins anymore and are visible to the users. </br> 
Excluded is a property which indicates a badge does not exist anymore. This is used by the `LivaBadgeCompiler` when it sees a badge is not included in the collection of the `BadgeImageCollector`. This will become more clear later, but for now it is useful to know its used in order to not directly delete the user progress in case there was a mistake of deletion.


## How to add a badge in the code

The badge architecture has been set up in order to make the creation of new badges a small task.
All one needs to do is simply create a class which inherits `BadgeImage`. For example:
```csharp
public class NewCoolBadge : BadgeImage
{
    public override string BadgeIdentifier()
    {
        return nameof(NewCoolBadge);
    }

    public override int CalculateNewProgress(GameEventData data, BadgeProgress currentProgress)
    {
        if (data.someProperty); // do something with the badge progression
    }

    protected override List<GameEvent> TriggerConditions()
    {
        return [GameEvent.SomeEvent];
    }

    protected override string? Name()
    {
        return "Cool Badge";
    }

    protected override string? Description()
    {
        return "Do some cool things in order to achieve this badge";
    }

    protected override string? Stamp()
    {
        return "path/to/cool stamp for new cool badge";
    }
}
```


`NewCoolBadge` never needs to be instantiated anywhere. This is all handled by some core logic which will be explained later. You are basically making a blueprint for a badge. Here are quick definitions of the overridable methods of BadgeImage. There are also summaries for this in the code of `BadgeImage`.
<li>Identifier: The identifier of the badge. Should be unique and not be used in any other badge. Maps to `Badge.Identifier` in the database</li>
<li>Name: The display name of the badge, this is what users will see as the title of the badge. Maps to `Badge.Name` in the database</li>
<li>Description: This describes how a badge will be accomplished. A user should be able to make up from this description what they have to do in order to achieve this badge. Maps to `Badge.Description` in the database</li>
<li>Stamp: The path in the frontend of image to show (relative to public/badges/). Maps to `Badge.Stamp` in the database</li>
<li>TriggerCondtions: The list of trigger conditions this badge should be triggered form. Maps to `Badge.TriggerCondtions` in the database</li>
<li>CalculateNewProgress: Is called when one of the game events is triggered defined in `TriggerConditions` (unless the badge is already completed). Is (or should be) a pure-like function which takes the current progress and the expected game event data and based on that calculates the new progress and returns it. An easy example is the following for calculating the new progression for a badge where you have to answer x items correctly in order to achieve the badge: 

```csharp
    public override int CalculateNewProgress(GameEventData gameEventData, BadgeProgress currentProgress)
    {
        ItemAnsweredData itemAnsweredData = (ItemAnsweredData) gameEventData;
        return currentProgress.Progress + (itemAnsweredData.Data.Correct ? 1 : 0);
    }
```
It expects a specific instance of GameEventData, checks whether the item answered is correct, if so adds one to the progression.
</li>

#### parameterized badges

The previous example already hinted it, you can make badges parameterized. This means you make a BadgeImage for the badge with a parameter. This parameter can then be used in the overridable methods like Name, Description, etc. Here is a real example:
```csharp
using API.Handlers.GameEventHandlers;
using API.Tools.Badges;
using API.Tools.Badges.BadgeImageCollecting;
using Models;

namespace API.Handlers.BadgeHandlers;

public class CorrectlyAnsweredItemsBadge : BadgeImage
{
    [BadgeParameter(EntryColumn = "ItemAmount")]
    public int ItemAmount;

    public override string BadgeIdentifier()
    {
        return ItemAmount + nameof(CorrectlyAnsweredItemsBadge);
    }

    /// <summary>
    /// Adds one progress to the old progress if the item answered by the user was correct.
    /// </summary>
    public override int CalculateNewProgress(GameEventData gameEventData, BadgeProgress currentProgress)
    {
        ItemAnsweredData itemAnsweredData = (ItemAnsweredData) gameEventData;
        return currentProgress.Progress + (itemAnsweredData.Data.Correct ? 1 : 0);
    }

    /// <summary>
    /// The badge has been achieved if the user has answered ItemAmount questions correctly.
    /// </summary>
    /// <returns></returns>
    public override int ProgressNeeded()
    {
        return ItemAmount;
    }

    protected override List<GameEvent> TriggerConditions()
    {
        return [GameEvent.ItemAnswered];
    }
    
    protected override string? Name()
    {
        return "Item smasher";
    }

    protected override string? Description()
    {
        return $"Answer {ItemAmount} questions correctly in order to achieve this badge";
    }
}
```

A parameter has been introduced called ItemAmount, by annotating it with the attribute `BadgeParameter`. The progress needed for this badge equals ItemAmount. This prevents having to create a lot of badges with a lot of duplicate code. The uniqueness of the identifier has also been ensured, by including the parameter in the Identifier method. <br/>
Now the question is where do we define the specific badges with the specific parameters. This is done in a table called `ParameterizedBadgeEntry` which is currently defined the following:
```csharp
[Table(nameof(ParameterizedBadgeEntry))]
public class ParameterizedBadgeEntry
{
    public int Id { get; set; }
    public string BadgeImage { get; set; }
    public Dictionary<string, object> Metadata { get; set; } = [];
    
    [Range(1, int.MaxValue)]
    public int? Amount { get; set; }
    public int? ScopeId { get; set; }
    public int? StreakCount { get; set; }
    [Column(TypeName = "time")]
    public TimeSpan? TimeBorder { get; set; }
    
    [IgnoreDataMember]
    [ForeignKey(nameof(ScopeId))]
    public Scope? Topic { get; set; } = null!;
}
```
The `EntryColumn` field in the `BadgeParameter` attribute refers to the column name of <b>this table</b>, so in this case to the Amount column. Column BadgeImage refers to the name of the BadgeImage, which in this example would be "CorrectlyAnsweredItemsBadge". There can be multiple rows for this badge. If there are three rows for example, it would mean there are three badges of this type. </br>
Then the question still remains what do the other columns like ScopeId, StreakCount and TimeBorder do here? These are columns for other badges and are ignored for the badge in the example, because there are no references to those columns.

## How are the badges compiled
We will not go into a lot of detail in this section. How this is done can be done through inspecting the code and reading the summaries. The architecture document also goes into this, but here is a global overview. There is a class called `BadgeImageCollector`. This class collects all badge images through reflection and also handles filling their parameters if there are any. This class makes all the badge images available through a collection: `Dictionary<string, BadgeImage>` which holds a key with the identifier and as value the BadgeImage instance with the parameters filled in. The `LiveBadgeCompiler` class subscribes to this class and handles all the badges in the collection being properly stored in the database. Finally the BadgeGameEventhandler is subscribed to any GameEvent being triggered. It will look what game event has been triggered and collect the triggered badges based on the game event. Per badge it will fetch the corresponding badge image from the collector and call the CalculateNewProgress method, then it will save that new progress in the database.
