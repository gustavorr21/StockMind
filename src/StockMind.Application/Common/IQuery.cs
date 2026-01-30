using MediatR;

namespace StockMind.Application.Common;

public interface IQuery<out TResponse> : IRequest<TResponse>
{
}
