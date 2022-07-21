using AutoMapper;
using Constellation.Application.Common.Mapping;
using Constellation.Core.Models;
using System;

namespace Constellation.Application.Features.Subject.Assignments.Models
{
    public class AssignmentSubmissionForList : IMapFrom<CanvasAssignmentSubmission>
    {
        public Guid Id { get; set; }
        public string StudentId { get; set; }
        public string StudentName { get; set; }
        public DateTime SubmittedDate { get; set; }
        public int Attempt { get; set; }

        public void Mapping(Profile profile)
        {
            profile.CreateMap<CanvasAssignmentSubmission, AssignmentSubmissionForList>()
                .ForMember(dest => dest.StudentName, opt => opt.MapFrom(src => src.Student.DisplayName));
        }
    }
}
