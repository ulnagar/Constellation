namespace Constellation.Application.Common.PresentationModels;

using Constellation.Core.Shared;
using System;
using System.Collections.Generic;

public abstract class ModalContent
{
    private readonly List<ModalContentButton> _buttons = [];

    public string Title { get; protected set; }
    public string Content { get; protected set; }
    public IReadOnlyList<ModalContentButton> Buttons => _buttons.AsReadOnly();
    public bool ButtonHasLink { get; protected set; }

    public sealed class ModalContentButton
    {
        public ModalContentButton(
            string text,
            string colour,
            string link)
        {
            Text = text;
            Colour = colour;
            Link = link;
        }

        public string Text { get; private set; }
        public string Colour { get; private set; }
        public string Link { get; private set; }
    }

    public void AddButton(
        string text,
        string colour,
        string link)
    {
        _buttons.Add(new(
            text,
            colour,
            link));
    }
}

public sealed class ErrorDisplay : ModalContent
{
    private ErrorDisplay() { }

    public static ErrorDisplay Create(
        Error error)
    {
        ErrorDisplay modal = new()
        {
            Title = "Error", 
            Content = $@"<div>{error.Code}</div><span>{error.Message}</span>"
        };

        modal.AddButton("Ok", "btn-warning", string.Empty);

        return modal;
    }

    public static ErrorDisplay Create(
        Error error,
        string link)
    {
        ErrorDisplay modal = new()
        {
            Title = "Error",
            Content = $@"<div>{error.Code}</div><span>{error.Message}</span>"
        };

        modal.AddButton("Ok", "btn-warning", link);

        return modal;
    }
}

public sealed class ExceptionDisplay : ModalContent
{
    private ExceptionDisplay() { }

    public static ExceptionDisplay Create(
        Exception ex)
    {
        ExceptionDisplay modal = new()
        {
            Title = "Exception", 
            Content = $@"<div>{ex.GetType()}</div><span>{ex.Message}</span>"
        };

        modal.AddButton("Ok", "btn-warning", string.Empty);

        return modal;
    }

    public static ExceptionDisplay Create(
        Exception ex,
        string link)
    {
        ExceptionDisplay modal = new()
        {
            Title = "Exception",
            Content = $@"<div>{ex.GetType()}</div><span>{ex.Message}</span>"
        };

        modal.AddButton("Ok", "btn-warning", link);

        return modal;
    }
}

public sealed class FeedbackDisplay : ModalContent
{
    private FeedbackDisplay() { }

    public static FeedbackDisplay Create(
        string title,
        string content,
        string buttonText,
        string buttonColour)
    {
        FeedbackDisplay modal = new FeedbackDisplay()
        {
            Title = title,
            Content = content
        };

        modal.AddButton(buttonText, buttonColour, string.Empty);

        return modal;
    }

    public static FeedbackDisplay Create(
        string title,
        string content,
        string buttonText,
        string buttonColour,
        string buttonLink)
    {
        FeedbackDisplay modal = new FeedbackDisplay()
        {
            Title = title,
            Content = content
        };

        modal.AddButton(buttonText, buttonColour, buttonLink);

        return modal;
    }

    public static FeedbackDisplay Create(
        string title,
        string content,
        List<(string Text, string Colour, string Link)> buttons)
    {
        FeedbackDisplay modal = new FeedbackDisplay()
        {
            Title = title,
            Content = content
        };

        foreach (var button in buttons)
        {
            modal.AddButton(button.Text, button.Colour, button.Link);
        }
        
        return modal;
    }
}
