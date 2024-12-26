namespace Practice.Helpers
{
    public class FileHelper
    {
        public static async Task<byte[]> ConvertToByteArrayAsync(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                throw new ArgumentException("The provided file is invalid.");
            }

            using var memoryStream = new MemoryStream();
            await file.CopyToAsync(memoryStream);
            return memoryStream.ToArray();
        }
    }
}
