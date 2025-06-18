
using System.ComponentModel.DataAnnotations.Schema;
using top.ebiz.service.Models.Create_Trip;

namespace top.ebiz.service.Models.Traveler_Profile
{
    public class UploadFileModel
    {
        public string token_login { get; set; }
        public string file_doc { get; set; }
        public string file_page { get; set; }
        public string file_emp { get; set; }
        public string file_type { get; set; }
        public ImgList img_list { get; set; } = new ImgList();

        [NotMapped]
        public afterTripModel? after_trip { get; set; } = new afterTripModel();
    }
}