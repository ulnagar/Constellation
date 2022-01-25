using Constellation.Application.DTOs;
using Constellation.Infrastructure.Templates.Views.Shared;
using System;
using System.Collections.Generic;

namespace Constellation.Infrastructure.Templates.Views.Emails.Lessons
{
    public class SecondWarningEmailViewModel : EmailLayoutBaseViewModel
    {
        public SecondWarningEmailViewModel()
        {
            Lessons = new List<LessonEntry>();
        }

        public string SchoolName { get; set; }
        public string Link { get; set; }
        public ICollection<LessonEntry> Lessons { get; set; }

        public class LessonEntry
        {
            public string Name { get; set; }
            public DateTime DueDate { get; set; }

            public static LessonEntry ConvertFromLessonItem(EmailDtos.LessonEmail.LessonItem lesson)
            {
                var viewModel = new LessonEntry
                {
                    Name = lesson.Name,
                    DueDate = lesson.DueDate
                };

                return viewModel;
            }
        }
    }
}
