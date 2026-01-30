using MediatR;

namespace StockMind.Application.Common;

public interface ICommand<out TResponse> : IRequest<TResponse>
{
}
