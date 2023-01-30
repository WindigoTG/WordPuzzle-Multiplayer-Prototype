using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Firebase.Storage;
using Firebase.Extensions;
using System.Threading.Tasks;

namespace WordPuzzle
{
    public class StorageService
    {
        #region Fields

        StorageReference _storage;

        #endregion


        #region ClassLifeCycles

        public StorageService()
        {
            _storage = FirebaseStorage.DefaultInstance.GetReference(References.USER_PHOTOS);
        }

        #endregion


        #region Methods

        public async Task<Texture2D> GetTexture()
        {
            var task =_storage.Child("Windigo-tennobet.png").GetBytesAsync(1 * 1024 * 1024);

            await task;

            Texture2D texture = new Texture2D(250, 250);
            texture.LoadImage(task.Result);

            return texture;
        }

        public async Task<Texture2D> GetPhotoByUserID(string userID)
        {
            Task<byte[]> task;
            do
            {
                task = _storage.Child($"{userID}.png").GetBytesAsync(1 * 1024 * 1024);
                while (!task.IsCompleted)
                    await Task.Yield();

                if (task.Exception != null)
                {
                    if (task.Exception.InnerException is StorageException)
                        return null;
                }
            } while (task.IsFaulted);

            Texture2D photo = new Texture2D(250, 250);
            photo.LoadImage(task.Result);

            return photo;
        }

        public async Task UploadUserPhoto(Texture2D photo, string userID)
        {
            var newMetadata = new MetadataChange();
            newMetadata.ContentType = "image/png";

            StorageReference uploadRef = _storage.Child($"{userID}.png");
 
            Task<StorageMetadata> task;
            do
            {
                task = uploadRef.PutBytesAsync(photo.EncodeToPNG(), newMetadata);
                await task;
            } while (task.IsFaulted);

        }

        #endregion
    }
}