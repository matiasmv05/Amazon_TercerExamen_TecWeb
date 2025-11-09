using Amazon.api.Responses;
using Amazon.Core.CustomEntities;
using Amazon.Core.Entities;
using Amazon.Core.Interface;
using Amazon.infrastructure.DTOs;
using Amazon.Infrastructure.Repositories;
using Amazon.Infrastructure.Validators;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace Amazon.api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IProductService _productService;
        private readonly IMapper _mapper;
        private readonly IValidationService _validationService;

        public ProductsController(
            IValidationService validationService,
            IMapper mapper,
            IProductService productService,
            IUserService userService)
        {
            _mapper = mapper;
            _validationService = validationService;
            _productService = productService;
            _userService = userService;
        }

        #region DTO con AutoMapper
        /// <summary>
        /// Obtiene todos los productos registrados en el sistema
        /// </summary>
        /// <remarks>
        /// Este endpoint recupera una lista completa de todos los productos disponibles en el catálogo.
        /// Incluye información básica como nombre, categoría, precio y stock de cada producto.
        /// 
        /// Ejemplo de uso:
        /// GET /api/Products
        /// </remarks>
        /// <returns>Lista completa de todos los productos del sistema</returns>
        /// <response code="200">Retorna la lista de productos exitosamente</response>
        /// <response code="500">Error interno del servidor al procesar la solicitud</response>
        [ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(ApiResponse<IEnumerable<ProductDto>>))]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        [HttpGet]
        public async Task<IActionResult> GetAllProducts()
        {
            try
            {
                var products = await _productService.GetAllAsync();
                var productsDto = _mapper.Map<IEnumerable<ProductDto>>(products);
                var response = new ApiResponse<IEnumerable<ProductDto>>(productsDto);
                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError,
                    new ApiResponse<string>($"Error: {ex.Message}"));
            }
        }

        /// <summary>
        /// Obtiene un producto específico por su identificador único
        /// </summary>
        /// <remarks>
        /// Este endpoint recupera la información completa de un producto utilizando su ID único.
        /// Incluye detalles como descripción, precio, stock, categoría e información del vendedor.
        /// 
        /// Ejemplo de uso:
        /// GET /api/Products/5
        /// </remarks>
        /// <param name="id">Identificador único del producto (mayor a 0)</param>
        /// <returns>Información detallada del producto solicitado</returns>
        /// <response code="200">Retorna el producto encontrado exitosamente</response>
        /// <response code="400">Error en la validación del ID del producto</response>
        /// <response code="404">No se encontró el producto con el ID especificado</response>
        /// <response code="500">Error interno del servidor al procesar la solicitud</response>
        [ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(ApiResponse<ProductDto>))]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetbyId(int id)
        {
            try
            {
                #region Validaciones
                var validationRequest = new GetByIdRequest { Id = id };
                var validationResult = await _validationService.ValidateAsync(validationRequest);

                if (!validationResult.IsValid)
                {
                    return BadRequest(new ApiResponse<object>(new
                    {
                        Message = "Error de validación del ID",
                        Errors = validationResult.Errors
                    }));
                }
                #endregion

                var product = await _productService.GetByIdAsync(id);
                if (product == null)
                    return NotFound(new ApiResponse<string>("Producto no encontrado"));

                var productDto = _mapper.Map<ProductDto>(product);
                var response = new ApiResponse<ProductDto>(productDto);
                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError,
                    new ApiResponse<string>($"Error: {ex.Message}"));
            }
        }

        /// <summary>
        /// Crea un nuevo producto en el sistema
        /// </summary>
        /// <remarks>
        /// Este endpoint permite registrar un nuevo producto en el catálogo de Amazon.
        /// Requiere validación de datos y verificación de que el vendedor exista.
        /// 
        /// Ejemplo de body:
        /// {
        ///   "name": "Laptop Gaming",
        ///   "description": "Laptop para gaming de alta performance",
        ///   "price": 1500.00,
        ///   "stock": 10,
        ///   "category": "Electrónicos",
        ///   "sellerId": 1
        /// }
        /// </remarks>
        /// <param name="productDto">Datos del producto a crear</param>
        /// <returns>Producto creado con su identificador asignado</returns>
        /// <response code="200">Producto creado exitosamente</response>
        /// <response code="400">Error en la validación de datos o vendedor no existe</response>
        /// <response code="500">Error interno del servidor al crear el producto</response>
        [ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(ApiResponse<ProductDto>))]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        [HttpPost]
        public async Task<IActionResult> InsertProduct([FromBody] ProductDto productDto)
        {
            try
            {
                #region Validaciones
                var validationResult = await _validationService.ValidateAsync(productDto);

                if (!validationResult.IsValid)
                {
                    return BadRequest(new ApiResponse<object>(new
                    {
                        Message = "Error de validación",
                        Errors = validationResult.Errors
                    }));
                }
                #endregion

                var seller = await _userService.GetByIdAsync(productDto.SellerId);
                if (seller == null)
                {
                    return BadRequest(new ApiResponse<string>("El vendedor especificado no existe"));
                }

                var product = _mapper.Map<Product>(productDto);
                await _productService.AddAsync(product);

                var createdProduct = await _productService.GetByIdAsync(product.Id);

                var productDtoResponse = _mapper.Map<ProductDto>(createdProduct);

                var response = new ApiResponse<ProductDto>(productDtoResponse);
                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError,
                    new ApiResponse<string>($"Error al crear producto: {ex.Message}"));
            }
        }

        /// <summary>
        /// Actualiza la información de un producto existente
        /// </summary>
        /// <remarks>
        /// Este endpoint permite modificar la información de un producto existente en el sistema.
        /// Todos los campos del producto pueden ser actualizados excepto el ID.
        /// 
        /// Ejemplo de uso:
        /// PUT /api/Products/5
        /// </remarks>
        /// <param name="id">Identificador único del producto a actualizar</param>
        /// <param name="productDto">Nuevos datos del producto</param>
        /// <returns>Producto actualizado con la nueva información</returns>
        /// <response code="200">Producto actualizado exitosamente</response>
        /// <response code="400">Error de validación o IDs no coinciden</response>
        /// <response code="404">No se encontró el producto a actualizar</response>
        /// <response code="500">Error interno del servidor al actualizar</response>
        [ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(ApiResponse<ProductDto>))]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateProduct(int id, [FromBody] ProductDto productDto)
        {
            try
            {
                if (id != productDto.Id)
                    return BadRequest(new ApiResponse<string>("El Id del Producto no coincide"));

                var product = await _productService.GetByIdAsync(id);
                if (product == null)
                    return NotFound(new ApiResponse<string>("Producto no encontrado"));

                #region Validaciones
                var validationResult = await _validationService.ValidateAsync(productDto);

                if (!validationResult.IsValid)
                {
                    return BadRequest(new ApiResponse<object>(new
                    {
                        Message = "Error de validación",
                        Errors = validationResult.Errors
                    }));
                }
                #endregion

                _mapper.Map(productDto, product);

                await _productService.Update(product);
                var updatedProduct = await _productService.GetByIdAsync(id);
                var productDtoResponse = _mapper.Map<ProductDto>(updatedProduct);

                var response = new ApiResponse<ProductDto>(productDtoResponse);
                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError,
                    new ApiResponse<string>($"Error al actualizar producto: {ex.Message}"));
            }
        }

        /// <summary>
        /// Elimina un producto del sistema
        /// </summary>
        /// <remarks>
        /// Este endpoint permite eliminar permanentemente un producto del catálogo.
        /// La eliminación es irreversible y requiere que el producto exista.
        /// 
        /// Ejemplo de uso:
        /// DELETE /api/Products/5
        /// </remarks>
        /// <param name="id">Identificador único del producto a eliminar</param>
        /// <returns>Respuesta sin contenido (204) si la eliminación fue exitosa</returns>
        /// <response code="204">Producto eliminado exitosamente</response>
        /// <response code="404">No se encontró el producto a eliminar</response>
        /// <response code="500">Error interno del servidor al eliminar</response>
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProductDtoMapper(int id)
        {
            try
            {
                var product = await _productService.GetByIdAsync(id);
                if (product == null)
                    return NotFound(new ApiResponse<string>("Producto no encontrado"));

                await _productService.Delete(product.Id);
                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError,
                    new ApiResponse<string>($"Error al eliminar producto: {ex.Message}"));
            }
        }

        #endregion

        /// <summary>
        /// Obtiene productos filtrados por categoría específica
        /// </summary>
        /// <remarks>
        /// Este endpoint recupera todos los productos que pertenecen a una categoría específica.
        /// Útil para filtrar productos por tipo (Electrónicos, Ropa, Hogar, etc.).
        /// 
        /// Ejemplo de uso:
        /// GET /api/Products/category/Electrónicos
        /// </remarks>
        /// <param name="category">Nombre de la categoría a filtrar</param>
        /// <returns>Lista de productos que pertenecen a la categoría especificada</returns>
        /// <response code="200">Retorna los productos de la categoría exitosamente</response>
        /// <response code="500">Error interno del servidor al procesar la solicitud</response>
        [ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(ApiResponse<IEnumerable<ProductDto>>))]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        [HttpGet("category/{category}")]
        public async Task<IActionResult> GetProductsByCategory(string category)
        {
            try
            {
                var products = await _productService.GetByCategoryAsync(category);
                var productsDto = _mapper.Map<IEnumerable<ProductDto>>(products);
                var response = new ApiResponse<IEnumerable<ProductDto>>(productsDto);
                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError,
                    new ApiResponse<string>($"Error: {ex.Message}"));
            }
        }

        /// <summary>
        /// Obtiene todos los productos de un vendedor específico
        /// </summary>
        /// <remarks>
        /// Este endpoint recupera el catálogo completo de productos de un vendedor particular.
        /// Útil para ver todos los productos que un vendedor tiene registrados en el sistema.
        /// 
        /// Ejemplo de uso:
        /// GET /api/Products/seller/1
        /// </remarks>
        /// <param name="sellerId">Identificador único del vendedor</param>
        /// <returns>Lista de productos asociados al vendedor especificado</returns>
        /// <response code="200">Retorna los productos del vendedor exitosamente</response>
        /// <response code="400">Error en la validación del ID del vendedor</response>
        /// <response code="500">Error interno del servidor al procesar la solicitud</response>
        [ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(ApiResponse<IEnumerable<ProductDto>>))]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        [HttpGet("seller/{sellerId}")]
        public async Task<IActionResult> GetProductsBySeller(int sellerId)
        {
            try
            {
                #region Validaciones
                var validationRequest = new GetByIdRequest { Id = sellerId };
                var validationResult = await _validationService.ValidateAsync(validationRequest);

                if (!validationResult.IsValid)
                {
                    return BadRequest(new ApiResponse<object>(new
                    {
                        Message = "Error de validación del ID del vendedor",
                        Errors = validationResult.Errors
                    }));
                }
                #endregion

                var products = await _productService.GetBySellerAsync(sellerId);
                var productsDto = _mapper.Map<IEnumerable<ProductDto>>(products);
                var response = new ApiResponse<IEnumerable<ProductDto>>(productsDto);
                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError,
                    new ApiResponse<string>($"Error: {ex.Message}"));
            }
        }

        /// <summary>
        /// Actualiza el stock de un producto específico
        /// </summary>
        /// <remarks>
        /// Este endpoint permite modificar únicamente la cantidad en stock de un producto.
        /// Útil para gestionar inventario sin modificar otros datos del producto.
        /// 
        /// Ejemplo de uso:
        /// PATCH /api/Products/5/stock/25
        /// </remarks>
        /// <param name="id">Identificador único del producto</param>
        /// <param name="quantity">Nueva cantidad en stock (debe ser mayor a 0)</param>
        /// <returns>Producto con el stock actualizado</returns>
        /// <response code="200">Stock actualizado exitosamente</response>
        /// <response code="400">Error de validación o cantidad inválida</response>
        /// <response code="404">No se encontró el producto</response>
        /// <response code="500">Error interno del servidor al actualizar</response>
        [ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(ApiResponse<Product>))]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        [HttpPatch("{id}/stock/{quantity}")]
        public async Task<IActionResult> UpdateStock(int id, int quantity)
        {
            try
            {
                #region Validaciones
                var validationRequest = new GetByIdRequest { Id = id };
                var validationResult = await _validationService.ValidateAsync(validationRequest);

                if (!validationResult.IsValid)
                {
                    return BadRequest(new ApiResponse<object>(new
                    {
                        Message = "Error de validación del ID",
                        Errors = validationResult.Errors
                    }));
                }
                #endregion

                var product = await _productService.GetByIdAsync(id);
                if (product == null)
                    return NotFound(new ApiResponse<string>("Producto no encontrado"));

                product.Stock = quantity;
                await _productService.Update(product);

                var response = new ApiResponse<Product>(product);
                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError,
                    new ApiResponse<string>($"Error: {ex.Message}"));
            }
        }


        /// <summary>
        /// Endpoint de prueba para verificar el funcionamiento del controlador
        /// </summary>
        /// <remarks>
        /// Este endpoint permite verificar que el controlador de productos esté funcionando correctamente.
        /// No realiza operaciones sobre la base de datos, solo retorna un mensaje de confirmación.
        /// 
        /// Ejemplo de uso:
        /// GET /api/Products/test
        /// </remarks>
        /// <returns>Mensaje de confirmación del funcionamiento</returns>
        /// <response code="200">API funcionando correctamente</response>
        [ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(ApiResponse<string>))]
        [HttpGet("test")]
        public IActionResult Test()
        {
            return Ok(new ApiResponse<string>("Products API funcionando correctamente"));
        }

    }
}