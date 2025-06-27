using CorrelationId.Abstractions;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace ECommerce.AdminUI.Infrastructure
{
    public class CorrelationIdDelegatingHandler : DelegatingHandler
    {
        private readonly ICorrelationContextAccessor _correlationContextAccessor;

        public CorrelationIdDelegatingHandler(ICorrelationContextAccessor correlationContextAccessor)
        {
            _correlationContextAccessor = correlationContextAccessor;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (_correlationContextAccessor.CorrelationContext != null)
            {
                // Add the correlation ID to the outgoing request
                request.Headers.Add(_correlationContextAccessor.CorrelationContext.Header,
                                   _correlationContextAccessor.CorrelationContext.CorrelationId);
            }

            return base.SendAsync(request, cancellationToken);
        }
    }
}
