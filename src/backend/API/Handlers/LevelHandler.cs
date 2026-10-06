/* 
 * This program has been developed by students from the bachelor Computer Science at 
 * Utrecht University within the Software Project course.
 * © Copyright Utrecht University (Department of Information and Computing Sciences)
 * Licensed under the MIT License. See the LICENSE file in the project root for details.
 */

namespace API.Handlers;

public class LevelHandler
{
    /// <summary>
    /// Calculate proficiency Gain
    /// </summary>
    /// <param name="userAnswerCorrect">Boolean if a question was correctly answered</param>
    /// <param name="gain">Gain for a given correct answer</param>
    /// <param name="loss">Loss for a given incorrect answer</param>
    /// <returns>The change in proficiency to be applied</returns>
    public decimal CalcProficiencyGain(bool userAnswerCorrect, decimal gain, decimal loss)
    {
        return userAnswerCorrect ? gain : loss;
    }
}
