using System;
using VirtoCommerce.Skyflow.Core.Models;
using Xunit;

namespace VirtoCommerce.Skyflow.Tests;

[Trait("Category", "Unit")]
public class SkyflowCardTest
{
    [Theory]
    [InlineData("10", "20", false)]
    [InlineData("01", "28", true)]
    [InlineData("02", "98", true)]
    public void MonthYearTest(string month, string year, bool isActive)
    {
        var card = new SkyflowCard { ExpiryMonth = month, ExpiryYear = year, };

        Assert.Equal(isActive, card.Active);
    }

    [Theory]
    [InlineData("10/20", false)]
    [InlineData("01/2028", true)]
    [InlineData("02/98", true)]
    public void ExpireDateTest(string expireDate, bool isActive)
    {
        var card = new SkyflowCard { CardExpiration = expireDate };

        Assert.Equal(isActive, card.Active);
    }

    [Fact]
    public void CurrentMonthTests()
    {
        var month = DateTime.UtcNow.Month.ToString("00");
        var year = DateTime.UtcNow.Year.ToString("00");
        var card = new SkyflowCard { ExpiryMonth = month, ExpiryYear = year, };

        Assert.True(card.Active);
    }


}
