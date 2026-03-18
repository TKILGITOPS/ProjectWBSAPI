namespace ProjectWBSAPI.Helper
{
    public interface IOrderService
    {
        Task ProcessOrdersAsync();
    }
    public class OrderService: IOrderService
    {
        public async Task ProcessOrdersAsync()
        {

            // Your logic here
            await Task.Delay(1000);
            Console.WriteLine("Orders processed");
        }
    }
}
