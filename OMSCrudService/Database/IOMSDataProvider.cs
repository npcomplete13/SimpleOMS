using SimpleOMS.Model;

namespace OMSCrudService.Database
{
    public interface IOMSDataProvider
    {
        Task<IEnumerable<Customer>> GetCustomers();
        Task<Customer> GetCustomer(int id);
        Task<Customer> CreateCustomer(Customer customer);
        Task<Customer> UpdateCustomer(int id, Customer customer);
        Task<Customer> DeleteCustomer(int id);

        Task<IEnumerable<Order>> GetOrders();
        Task<Order> GetOrder(int id);
        Task<IEnumerable<Order>> GetCustomerOrders(int id);
        Task<Order> CreateOrder(Order Order);
        Task<Order> UpdateOrder(int id, Order Order);
        Task<Order> DeleteOrder(int id);


        Task<IEnumerable<Product>> GetProducts();
        Task<IEnumerable<Employee>> GetEmployees();

        
    }
}
