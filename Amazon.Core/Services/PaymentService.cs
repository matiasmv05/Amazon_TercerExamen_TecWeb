using Amazon.Core.Entities;
using Amazon.Core.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Amazon.Core.Services
{
    public class PaymentService : IPaymentService
    {
        //private readonly IOrderRepository _orderRepository;
        //private readonly IUserRepository _userRepository;
        //private readonly IProductRepository _productRepository;
        //private readonly IPaymentRepository _paymentRepository;

        public readonly IUnitOfWork _unitOfWork;


        public PaymentService(
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

        public async Task<IEnumerable<Payment>> GetAllAsync()
        {
            return await _unitOfWork.PaymentRepository.GetAllAsync();
        }

        public async Task<Payment> GetByIdAsync(int id)
        {
            return await _unitOfWork.PaymentRepository.GetById(id);
        }

        public async Task AddAsync(Payment payment)
        {
            await _unitOfWork.PaymentRepository.Add(payment);
            await _unitOfWork.SaveChangesAsync();

        }

        public async Task Update(Payment payment)
        {
            await _unitOfWork.PaymentRepository.Update(payment);
            await _unitOfWork.SaveChangesAsync();

        }

        public async Task Delete(int id)
        {
            await _unitOfWork.PaymentRepository.Delete(id);
            await _unitOfWork.SaveChangesAsync();

        }

        public Task<Payment> GetByOrderIdAsync(int orderId)
        {
           return _unitOfWork.PaymentRepository.GetByOrderIdAsync(orderId);
        }
    }
}
