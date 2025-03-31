using Microsoft.AspNetCore.Mvc;
using OMSCrudService.Database;
using SimpleOMS.Model;
using System.Collections.Generic;
using System.Linq;

namespace OMSCrudService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OMSController : ControllerBase
    {
        private readonly IOMSDataProvider _dataprovider;

        public OMSController(IOMSDataProvider omsDataProvider)
        {
            _dataprovider = omsDataProvider;
        }

        [HttpGet("customers")]
        public async Task<ActionResult<IEnumerable<Customer>>> GetCustomers()
        {
            var customers = await _dataprovider.GetCustomers();
            return Ok(customers);
        }

        [HttpGet("customers/{id}")]
        public async Task<ActionResult<Customer>> GetCustomer(int id)
        {
            var customer = await _dataprovider.GetCustomer(id);
            if (customer == null)
            {
                return NotFound();
            }
            return Ok(customer);
        }

        [HttpPost("customers")]
        public async Task<ActionResult<Customer>> CreateCustomer(Customer customer)
        {
            var createdCustomer = await _dataprovider.CreateCustomer(customer);
            return Ok(createdCustomer); ;
        }

        [HttpPut("customers/{id}")]
        public async Task<IActionResult> UpdateCustomer(int id, Customer customer)
        {
            var updatedCustomer = await _dataprovider.UpdateCustomer(id, customer);
            if (updatedCustomer == null)
            {
                return NotFound();
            }
            return Ok(updatedCustomer);
        }

        [HttpDelete("customers/{id}")]
        public async Task<IActionResult> DeleteCustomer(int id)
        {
            var deletedCustomer = await _dataprovider.DeleteCustomer(id);
            if (deletedCustomer == null)
            {
                return NotFound();
            }
            return Ok(deletedCustomer);
        }

        // Order CRUD operations
        [HttpGet("CustomerOrders/{id}")]
        public async Task<ActionResult<IEnumerable<Order>>> GetCustomerOrders(int id)
        {
            var orders = await _dataprovider.GetCustomerOrders(id);
            return Ok(orders);
        }

        [HttpGet("orders/{id}")]
        public async Task<ActionResult<Order>> GetOrder(int id)
        {
            var order = await _dataprovider.GetOrder(id);
            if (order == null)
            {
                return NotFound();
            }
            return Ok(order);
        }

        [HttpPost("orders")]
        public async Task<ActionResult<Order>> CreateOrder(Order order)
        {
            var createdOrder = await _dataprovider.CreateOrder(order);
            return Ok(createdOrder);
        }

        [HttpPut("orders/{id}")]
        public async Task<ActionResult<Order>> UpdateOrder(int id, Order order)
        {
            var updatedOrder = await _dataprovider.UpdateOrder(id, order);
            if (updatedOrder == null)
            {
                return NotFound();
            }
            return Ok(updatedOrder);
        }

        [HttpDelete("orders/{id}")]
        public async Task<IActionResult> DeleteOrder(int id)
        {
            var deletedOrder = await _dataprovider.DeleteOrder(id);
            if (deletedOrder == null)
            {
                return NotFound();
            }
            return Ok(deletedOrder);
        }

        // Get Employees
        [HttpGet("employees")]
        public async Task<ActionResult<IEnumerable<Employee>>> GetEmployees()
        {
            var employees = await _dataprovider.GetEmployees();
            return Ok(employees);
        }

        // Get Products
        [HttpGet("products")]
        public async Task<ActionResult<IEnumerable<Product>>> GetProducts()
        {
            var products = await _dataprovider.GetProducts();
            return Ok(products);
        }
    }
}
