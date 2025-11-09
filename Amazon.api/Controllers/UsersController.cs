using Amazon.api.Responses;
using Amazon.Core.CustomEntities;
using Amazon.Core.Entities;
using Amazon.Core.Interface;
using Amazon.Core.Services;
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
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IMapper _mapper;
        private readonly IValidationService _validationService;

        public UsersController(IUserService userService, IMapper mapper, IValidationService validationService)
        {
            _mapper = mapper;
            _validationService = validationService;
            _userService = userService;
        }

        /// <summary>
        /// Obtiene todos los usuarios registrados en el sistema
        /// </summary>
        /// <remarks>
        /// Este endpoint recupera una lista completa de todos los usuarios del sistema.
        /// Incluye información básica como nombre, email y saldo de billetera.
        /// </remarks>
        /// <returns>Lista completa de todos los usuarios del sistema</returns>
        /// <response code="200">Retorna la lista de usuarios exitosamente</response>
        /// <response code="500">Error interno del servidor al procesar la solicitud</response>
        [HttpGet]
        public async Task<IActionResult> GetAllUsers()
        {
            try
            {
                var users = await _userService.GetAllAsync();
                var usersDto = _mapper.Map<IEnumerable<UserDto>>(users);
                var response = new ApiResponse<IEnumerable<UserDto>>(usersDto);
                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError,
                new ApiResponse<string>($"Error: {ex.Message}"));
            }
        }

        /// <summary>
        /// Obtiene un usuario específico por su identificador único
        /// </summary>
        /// <remarks>
        /// Este endpoint recupera la información completa de un usuario utilizando su ID único.
        /// Incluye detalles como nombre, email, saldo de billetera y fecha de creación.
        /// </remarks>
        /// <param name="id">Identificador único del usuario (mayor a 0)</param>
        /// <returns>Información detallada del usuario solicitado</returns>
        /// <response code="200">Retorna el usuario encontrado exitosamente</response>
        /// <response code="400">Error en la validación del ID del usuario</response>
        /// <response code="404">No se encontró el usuario con el ID especificado</response>
        /// <response code="500">Error interno del servidor al procesar la solicitud</response>
        [ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(ApiResponse<UserDto>))]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetUserById(int id)
        {
            try
            {
                #region Validaciones
                var validationRequest = new GetByIdRequest { Id = id };
                var validationResult = await _validationService.ValidateAsync(validationRequest);

                if (!validationResult.IsValid)
                {
                    return BadRequest(new
                    {
                        Message = "Error de validación del ID",
                        Errors = validationResult.Errors
                    });
                }
                #endregion

                var user = await _userService.GetByIdAsync(id);

                var userDto = _mapper.Map<UserDto>(user);
                var response = new ApiResponse<UserDto>(userDto);
                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError,
                    new ApiResponse<string>($"Error: {ex.Message}"));
            }
        }

        /// <summary>
        /// Crea un nuevo usuario en el sistema
        /// </summary>
        /// <remarks>
        /// Este endpoint permite registrar un nuevo usuario en el sistema Amazon.
        /// Requiere validación de datos y verificación de que el email no esté registrado.
        /// </remarks>
        /// <param name="userDto">Datos del usuario a crear</param>
        /// <returns>Usuario creado con su identificador asignado</returns>
        /// <response code="200">Usuario creado exitosamente</response>
        /// <response code="400">Error en la validación de datos o email ya registrado</response>
        /// <response code="500">Error interno del servidor al crear el usuario</response>
        [ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(ApiResponse<User>))]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        [HttpPost]
        public async Task<IActionResult> CreateUser([FromBody] UserDto userDto)
        {
            try
            {
                #region Validaciones
                var validationResult = await _validationService.ValidateAsync(userDto);
                if (!validationResult.IsValid)
                {
                    return BadRequest(new ApiResponse<object>(new
                    {
                        Message = "Error de validación",
                        Errors = validationResult.Errors
                    }));
                }

                // Validar si el email ya existe
                var existingUser = await _userService.GetByEmailAsync(userDto.Email);
                
                #endregion

                var user = _mapper.Map<User>(userDto);
                await _userService.AddAsync(user);


                var response = new ApiResponse<User>(user);
                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError,
                     new ApiResponse<string>($"Error: {ex.Message}"));
            }
        }

        /// <summary>
        /// Actualiza la información de un usuario existente
        /// </summary>
        /// <remarks>
        /// Este endpoint permite modificar la información de un usuario existente en el sistema.
        /// Todos los campos del usuario pueden ser actualizados excepto el ID.
        /// </remarks>
        /// <param name="id">Identificador único del usuario a actualizar</param>
        /// <param name="userDto">Nuevos datos del usuario</param>
        /// <returns>Usuario actualizado con la nueva información</returns>
        /// <response code="200">Usuario actualizado exitosamente</response>
        /// <response code="400">Error de validación o IDs no coinciden</response>
        /// <response code="404">No se encontró el usuario a actualizar</response>
        /// <response code="500">Error interno del servidor al actualizar</response>
        [ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(ApiResponse<User>))]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(int id, [FromBody] UserDto userDto)
        {
            try
            {
                if (id != userDto.Id)
                    return BadRequest("El Id del Usuario no coincide");

                var user = await _userService.GetByIdAsync(id);
                if (user == null)
                    return NotFound("Usuario no encontrado");

                #region Validaciones
                var validationResult = await _validationService.ValidateAsync(userDto);

                if (!validationResult.IsValid)
                {
                    return BadRequest(new { Errors = validationResult.Errors });
                }
                #endregion

                _mapper.Map(userDto, user);
                await _userService.Update(user);

                var response = new ApiResponse<User>(user);
                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError,
                    new ApiResponse<string>($"Error: {ex.Message}"));
            }
        }

        /// <summary>
        /// Elimina un usuario del sistema
        /// </summary>
        /// <remarks>
        /// Este endpoint permite eliminar permanentemente un usuario del sistema.
        /// La eliminación es irreversible y requiere que el usuario exista.
        /// </remarks>
        /// <param name="id">Identificador único del usuario a eliminar</param>
        /// <returns>Respuesta sin contenido (204) si la eliminación fue exitosa</returns>
        /// <response code="204">Usuario eliminado exitosamente</response>
        /// <response code="404">No se encontró el usuario a eliminar</response>
        /// <response code="500">Error interno del servidor al eliminar</response>
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            try
            {
                var user = await _userService.GetByIdAsync(id);
                if (user == null)
                    return NotFound("Usuario no encontrado");

                await _userService.Delete(user.Id);
                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError,
                    new ApiResponse<string>($"Error: {ex.Message}"));
            }
        }

        /// <summary>
        /// Obtiene el saldo de billetera de un usuario específico
        /// </summary>
        /// <remarks>
        /// Este endpoint recupera únicamente el saldo actual de la billetera de un usuario.
        /// Útil para consultas rápidas de saldo sin cargar toda la información del usuario.
        /// </remarks>
        /// <param name="id">Identificador único del usuario</param>
        /// <returns>Saldo actual de la billetera del usuario</returns>
        /// <response code="200">Retorna el saldo de billetera exitosamente</response>
        /// <response code="400">Error en la validación del ID del usuario</response>
        /// <response code="404">No se encontró el usuario especificado</response>
        /// <response code="500">Error interno del servidor al procesar la solicitud</response>
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        [HttpGet("{id}/billetera")]
        public async Task<IActionResult> GetUserBalance(int id)
        {
            try
            {
                #region Validaciones
                var validationRequest = new GetByIdRequest { Id = id };
                var validationResult = await _validationService.ValidateAsync(validationRequest);

                    if (!validationResult.IsValid)
                    {
                        return BadRequest(new { Errors = validationResult.Errors });
                    }
                
                #endregion

                var user = await _userService.GetByIdAsync(id);
                if (user == null)
                    return NotFound("Usuario no encontrado");

                return Ok(new { UserId = user.Id, Billetera = user.Billetera });
            }
            catch (Exception ex)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError,
                    new ApiResponse<string>($"Error: {ex.Message}"));
            }
        }

        /// <summary>
        /// Recarga saldo en la billetera de un usuario
        /// </summary>
        /// <remarks>
        /// Este endpoint permite agregar saldo a la billetera de un usuario existente.
        /// El monto puede ser positivo (recarga) o negativo (debito).
        /// </remarks>
        /// <param name="id">Identificador único del usuario</param>
        /// <param name="amount">Monto a recargar en la billetera</param>
        /// <returns>Usuario con el saldo actualizado</returns>
        /// <response code="200">Billetera recargada exitosamente</response>
        /// <response code="404">No se encontró el usuario especificado</response>
        /// <response code="500">Error interno del servidor al procesar la recarga</response>
        [ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(ApiResponse<User>))]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        [HttpPost("{id}/billetera/llenar/{amount}")]
        public async Task<IActionResult> Wallet(int id, decimal amount)
        {
            try
            {

                // 1. Verificar usuario
                var user = await _userService.GetByIdAsync(id);

                // 2. Intentar actualizar
                await _userService.UpdateWalletAsync(id, amount);

                // 3. Verificar resultado
                var UserWallet = await _userService.GetByIdAsync(id);
                var userResponse = new ApiResponse<User>(UserWallet);

                return Ok(userResponse);
            }
            catch (Exception ex)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError,
                    new ApiResponse<string>($"Error: {ex.Message}"));
            }
        }
    }
}
