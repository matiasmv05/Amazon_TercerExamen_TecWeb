using Amazon.Core.Entities;
using Amazon.Core.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Amazon.Core.Services
{
    public class UserService : IUserService
    {
        //private readonly IOrderRepository _orderRepository;
        //private readonly IUserRepository _userRepository;
        //private readonly IProductRepository _productRepository;
        //private readonly IPaymentRepository _paymentRepository;

        public readonly IUnitOfWork _unitOfWork;


        public UserService(
             //IOrderRepository orderRepository,
             //IUserRepository userRepository,
             //IProductRepository productRepository,
             //IPaymentRepository paymentRepository)
             IUnitOfWork unitOfWork)
        {
            //_orderRepository = orderRepository;
            //_userRepository = userRepository;
            //_productRepository = productRepository;
            //_paymentRepository = paymentRepository;
            _unitOfWork = unitOfWork;
        }


        /// //////////////////////////////////////////////////////////////////
        public async Task<User> GetByEmailAsync(string email)
        {
        return await _unitOfWork.UserRepository.GetByEmailAsync(email);
        }

        public async Task<IEnumerable<User>> GetAllAsync()
        {
        return await _unitOfWork.UserRepository.GetAllAsync();
        }

        public async Task UpdateWalletAsync(int userId, decimal amount)
        {
        await _unitOfWork.UserRepository.UpdateWalletAsync(userId, amount);
            await _unitOfWork.SaveChangesAsync();

        }

        public async Task<User> GetByIdAsync(int id)
        {
            return await _unitOfWork.UserRepository.GetById(id);
        }

        public async Task AddAsync(User user)
        {
        await _unitOfWork.UserRepository.Add(user);
            await _unitOfWork.SaveChangesAsync();

        }

        public async Task Update(User user)
        {
            await _unitOfWork.UserRepository.Update(user);
            await _unitOfWork.SaveChangesAsync();

        }

        public async Task Delete(int id)
        {
            await _unitOfWork.UserRepository.Delete(id);
            await _unitOfWork.SaveChangesAsync();

        }
    }
}
