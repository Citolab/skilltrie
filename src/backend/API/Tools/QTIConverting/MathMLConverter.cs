/* 
 * This program has been developed by students from the bachelor Computer Science at 
 * Utrecht University within the Software Project course.
 * © Copyright Utrecht University (Department of Information and Computing Sciences)
 * Licensed under the MIT License. See the LICENSE file in the project root for details.
 */

using Models;
using Jint;
using System.Text.RegularExpressions;

namespace API.Tools.MathMLConverter;

public interface IMathMLConverter
{
    Task ConvertItemsToMathML(Item[] items);
}

/// <summary>
/// Handles the conversion form LaTex to MathML formulas.
/// A JS enironment is mocked via Jint, in order to run the Temml library.
/// </summary>
public class MathMLConverter : IMathMLConverter
{
    private static readonly Engine _engine;

    static MathMLConverter()
    {
        _engine = new Engine();
        _engine.Execute("var document = { createElement: function() { return { setAttribute: function(){} }; } };");
        _engine.Execute("var window = {};");
        _engine.Execute("var exports = {};");
        _engine.Execute("var module = { exports: exports };");

        var scriptPath = Path.Combine(AppContext.BaseDirectory, "Tools", "QTIConverting", "temml.cjs");
        _engine.Execute(File.ReadAllText(scriptPath));

        _engine.Execute("var temml = module.exports;");
    }

    public async Task ConvertItemsToMathML(Item[] items)
    {
        foreach (var item in items)
        {
            if (item.QuestionText != null && item.QuestionText.Contains(@"\begin{math}"))
                item.QuestionText = ConvertLaTeXToMathML(item.QuestionText);
            if (item.AnswerExplanation != null && item.AnswerExplanation.Contains(@"\begin{math}"))
                item.AnswerExplanation = ConvertLaTeXToMathML(item.AnswerExplanation);
            foreach (var answer in item.Answers)            
            {
                if (answer.AnswerText != null && answer.AnswerText.Contains(@"\begin{math}"))
                    answer.AnswerText = ConvertLaTeXToMathML(answer.AnswerText);
            }
        }
    }
    private string ConvertLaTeXToMathML(string itemText)
    {
        // This is a very naive way to extract the LaTeX from the string, but it should work for our use case
        // We assume that the LaTeX is always in the form \begin{math} ... \end{math}
        var regex = new Regex(@"\\begin{math}(.*?)\\end{math}", RegexOptions.Singleline);
        var match = regex.Matches(itemText);
        if (match.Count == 0)
            throw new InvalidOperationException($"No correctLaTeX found in: {itemText}");

        foreach (Match m in match)
        {
            var tex = m.Groups[1].Value.Trim();

            _engine.SetValue("__tex", tex);
            var result = _engine.Evaluate("temml.renderToString(__tex)");

            if (result.AsString().Contains("temml-error"))
                throw new InvalidOperationException($"TeX conversion failed for: {tex}");
            itemText = itemText.Replace(m.Value, result.AsString());
        }
        return itemText.Replace("<math>", "<math xmlns=\"http://www.w3.org/1998/Math/MathML\">");
    }    
}
