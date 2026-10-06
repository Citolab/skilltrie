/* 
 * This program has been developed by students from the bachelor Computer Science at 
 * Utrecht University within the Software Project course.
 * © Copyright Utrecht University (Department of Information and Computing Sciences)
 * Licensed under the MIT License. See the LICENSE file in the project root for details.
 */


namespace API.Tools.QTIConverting;

/// <exclude />
public class ItemNoCorrectAnswers : Exception
{
    public ItemNoCorrectAnswers() { }

    public ItemNoCorrectAnswers(string message) : base(message) { }

    public ItemNoCorrectAnswers(string message, Exception inner) : base(message, inner) { }
}

/// <exclude />
public class ItemNoAnswerIdentifier : Exception
{
    public ItemNoAnswerIdentifier() { }

    public ItemNoAnswerIdentifier(string message) : base(message) { }

    public ItemNoAnswerIdentifier(string message, Exception inner) : base(message, inner) { }
}
