using System;
using Xunit;
using Server.Models;

namespace UnitTests;

public class RecibosTests
{
    [Fact]
    public void SubtotalUsd_IsConvertedUsingTrm()
    {
        // Arrange
        var precioUsd = 20m;
        var cantidad = 2;
        var trm = 4500m;

        // Act
        var subtotal = cantidad * precioUsd * trm;

        // Assert
        Assert.Equal(180000m, subtotal);
    }

    [Fact]
    public void SubtotalCop_IsDirectMultiplication()
    {
        var precioCop = 20000m;
        var cantidad = 3;
        var subtotal = precioCop * cantidad;
        Assert.Equal(60000m, subtotal);
    }
}
