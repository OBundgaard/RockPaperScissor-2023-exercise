using EasyNetQ;

namespace Helpers;

public static class ConnectionHelper
{
    public static IBus GetRMQConnection()
    {
        return RabbitHutch.CreateBus("host=localhost;username=application;password=pepsi");
    }
}