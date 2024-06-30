namespace Constellation.Presentation.Shared.Pages.Shared.Components.PhoneNumber;

using Microsoft.AspNetCore.Mvc;

public sealed class PhoneNumberViewComponent : ViewComponent
{
    public IViewComponentResult Invoke(string number)
    {
        if (string.IsNullOrWhiteSpace(number))
            return View("PhoneNumber", number);

        number = number.Replace(" ", "");

        string formattedNumber = string.Empty;

        switch (number.Length)
        {
            case 10 when (number.StartsWith("04") || number.StartsWith("13")):
                formattedNumber = $"{number.Substring(0, 4)} {number.Substring(4, 3)} {number.Substring(7, 3)}";
                break;

            case 10 when number.StartsWith("0"):
                formattedNumber = $"({number.Substring(0, 2)}) {number.Substring(2, 4)} {number.Substring(6, 4)}";
                break;

            default:
                {
                    if (number.StartsWith("+"))
                    {
                        formattedNumber = $"{number.Substring(0, 4)}";

                        int length = number.Length - 7;

                        switch (length)
                        {
                            case var l when (length < 5):
                                formattedNumber += $" {number.Substring(4, l)}";
                                break;
                            case 5:
                                formattedNumber += $" {number.Substring(4, 2)} {number.Substring(6, 3)}";
                                break;
                            case 6:
                                formattedNumber += $" {number.Substring(4, 3)} {number.Substring(7, 3)}";
                                break;
                            case 7:
                                formattedNumber += $" {number.Substring(4, 3)} {number.Substring(7, 4)}";
                                break;
                            case 8:
                                formattedNumber += $" {number.Substring(4, 4)} {number.Substring(8, 4)}";
                                break;
                        }
                    }
                    else if (number.StartsWith("0011"))
                    {
                        formattedNumber = $"{number.Substring(0, 4)} {number.Substring(4, 3)}";

                        int length = number.Length - 7;

                        switch (length)
                        {
                            case var l when (length < 5):
                                formattedNumber += $" {number.Substring(7, l)}";
                                break;
                            case 5:
                                formattedNumber += $" {number.Substring(7, 2)} {number.Substring(8, 3)}";
                                break;
                            case 6:
                                formattedNumber += $" {number.Substring(7, 3)} {number.Substring(10, 3)}";
                                break;
                            case 7:
                                formattedNumber += $" {number.Substring(7, 3)} {number.Substring(10, 4)}";
                                break;
                            case 8:
                                formattedNumber += $" {number.Substring(7, 4)} {number.Substring(11, 4)}";
                                break;
                        }
                    }
                    else if (number.Length == 8)
                    {
                        formattedNumber = $"(02) {number.Substring(0, 4)} {number.Substring(4, 4)}";
                    }
                    else
                    {
                        formattedNumber = number;
                    }
                    break;
                }
        }

        return View("PhoneNumber", formattedNumber);
    } 
}
