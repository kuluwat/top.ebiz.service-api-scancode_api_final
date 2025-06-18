using Microsoft.Graph.Models;
using System.Text.Json.Serialization;

namespace top.ebiz.service.Service.AzureAD
{
    public class GraphMicrosoftUser
    {
        public string displayName { get; set; }
        public string givenName { get; set; }
        public string jobTitle { get; set; }
        public string mail { get; set; }
        public string mobilePhone { get; set; }
        public string officeLocation { get; set; }
        public string preferredLanguage { get; set; }
        public string surname { get; set; }
        public string userPrincipalName { get; set; }
        public string id { get; set; }

        public GraphMicrosoftUser(User user)
        {
            displayName = user.DisplayName ?? "";
            givenName = user.GivenName ?? "";
            jobTitle = user.JobTitle ?? "";
            mail = user.Mail ?? "";
            mobilePhone = user.MobilePhone ?? "";
            officeLocation = user.OfficeLocation ?? "";
            preferredLanguage = user.PreferredLanguage ?? "";
            surname = user.Surname ?? "";
            userPrincipalName = user.UserPrincipalName ?? "";
            id = user.Id ?? "";
        }
        public string getFullName()
        {
            return $"{givenName ?? ""} {surname ?? ""}";
        }


    }
}



