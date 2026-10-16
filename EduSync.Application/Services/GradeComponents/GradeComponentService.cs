using EduSync.Application.Abstractions.Persistence;
using EduSync.Application.Common.Exceptions;
using EduSync.Application.DTOs.GradeComponents;
using EduSync.Domain.Entities;

namespace EduSync.Application.Services.GradeComponents;

public sealed class GradeComponentService(IGradeComponentRepository components) : IGradeComponentService
{
    public Task<GradeComponentDto> CreateAsync(
        CreateGradeComponentCommand command, CancellationToken cancellationToken = default)
    {
        var name = command.Name?.Trim();
        if (command.CourseSectionId == Guid.Empty || string.IsNullOrWhiteSpace(name) || name.Length > 100
            || command.Weight is <= 0 or > 100 || command.MaxScore is <= 0 or > 999.99m
            || decimal.Round(command.Weight, 2) != command.Weight
            || decimal.Round(command.MaxScore, 2) != command.MaxScore)
        {
            throw new GradeComponentException(GradeComponentError.InvalidData,
                "Name is required (max 100 characters). Weight must be greater than 0 and at most 100; " +
                "maxScore must be greater than 0 and at most 999.99. Use at most two decimal places.");
        }

        return components.WithSectionLockAsync(command.CourseSectionId, async () =>
        {
            if (!await components.SectionExistsAsync(command.CourseSectionId, cancellationToken))
            {
                throw new GradeComponentException(GradeComponentError.SectionNotFound, "Course section does not exist.");
            }

            if (await components.NameExistsAsync(command.CourseSectionId, name, cancellationToken))
            {
                throw new GradeComponentException(GradeComponentError.AlreadyExists,
                    "Grade component name already exists in this course section.");
            }

            var totalWeight = await components.GetTotalWeightAsync(command.CourseSectionId, cancellationToken);
            if (totalWeight + command.Weight > 100)
            {
                throw new GradeComponentException(GradeComponentError.WeightExceeded,
                    "Total grade component weight cannot exceed 100 percent.");
            }

            var component = new GradeComponentEntity
            {
                Id = Guid.NewGuid(),
                CourseSectionId = command.CourseSectionId,
                Name = name,
                Weight = command.Weight,
                MaxScore = command.MaxScore
            };
            await components.CreateAsync(component, cancellationToken);
            return new GradeComponentDto(component.Id, component.CourseSectionId,
                component.Name, component.Weight, component.MaxScore);
        }, cancellationToken);
    }
}
