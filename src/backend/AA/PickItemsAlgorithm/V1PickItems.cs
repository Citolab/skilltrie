/* 
 * This program has been developed by students from the bachelor Computer Science at 
 * Utrecht University within the Software Project course.
 * © Copyright Utrecht University (Department of Information and Computing Sciences)
 * Licensed under the MIT License. See the LICENSE file in the project root for details.
 */

using AA.PickItemsAlgorithm.ItemPickers;
using AA.PickItemsAlgorithm.ItemPickerChains;
using Models;

namespace AA.PickItemsAlgorithm;

[Algorithm("V1PickItems")]
public class V1PickItems(AppDbContext appDbContext) : IPickItemsAlgorithm
{
    private readonly Random _rng = new ();

    public List<Item> PickItems(ICollection<Item> availableItems)
    {
        var activeSetting = appDbContext.Settings
            .OrderByDescending(s => s.LastActive)
            .FirstOrDefault();

        int itemAmount = activeSetting?.LevelSize ?? 10;
        double aiFactor = activeSetting?.AiFactor ?? 0.2d;

        int amountAiGeneratedQuestions = (int)(aiFactor * itemAmount);
        itemAmount -= amountAiGeneratedQuestions;
        return _pickNormalAndAi(availableItems, itemAmount, amountAiGeneratedQuestions);
    }

    private List<Item> _pickNormalAndAi(ICollection<Item> availableItems, int normalAmount, int aiAmount)
    {
        ItemPickerChain itemPickerChain = new ItemPickerChain();
        itemPickerChain.AddPicker(new NonGeneratedItemPicker());

        ItemPickerChain aiItemPickerChain = new ItemPickerChain();
        aiItemPickerChain.AddPicker(new AiGeneratedItemPicker());
        aiItemPickerChain.AddPicker(new NonGeneratedItemPicker());

        var normalItems = itemPickerChain.PickNItems(availableItems, normalAmount);
        var aiGeneratedItems = aiItemPickerChain.PickNItems(
            availableItems.Except(normalItems).ToList(), 
            aiAmount
        );
        return normalItems
            .Concat(aiGeneratedItems)
            .OrderBy(_ => _rng.Next())
            .ToList();
    }
}