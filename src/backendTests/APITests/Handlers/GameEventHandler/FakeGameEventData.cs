/* 
 * This program has been developed by students from the bachelor Computer Science at 
 * Utrecht University within the Software Project course.
 * © Copyright Utrecht University (Department of Information and Computing Sciences)
 * Licensed under the MIT License. See the LICENSE file in the project root for details.
 */

using API.Handlers.GameEventHandlers;
using Models;

namespace APITests.Handlers.GameEventHandler;

public class FakeItemCorrectData : GameEventData
{
    public required bool Correct;
    
    public override GameEvent GameEventTriggered()
    {
        return GameEvent.ItemAnswered;
    }
}