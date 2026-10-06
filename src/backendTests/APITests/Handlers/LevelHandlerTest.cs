/* 
 * This program has been developed by students from the bachelor Computer Science at 
 * Utrecht University within the Software Project course.
 * © Copyright Utrecht University (Department of Information and Computing Sciences)
 * Licensed under the MIT License. See the LICENSE file in the project root for details.
 */

using API.Handlers;

namespace APITests.Handlers;

public class LevelHandlerTest
{
    private readonly LevelHandler _levelHandler = new LevelHandler();

    [Fact(DisplayName = "CalcProficiencyGain returns gain on correct answer")]
    public void CalcProficiencyGainCorrectGain()
    {
        bool correct = true;
        decimal gain = 0.1m;
        decimal loss =  0.3m;

        var res = _levelHandler.CalcProficiencyGain(correct, gain, loss);

        Assert.Equal(gain, res);
    }

    [Fact(DisplayName = "CalcProficiencyGain returns loss on incorrect answer")]
    public void CalcProficiencyGainIncorrectLoss()
    {
        bool correct = false;
        decimal gain = 0.1m;
        decimal loss =  0.3m;

        var res = _levelHandler.CalcProficiencyGain(correct, gain, loss);

        Assert.Equal(loss, res);
    }
}
