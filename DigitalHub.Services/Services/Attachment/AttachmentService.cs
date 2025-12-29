using AutoMapper;
using DigitalHub.Domain.Domains;
using DigitalHub.Services.DTO;
using DigitalHub.Services.Shared;
using ImageMagick;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System.Drawing;

namespace DigitalHub.Services.Services.Attachment
{
    public class AttachmentService(IGenericRepository<AttachmentTransaction> request, IConfiguration configuration, IMapper mapper) : IAttachmentService
    {
        public IGenericRepository<AttachmentTransaction> _request { get; } = request;
        public IConfiguration Configuration { get; } = configuration;
        public IMapper Mapper { get; } = mapper;
        public string CurDate => DateTime.Now.ToString("dd/MM/yyyy").Replace("/", "");
        public string AllowedExt => Configuration["FileServer:AllowedExtension"];
        public string FileURL => Configuration["FileServer:FileURL"];
        public string DirectoryPath => Configuration["FileServer:Directory"];
        public string AllowedFileSize => Configuration["FileServer:AllowedFileSize"];
        public string FileServer_Extension => Configuration["FileServer:Extension"];
        public async Task<List<AttachmentTransaction>> UploadAttachment(List<IFormFile> files)
        {
            var list = new List<AttachmentTransaction>();
            foreach (var file in files)
            {
                string contentType = file.ContentType;
                string FileID = Guid.NewGuid().ToString().ToUpper().Replace("-", "");
                string FileExtension = Path.GetExtension(file.FileName);
                string Save_FileName = FileID + FileExtension;
                var Save_ThumbFile = FileID + "_thumb" + FileExtension;
                string path = GetFileServerPath();

                var filePath = Path.Combine(path, Save_FileName);
                if (contentType != null && contentType.Contains(';'))
                {
                    contentType = contentType.Split(";")[0];
                }
                if (file.Length > int.Parse(AllowedFileSize))
                {
                    continue;
                }
                using (var stream = new FileStream(filePath, FileMode.Create, FileAccess.Write))
                {
                    await file.CopyToAsync(stream);
                }
                var Thumb_Path = Path.Combine(path, Save_ThumbFile);
                var IsThumb = SaveThumbImage(FileExtension[1..], Thumb_Path, filePath);
               // IsThumb = await GenerateThumbnailVideo(filePath, Thumb_Path);

                var attach = new AttachmentTransaction
                {
                    // FilePath = Path.Combine(FileURL, CurDate, Save_FileName),
                    FilePath = CurDate,
                    FileMimeType = file.ContentType,
                    FileExtension = FileExtension,
                    FileSize = (int)file.Length,
                    FileId = FileID,
                    FileName = file.FileName,
                    IsThumb = IsThumb,
                    //ThumbPath = IsThumb ? CurDate : GetIconImg(FileExtension),
                };

                await _request.InsertAsync(attach, true);
                list.Add(attach);
            }

            return list;
        }
        public async Task<bool> AddAttachment(AttachmentTransactionDTO model)
        {
            var result = Mapper.Map<AttachmentTransaction>(model);
            await _request.InsertAsync(result, true);

            return true;
        }
        public async Task<bool> UpdateAttachment(AttachmentTransactionDTO model)
        {
            if (model == null)
            {
                return false;
            }
            var idea = Mapper.Map<AttachmentTransaction>(model);
            await _request.InsertAsync(idea, true);

            return true;
        }
        public bool SaveThumbImage(string FileExt, string Thumb_Path, string filePath)
        {
            try
            {
                uint reduce = 60;
                string ImageFileEx = FileServer_Extension;
                if (ImageFileEx.Contains(FileExt.ToUpper()))
                {
                    using var image = new MagickImage(filePath);
                    image.Resize(image.Width / 2, image.Height / 2);
                    image.Strip();
                    image.Quality = 100 - reduce;
                    image.Write(Thumb_Path);
                    return true;
                }
                return false;
            }
            catch (Exception e)
            {
                return false;
            }
        }
        private async Task<bool> GenerateThumbnailVideo(string videoPath, string thumbPath)
        {
            try
            {
                var ffmpegPath = Path.Combine(AppContext.BaseDirectory, "Tools", "ffmpeg.exe");
                var ffmpeg = new System.Diagnostics.Process
                {
                    StartInfo = {
                        FileName = ffmpegPath, // Use full path here
                        Arguments = $"-i \"{videoPath}\" -ss 00:00:01 -vframes 1 \"{thumbPath}\"",
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    }
                };

                ffmpeg.Start();
                await ffmpeg.WaitForExitAsync();

                if (ffmpeg.ExitCode != 0)
                {
                    var error = await ffmpeg.StandardError.ReadToEndAsync();
                    throw new Exception($"FFmpeg error: {error}");
                }

                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
        public string GetFileServerPath()
        {
            var FolderPath = Path.Combine(DirectoryPath, CurDate);
            var path = new Uri(FolderPath).LocalPath;
            if (!Directory.Exists(path)) Directory.CreateDirectory(path);
            return path;
        }
        public async Task<bool> DeleteFile(AttachmentTransaction model)
        {
            await request.DeleteAsync(model, true);
            return true;
        }
        public bool DeletePhysicalFile(string folder, string file)
        {
            var dp = Path.Combine(DirectoryPath, folder, file);
            var output = new Uri(dp).LocalPath;
            if (File.Exists(new Uri(output).LocalPath))
            {
                File.Delete(output);

                return true;
            }
            return false;
        }
        //public static bool IsImageFile(string filePath)
        //{
        //    try
        //    {
        //        using var img = Image.FromFile(filePath);
        //        return true;
        //    }
        //    catch
        //    {
        //        return false;
        //    }
        //}
        //public async Task<bool> DeleteAttachment(FileAttachmentsTransactionDTO mod)
        //{

        //    UploadServiceBase upload = new(configuration);
        //    var mapFile = await GetFileAttachment(mod.FileId);
        //    var data = await DeleteFileAttachment(mapFile);
        //    if (data)
        //    {
        //        upload.DeletePhysicalFile(mapFile.FilePath, mapFile.FileId + mapFile.FileExtension);
        //        if (mapFile.IsThumb)
        //            upload.DeletePhysicalFile(mapFile.FilePath, mapFile.FileId + "_thumb" + mapFile.FileExtension);
        //    }
        //    return await Task.Run(() => true);

        //}
    }
}
