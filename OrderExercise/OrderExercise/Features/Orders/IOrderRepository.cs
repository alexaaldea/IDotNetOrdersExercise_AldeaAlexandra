using Features.Orders;
using System.Threading.Tasks;

public interface IOrderRepository
{
    Task<Order?> GetByISBNAsync(string isbn);
    Task AddAsync(Order order);

    Task<bool> ExistsTitleForAuthorAsync(string title, string author);
    Task<bool> ExistsISBNAsync(string isbn);
    Task<int> CountOrdersAddedTodayAsync();
}