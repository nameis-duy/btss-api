using API.Controllers;
using Domain.Entities;
using HotChocolate.Execution;
using HotChocolate.Subscriptions;

namespace API.GraphQL.Subscriptions
{
    public partial class Subscription
    {
        public ValueTask<ISourceStream<Transaction>> SubscribeTopUp(int transactionId, [Service] ITopicEventReceiver receiver)
        {
            var topic = string.Format(TransactionController.TOPUP_TOPIC_FORMAT, transactionId);
            return receiver.SubscribeAsync<Transaction>(topic);
        }
        [Subscribe(With = nameof(SubscribeTopUp))]
        public Transaction TopUpStatus([EventMessage] Transaction transaction) => transaction;
    }
}
