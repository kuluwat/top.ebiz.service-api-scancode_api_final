using top.ebiz.service.Models.Create_Trip;

namespace top.ebiz.Repositoires;
public interface IUserAuthenRepository
{
    public loginResultModel Login(loginModel value);
}