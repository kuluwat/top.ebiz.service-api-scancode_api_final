
using System.ComponentModel.DataAnnotations.Schema;
using top.ebiz.service.Models.Create_Trip;


namespace top.ebiz.service.Models.Traveler_Profile
{
    public class ResendEmailModel
    {
        public string token_login { get; set; }
        public string? doc_id { get; set; } = string.Empty;
        public int page_index { get; set; } = 0;
        public int page_size { get; set; } = 20;
    }

    public class ResendEmailOutModel
    {
        public string? token_login { get; set; } = string.Empty;
        public string? doc_id { get; set; } = string.Empty;
        public string? id { get; set; } = string.Empty;
        public Boolean user_admin { get; set; }
        public string data_type { get; set; }
        public int totalCount { get; set; }
        public List<listResendEmailModel> emailList { get; set; } = new List<listResendEmailModel>();

        [NotMapped]
        public afterTripModel after_trip { get; set; } = new afterTripModel();
    }
    public class listResendEmailModel
    {
        public string? doc_id { get; set; }
        public string? id { get; set; }
        public string? stepflow { get; set; }
        public string? fromemail { get; set; }
        public string? torecipients { get; set; }
        public string? ccrecipients { get; set; }
        public string? bccrecipients { get; set; }
        public string? subject { get; set; }
        public string? body { get; set; }
        public string? attachments { get; set; }
        public string? statussend { get; set; }
        public string? errorsend { get; set; }
        public string? datesendtext { get; set; }
        public string? activetype { get; set; }
        public string? action_type { get; set; }
        public string? action_change { get; set; }
    }
}