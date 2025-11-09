using Amazon.Core.Entities;
using Amazon.Core.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Amazon.Core.Services
{
    public class ProductService : IProductService
    {
        //private readonly IOrderRepository _orderRepository;
        //private readonly IUserRepository _userRepository;
        //private readonly IProductRepository _productRepository;
        //private readonly IPaymentRepository _paymentRepository;

        public readonly IUnitOfWork _unitOfWork;


        public ProductService(
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

        public async Task AddAsync(Product product)
        {
            await _unitOfWork.ProductRepository.Add(product);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task Delete(int id)
        {
           await _unitOfWork.ProductRepository.Delete(id);
            await _unitOfWork.SaveChangesAsync();

        }

        public async Task<IEnumerable<Product>> GetAllAsync()
        {
            return await _unitOfWork.ProductRepository.GetAll();
        }

        public async Task<IEnumerable<Product>> GetByCategoryAsync(string category)
        {
            return await _unitOfWork.ProductRepository.GetByCategoryAsync(category);
        }

        public async Task<Product> GetByIdAsync(int id)
        {
            return await _unitOfWork.ProductRepository.GetById(id);
        }

        public Task<IEnumerable<Product>> GetBySellerAsync(int sellerId)
        {
            return _unitOfWork.ProductRepository.GetBySellerAsync(sellerId);
        }

        public async Task Update(Product product)
        {
            await _unitOfWork.ProductRepository.Update(product);
            await _unitOfWork.SaveChangesAsync();
        }
    }
}
