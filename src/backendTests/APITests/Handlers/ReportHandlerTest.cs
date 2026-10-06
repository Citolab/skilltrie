/* 
 * This program has been developed by students from the bachelor Computer Science at 
 * Utrecht University within the Software Project course.
 * © Copyright Utrecht University (Department of Information and Computing Sciences)
 * Licensed under the MIT License. See the LICENSE file in the project root for details.
 */

using API.Handlers;
using Models;

namespace APITests.Handlers;

public class ReportHandlerTest
{
    private readonly  ReportHandler _reportHandler = new ReportHandler();

    [Fact(DisplayName = "Creates new report on valid inputs")]
    public void CreateReportOnValidInputs()
    {
        int itemId = 1;
        ItemError error = ItemError.AnswersWrong;
        int userId = 88888;

        var res = _reportHandler.CreateReportForDb(itemId, error, userId);

        Assert.NotNull(res);
        Assert.Equal(itemId, res.ItemId);
        Assert.Equal(error, res.ItemError);
        Assert.Equal(userId, res.UserId);
    }
}
