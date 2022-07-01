using Constellation.Application.Extensions;
using System;

namespace Constellation.Application.DTOs
{
    public class AwardDetailDto
    {
        public string AwardCategory { get; set; }
        public string AwardType { get; set; }
        public DateTime AwardedDate { get; set; }
        public DateTime AwardCreated { get; set; }
        public string AwardSource { get; set; }
        public string SentralStudentId { get; set; }
        public string StudentId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }

        public static AwardDetailDto ConvertFromFileLine(string[] line)
        {
            var viewModel = new AwardDetailDto
            {
                AwardCategory = line[0].FormatField(),
                AwardType = line[1].FormatField(),
                AwardSource = line[4].FormatField(),
                SentralStudentId = line[5].FormatField(),
                StudentId = line[6].FormatField(),
                FirstName = line[7].FormatField(),
                LastName = line[8].FormatField()
            };

            var test = DateTime.Parse(line[2].FormatField());

            if (DateTime.TryParse(line[2].FormatField(), out DateTime awardedDate))
            {
                viewModel.AwardedDate = awardedDate;
            }

            if (DateTime.TryParse(line[3].FormatField(), out DateTime awardCreated))
            {
                viewModel.AwardCreated = awardCreated;
            }

            return viewModel;
        }
    }
}