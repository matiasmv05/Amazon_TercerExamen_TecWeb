using Amazon.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Amazon.Core.Interface
{
    public interface IUserService
    {
        Task<User> GetByEmailAsync(string email);
        Task<IEnumerable<User>> GetAllAsync();
        Task UpdateWalletAsync(int userId, decimal amount);

        Task<User> GetByIdAsync(int id);
        Task AddAsync(User user);
        Task Update(User user);
        Task Delete(int id);
    }
}
