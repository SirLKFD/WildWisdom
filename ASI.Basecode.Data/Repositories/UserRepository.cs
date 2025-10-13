using ASI.Basecode.Data.EFCore;
using ASI.Basecode.Data.Interfaces;
using ASI.Basecode.Data.Models;
using Basecode.Data.Repositories;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ASI.Basecode.Data.Repositories
{
    public class UserRepository : BaseRepository<User>, IUserRepository
    {
        public UserRepository(IUnitOfWork unitOfWork) : base(unitOfWork) 
        {

        }

        public IQueryable<User> GetUsers()
        {
            return this.GetDbSet<User>();
        }

        public bool UserExists(string userId)
        {
            return this.GetDbSet<User>().Any(x => x.UserId == userId);
        }

        public void AddUser(User user)
        {
            this.GetDbSet<User>().Add(user);
            UnitOfWork.SaveChanges();
        }

        public IQueryable<User> GetUsersByRole(int role)
        {
            return this.GetDbSet<User>().Where(x => x.Role == role);
        }

        public void UpdateUserRole(string userId, int role)
        {
            var user = this.GetDbSet<User>().FirstOrDefault(x => x.UserId == userId);
            if (user != null)
            {
                user.Role = role;
                UnitOfWork.SaveChanges();
            }
        }

        public User GetById(int id)
        {
            return this.GetDbSet<User>()
                .Include(u => u.Organization)
                .FirstOrDefault(x => x.Id == id);
        }

        public User GetByUserId(string userId)
        {
            return this.GetDbSet<User>()
                .Include(u => u.Organization)
                .FirstOrDefault(x => x.UserId == userId);
        }

        public void UpdateUser(User user)
        {
            this.GetDbSet<User>().Update(user);
            UnitOfWork.SaveChanges();
        }

        public void DeleteUser(int id)
        {
            var user = this.GetDbSet<User>().FirstOrDefault(x => x.Id == id);
            if (user != null)
            {
                this.GetDbSet<User>().Remove(user);
                UnitOfWork.SaveChanges();
            }
        }
    }
}
