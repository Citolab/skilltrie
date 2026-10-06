/* 
 * This program has been developed by students from the bachelor Computer Science at 
 * Utrecht University within the Software Project course.
 * © Copyright Utrecht University (Department of Information and Computing Sciences)
 * Licensed under the MIT License. See the LICENSE file in the project root for details.
 */

using System.Xml.Linq;
using Models;

namespace API.Tools.QTIConverting;

/// <summary>
/// Class to convert <see cref="Item"/>s to XML.
/// Can also generate an assessment.xml for multiple <see cref="Item"/>s.
/// </summary>
public static class QTIXML
{
    /// <summary>
    /// Converts an <see cref="Item"/> to a QTI XML item ('qti-assessment-item' element).
    /// </summary>
    /// <param name="item">The <see cref="Item"/> to be converted to XML.</param>
    /// <returns>A string containing the XML representation of the item.</returns>
    public static string Convert(Item item)
    {
        string templatePath = item.Type switch
        {
            Item.ItemType.MultipleChoice => "./XMLpresets/multiplechoice.xml",
            Item.ItemType.Numerical => "./XMLpresets/numerical.xml",
            Item.ItemType.Open => "./XMLpresets/open.xml",
        };

        XDocument doc = XDocument.Load(Path.Combine(AppContext.BaseDirectory, templatePath));

        ItemBuilder builder = item.Type switch
        {
            Item.ItemType.MultipleChoice => new MutltipleChoiceItemBuilder(doc, item),
            Item.ItemType.Numerical => new OpenItemBuilder(doc, item),
            Item.ItemType.Open => new OpenItemBuilder(doc, item)
        };

        if (item.Answers.All(a => !a.Correct) && item.Type == Item.ItemType.MultipleChoice)
            throw new ItemNoCorrectAnswers($"Item ${item.Id} has no correct answers");

        if (item.Answers.Any(a => a.AnswerIdentifier == "" || a.AnswerIdentifier == null))
            throw new ItemNoAnswerIdentifier($"Item ${item.Id} contains answers without identifiers");

        builder.Setup();
        builder.SetQuestionText();
        builder.SetAnwsers();
        builder.SetFeedback();

        return builder.Result;
    }

    /// <summary>
    /// Generates an assessment XML string by loading a preset assessment.xml template and adding references to the provided items.
    /// Each item is represented as a 'qti-assessment-item-ref' element with its identifier and href attributes set.
    /// </summary>
    /// <param name="items">An array of <see cref="Item"/> objects to be included in the assessment.</param>
    /// <returns>
    /// A string containing the modified assessment XML with item references added.
    /// </returns>
    public static string GenerateAssesment(Item[] items)
    {
        XDocument doc = XDocument.Load(Path.Combine(AppContext.BaseDirectory, "XMLpresets", "assessment.xml"));

        XNamespace ns = doc.Root!.Name.Namespace;

        XElement assessmentSection = doc.Descendants(ns + "qti-assessment-section").FirstOrDefault()!;

        XElement[] itemRefs = items.Select(item =>
        {
            XElement itemRef = new XElement(ns + "qti-assessment-item-ref");

            itemRef.SetAttributeValue("identifier", $"item-{item.Id}");
            itemRef.SetAttributeValue("href", $"api/qti/get-item/{item.Id}");

            return itemRef;
        }).ToArray();

        assessmentSection.Add(itemRefs);

        return doc.ToString(SaveOptions.DisableFormatting);
    }
}
