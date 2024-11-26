namespace Constellation.Application.Common.PresentationModels;

using Constellation.Core.Shared;
using System;

public abstract class ModalContent
{
    public string Title { get; protected set; }
    public string Content { get; protected set; }
    public string ButtonText { get; protected set; }
    public string ButtonColour { get; protected set; }
    public bool ButtonHasLink { get; protected set; }
    public string ButtonLink { get; protected set; }
}

public sealed class ErrorDisplay : ModalContent
{
    public ErrorDisplay(Error error)
    {
        Title = "Error";
        Content = $@"<div>{error.Code}</div><span>{error.Message}</span>";
        ButtonColour = "btn-warning";
        ButtonText = "Ok";
        ButtonHasLink = false;
    }

    public ErrorDisplay(Error error, string link)
    {
        Title = "Error";
        Content = $@"<div>{error.Code}</div><span>{error.Message}</span>";
        ButtonText = "Ok";
        ButtonColour = "btn-warning";
        ButtonHasLink = true;
        ButtonLink = link;
    }
}

public sealed class ExceptionDisplay : ModalContent
{
    public ExceptionDisplay(Exception ex)
    {
        Title = "Exception";
        Content = $@"<div>{ex.GetType()}</div><span>{ex.Message}</span>";
        ButtonColour = "btn-warning";
        ButtonText = "Ok";
        ButtonHasLink = false;
    }

    public ExceptionDisplay(Exception ex, string link)
    {
        Title = "Exception";
        Content = $@"<div>{ex.GetType()}</div><span>{ex.Message}</span>";
        ButtonColour = "btn-warning";
        ButtonText = "Ok";
        ButtonHasLink = true;
        ButtonLink = link;
    }
}

public sealed class FeedbackDisplay : ModalContent
{
    public FeedbackDisplay(
        string title,
        string content,
        string buttonText,
        string buttonColour)
    {
        Title = title;
        Content = content;
        ButtonText = buttonText;
        ButtonColour = buttonColour;
        ButtonHasLink = false;
    }

    public FeedbackDisplay(
        string title,
        string content,
        string buttonText,
        string buttonColour,
        string buttonLink)
    {
        Title = title;
        Content = content;
        ButtonText = buttonText;
        ButtonColour = buttonColour;
        ButtonHasLink = true;
        ButtonLink = buttonLink;
    }
}
