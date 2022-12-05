namespace Constellation.Application.Features.Home.Commands;

using MediatR;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

public record GeneratePepTalkCommand : IRequest<string> { }

public class GeneratePepTalkCommandHandler : IRequestHandler<GeneratePepTalkCommand, string>
{
    // https://learn.microsoft.com/en-us/nuget/quickstart/create-and-publish-a-package-using-visual-studio?tabs=netcore-cli

    private readonly List<string> Phrase1 = new List<string>
    {
        "Champ,",
        "Fact:",
        "Everybody says",
        "Dang...",
        "Check it:",
        "Just saying...",
        "Superstar,",
        "Tiger,",
        "Self,",
        "Know this:",
        "News alert:",
        "Girl,",
        "Ace,",
        "Excuse me but",
        "Experts agree",
        "In my opinion,",
        "Hear ye, hear ye:",
        "Okay, listen up:"
    };

    private readonly List<string> Phrase2 = new List<string>
    {
        "the mere idea of you",
        "your soul",
        "your hair today",
        "everything you do",
        "your personal style",
        "every thought you have",
        "that sparkle in your eye",
        "your presence here",
        "what you got going on",
        "the essential you",
        "your life's journey",
        "that saucy personality",
        "your DNA",
        "that brain of yours",
        "your choice of attire",
        "the way you roll",
        "whatever your secret is",
        "all of y'all"
    };

    private readonly List<string> Phrase3 = new List<string>
    {
        "has serious game,",
        "rains magic,",
        "deserves the Nobel Prize,",
        "raises the roof,",
        "breeds miracles,",
        "is paying off big time,",
        "shows mad skills,",
        "just shimmers,",
        "is a national treasure,",
        "gets the party hopping,",
        "is the next big thing,",
        "roars like a lion,",
        "is a rainbow factory,",
        "is made of diamonds,",
        "makes birds sing,",
        "should be taught in school,",
        "makes my world go 'round,",
        "is 100% legit,"
    };

    private readonly List<string> Phrase4 = new List<string>
    {
        "24/7.",
        "can I get an amen?",
        "and that's a fact.",
        "so treat yourself.",
        "you feel me?",
        "that's just science.",
        "would I lie?",
        "for reals.",
        "mic drop.",
        "you hidden gem.",
        "snuggle bear.",
        "period.",
        "can you dig it?",
        "now let's dance.",
        "high five.",
        "say it again!",
        "according to CNN.",
        "so get used to it."
    };

    public async Task<string> Handle(GeneratePepTalkCommand request, CancellationToken cancellationToken)
    {
        var random = new Random();

        int index1 = random.Next(Phrase1.Count);
        int index2 = random.Next(Phrase2.Count);
        int index3 = random.Next(Phrase3.Count);
        int index4 = random.Next(Phrase4.Count);

        var seed = $"{(index1 + 1)}.{(index2 + 1)}.{(index3 + 1)}.{(index4 + 1)}";

        var result = $"{Phrase1[index1]} {Phrase2[index2]} {Phrase3[index3]} {Phrase4[index4]} ({seed})";

        return result;
    }
}
