using Amazon.api.Responses;
using Amazon.Core.CustomEntities;
using Amazon.Core.Entities;
using Amazon.Core.Interface;
using Amazon.Infrastructure.DTOs;
using Amazon.Infrastructure.Validators;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using System.Net;


namespace Amazon.api.Controllers
    {
    [Route("api/[controller]")]
    [ApiController]
    public class PaymentsController : ControllerBase
    {
        private readonly IPaymentService _paymentService;
        private readonly IMapper _mapper;
        private readonly IValidationService _validationService;

        public PaymentsController(
            IPaymentService paymentService,
            IMapper mapper,
            IValidationService validationService)
        {
            _paymentService = paymentService;
            _mapper = mapper;
            _validationService = validationService;
        }


        /// <summary>
        /// Obtiene un pago específico por su identificador único
        /// </summary>
        /// <remarks>
        /// Este endpoint recupera la información completa de un pago utilizando su ID único.
        /// Incluye detalles como monto, estado, fecha de creación y relación con la orden.
        /// 
        /// Ejemplo de uso:
        /// GET /api/Payments/1
        /// </remarks>
        /// <param name="id">Identificador único del pago (mayor a 0)</param>
        /// <returns>Información detallada del pago solicitado</returns>
        /// <response code="200">Retorna el pago encontrado exitosamente</response>
        /// <response code="400">Error en la validación del ID del pago</response>
        /// <response code="404">No se encontró el pago con el ID especificado</response>
        /// <response code="500">Error interno del servidor al procesar la solicitud</response>
        [ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(ApiResponse<PaymentDto>))]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetPaymentById(int id)
        {
            try
            {
                var payment = await _paymentService.GetByIdAsync(id);
                
                var paymentDto = _mapper.Map<PaymentDto>(payment);
                return Ok(new ApiResponse<PaymentDto>(paymentDto));
            }
            catch (Exception ex)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError,
                    new ApiResponse<string>($"Error: {ex.Message}"));
            }
        }

        /// <summary>
        /// Obtiene el pago asociado a una orden específica
        /// </summary>
        /// <remarks>
        /// Este endpoint recupera la información del pago vinculado a una orden particular.
        /// Útil para verificar el estado de pago de una orden existente.
        /// 
        /// Ejemplo de uso:
        /// GET /api/Payments/order/5
        /// </remarks>
        /// <param name="orderId">Identificador único de la orden (mayor a 0)</param>
        /// <returns>Información del pago asociado a la orden</returns>
        /// <response code="200">Retorna el pago asociado a la orden exitosamente</response>
        /// <response code="400">Error en la validación del ID de la orden</response>
        /// <response code="404">No se encontró pago para la orden especificada</response>
        /// <response code="500">Error interno del servidor al procesar la solicitud</response>
        [ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(ApiResponse<PaymentDto>))]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        [HttpGet("order/{orderId}")]
        public async Task<IActionResult> GetPaymentByOrderId(int orderId)
        {
            try
            { 
                var payment = await _paymentService.GetByOrderIdAsync(orderId);

                var paymentDto = _mapper.Map<PaymentDto>(payment);
                return Ok(new ApiResponse<PaymentDto>(paymentDto));
            }
            catch (Exception ex)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError,
                    new ApiResponse<string>($"Error: {ex.Message}"));
            }
        }

        /// <summary>
        /// Obtiene todos los pagos registrados en el sistema
        /// </summary>
        /// <remarks>
        /// Este endpoint recupera una lista completa de todos los pagos existentes en el sistema.
        /// Los resultados incluyen pagos en todos los estados (Pending, Completed, Failed).
        /// 
        /// Ejemplo de uso:
        /// GET /api/Payments
        /// </remarks>
        /// <returns>Lista completa de todos los pagos del sistema</returns>
        /// <response code="200">Retorna la lista de pagos exitosamente</response>
        /// <response code="500">Error interno del servidor al procesar la solicitud</response>
        [ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(ApiResponse<IEnumerable<PaymentDto>>))]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        [HttpGet]
        public async Task<IActionResult> GetAllPayments()
        {
            try
            {
                var payments = await _paymentService.GetAllAsync();
                var paymentsDto = _mapper.Map<IEnumerable<PaymentDto>>(payments);
                return Ok(new ApiResponse<IEnumerable<PaymentDto>>(paymentsDto));
            }
            catch (Exception ex)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError,
                    new ApiResponse<string>($"Error: {ex.Message}"));
            }
        }
        
    }
  }
