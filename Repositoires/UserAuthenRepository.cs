using top.ebiz.service.Models.Create_Trip;
using top.ebiz.service.Service.Create_Trip;

namespace top.ebiz.Repositoires;
public class UserAuthenRepository : IUserAuthenRepository
{
    public loginResultModel Login(loginModel value)
    {
        var service = new userAuthenService();
        return service.login(value);
    }
}