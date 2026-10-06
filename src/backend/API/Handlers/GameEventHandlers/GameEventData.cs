/* 
 * This program has been developed by students from the bachelor Computer Science at 
 * Utrecht University within the Software Project course.
 * © Copyright Utrecht University (Department of Information and Computing Sciences)
 * Licensed under the MIT License. See the LICENSE file in the project root for details.
 */

using Models;

namespace API.Handlers.GameEventHandlers;

/// <summary>
/// A marker for data to be part of a game event.
/// The child classes have to indicate which game event has been fired.
/// The user id can optionally also be given, in order to indicate for which user the game event has been triggered.
/// </summary>
public abstract class GameEventData
{
    /// Is set automatically in <see cref="GameEventManager.TriggerGameEvent"/>
    public DateTime FiredAt;
    public int? UserId = null;
    public abstract GameEvent GameEventTriggered();
}

/// <summary>
/// Indicates an item has been answered. The Data includes all the properties associated with a user answering an item.
/// </summary>
public class ItemAnsweredData : GameEventData
{
    public required UserAnswer Data;
    
    public override GameEvent GameEventTriggered()
    {
        return GameEvent.ItemAnswered;
    }
}

/// <summary>
/// Indicates a test is completed.
/// The test (Level) and whether the test was mastered (Mastered) are required properties.
/// </summary>
public class TestCompletedData : GameEventData
{
    public required LevelResult Level;
    public required bool Mastered;
    
    public override GameEvent GameEventTriggered()
    {
        return GameEvent.TestCompleted;
    }
}

/// <summary>
/// Indicates a streak of a user has been prolonged and required the streak data to be available.
/// </summary>
public class StreakProlongedData : GameEventData
{
    public required UserStreak Streak;
    
    public override GameEvent GameEventTriggered()
    {
        return GameEvent.StreakProlonged;
    }
}

/// <summary>
/// A class indicating a user has registered, also containing which user has registered.
/// </summary>
public class UserRegisteredData : GameEventData
{
    public required User User;
    
    public override GameEvent GameEventTriggered()
    {
        return GameEvent.UserRegistered;
    }
}

/// <summary>
/// A class indicating a user has selected a new character, containing which character has been selected.
/// </summary>
public class CharacterSelectedData : GameEventData
{
    public required UserCharacter Character;
    
    public override GameEvent GameEventTriggered()
    {
        return GameEvent.CharacterSelected;
    }
}

/// <summary>
/// A class indicating a user has selected a new character, containing which character has been selected.
/// </summary>
public class ItemReportedData : GameEventData
{
    public required Item Item;
    
    public override GameEvent GameEventTriggered()
    {
        return GameEvent.ItemReported;
    }
}