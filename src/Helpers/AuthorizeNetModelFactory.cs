using Dynamicweb.Core;
using Dynamicweb.Ecommerce.CheckoutHandlers.AuthorizeNetApi.Models;
using Dynamicweb.Ecommerce.Orders;
using System.Collections.Generic;
using System.Linq;

namespace Dynamicweb.Ecommerce.CheckoutHandlers.AuthorizeNetApi.Helpers;

internal static class AuthorizeNetModelFactory
{
    private const string FallbackCustomerFirstName = "Customer";
    private const string FallbackCustomerLastName = "Name";

    public static NameAndAddressType CreateShipAddress(Order order)
    {
        string firstName = GetCustomerFirstNameOrFallback([
            order.DeliveryFirstName,
            order.CustomerFirstName,
            order.DeliveryName,
            order.CustomerName
        ]);

        string lastName = GetCustomerLastNameOrFallback([
            order.DeliverySurname,
            order.CustomerSurname,
            order.DeliveryName,
            order.CustomerName
        ]);

        return new()
        {
            FirstName = StringHelper.Crop(firstName, 50),
            LastName = StringHelper.Crop(lastName, 50),
            Company = StringHelper.Crop(order.DeliveryCompany ?? order.CustomerCompany, 50),
            Address = StringHelper.Crop(order.DeliveryAddress, 60),
            City = StringHelper.Crop(order.DeliveryCity, 40),
            State = StringHelper.Crop(order.DeliveryRegion, 40),
            Zip = StringHelper.Crop(order.DeliveryZip, 20),
            Country = StringHelper.Crop(order.DeliveryCountryCode, 60)
        };
    }

    public static CustomerAddressType CreateBillAddress(Order order)
    {
        string firstName = GetCustomerFirstNameOrFallback([
            order.CustomerFirstName,
            order.CustomerName,
        ]);

        string lastName = GetCustomerLastNameOrFallback([
            order.CustomerSurname,
            order.CustomerName
        ]);

        return new()
        {
            FirstName = StringHelper.Crop(firstName, 50),
            LastName = StringHelper.Crop(lastName, 50),
            Company = StringHelper.Crop(order.CustomerCompany, 50),
            Address = StringHelper.Crop(order.CustomerAddress, 60),
            City = StringHelper.Crop(order.CustomerCity, 40),
            State = StringHelper.Crop(order.CustomerRegion, 40),
            Zip = StringHelper.Crop(order.CustomerZip, 20),
            Country = StringHelper.Crop(order.CustomerCountryCode, 60),
            PhoneNumber = StringHelper.Crop(order.CustomerPhone, 25),
            Email = StringHelper.Crop(order.CustomerEmail, 255)
        };
    }

    public static LineItems CreateLineItems(Order order)
    {
        var result = new List<LineItem>();

        foreach (OrderLine line in order.OrderLines)
        {
            if (line.OrderLineType is OrderLineType.Product or OrderLineType.Fixed)
            {
                result.Add(new()
                {
                    ItemId = line.Id,
                    Name = StringHelper.Crop(line.Product.Name, 31),
                    Description = StringHelper.Crop(line.Product.ShortDescription, 255),
                    Quantity = Converter.ToDouble(line.Quantity),
                    UnitPrice = Ecommerce.Services.Currencies.Round(order.Currency, line.Price.Price),
                });
            }
        }

        return new()
        {
            LineItem = result
        };
    }

    private static string GetCustomerFirstNameOrFallback(IEnumerable<string> fields)
    {
        string? firstName = fields
            .Select(field => field?.Split(' ').FirstOrDefault())
            .FirstOrDefault(field => !string.IsNullOrWhiteSpace(field));

        if (firstName is not null)
            return firstName;

        return FallbackCustomerFirstName;
    }

    private static string GetCustomerLastNameOrFallback(IEnumerable<string> fields)
    {
        string? lastName = fields
            .Select(field => string.Join(" ", field?.Split(' ').Skip(1) ?? []))
            .FirstOrDefault(field => !string.IsNullOrWhiteSpace(field));

        if (lastName is not null)
            return lastName;

        return FallbackCustomerLastName;
    }

}