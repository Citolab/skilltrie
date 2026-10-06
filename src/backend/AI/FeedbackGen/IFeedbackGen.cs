﻿/*
 * This program has been developed by students from the bachelor Computer Science at
 * Utrecht University within the Software Project course.
 * © Copyright Utrecht University (Department of Information and Computing Sciences)
 * Licensed under the MIT License. See the LICENSE file in the project root for details.
 */
using System;

/// <summary>
/// This interface defines the contract for generating feedback using AI. Implementations of this interface
/// can use different strategies or AI models to create feedback based on the provided test results, which include the question text, student answer, correct answer, and answer explanation. 
/// The generated feedback should be relevant and helpful for the student to understand their performance and areas for improvement.
/// </summary>
public interface IFeedbackGen
{
    /// <summary>
    ///  Generates feedback based on a completed test.
    /// </summary>
    /// <param name="results">A list of tuples, each containing four string values representing test result details.</param>
    /// <returns>An asynchronous enumerable of strings. It returns the stream of tokens that make up the generated feedback.</returns>
    public IAsyncEnumerable<string> GenerateFeedback(List<Tuple<string, string, string, string>> results);
}
