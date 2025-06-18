
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
 
namespace top.ebiz.service.SwaggerHeader
{
    public class SwaggerHeaderFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            if (operation.Parameters == null)
                operation.Parameters = new List<OpenApiParameter>();

            // ตรวจสอบว่าไม่ใช่คำขอสำหรับ GetAntiForgeryToken เพื่อไม่ต้องบังคับให้มี X-CSRF-TOKEN
            var ignoreCsrfTokenRoutes = new List<string> { "GetAntiForgeryToken" }; // เส้นทางหรือ method ที่ต้องการข้าม

            // ข้ามการบังคับใช้ CSRF Token สำหรับ method ที่เราต้องการข้าม
            if (!ignoreCsrfTokenRoutes.Contains(context.ApiDescription.RelativePath))
            {
                // เพิ่ม Header สำหรับ X-CSRF-TOKEN ในคำขอที่ไม่ใช่ GetAntiForgeryToken
                operation.Parameters.Add(new OpenApiParameter
                {
                    Name = "X-CSRF-TOKEN",
                    In = ParameterLocation.Header,
                    Required = true, // บังคับให้ต้องส่ง header นี้ในคำขออื่น ๆ
                    Schema = new OpenApiSchema
                    {
                        Type = "string"
                    },
                    Description = "CSRF token ที่ได้รับจากการเรียก GetAntiForgeryToken"
                });
            }
        }
    }

}