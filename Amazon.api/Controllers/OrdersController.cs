using Amazon.api.Responses;
using Amazon.Core.CustomEntities;
using Amazon.Core.Entities;
using Amazon.Core.Interface;
using Amazon.infrastructure.DTOs;
using Amazon.Infrastructure.DTOs;
using Amazon.Infrastructure.Repositories;
using Amazon.Infrastructure.Validators;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using System.Net;

namespace Amazon.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrdersController : ControllerBase
    {
        private readonly IOrderService _orderService;
        private readonly IMapper _mapper;
        private readonly IValidationService _validationService;

        public OrdersController(
            IOrderService orderService,
            IMapper mapper,
            IValidationService validationService

            )
        {
            _orderService = orderService;
            _mapper = mapper;
            _validationService = validationService;
        }

        /// <summary>
        /// Recupera todas las órdenes registradas en el sistema.
        /// </summary>
        /// <remarks>
        /// Devuelve una lista completa de órdenes con sus respectivos datos mapeados al DTO.
        /// </remarks>
        /// <returns>Lista de órdenes</returns>
        /// <response code="200">Retorna todas las órdenes</response>
        /// <response code="500">Error interno del servidor</response>
        [ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(ApiResponse<OrderDto>))]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        [HttpGet]

        public async Task<IActionResult> GetOrders()
        {
            try
            {

                var orders = await _orderService.GetAllOrderAsync();
                var ordersDto = _mapper.Map<IEnumerable<OrderDto>>(orders);
                var response = new ApiResponse<IEnumerable<OrderDto>>(ordersDto);
                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError,
                    new ApiResponse<string>($"Error: {ex.Message}"));
            }
        }

        /// <summary>
        /// Obtiene una orden específica por su identificador.
        /// </summary>
        /// <param name="id">ID único de la orden</param>
        /// <returns>Orden encontrada</returns>
        /// <response code="200">Orden encontrada</response>
        /// <response code="404">No se encontró la orden</response>
        /// <response code="500">Error interno del servidor</response>
        [ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(ApiResponse<OrderDto>))]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetOrderId(int id)
        {
            try
            {
                var orders = await _orderService.GetByIdOrderAsync(id);
                var orderDto = _mapper.Map<OrderDto>(orders);
                var response = new ApiResponse<OrderDto>(orderDto);
                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError,
                    new ApiResponse<string>($"Error: {ex.Message}"));
            }
        }

        /// <summary>
        /// Crea una nueva orden simple.
        /// </summary>
        /// <remarks>
        /// Este endpoint permite registrar una nueva orden en el sistema validando los datos de entrada.
        /// </remarks>
        /// <param name="CrearOrden">Objeto con la información de la orden</param>
        /// <returns>Orden creada</returns>
        /// <response code="200">Orden creada exitosamente</response>
        /// <response code="400">Error de validación</response>
        /// <response code="500">Error interno del servidor</response>
        [ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(ApiResponse<OrderDto>))]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        [HttpPost]
        public async Task<IActionResult> CreateSimpleOrder([FromBody] CrearOrdenRequest CrearOrden)
        {
            try {
                var validationResult = await _validationService.ValidateAsync(CrearOrden);

                if (!validationResult.IsValid)
                {
                    return BadRequest(new { Errors = validationResult.Errors });
                }
                var order = _mapper.Map<Order>(CrearOrden);
                await _orderService.CreatedOrder(order);

                var Orden = await _orderService.GetByIdOrderAsync(order.Id);
                var OrdenRequest = _mapper.Map<OrderDto>(Orden);
                var response = new ApiResponse<OrderDto>(OrdenRequest);
                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError,
                    new ApiResponse<string>($"Error : {ex.Message}"));
            }
        }

        /// <summary>
        /// Elimina una orden por su identificador.
        /// </summary>
        /// <param name="id">ID de la orden</param>
        /// <response code="204">Orden eliminada correctamente</response>
        /// <response code="500">Error interno del servidor</response>
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteOrderDtoMapper(int id)
        {
            try
            {
                var order = await _orderService.GetByIdOrderAsync(id);
                await _orderService.DeleteAsync(order);
                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError,
                    new ApiResponse<string>($"Error : {ex.Message}"));
            }
        }
        /// <summary>
        /// Obtiene el carrito actual del usuario.
        /// </summary>
        /// <param name="userId">ID del usuario</param>
        /// <returns>Orden con estado de carrito</returns>
        [ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(ApiResponse<OrderDto>))]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        [HttpGet("user/{userId}/cart")]
        public async Task<IActionResult> GetUserCartDtoMapper(int userId)
        {
            try
            {
                #region Validaciones
                var validationRequest = new GetByIdRequest { Id = userId };
                var validationResult = await _validationService.ValidateAsync(validationRequest);

                if (!validationResult.IsValid)
                {
                    return BadRequest(new { Errors = validationResult.Errors });
                }
                #endregion

                var cart = await _orderService.GetUserCartAsync(userId);
                var cartDto = _mapper.Map<OrderDto>(cart);
                var response = new ApiResponse<OrderDto>(cartDto);
                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError,
                    new ApiResponse<string>($"Error: {ex.Message}"));
            }
        }

        /// <summary>
        /// Inserta un producto en el carrito del usuario.
        /// </summary>
        /// <param name="productId">ID del producto</param>
        /// <param name="orderId">ID de la orden (carrito)</param>
        /// <param name="quantity">Cantidad del producto</param>
        /// <returns>Producto agregado al carrito</returns>
        [ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(ApiResponse<OrderItemDto>))]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        [HttpPost("Product/{productId}/Order/{orderId}/quantity/{quantity}/cart")]
        public async Task<IActionResult> IntroduceItemCart(int productId, int orderId, int quantity)
        {
            try
            {
                var newItem=await _orderService.InsertProductIntoCart(productId,orderId,quantity);
                var newItemDto = _mapper.Map<OrderItemDto>(newItem);
                var response = new ApiResponse<OrderItemDto>(newItemDto);
                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError, ex.Message);

            }
        }


        /// <summary>
        /// Elimina un producto del carrito del usuario.
        /// </summary>
        /// <param name="userId">ID del usuario</param>
        /// <param name="productId">ID del producto a eliminar</param>
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        [HttpDelete("user/{userId}/products/{productId}")]
        public async Task<IActionResult> EliminarItemCarrito(int userId, int productId)
        {

            await _orderService.DeleteItemAsync( userId, productId);
            return NoContent();


        }

        /// <summary>
        /// Procesa el pago del carrito del usuario.
        /// </summary>
        /// <param name="userId">ID del usuario</param>
        /// <returns>Resultado del pago</returns>
        [ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(ApiResponse<PaymentDto>))]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        [HttpPost("user/{userId}/process-payment")]
        public async Task<IActionResult> ProcessPaymentAsync(int userId)
        {
            try
            {
                var payment = await _orderService.ProcessPaymentAsync(userId);
                var paymentDto = _mapper.Map<PaymentDto>(payment);
                var response = new ApiResponse<PaymentDto>(paymentDto);
                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError,
                    new ApiResponse<string>($"Error: {ex.Message}"));
            }

        }

        /// <summary>
        /// Obtiene todas las órdenes pagadas del usuario.
        /// </summary>
        /// <param name="userId">ID del usuario</param>
        /// <returns>Lista de órdenes completadas</returns>
        [ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(ApiResponse<IEnumerable<OrderDto>>))]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        [HttpGet("user/{userId}/orders")]
        public async Task<IActionResult> GetUserOrders(int userId)
        {
            try
            {
                #region Validaciones
                var validationRequest = new GetByIdRequest { Id = userId };
                var validationResult = await _validationService.ValidateAsync(validationRequest);

                if (!validationResult.IsValid)
                {
                    return BadRequest(new ApiResponse<object>(new
                    {
                        Message = "Error de validación del ID de usuario",
                        Errors = validationResult.Errors
                    }));
                }
                #endregion

                var orders = await _orderService.GetAllOderUserAsync(userId);
                var ordersDto = _mapper.Map<IEnumerable<OrderDto>>(orders);

                var response = new ApiResponse<IEnumerable<OrderDto>>(ordersDto);
                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError,
                    new ApiResponse<string>($"Error: {ex.Message}"));
            }
        }

        /// <summary>
        /// Obtiene el reporte mensual de ventas.
        /// </summary>
        [HttpGet("dapper/1")]
        public async Task<IActionResult> GetReporteMensualVentas()
        {
            var posts = await _orderService.GetReporteMensualVentas();


            var response = new ApiResponse<IEnumerable<ReporteMensualVentasResponse>> (posts);

            return Ok(response);
        }

        /// <summary>
        /// Obtiene estadísticas generales del tablero.
        /// </summary>
        [HttpGet("dapper/2")]
        public async Task<IActionResult> GetBoardStats()
        {
            var posts = await _orderService.GetBoardStats();


            var response = new ApiResponse<IEnumerable<BoardStatsResponse>> (posts);

            return Ok(response);
        }

        /// <summary>
        /// Obtiene los productos más vendidos.
        /// </summary>
        [HttpGet("dapper/3")]
        public async Task<IActionResult> GetTopProductosVendidos()
        {
            var posts = await _orderService.GetTopProductosVendidos();


            var response = new ApiResponse<IEnumerable<TopProductosVendidosResponse>> (posts);

            return Ok(response);
        }


        /// <summary>
        /// Obtiene los productos con bajo stock.
        /// </summary>
        [HttpGet("dapper/4")]
        public async Task<IActionResult> GetLowStockProductResponse()
        {
            var posts = await _orderService.GetLowStockProductResponse();


            var response = new ApiResponse<IEnumerable<LowStockProductResponse>> (posts);

            return Ok(response);
        }

        /// <summary>
        /// Obtiene los usuarios con mayor gasto total.
        /// </summary>
        [HttpGet("dapper/5")]
        public async Task<IActionResult> GetTopUsersBySpending()
        {
            var posts = await _orderService.GetTopUsersBySpending();


            var response = new ApiResponse<IEnumerable<TopUsersBySpendingResponse>> (posts);

            return Ok(response);
        }

    }
}