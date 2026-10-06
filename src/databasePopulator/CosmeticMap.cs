/* 
 * This program has been developed by students from the bachelor Computer Science at 
 * Utrecht University within the Software Project course.
 * © Copyright Utrecht University (Department of Information and Computing Sciences)
 * Licensed under the MIT License. See the LICENSE file in the project root for details.
 */

using CsvHelper.Configuration;
using Models;

namespace DatabasePopulator;

internal class CosmeticMap : ClassMap<Cosmetic>
{
    public CosmeticMap()
    {
        Map(m => m.Name).Name("Name");
        Map(m => m.Price).Name("Price");
        Map(m => m.ClothingType).Name("ClothingType");
        Map(m => m.RiveFile).Name("RiveFile");
        Map(m => m.RiveArtboard).Name("RiveArtboard");
        Map(m => m.RiveStateMachine).Name("RiveStateMachine");
        Map(m => m.RiveInput).Name("RiveInput");
        Map(m => m.RiveInputValue).Name("RiveInputValue");
        Map(m => m.DefaultOwned).Name("DefaultOwned");
        Map(m => m.IconFile).Name("IconFile");
    }
}
