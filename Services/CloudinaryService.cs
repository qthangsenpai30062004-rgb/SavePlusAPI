using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Http;

namespace SavePlus_API.Services
{
    public class CloudinaryService : ICloudinaryService
    {
        private readonly Cloudinary _cloudinary;
        private readonly ILogger<CloudinaryService> _logger;

        public CloudinaryService(IConfiguration configuration, ILogger<CloudinaryService> logger)
        {
            var cloudName = configuration["Cloudinary:CloudName"];
            var apiKey = configuration["Cloudinary:ApiKey"];
            var apiSecret = configuration["Cloudinary:ApiSecret"];

            if (string.IsNullOrEmpty(cloudName) || string.IsNullOrEmpty(apiKey) || string.IsNullOrEmpty(apiSecret))
            {
                throw new Exception("Cloudinary configuration is missing");
            }

            var account = new Account(cloudName, apiKey, apiSecret);
            _cloudinary = new Cloudinary(account);
            _logger = logger;
        }

        public async Task<string?> UploadImageAsync(IFormFile file, string folder = "tenants")
        {
            if (file == null || file.Length == 0)
            {
                return null;
            }

            try
            {
                using var stream = file.OpenReadStream();
                var uploadParams = new ImageUploadParams
                {
                    File = new FileDescription(file.FileName, stream),
                    Folder = folder,
                    Transformation = new Transformation()
                        .Quality("auto")
                        .FetchFormat("auto")
                };

                var uploadResult = await _cloudinary.UploadAsync(uploadParams);

                if (uploadResult.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    return uploadResult.SecureUrl.ToString();
                }

                _logger.LogError("Cloudinary upload failed: {Error}", uploadResult.Error?.Message);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading image to Cloudinary");
                return null;
            }
        }

        public async Task<bool> DeleteImageAsync(string publicId)
        {
            if (string.IsNullOrEmpty(publicId))
            {
                return false;
            }

            try
            {
                var deleteParams = new DeletionParams(publicId);
                var result = await _cloudinary.DestroyAsync(deleteParams);
                
                return result.StatusCode == System.Net.HttpStatusCode.OK;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting image from Cloudinary: {PublicId}", publicId);
                return false;
            }
        }

        public string? GetPublicIdFromUrl(string url)
        {
            if (string.IsNullOrEmpty(url))
            {
                return null;
            }

            try
            {
                // URL format: https://res.cloudinary.com/{cloud_name}/image/upload/{transformations}/{version}/{public_id}.{format}
                var uri = new Uri(url);
                var segments = uri.AbsolutePath.Split('/');
                
                // Tìm segment "upload"
                var uploadIndex = Array.IndexOf(segments, "upload");
                if (uploadIndex == -1 || uploadIndex >= segments.Length - 1)
                {
                    return null;
                }

                // Lấy phần sau "upload", bỏ qua version nếu có
                var publicIdParts = segments.Skip(uploadIndex + 1).ToList();
                
                // Bỏ phần version nếu có (vXXXXXXXXXX)
                if (publicIdParts.Count > 0 && publicIdParts[0].StartsWith("v"))
                {
                    publicIdParts.RemoveAt(0);
                }

                // Ghép lại public_id và bỏ extension
                var publicIdWithExt = string.Join("/", publicIdParts);
                var lastDotIndex = publicIdWithExt.LastIndexOf('.');
                
                return lastDotIndex > 0 ? publicIdWithExt.Substring(0, lastDotIndex) : publicIdWithExt;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error parsing Cloudinary URL: {Url}", url);
                return null;
            }
        }
    }
}

