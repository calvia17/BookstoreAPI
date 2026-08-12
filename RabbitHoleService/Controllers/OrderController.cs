using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
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
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The orders.</returns>
        [EnableRateLimiting("ReadPolicy")]
        [Authorize(Policy = "StaffOrAdmin")]
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<OrderDto>>> GetAllOrders(CancellationToken cancellationToken)
        {
            var orders = await this.orderService.GetAllAsync(cancellationToken);
            return Ok(orders);
        }

        /// <summary>
        /// Gets the orders for a customer.
        /// </summary>
        /// <param name="customerId">The customer id.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The orders.</returns>
        [EnableRateLimiting("ReadPolicy")]
        [Authorize(Policy = "StaffOrAdmin")]
        [HttpGet("customer/{customerId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<IEnumerable<OrderDto>>> GetOrdersForCustomer([FromRoute] Guid customerId, CancellationToken cancellationToken)
        {
            try
            {
                var orders = await this.orderService.GetOrdersForCustomerAsync(customerId, cancellationToken);
                return Ok(orders);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (PersonNotFoundException<Customer> ex)
            {
                return NotFound(new
                {
                    Title = "Customer Not Found",
                    Status = StatusCodes.Status404NotFound,
                    Detail = ex.Message,
                    CustomerId = ex.PersonId
                });
            }
        }

        /// <summary>
        /// Gets the orders for the authenticated customer.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The orders.</returns>
        [EnableRateLimiting("ReadPolicy")]
        [Authorize]
        [HttpGet("me")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<IEnumerable<OrderDto>>> GetOrders(CancellationToken cancellationToken)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (userId == null)
                {
                    return Unauthorized();
                }

                var orders = await this.orderService.GetOrdersAsync(userId, cancellationToken);
                return Ok(orders);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (PersonNotFoundException<Customer> ex)
            {
                return NotFound(new
                {
                    Title = "Customer Not Found",
                    Status = StatusCodes.Status404NotFound,
                    Detail = ex.Message,
                    CustomerId = ex.PersonId
                });
            }
        }

        /// <summary>
        /// Gets an order.
        /// </summary>
        /// <param name="id">The order id.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The customer.</returns>
        [EnableRateLimiting("ReadPolicy")]
        [Authorize]
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<OrderDto>> GetOrder([FromRoute] Guid id, CancellationToken cancellationToken)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var isStaffOrAdmin = !User.IsInRole(RoleType.Customer.ToString());
                if (userId == null)
                {
                    return Unauthorized();
                }

                var order = await this.orderService.GetAsync(id, isStaffOrAdmin, userId, cancellationToken);
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
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The added order.</returns>
        [EnableRateLimiting("CheckoutPolicy")]
        [Authorize(Policy = "StaffOrAdmin")]
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<ActionResult<OrderDto>> AddOrderForCustomer(
            [FromBody] CreateOrderForCustomerDto newOrderData,
            [FromHeader(Name = "X-Idempotency-Key")] string idempotencyKey,
            CancellationToken cancellationToken)
        {
            try
            {
                var createdOrder = await this.orderService.CreateForCustomerAsync(idempotencyKey, newOrderData, cancellationToken);
                return CreatedAtAction(nameof(this.GetOrder), new { id = createdOrder.Id }, createdOrder);
            }
            catch (ArgumentNullException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (PersonNotFoundException<Customer> ex)
            {
                return NotFound(new
                {
                    Title = "Customer Not Found",
                    Status = StatusCodes.Status404NotFound,
                    Detail = ex.Message,
                    CustomerId = ex.PersonId
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
        /// <param name="idempotencyKey">The idempotency key.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The added order.</returns>
        [EnableRateLimiting("CheckoutPolicy")]
        [Authorize]
        [HttpPost("me")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<ActionResult<OrderDto>> AddOrder(
            [FromBody] CreateOrderDto newOrderData,
            [FromHeader(Name = "X-Idempotency-Key")] string idempotencyKey,
            CancellationToken cancellationToken)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (userId == null)
                {
                    return Unauthorized();
                }

                var createdOrder = await this.orderService.CreateAsync(idempotencyKey, newOrderData, userId, cancellationToken);
                return CreatedAtAction(nameof(this.GetOrder), new { id = createdOrder.Id }, createdOrder);
            }
            catch (ArgumentNullException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (PersonNotFoundException<Customer> ex)
            {
                return NotFound(new
                {
                    Title = "Customer Not Found",
                    Status = StatusCodes.Status404NotFound,
                    Detail = ex.Message,
                    CustomerId = ex.PersonId
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
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>No content.</returns>
        [EnableRateLimiting("CheckoutPolicy")]
        [Authorize(Policy = "StaffOrAdmin")]
        [HttpPatch("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateOrderStatus([FromRoute] Guid id, [FromBody] UpdateOrderStatusDto updateData, CancellationToken cancellationToken)
        {
            try
            {
                var isAdmin = User.IsInRole(RoleType.Admin.ToString());
                await this.orderService.UpdateStatusAsync(id, updateData, isAdmin, cancellationToken);
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
    }
}