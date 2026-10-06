/* 
 * This program has been developed by students from the bachelor Computer Science at 
 * Utrecht University within the Software Project course.
 * © Copyright Utrecht University (Department of Information and Computing Sciences)
 * Licensed under the MIT License. See the LICENSE file in the project root for details.
 */

using CsvHelper.Configuration;
using Models;

namespace DatabasePopulator;

internal class CharacterMap : ClassMap<Character>
{
    public CharacterMap()
    {
        Map(m => m.Name).Name("Name");
    }
}