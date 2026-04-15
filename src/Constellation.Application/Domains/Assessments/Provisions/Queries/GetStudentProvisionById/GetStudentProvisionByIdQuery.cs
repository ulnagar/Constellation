namespace Constellation.Application.Domains.Assessments.Provisions.Queries.GetStudentProvisionById;

using Abstractions.Messaging;
using Core.Models.Assessments.Identifiers;
using Models;

public sealed record GetStudentProvisionByIdQuery(
    StudentProvisionId Id)
    : IQuery<StudentProvisionResponse>;