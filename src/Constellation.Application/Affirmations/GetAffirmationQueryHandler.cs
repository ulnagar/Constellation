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

    private readonly string[] _completeAffirmations = new string[]
    {
        "I am strong, smart, and capable. I am good enough and worthy.", "Today I will strive to do my best.",
        "Every day I am learning to become a better me.", "I am excited for the possibilities that today holds.",
        "I am fulfilling my purpose in this world.",
        "I will be productive and wise with my time today so I can achieve my goals.",
        "I will be present in every moment.",
        "Today is an opportunity to learn, grow, and to become a better version of myself.",
        "Every day I am better than before.", "My work enhances my life, but does not define who I am.",
        "I am focused and productive, making the most of each moment to achieve my goals.",
        "I attract success by consistently putting in my best effort and embracing continuous improvement.",
        "I am creating a work life that inspires and motivates me.",
        "I am talented, ambitious and making my dreams come true.",
        "I have the power to create all the success and prosperity I desire.",
        "I am willing to put in the work needed to achieve my professional goals.",
        "Today I will attract success, abundance and well-being.",
        "I am consistent in my work. Every day I deliver something of value.",
        "I am the author of my own success story.",
        "I can accomplish anything through hard work, dedication and focus.",
        "Success begins with my mindset and I choose to remain positive.",
        "My ability to conquer any challenge is limitless. My potential for success is infinite.",
        "I approach challenges with a positive attitude, knowing that each obstacle is an opportunity in disguise.",
        "My skills and abilities are constantly growing, allowing me to take on new tasks with confidence.",
        "I am a valuable member of my team, contributing my unique strengths to our collective success.",
        "I can overcome any challenge that comes my way.", "I use challenges to create new opportunities.",
        "I will focus on the things I can control and let go of what I can’t.", "I am imperfect and that is OK.",
        "I am thankful I have a job and a career path I am passionate about.",
        "I handle stressful situations with wisdom and clarity.", "I exhale negative energy and inhale calmness.",
        "Pressure situations bring out the best in me.",
        "I have navigated more stressful situations than this and will navigate my way through this one as well.",
        "I’m not feeling my best today and that is OK. The work will be there tomorrow and tomorrow is a new day.",
        "I am in control of my time and priorities, effectively balancing my work and personal life.",
        "I am worthy, no matter what I do or don’t accomplish today.", "To be positive is to be productive.",
        "I am letting go of my fears and anxiety.", "I will not let failures stop me from my goals.",
        "I am adept at handling any situation and will not let this overwhelm me.", "This too shall pass.",
        "It is okay to make mistakes. Every misstep is a learning opportunity.",
        "I choose to react positively to challenging situations.",
        "I will move beyond my anxiety through patience and courage.",
        "I will avoid negative self-talk and focus on self-care today.",
        "My creativity flows effortlessly, enabling me to find innovative solutions to any problem.",
        "Only good things will flow into my life today and I am claiming it.",
        "I am creating the life I want both personally and professionally.",
        "Today and every day, I will make time to work towards career and personal success.",
        "It’s okay to relax and reset after a long day. Rest is not a luxury, but a priority.",
        "I am comfortable saying ‘no’ in order to prioritize myself and those most important to me.",
        "I will keep my work at work.",
        "I know that living a balanced life includes taking time to do nothing at all.",
        "My job does not define my worth or who I am.", "I take time throughout the day for breaks.",
        "I make time for my family and friends because they are important to me and make me happy.",
        "I have a positive impact on people around me.", "I respect myself, so others may respect me.",
        "I am an inspiring mentor to those around me.", "I sense love and support from my colleagues.",
        "My team is creative and inspires me to achieve success.",
        "My colleagues and I work together to reach our goals.", "I can achieve anything with the help of my team.",
        "I am surrounded by smart, capable, and supportive individuals.", "My coworkers bring out the best in me.",
        "I radiate enthusiasm and positivity, inspiring those around me to do their best as well.",
        "I sense love and support from my colleagues.",
        "I treat everyone with respect, even when it’s challenging to do.",
        "Even in negative situations, I am committed to finding the positive within it.",
        "It is okay to set boundaries when dealing with difficult individuals.",
        "I cannot control others’ actions. I can only control how I respond and will do so in a productive and respectful manner.",
        "I will remain calm even when dealing with difficult people or situations.",
        "I can interact with difficult colleagues without taking things personally and getting upset.",
        "I will use this experience to deepen my understanding of others and improve my professional relationships.",
        "I am comfortable speaking honestly and openly with my colleagues and they feel the same way towards me.",
        "I am open to opportunities.", "I can do great things.",
        "I am confident to speak up and share my ideas and talent.",
        "I will step out of my comfort zone and view every day as a learning opportunity.",
        "I am deserving of recognition and advancement, and I confidently seize opportunities for growth."
    };

    private readonly string[] _quoteAffirmations = new string[]
    {
        @"""I am in the right place at the right time, doing the right thing"" - Louise Hay",
        @"""Conscious breathing is my anchor"" - Thích Nhất Hạnh",
        @"""You are loved just for being who you are, just for existing"" - Ram Dass",
        @"""The chance to love and be loved exists no matter where you are"" - Oprah",
        @"""Courage starts with showing up and letting ourselves be seen"" - Brené Brown",
        @"""Make way for the unprecedented and watch your reality rearrange itself"" - Yrsa Daley-Ward",
        @"""Open your heart and drink in this glorious day"" - Heather Havrilesky",
        @"""Am I good enough? Yes I am."" - Michelle Obama", @"""The perfect moment is this one"" - Jon Kabat-Zinn",
        @"""I am deliberate and afraid of nothing"" - Audre Lord",
        @"""Your life is about to be incredible"" - Tim Storey",
        @"""Who you are inside is what helps you make and do everything in life"" - Mister Rogers",
        @"""Your perspective is unique. It's important and it counts"" - Glenn Close",
        @"""Every day above earth is a good day"" - Ernest Hemingway",
        @"""Nothing can dim the light that shines from within"" - Maya Angelou",
        @"""You must do the things you think you cannot do"" - Eleanor Roosevelt",
        @"""The secret of attraction is to love yourself"" - Deepak Chopra",
        @"""Good riddance to decisions that don't support self-care, self-value, and self-worth"" - Oprah",
        @"""I am the greatest. I said that even before I knew I was."" - Muhammad Ali",
        @"""I say looking on the bright side of life never killed anybody"" - Jenny Han",
        @"""I'm better than I used to be. Better than I was yesterday. But hopefully not as good as I'll be tomorrow."" - Marianne Williamson",
        @"""I have never ever focused on the negative of things. I always look at the positive"" - Sonia Sotomayor",
        @"""I'm giving you permission to root for yourself and while you're at it root for those around you, too"" - Mindy Kaling",
        @"""We must accept finite disappointment, but never lose infinite hope."" - Martin Luther King Jr.",
        @"""Your life is already a miracle of change waiting for you to shape its destiny"" - Toni Morrison",
        @"""If you really think small, your world will be small. If you think big, your world will be big"" - Paulo Coelho",
        @"""Embrace the glorious mess that you are"" - Elizabeth Gilbert",
        @"""The ultimate truth of who you are is not I am this or I am that, but I am."" - Eckhard Tolle",
        @"""Gratitude is a celebration we are all invited to"" - Cleo Wade",
        @"""We must be willing to let go of the life we planned so as to have the life that is waiting for us"" - Joseph Campbell",
        @"""Nothing is impossible. The word itself says 'I'm possible!'."" - Audrey Hepburn",
        @"""The only courage you ever need is the courage to fulfill the dreams of your own life"" - Oprah",
        @"""Failure is just another way to learn how to do something right"" - Marian Wright Edelman",
        @"""The emotion that can break your heart is sometimes the very one that heals it"" - Nicholas Sparks",
        @"""Your crown has been bought and paid for. Put it on your head and wear it"" - Maya Angelou",
        @"""Everything passes if you learn to hold things lightly"" - Oprah",
        @"""Write it on your heart that every day is the best day in the year"" - Ralph Waldo Emerson",
        @"""Hold up your head! You were note made for failure, you were made for victory"" - Anne Gilchrist",
        @"""If you have good thoughts they will shine out of your face like sunbeams and you will always look lovely"" - Roald Dahl",
        @"""There is nothing either good or bad, but thinking makes it so"" - William Shakespeare"
    };

#pragma warning disable CA5394
    public async Task<Result<string>> Handle(GetAffirmationQuery request, CancellationToken cancellationToken)
    {
        int dateTicks = (int)DateTime.Today.Ticks;
        bool success = int.TryParse(request.UserId, out int userId);

        Random random = !success ? new(dateTicks) : new(userId + dateTicks);

        double typeSelector = random.NextDouble();

        return typeSelector switch
        {
            < 0.01 => GetMultipartAffirmation(random),
            >= 0.01 and < 0.45 => _completeAffirmations[random.Next(_completeAffirmations.Length)],
            >= 0.45 or _ => _quoteAffirmations[random.Next(_quoteAffirmations.Length)]
        };
    }

    private string GetMultipartAffirmation(Random random)
    {
        int index1 = random.Next(_phrase1.Length);
        int index2 = random.Next(_phrase2.Length);
        int index3 = random.Next(_phrase3.Length);
        int index4 = random.Next(_phrase4.Length);

        string seed = $"{(index1)}.{(index2)}.{(index3)}.{(index4)}";

        string result = $"{_phrase1[index1]} {_phrase2[index2]} {_phrase3[index3]} {_phrase4[index4]} ({seed})";

        return result;
    }
#pragma warning restore CA5394
}