using ASI.Basecode.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ASI.Basecode.Data.Interfaces
{
    public interface IUserRepository
    {
        IQueryable<User> GetUsers();
        bool UserExists(string userId);
        void AddUser(User user);
        User GetById(int id);
        User GetByUserId(string userId);
        void UpdateUser(User user);
        void DeleteUser(int id);
    }
}
