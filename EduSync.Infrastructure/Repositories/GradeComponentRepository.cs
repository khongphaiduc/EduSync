using EduSync.Application.Abstractions.Persistence;
using EduSync.Application.Common.Exceptions;
using EduSync.Application.DTOs.GradeComponents;
using EduSync.Domain.Entities;
using EduSync.Infrastructure.Mappings;
using EduSync.Infrastructure.Models;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace EduSync.Infrastructure.Repositories;

public sealed class GradeComponentRepository(MyFschoolDemoContext context) : IGradeComponentRepository
{
    public async Task<T> WithSectionLockAsync<T>(
        Guid sectionId, Func<Task<T>> action, CancellationToken cancellationToken)
    {
        await using var transaction = await context.Database.BeginTransactionAsync(cancellationToken);
        var resource = $"EduSync.GradeComponents.{sectionId:N}";
        try
        {
            await context.Database.ExecuteSqlInterpolatedAsync($"""
                DECLARE @result int;
                EXEC @result = sp_getapplock @Resource = {resource},
                    @LockMode = 'Exclusive', @LockOwner = 'Transaction', @LockTimeout = 10000;
                IF @result < 0 THROW 51002, 'Could not acquire grade component lock.', 1;
                """, cancellationToken);
        }
        catch (SqlException exception) when (exception.Number == 51002)
        {
            throw new GradeComponentException(GradeComponentError.Busy,
                "Grade components are being updated. Please retry.");
        }

        var result = await action();
        await transaction.CommitAsync(cancellationToken);
        return result;
    }

    public Task<bool> SectionExistsAsync(Guid sectionId, CancellationToken cancellationToken) =>
        context.CourseSections.AnyAsync(section => section.Id == sectionId, cancellationToken);

    public Task<bool> NameExistsAsync(Guid sectionId, string name, CancellationToken cancellationToken) =>
        context.GradeComponents.AnyAsync(component => component.CourseSectionId == sectionId
            && component.Name == name, cancellationToken);

    public async Task<decimal> GetTotalWeightAsync(Guid sectionId, CancellationToken cancellationToken) =>
        await context.GradeComponents.Where(component => component.CourseSectionId == sectionId)
            .SumAsync(component => (decimal?)component.Weight, cancellationToken) ?? 0m;

    public async Task<IReadOnlyList<GradeComponentDto>> ListAsync(
        Guid sectionId, CancellationToken cancellationToken)
    {
        return await context.GradeComponents.AsNoTracking()
            .Where(component => component.CourseSectionId == sectionId)
            .OrderBy(component => component.Name)
            .ThenBy(component => component.Id)
            .Select(component => new GradeComponentDto(
                component.Id,
                component.CourseSectionId,
                component.Name,
                component.Weight,
                component.MaxScore))
            .ToArrayAsync(cancellationToken);
    }

    public async Task CreateAsync(GradeComponentEntity component, CancellationToken cancellationToken)
    {
        context.GradeComponents.Add(component.ToDatabaseModel());
        try
        {
            await context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException exception) when (exception.InnerException is SqlException { Number: 2601 or 2627 })
        {
            throw new GradeComponentException(GradeComponentError.AlreadyExists,
                "Grade component name already exists in this course section.");
        }
        catch (DbUpdateException exception) when (exception.InnerException is SqlException { Number: 547 })
        {
            throw new GradeComponentException(GradeComponentError.SectionNotFound, "Course section no longer exists.");
        }
    }
}
