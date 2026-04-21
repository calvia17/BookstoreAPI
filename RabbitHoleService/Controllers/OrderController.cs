using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RabbitHoleService.Dtos;
using RabbitHoleService.Exceptions;
using RabbitHoleService.Objects;
using RabbitHoleService.Services;
using System.Security.Claims;

namespace RabbitHoleService.Controllers
{
    /// <summary>
    /// The order controller.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class OrderController : ControllerBase
    {
        private readonly IOrderService orderService;

        /// <summary>
        /// Initialises the order controller.
        /// </summary>
        /// <param name="orderService">The order service.</param>
        public OrderController(IOrderService orderService)
        {
            this.orderService = orderService;
        }

        /// <summary>
        /// Gets all the orders.
        /// </summary>
        /// <returns>The orders.</returns>
        [Authorize(Policy = "StaffOrAdmin")]
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<OrderDto>>> GetAllOrders()
        {
            var orders = await this.orderService.GetAllAsync();
            return Ok(orders);
        }

        /// <summary>
        /// Gets an order.
        /// </summary>
        /// <param name="id">The order id.</param>
        /// <returns>The customer.</returns>
        [Authorize]
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<OrderDto>> GetOrder([FromRoute] Guid id)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var isStaffOrAdmin = !User.IsInRole(RoleType.Customer.ToString());
                if (userId == null)
                {
                    return Unauthorized();
                }

                var order = await this.orderService.GetAsync(id, isStaffOrAdmin, userId);
                return Ok(order);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (OrderNotFoundException ex)
            {
                return NotFound(new
                {
                    Title = "Order not found",
                    Status = StatusCodes.Status404NotFound,
                    Detail = ex.Message,
                    OrderId = ex.OrderId
                });
            }
        }

        /// <summary>
        /// Adds an order for a customer.
        /// </summary>
        /// <param name="newOrderData">The new order data.</param>
        /// <returns>The added order.</returns>
        [Authorize(Policy = "StaffOrAdmin")]
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<ActionResult<OrderDto>> AddOrderForCustomer(
            [FromBody] CreateOrderForCustomerDto newOrderData,
            [FromHeader(Name = "X-Idempotency-Key")] string idempotencyKey)
        {
            try
            {
                var createdOrder = await this.orderService.CreateForCustomerAsync(idempotencyKey, newOrderData);
                return CreatedAtAction(nameof(this.GetOrder), new { id = createdOrder.Id }, createdOrder);
            }
            catch (ArgumentNullException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (CustomerNotFoundException ex)
            {
                return NotFound(new
                {
                    Title = "Customer Not Found",
                    Status = StatusCodes.Status404NotFound,
                    Detail = ex.Message,
                    CustomerId = ex.CustomerId
                });
            }
            catch (BookNotFoundException ex)
            {
                return NotFound(new
                {
                    Title = "Book Not Found",
                    Status = StatusCodes.Status404NotFound,
                    Detail = ex.Message,
                    BookIds = ex.BookIds
                });
            }
            catch(DuplicateItemException ex)
            {
                return BadRequest(new
                {
                    Title = "Duplicate item",
                    Status = StatusCodes.Status400BadRequest,
                    Detail = ex.Message,
                    DuplicateItems = ex.DuplicateItems
                });
            }
            catch (InsufficientStockException ex)
            {
                return BadRequest(new
                {
                    Title = "Inventory Conflict",
                    Status = StatusCodes.Status400BadRequest,
                    Detail = ex.Message,
                    InsufficientStockItems = ex.InsufficientStockItems
                });
            }
            catch (IdempotencyKeyExpiredException ex)
            {
                return BadRequest(new
                {
                    Title = "Idempotency Key Expired",
                    Status = StatusCodes.Status400BadRequest,
                    Detail = ex.Message
                });
            }
            catch (OrderProcessingException ex)
            {
                return Conflict(new
                {
                    Title = "Processing Order",
                    Status = StatusCodes.Status409Conflict,
                    Detail = ex.Message,
                    OrderId = ex.OrderId
                });
            }
        }

        /// <summary>
        /// Adds an order for the authenticated customer.
        /// </summary>
        /// <param name="newOrderData">The new order data.</param>
        /// <returns>The added order.</returns>
        [Authorize]
        [HttpPost("me")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<ActionResult<OrderDto>> AddOrder(
            [FromBody] CreateOrderDto newOrderData,
            [FromHeader(Name = "X-Idempotency-Key")] string idempotencyKey)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (userId == null)
                {
                    return Unauthorized();
                }

                var createdOrder = await this.orderService.CreateAsync(idempotencyKey, newOrderData, userId);
                return CreatedAtAction(nameof(this.GetOrder), new { id = createdOrder.Id }, createdOrder);
            }
            catch (ArgumentNullException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (CustomerNotFoundException ex)
            {
                return NotFound(new
                {
                    Title = "Customer Not Found",
                    Status = StatusCodes.Status404NotFound,
                    Detail = ex.Message,
                    CustomerId = ex.CustomerId
                });
            }
            catch (BookNotFoundException ex)
            {
                return NotFound(new
                {
                    Title = "Book Not Found",
                    Status = StatusCodes.Status404NotFound,
                    Detail = ex.Message,
                    BookIds = ex.BookIds
                });
            }
            catch (DuplicateItemException ex)
            {
                return BadRequest(new
                {
                    Title = "Duplicate item",
                    Status = StatusCodes.Status400BadRequest,
                    Detail = ex.Message,
                    DuplicateItems = ex.DuplicateItems
                });
            }
            catch (InsufficientStockException ex)
            {
                return BadRequest(new
                {
                    Title = "Inventory Conflict",
                    Status = StatusCodes.Status400BadRequest,
                    Detail = ex.Message,
                    InsufficientStockItems = ex.InsufficientStockItems
                });
            }
            catch (IdempotencyKeyExpiredException ex)
            {
                return BadRequest(new
                {
                    Title = "Idempotency Key Expired",
                    Status = StatusCodes.Status400BadRequest,
                    Detail = ex.Message
                });
            }
            catch (OrderProcessingException ex)
            {
                return Conflict(new
                {
                    Title = "Processing Order",
                    Status = StatusCodes.Status409Conflict,
                    Detail = ex.Message,
                    OrderId = ex.OrderId
                });
            }
        }

        /// <summary>
        /// Updates the order status.
        /// </summary>
        /// <param name="id">The order id.</param>
        /// <param name="updateData">The data to update.</param>
        /// <returns>No content.</returns>
        [Authorize(Policy = "StaffOrAdmin")]
        [HttpPatch("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateOrderStatus([FromRoute] Guid id, [FromBody] UpdateOrderStatusDto updateData)
        {
            try
            {
                var isAdmin = User.IsInRole(RoleType.Admin.ToString());
                await this.orderService.UpdateStatusAsync(id, updateData, isAdmin);
                return NoContent();
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (OrderNotFoundException ex)
            {
                return NotFound(new
                {
                    Title = "Order not found",
                    Status = StatusCodes.Status404NotFound,
                    Detail = ex.Message,
                    OrderId = ex.OrderId
                });
            }
            catch (InvalidOrderStatusChangeException ex)
            {
                return Conflict(new
                {
                    Title = "Invalid status change",
                    Status = StatusCodes.Status409Conflict,
                    Detail = ex.Message,
                    OrderId = ex.OrderId,
                    OldStatus = ex.OldStatus,
                    NewStatus = ex.NewStatus
                });
            }
        }

        /// <summary>
        /// Gets the orders for a customer.
        /// </summary>
        /// <param name="customerId">The customer id.</param>
        /// <returns>The orders.</returns>
        [Authorize(Policy = "StaffOrAdmin")]
        [HttpGet("customer/{customerId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<IEnumerable<OrderDto>>> GetOrdersForCustomer([FromRoute] Guid customerId)
        {
            try
            {
                var orders = await this.orderService.GetOrdersForCustomerAsync(customerId);
                return Ok(orders);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (CustomerNotFoundException ex)
            {
                return NotFound(new
                {
                    Title = "Customer Not Found",
                    Status = StatusCodes.Status404NotFound,
                    Detail = ex.Message,
                    CustomerId = ex.CustomerId
                });
            }
        }

        /// <summary>
        /// Gets the orders for the authenticated customer.
        /// </summary>
        /// <returns>The orders.</returns>
        [Authorize]
        [HttpGet("me")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<IEnumerable<OrderDto>>> GetOrders()
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (userId == null)
                {
                    return Unauthorized();
                }

                var orders = await this.orderService.GetOrdersAsync(userId);
                return Ok(orders);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (CustomerNotFoundException ex)
            {
                return NotFound(new
                {
                    Title = "Customer Not Found",
                    Status = StatusCodes.Status404NotFound,
                    Detail = ex.Message,
                    CustomerId = ex.CustomerId
                });
            }
        }
    }
}