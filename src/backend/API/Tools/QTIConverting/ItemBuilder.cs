/* 
 * This program has been developed by students from the bachelor Computer Science at 
 * Utrecht University within the Software Project course.
 * © Copyright Utrecht University (Department of Information and Computing Sciences)
 * Licensed under the MIT License. See the LICENSE file in the project root for details.
 */

using System.Text.RegularExpressions;
using System.Xml.Linq;
using Models;

namespace API.Tools.QTIConverting;

/// <summary>
/// Quasi-Builder design pattern.
/// Allows for incrementally configuring / adding to
/// a XML preset, given a preset and an <see cref="Item" />
/// </summary>
abstract class ItemBuilder
{
    public string Result
    {
        get { return _doc.ToString(SaveOptions.DisableFormatting); }
    }
    protected readonly XDocument _doc;
    protected readonly XNamespace _ns;
    protected readonly Item _item;

    public ItemBuilder(XDocument template, Item item)
    {
        _doc = template;

        // when searching for elements we need to 'prepend' the namespace
        // to each query, or the System.Xml.Linq library will yield no results
        _ns = _doc.Root!.Name.Namespace;

        _item = item;
    }

    /// <summary>
    /// Configures global variables in the preset.
    /// </summary>
    public virtual void Setup()
    {
        /* ### add miscellaneous metadata ### */

        _doc.Root!.SetAttributeValue("title", $"item-{_item.Id}");
        _doc.Root!.SetAttributeValue(XNamespace.Xml + "lang", _item.Lang);
        _doc.Root!.SetAttributeValue("identifier", $"item-{_item.Id}");
    }

    /// <summary>
    /// Injects the item's question text into the preset.
    /// </summary>
    public virtual void SetQuestionText()
    {
        /* ### add question text ### */

        XElement questionBody = _doc.Descendants(_ns + "qti-item-body").FirstOrDefault()!;

        string rawHtml = _item.QuestionText;
        // wrap questiontext in a root element in case there are multiple top-level elements
        // (the library doesn't like that)
        XElement wrapper = XElement.Parse($"<Root>{rawHtml}</Root>");

        questionBody.Add(wrapper.Elements());
    }

    /// <summary>
    /// Configures the contents of a feedback modal.
    /// Does NOT configure the conditions for showing this modal
    /// </summary>
    public virtual void SetFeedback()
    {
        /* ### add answer explanation ### */

        if (_item.AnswerExplanation != "" && _item.AnswerExplanation != null)
        {
            XElement feedbackModal = _doc.Descendants(_ns + "qti-modal-feedback").FirstOrDefault()!;
            feedbackModal
                .Descendants(_ns + "qti-content-body")
                .FirstOrDefault()!
                .Add(XElement.Parse($"<Root>{_item.AnswerExplanation}</Root>"));
        }
    }

    /// <summary>
    /// Abstract. Should configure 1. what the correct answer is & 2. how the
    /// correct answers should be graded (= 'qti-response-processing')
    /// </summary>
    public abstract void SetAnwsers();
}

/// <summary>
/// Concrete builder implementation of the
/// <see cref="ItemBuilder"/> for open items
/// </summary>
/// <param name="doc">The parsed XML template for an open item</param>
/// <param name="item">The open item</param>
class OpenItemBuilder(XDocument doc, Item item) : ItemBuilder(doc, item)
{
    private ItemAnswer Answer
    {
        get { return _item.Answers.First(); }
    }

    /// <summary>
    /// Injects the correct answer into the template along with
    /// setting the appropiate identifiers to enable response processing
    /// </summary>
    public override void SetAnwsers()
    {
        /* ### set identifier attributes ### */

        // set all undeclared identifiers in qti-response-processing to point to the qti-response-declaration.
        // see the presets folder for more comments
        XElement responseProcessing = _doc.Descendants(_ns + "qti-response-processing").FirstOrDefault()!;
        foreach (XElement el in responseProcessing.Descendants())
        {
            if (el.Attribute("identifier")?.Value == "")
            {
                el.SetAttributeValue("identifier", Answer.AnswerIdentifier);
            }
        }

        // for open/numerical items, the AnswerIdentifier is supposed to be a reference to
        // qti-response-declaration. So, AnswerIdentifier is the identifier of qti-response-declaration
        XElement responseDeclaration = _doc.Descendants(_ns + "qti-response-declaration").FirstOrDefault()!;
        responseDeclaration.SetAttributeValue("identifier", Answer.AnswerIdentifier);

        /* ### add correct response value ### */

        XElement correctResponseValue = responseDeclaration.Descendants(_ns + "qti-value").FirstOrDefault()!;
        correctResponseValue.SetValue(Answer.AnswerText!); // answertext cannot be null for numerical type

        /* ### brick the response processing if there is no 'correct' response anyway ### */

        // matches N/A, NA, na, "", " ", etc.
        string pattern = @"[Nn]\/?[Aa]|""\s*?""|^\s+$";

        if (Regex.IsMatch(Answer.AnswerText ?? " ", pattern)) responseProcessing.Remove();
    }

    public override void SetFeedback()
    {
        /* ### remove feedback if there is no 'correct' response anyway ### */

        // matches N/A, NA, na, "", " ", etc.
        string pattern = @"[Nn]\/?[Aa]|""\s*?""|^\s+$";

        if (Regex.IsMatch(Answer.AnswerText ?? " ", pattern))
        {
            _doc.Descendants(_ns + "qti-modal-feedback").FirstOrDefault()!.Remove();
        }
        else
        {
            base.SetFeedback();
        }
    }
}

/// <summary>
/// Concrete builder implementation of the
/// <see cref="ItemBuilder"/> for multiple choice items
/// </summary>
/// <param name="doc">The parsed XML template for a multiple choice item</param>
/// <param name="item">The multiple choice item</param>
class MutltipleChoiceItemBuilder(XDocument doc, Item item) : ItemBuilder(doc, item)
{
    /// <summary>
    /// Calls base.SetQuestionText(), also explicitly adds the qti-choice-interaction
    /// to the item body
    /// </summary>
    public override void SetQuestionText()
    {
        base.SetQuestionText();

        /* ### Adding the multiple choice 'form' to the qti-item-body ### */

        XElement questionBody = _doc.Descendants(_ns + "qti-item-body").FirstOrDefault()!;

        XElement choiceInteraction = new(_ns + "qti-choice-interaction");
        choiceInteraction.SetAttributeValue("response-identifier", "RESPONSE"); // points to qti-response-declaration

        XElement[] simpleChoices = _item.Answers.Select(answer =>
        {
            XElement simpleChoice = new(_ns + "qti-simple-choice");

            simpleChoice.SetAttributeValue("identifier", answer.AnswerIdentifier);

            XElement wrapper = XElement.Parse($"<Root>{answer.AnswerText}</Root>");
            simpleChoice.Add(wrapper.Elements());

            return simpleChoice;
        }).ToArray();

        choiceInteraction.Add(simpleChoices);

        questionBody.Add(choiceInteraction);
    }

    /// <summary>
    /// Configures the qti-response-declaration for multiple choice items.
    /// </summary>
    public override void SetAnwsers()
    {
        XElement responseDeclaration = _doc.Descendants(_ns + "qti-response-declaration").FirstOrDefault()!;
        responseDeclaration.SetAttributeValue("identifier", "RESPONSE");

        XElement correctResponse = new(_ns + "qti-correct-response");

        XElement[] responseValues =
            _item.Answers
                .Where(answer => answer.Correct)
                .Select(answer => new XElement(_ns + "qti-value", answer.AnswerIdentifier))
                .ToArray();

        correctResponse.Add(responseValues);

        responseDeclaration.Add(correctResponse);
    }
}
