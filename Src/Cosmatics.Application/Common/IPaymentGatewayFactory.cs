namespace Cosmatics.Application.Common
{
    public interface IPaymentGatewayFactory
    {
        IPaymentGateway Get(string method);
    }

}
