using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WaterServer.Model.Repositories;

public interface IUserRepository
{
    Task<User> GetById(int id);
    Task<User> GetByLoginName(string loginName);

    Task Add(User user);
    Task Update(User user);
    Task Delete(int id);
}
