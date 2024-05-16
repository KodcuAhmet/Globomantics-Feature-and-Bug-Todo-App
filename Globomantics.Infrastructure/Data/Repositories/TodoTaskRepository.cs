using Globomantics.Domain;
using Microsoft.EntityFrameworkCore;

namespace Globomantics.Infrastructure.Data.Repositories;

public class TodoTaskRepository : TodoRepository<TodoTask>
{
    public TodoTaskRepository(GlobomanticsDbContext context) : base(context)
    {
    }

    public override async Task AddAsync(TodoTask todoTask)
    {
        var todotaskToAdd = DomainToDataMapping.MapTodoFromDomain<TodoTask, Models.TodoTask>(todoTask);

        await Context.AddAsync(todotaskToAdd);
    }

    public override async Task<TodoTask> GetAsync(Guid id)
    {
        var data = await Context.TodoTasks.SingleAsync(bug => bug.Id == id);

        return DataToDomainMapping.MapTodoFromData<Models.TodoTask, TodoTask>(data);
    }
}
