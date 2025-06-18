 
namespace top.ebiz.service.Models.Create_Trip
{
    public class loginModel
    {
        public string? token_login { get; set; }
        public string? user_id { get; set; }
        public string? user_name { get; set; }
    }
    public class loginProfileModel
    {
        public string token_login { get; set; }
    }
    public class loginClientModel
    {
        public string? user { get; set; }
        public string? pass { get; set; }
    }
    public class loginProfileResultModel
    {
        public string? empId { get; set; }
        public string? empName { get; set; }
        public string? deptName { get; set; }
        public string? imgUrl { get; set; }
    }
    public class loginResultModel
    {
        public string? msg_sts { get; set; }
        public string? msg_txt { get; set; }
        public string? token_login { get; set; }
    }
    public class loginWebResultModel
    {
        public string? token { get; set; }
        public string? name { get; set; }
        public string? msg_sts { get; set; }
        public string? msg_txt { get; set; }
    }
    public class logoutModel
    {
        public string token { get; set; }
    }
    public class Users
    {
        public string? Email { get; set; }
        public string? UserName { get; set; }
        public string? DisplayName { get; set; }
        public bool? isMapped { get; set; }
        public string? MemberOf { get; set; }
        public string? Remark { get; set; }
    }
}