using Microsoft.AspNetCore.Http;

namespace SavePlus_API.Services
{
    public interface ICloudinaryService
    {
        /// <summary>
        /// Upload ảnh lên Cloudinary
        /// </summary>
        Task<string?> UploadImageAsync(IFormFile file, string folder = "tenants");

        /// <summary>
        /// Xóa ảnh từ Cloudinary
        /// </summary>
        Task<bool> DeleteImageAsync(string publicId);

        /// <summary>
        /// Lấy public ID từ URL Cloudinary
        /// </summary>
        string? GetPublicIdFromUrl(string url);
    }
}
