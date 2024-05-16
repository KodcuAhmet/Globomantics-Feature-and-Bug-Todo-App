using Globomantics.Domain;
using Microsoft.EntityFrameworkCore;

namespace Globomantics.Infrastructure.Data.Repositories;

public class BugRepository : TodoRepository<Bug>
{
    public BugRepository(GlobomanticsDbContext context) : base(context) { }

    public override async Task AddAsync(Bug bug)
    {
        var existingBug = await Context.Bugs.FirstOrDefaultAsync(
            b => b.Id == bug.Id
        );

        var user = await Context.Users.FirstOrDefaultAsync(
            u => u.Id == bug.CreatedBy.Id
        );

        user ??= new() { Id = bug.CreatedBy.Id, Name = bug.CreatedBy.Name };

        if (existingBug is not null)
        {
            await UpdateAsync(bug, existingBug, user);
        }
        else
        {
            {
                await CreateAsync(bug, user);
            }
        }
    }

    private async Task CreateAsync(Bug bug, Models.User user)
    {
        var bugToAdd
            = DomainToDataMapping.MapTodoFromDomain<Bug, Data.Models.Bug>(bug);

        await SetParentAsync(bugToAdd, bug);

        bugToAdd.CreatedBy = user;

        await Context.AddAsync(bugToAdd);
    }

    private async Task UpdateAsync(Bug bug, Models.Bug existingBug, Models.User user)
    {
        existingBug.IsCompleted = bug.IsCompleted;
        existingBug.AffectedVersion = bug.AffectedVersion;
        existingBug.AffectedUsers = existingBug.AffectedUsers;
        existingBug.Description = existingBug.Description;
        existingBug.Title = existingBug.Title;
        existingBug.Severity = (Data.Models.Severity)existingBug.Severity;

        await SetParentAsync(existingBug, bug);

        Context.Bugs.Update(existingBug);
    }

    public async Task SetParentAsync(Data.Models.Todo toBeAdded, Todo item)
    {
        Data.Models.TodoTask? existingParent = null;

        if (item.Parent is not null)
        {
            existingParent = await Context.TodoTasks.FirstOrDefaultAsync(
                i => i.Id == item.Parent.Id
            );
        }

        if (existingParent is not null)
        {
            toBeAdded.Parent = existingParent;
        }
        else if (item.Parent is not null)
        {
            var parentToAdd =
                DomainToDataMapping.MapTodoFromDomain<TodoTask, Data.Models.TodoTask>(item.Parent);
            toBeAdded.Parent = parentToAdd;
            await Context.AddAsync(parentToAdd);
        }
    }

    public override async Task<Bug> GetAsync(Guid id)
    {
        var data = await Context.Bugs.SingleAsync(bug => bug.Id == id);

        return DataToDomainMapping.MapTodoFromData<Data.Models.Bug, Domain.Bug>(data);
    }
}
