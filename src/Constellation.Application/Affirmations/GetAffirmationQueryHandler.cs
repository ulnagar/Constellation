namespace Constellation.Application.Affirmations;

using Abstractions.Messaging;
using Core.Shared;
using System;
using System.Threading;
using System.Threading.Tasks;

internal sealed class GetAffirmationQueryHandler 
    : IQueryHandler<GetAffirmationQuery, string>
{
    // https://learn.microsoft.com/en-us/nuget/quickstart/create-and-publish-a-package-using-visual-studio?tabs=netcore-cli

    private readonly string[] _phrase1 = new string[]
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

    private readonly string[] _phrase2 = new string[]
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

    private readonly string[] _phrase3 = new string[]
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

    private readonly string[] _phrase4 = new string[]
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

    public async Task<Result<string>> Handle(GetAffirmationQuery request, CancellationToken cancellationToken)
    {
        Random random = new Random();
        bool multipart = random.Next(0, 2) == 1;

        if (multipart)
        {
            bool success = int.TryParse(request.UserId, out int userId);
            int dateTicks = (int)DateTime.Today.Ticks;


            random = !success ? new Random(dateTicks) : new Random(userId + dateTicks);

            int index1 = random.Next(_phrase1.Length);
            int index2 = random.Next(_phrase2.Length);
            int index3 = random.Next(_phrase3.Length);
            int index4 = random.Next(_phrase4.Length);

            string seed = $"{(index1)}.{(index2)}.{(index3)}.{(index4)}";

            string result = $"{_phrase1[index1]} {_phrase2[index2]} {_phrase3[index3]} {_phrase4[index4]} ({seed})";

            return result;
        }

        return "";
    }
}