using System;
using VirtoCommerce.Skyflow.Core.Models;
using Xunit;

namespace VirtoCommerce.Skyflow.Tests;

[Trait("Category", "Unit")]
public class SkyflowCardTest
{
    [Theory]
    [InlineData("10", "20", true)]
    [InlineData("01", "28", false)]
    [InlineData("02", "98", false)]
    public void MonthYearTest(string month, string year, bool inactive)
    {
        var card = new SkyflowCard { ExpiryMonth = month, ExpiryYear = year, };

        Assert.Equal(inactive, card.Inactive);
    }

    [Theory]
    [InlineData("10/20", true)]
    [InlineData("01/2028", false)]
    [InlineData("02/98", false)]
    public void ExpireDateTest(string expireDate, bool inactive)
    {
        var card = new SkyflowCard { CardExpiration = expireDate };

        Assert.Equal(inactive, card.Inactive);
    }

    [Fact]
    public void CurrentMonthTests()
    {
        var month = DateTime.UtcNow.Month.ToString("00");
        var year = DateTime.UtcNow.Year.ToString("00");
        var card = new SkyflowCard { ExpiryMonth = month, ExpiryYear = year, };

        Assert.False(card.Inactive);
    }


}
